CREATE TABLE IF NOT EXISTS ItemsInShop(
	-- przedmioty na stanie w sklepie.
	-- id: id rzeczy w sklepie,
	-- name: nazwa rzeczy,
	-- description: opis danej rzeczy,
	-- price: cena danej rzeczy
	-- quantity_left: ile zostalo na stanie
	id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	name VARCHAR(100) NOT NULL,
	description VARCHAR(200) NOT NULL,
	price NUMERIC NOT NULL,
	quantity_left INT NOT NULL
)