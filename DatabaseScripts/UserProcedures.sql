DELIMITER //

CREATE OR REPLACE PROCEDURE add_user_credentials(
	IN name VARCHAR(100),
	IN last_name VARCHAR(100),
	IN birthday DATE,
	IN gender CHAR(1),
	IN phone_number INT,
	IN address VARCHAR(200)
)
-- Dodaj dane osobowe uzytkownika do tabeli.
BEGIN
	INSERT INTO 
		Users (name, last_name, birthday, gender, phone_number, address)
	VALUES 
		(name, last_name, birthday, gender, phone_number, address);
END //
          
CREATE OR REPLACE PROCEDURE add_user_account(
	IN user_id INT,
	IN login_hash VARCHAR(64),
	IN passwd_hash VARCHAR(64),
	IN email VARCHAR(100)
)
BEGIN
	-- Dodaj dane do logowania uzytkownika do tabeli.
	INSERT INTO 
		UserCredentials (user_id, when_registered, login, email, password)
	VALUES
		(user_id, NOW(), login_hash, email, passwd_hash);
END //


CREATE OR REPLACE PROCEDURE add_user_full_info(
	IN login_hash VARCHAR(64),
	IN passwd_hash VARCHAR(64),
	IN email VARCHAR(100),	
	IN name VARCHAR(100),
	IN last_name VARCHAR(100),
	IN birthday DATE,
	IN gender CHAR(1),
	IN phone_number INT,
	IN address VARCHAR(200)
)
BEGIN
	-- Wrapper wokol dwoch procedur, uzywac tylko tego.
	DECLARE user_id INT;
	CALL add_user_credentials(name, last_name, birthday, gender, phone_number, address);
	SET user_id = LAST_INSERT_ID();
	CALL add_user_account(user_id, login_hash, passwd_hash, email);
END //

CREATE TRIGGER new_user_login_status
AFTER INSERT ON UserCredentials
FOR EACH ROW
BEGIN
	CALL add_recent_login(NEW.login);
END //

CREATE OR REPLACE FUNCTION user_in_db(
	login VARCHAR(100),
	passwd VARCHAR(255)
)
RETURNS BOOLEAN DETERMINISTIC
BEGIN
	IF EXISTS (
        SELECT 1
        FROM UserCredentials UC
        WHERE UC.login = login AND UC.password = passwd
    ) THEN
		RETURN TRUE;
	ELSE
		RETURN FALSE;
	END IF;
END //

CREATE OR REPLACE FUNCTION fetch_user_id(
	login VARCHAR(100)
)
RETURNS INT DETERMINISTIC
BEGIN
	DECLARE user_id INT;
	SET user_id = (
		SELECT 
			user_id 
		FROM 
			UserCredentials UC
		WHERE
			TRIM(UC.login) = TRIM(login)
		);
	RETURN user_id;
END //

CREATE OR REPLACE PROCEDURE add_recent_login(
	IN login VARCHAR(100)
)
BEGIN
	DECLARE user_id INT;
	SET user_id = fetch_user_id(login);

	INSERT INTO
		UserLogIns
	VALUES
		(user_id, NOW());
END //
DELIMITER ;