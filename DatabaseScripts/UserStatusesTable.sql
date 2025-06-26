CREATE TABLE IF NOT EXISTS UserStatuses(
	user_id INT PRIMARY KEY UNIQUE NOT NULL AUTO_INCREMENT,
	FOREIGN KEY(user_id) REFERENCES Users(id),
	status ENUM('Active', 'InActive', 'Banned') DEFAULT 'Active' NOT NULL
)