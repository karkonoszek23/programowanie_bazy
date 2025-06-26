CREATE TABLE IF NOT EXISTS Payments(
	-- id: id platnosci,
	-- userid: uzytkownik powiazany z platnoscia,
	-- cartid: koszyk powiazany z platnoscia,
	-- status: status platnosci
	id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	userid INT NOT NULL,
	FOREIGN KEY(userid) REFERENCES Users(id),
	orderid INT  NOT NULL,
	FOREIGN KEY(orderid) REFERENCES Orders(id),
	status ENUM('Pending', 'Finished', 'Abandoned') DEFAULT 'Pending'
);