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
	userid INT,
	tokenid INT,
	CONSTRAINT uniquetoken UNIQUE (token),
	FOREIGN KEY (userid) REFERENCES useraccount(id),
	FOREIGN KEY (tokenid) REFERENCES tokenkey(id)
);

CREATE TABLE roles(
	id SERIAL NOT NULL PRIMARY KEY,
	name VARCHAR(50) NOT NULL DEFAULT 'NOT DEFINED'
);

CREATE TABLE useraccount_roles(
	useraccountid INT NOT NULL,
	rolesid INT NOT NULL,
	PRIMARY KEY (useraccountid,rolesid),
	CONSTRAINT user_role UNIQUE (useraccountid,rolesid),
	FOREIGN KEY (useraccountid) REFERENCES useraccount(id),
	FOREIGN KEY (rolesid) REFERENCES roles(id)
);

CREATE TABLE token_roles(
	tokenid INT NOT NULL,
	rolesid INT NOT NULL,
	PRIMARY KEY (tokenid,rolesid),
	CONSTRAINT token_role UNIQUE (tokenid,rolesid),
	FOREIGN KEY (tokenid) REFERENCES tokenkey(id),
	FOREIGN KEY (rolesid) REFERENCES roles(id)
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

CREATE OR REPLACE FUNCTION getuserbyid (key INT)
	RETURNS TABLE (id INT, username VARCHAR(250), password VARCHAR(250), lockout INT, lockexpire TIMESTAMP)
AS $$
	SELECT * FROM useraccount WHERE id = key;
$$ LANGUAGE SQL;

CREATE OR REPLACE FUNCTION gettokenbyid (key INT)
	RETURNS TABLE (id INT, token VARCHAR(250), lockout INT, lockexpire TIMESTAMP)
AS $$
	SELECT * FROM tokenkey WHERE id = key;
$$ LANGUAGE SQL;

CREATE OR REPLACE FUNCTION getrefreshtoken (reftoken VARCHAR(250))
	RETURNS TABLE (id INT, token VARCHAR(250), used BOOLEAN, expired TIMESTAMP, userid INT, tokenid INT)
AS $$
	SELECT * FROM refreshtoken WHERE token = reftoken;
$$ LANGUAGE SQL;


/* Procedures */

CREATE OR REPLACE PROCEDURE adduser(name VARCHAR(250), pass VARCHAR(250))
AS $$
	INSERT  INTO useraccount (username, password) VALUES(name,pass);
$$
LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE addtoken(key VARCHAR(250))
AS $$
	INSERT  INTO tokenkey (token) VALUES(key);
$$
LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE addfreshtokenuser(reftoken VARCHAR(250), userkey INT)
AS $$
	INSERT INTO refreshtoken (token, userid) VALUES(reftoken, userkey);
$$
LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE addfreshtokentoken(reftoken VARCHAR(250), tokenkey INT)
AS $$
	INSERT INTO refreshtoken (token, tokenid) VALUES(reftoken, tokenkey);
$$
LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE adduserrole(userkey INT, roleskey INT)
AS $$ 
	INSERT INTO useraccount_roles (useraccountid,rolesid) VALUES (userkey, roleskey);
$$ LANGUAGE SQL;

CREATE OR REPLACE PROCEDURE addtokenrole(tokenkey INT, roleskey INT)
AS $$ 
	INSERT INTO token_roles (tokenid,rolesid) VALUES (tokenkey, roleskey);
$$ LANGUAGE SQL;

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