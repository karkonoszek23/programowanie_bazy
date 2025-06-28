CREATE TABLE IF NOT EXISTS Users(
	-- id: id uzytkownika,
	-- name: imie uzytkownika,
	-- last_name: nazwisko uzytkownika,
	-- birthday: data urodzin uzytkownika,
	-- gender: plec,
	-- phone_number: numer telefonu komorkowego,
	-- address: miejsce zamieszkania,
	id INT PRIMARY KEY UNIQUE NOT NULL AUTO_INCREMENT,
	name VARCHAR(100) NOT NULL,
	last_name VARCHAR(100) NOT NULL,
	birthday DATE NOT NULL,
	gender ENUM('M', 'F', 'N') DEFAULT 'N',
	phone_number CHAR(9) NOT NULL,
	address VARCHAR(200) NOT NULL
)