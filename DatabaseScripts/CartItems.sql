CREATE TABLE IF NOT EXISTS CartItems(
	-- przedmiot w danym koszyku
	-- id: id rzeczy w sklepie, ta rzecz jest w koszyku,
	-- cartid: koszyk do ktorego przedmiot bedzie nalezec,
	-- quantity: ile zamowiono,
	id INT NOT NULL,
	FOREIGN KEY(id) REFERENCES ItemsInShop(id),
	
	cartid INT NOT NULL,
	FOREIGN KEY(cartid) REFERENCES Carts(id),
	
	added_by INT NOT NULL,
	FOREIGN KEY(added_by) REFERENCES Users(id),
	
	quantity INT NOT NULL
)