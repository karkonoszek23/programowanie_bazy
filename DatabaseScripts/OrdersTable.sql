CREATE TABLE IF NOT EXISTS Orders(
	id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	
	userid INT NOT NULL,
	FOREIGN KEY(userid) REFERENCES Users(id),
	
	cartid INT  NOT NULL,
	FOREIGN KEY(cartid) REFERENCES Carts(id),

	status ENUM('Delivered', 'InTransit', 'FailedToDeliver') DEFAULT 'InTransit'	
);