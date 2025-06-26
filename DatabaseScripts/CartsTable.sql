CREATE TABLE IF NOT EXISTS Carts(
	-- id: id koszyka
	-- userid: czyj koszyk
	-- status: status koszyka
	id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	userid INT NOT NULL,
	FOREIGN KEY(userid) REFERENCES Users(id),
	status ENUM('Ordered', 'Pending', 'Abandoned') DEFAULT 'Pending'
)