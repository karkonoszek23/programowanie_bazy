DELIMITER //
CREATE OR REPLACE PROCEDURE user_cart(
	IN user_id INT
)
BEGIN
	SELECT
		CI.id AS cart_item_id,
	    CI.id,
	    IS_.name AS product_name,
	    IS_.description,
	    IS_.price,
	    CI.quantity,
	    (IS_.price * CI.quantity) AS total_item_price
	FROM
		CartItems CI
	JOIN
		ItemsInShop IS_ ON CI.id = IS_.id
	JOIN
		Carts C ON CI.cartid = C.id
	WHERE
		C.userid = user_id AND C.status = 'Pending';
END //

DELIMITER ;