/* PostgreSql script */

CREATE USER testuser WITH
    LOGIN
    NOSUPERUSER
    NOCREATEDB
    NOCREATEROLE
    INHERIT
    NOREPLICATION
    CONNECTION LIMIT -1
    PASSWORD 'helloword';

CREATE DATABASE testdb
    WITH 
    OWNER = testuser
    ENCODING = 'UTF8'
    CONNECTION LIMIT = -1;

CREATE TABLE useraccount (
	id serial not null primary key,
	username varchar(250) not null,
	password varchar(250) not null,
	lockout int not null default 0,
	lockexpire timestamp default now(),
	constraint username unique (username)
);

CREATE TABLE tokenkey (
	id serial not null primary key,
	token varchar(250) not null,
	lockout int not null default 0,
	lockexpire timestamp default now(),
	constraint token unique (token)
);

CREATE TABLE refreshtoken (
	id SERIAL NOT NULL PRIMARY KEY,
	token VARCHAR(250) NOT NULL,
	used boolean NOT NULL DEFAULT '0',
	expire TIMESTAMP NOT NULL DEFAULT NOW(),
	userid INT NOT NULL DEFAULT 0,
	tokenid INT NOT NULL DEFAULT 0,
	CONSTRAINT uniquetoken UNIQUE (token),
	FOREIGN KEY (userid) REFERENCES useraccount(id),
	FOREIGN KEY (tokenid) REFERENCES tokenkey(id)
);

/* Functions */

CREATE OR REPLACE FUNCTION getuser(iusername VARCHAR(250))
	RETURNS TABLE (id INT, username VARCHAR(250), password VARCHAR(250), lockout INT, lockexpire TIMESTAMP)
AS $$
	SELECT * FROM useraccount WHERE username = iusername;
$$ LANGUAGE SQL;

CREATE OR REPLACE FUNCTION gettoken(itoken VARCHAR(250))
	RETURNS TABLE (id INT, token VARCHAR(250), lockout INT, lockexpire TIMESTAMP)
AS $$
	SELECT * FROM tokenkey WHERE token = itoken;
$$ LANGUAGE SQL;


/* Procedures */

CREATE OR REPLACE PROCEDURE createuser(name VARCHAR(250), pass VARCHAR(250))
AS $$
	INSERT  INTO useraccount (username, password) VALUES(name,pass);
$$
LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE createtoken(key VARCHAR(250))
AS $$
	INSERT  INTO tokenkey (token) VALUES(key);
$$
LANGUAGE SQL;

/* Lock tracking Statments */

/* Lock if statments*/
CREATE OR REPLACE PROCEDURE failduserauth(userid INT) 
AS $$
	Declare
		lock INT;
BEGIN 
  select lockout into lock from useraccount where id = userid;
  IF lock > 2 THEN
    	call lockaccount(userid);
	ELSE
		call updateuserlockout(userid);
	END IF;
END; 
$$ 
LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE faildtokenauth(tokenid INT) 
AS $$
	Declare
		lock INT;
BEGIN 
  select lockout into lock from tokenkey where id = tokenid;
  IF lock > 2 THEN
    	call locktoken(tokenid);
	ELSE
		call updatetokenlockout(tokenid);
	END IF;
END; 
$$ 
LANGUAGE plpgsql;

/*Unlock logic*/
CREATE OR REPLACE PROCEDURE unlockaccount(userid INT) 
AS $$
BEGIN 
	UPDATE useraccount SET lockout = 0, lockexpire = NOW() - INTERVAL '15 MINUTE' WHERE id = userid;
END; 
$$ 
LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE unlocktoken(key INT) 
AS $$
BEGIN 
	UPDATE tokenkey SET lockout = 0, lockexpire = NOW() - INTERVAL '15 MINUTE' WHERE id = key;
END; 
$$ 
LANGUAGE SQL;

/* updating lockout Statments */
CREATE PROCEDURE failauthuser(key INT)
AS $$ 
	UPDATE useraccount SET lockout = lockout + 1 WHERE id = key;
$$ LANGUAGE SQL;

CREATE PROCEDURE failauthtoken(key INT)
AS $$ 
	UPDATE tokenkey SET lockout = lockout + 1 WHERE id = key;
$$ LANGUAGE SQL;

/* Lockaccount Statments */
CREATE OR REPLACE PROCEDURE lockaccount(key INT)
AS $$
	UPDATE useraccount SET lockout = 3, lockexpire = now() WHERE id = key;
$$
LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE locktoken(key INT)
AS $$
	UPDATE tokenkey SET lockout = 3, lockexpire = now() WHERE id = key;
$$
LANGUAGE SQL;


GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO testuser;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO testuser;