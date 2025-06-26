CREATE TABLE IF NOT EXISTS Deliveries(
	id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	orderid INT NOT NULL,
	FOREIGN KEY(orderid) REFERENCES Orders(id),
	status ENUM('Delivered', 'InTransit', 'Lost') DEFAULT 'InTransit'
)