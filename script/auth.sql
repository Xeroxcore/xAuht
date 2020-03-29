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

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO testuser;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO testuser;