// Database.cs
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Requests;

public class DBConnection
{
    private string DbAddress = "localhost";
    private int Port = 3306;
    private string DbName = "shop";
    private string UserName;
    private string Password;
    private string ConnectionString;

    public DBConnection(string user, string passwd)
    {
        UserName = user;
        Password = passwd;
        ConnectionString = $"Server={DbAddress};Port={Port};Database={DbName};Uid={UserName};Pwd={Password};";
    }

    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(ConnectionString);
    }

    public bool LoginUser(string username, string password)
    {
        string query = "SELECT user_in_db(@login, @passwd)";
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@login", username);
                    command.Parameters.AddWithValue("@passwd", password);
                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas logowania: {ex.Message}");
                return false;
            }
        }
    }

    public int GetUserId(string username)
    {
        string query = @"
            SELECT 
            user_id 
            FROM 
            UserCredentials UC
            WHERE
            UC.login = @login
            ";
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@login", username);
                    object result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : -1;
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas pobierania ID użytkownika: {ex.Message}");
                return -1;
            }
        }
    }

    public void RegisterUser(string[] userData)
    {
        if (userData.Length != 9)
        {
            throw new ArgumentException("Tablica userData musi zawierać dokładnie 9 elementów do rejestracji użytkownika.");
        }

        string query = "CALL add_user_full_info(@login_hash, @passwd_hash, @email, @name, @last_name, @birthday, @gender, @phone_number, @address)";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@login_hash", userData[0]);
                    command.Parameters.AddWithValue("@passwd_hash", userData[1]);
                    command.Parameters.AddWithValue("@email", userData[2]);
                    command.Parameters.AddWithValue("@name", userData[3]);
                    command.Parameters.AddWithValue("@last_name", userData[4]);
                    command.Parameters.AddWithValue("@birthday", Convert.ToDateTime(userData[5]));
                    command.Parameters.AddWithValue("@gender", userData[6]);
                    command.Parameters.AddWithValue("@phone_number", Convert.ToInt32(userData[7]));
                    command.Parameters.AddWithValue("@address", userData[8]);
                    command.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas rejestracji: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine($"Błąd formatu danych podczas rejestracji użytkownika: {ex.Message}. Sprawdź format daty urodzenia i numeru telefonu.");
                throw;
            }
        }
    }

    public List<CartItem> GetCartItems(int userId)
    {
        var cartItems = new List<CartItem>();
        string query = @"
            SELECT
            CI.id AS item_id,
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
            C.userid = @userId AND C.status = 'Pending';";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    Console.WriteLine($"User with ID: {userId} requested.");
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cartItems.Add(new CartItem
                            {
                                ItemId = reader.GetInt32("item_id"),
                                ProductName = reader.GetString("product_name"),
                                Description = reader.GetString("description"),
                                Price = reader.GetDecimal("price"),
                                Quantity = reader.GetInt32("quantity"),
                                TotalItemPrice = reader.GetDecimal("total_item_price")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas pobierania przedmiotów z koszyka: {ex.Message}");
            }
        }
        return cartItems;
    }

    public CartInfo GetCartInfo(int userId)
    {
        CartInfo cartInfo = null;
        string query = @"
            SELECT
            C.id AS cartid,
        COUNT(CI.id) AS item_count,
        COALESCE(SUM(IIS.price * CI.quantity), 0) AS total
            FROM
            Carts C
            LEFT JOIN
            CartItems CI ON C.id = CI.cartid
            LEFT JOIN
            ItemsInShop IIS ON CI.id = IIS.id
            WHERE
            C.userid = @userId AND C.status = 'Pending';
        GROUP BY
            C.id;";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cartInfo = new CartInfo
                            {
                                CartId = reader.GetInt32("cart_id"),
                                ItemCount = reader.GetInt32("item_count"),
                                TotalValue = reader.GetDecimal("total_value")
                            };
                        }
                    }
                }

                if (cartInfo == null)
                {
                    cartInfo = CreateNewCart(userId);
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas pobierania informacji o koszyku: {ex.Message}");
            }
        }
        return cartInfo;
    }

    private CartInfo CreateNewCart(int userId)
    {
        string query = "INSERT INTO Carts (userid, status) VALUES (@userId, 'Pending'); SELECT LAST_INSERT_ID();";
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    int newCartId = Convert.ToInt32(command.ExecuteScalar());
                    return new CartInfo { CartId = newCartId, ItemCount = 0, TotalValue = 0 };
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas tworzenia nowego koszyka: {ex.Message}");
                return null;
            }
        }
    }

    public bool AddToCart(int userId, int itemId, int quantity = 1)
    {
        var cartInfo = GetCartInfo(userId);
        if (cartInfo == null) return false;

        ShopItem shopItem = GetShopItemById(itemId);
        if (shopItem == null || shopItem.StockQuantity < quantity)
        {
            Console.WriteLine($"Produkt o ID {itemId} nie jest dostępny w wystarczającej ilości.");
            return false;
        }

        string query = @"
            INSERT INTO CartItems (itemid, cartid, added_by, quantity)
            VALUES (@itemId, @cartId, @userId, @quantity)
            ON DUPLICATE KEY UPDATE quantity = quantity + @quantity";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@itemId", itemId);
                    command.Parameters.AddWithValue("@cartId", cartInfo.CartId);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas dodawania do koszyka: {ex.Message}");
                return false;
            }
        }
    }

    public bool RemoveFromCart(int userId, int itemId, int quantity = 1)
    {
        var cartInfo = GetCartInfo(userId);
        if (cartInfo == null) return false;

        string query = @"
            UPDATE CartItems
            SET quantity = GREATEST(0, quantity - @quantity)
            WHERE itemid = @itemId AND cartid = @cartId;

        DELETE FROM CartItems WHERE quantity = 0 AND cartid = @cartId;";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@itemId", itemId);
                    command.Parameters.AddWithValue("@cartId", cartInfo.CartId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas usuwania z koszyka: {ex.Message}");
                return false;
            }
        }
    }

    public bool ClearCart(int userId)
    {
        var cartInfo = GetCartInfo(userId);
        if (cartInfo == null) return false;

        string query = "DELETE FROM CartItems WHERE cartid = @cartId";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@cartId", cartInfo.CartId);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas czyszczenia koszyka: {ex.Message}");
                return false;
            }
        }
    }

    public List<ShopItem> GetShopItems()
    {
        var shopItems = new List<ShopItem>();
        string query = "SELECT id, name, description, price, quantity_left FROM ItemsInShop";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shopItems.Add(new ShopItem
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Description = reader.GetString("description"),
                                Price = reader.GetDecimal("price"),
                                StockQuantity = reader.GetInt32("quantity_left")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas pobierania przedmiotów ze sklepu: {ex.Message}");
            }
        }
        return shopItems;
    }

    public ShopItem GetShopItemById(int itemId)
    {
        ShopItem shopItem = null;
        string query = "SELECT id, name, description, price, quantity_left FROM ItemsInShop WHERE id = @itemId";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@itemId", itemId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            shopItem = new ShopItem
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Description = reader.GetString("description"),
                                Price = reader.GetDecimal("price"),
                                StockQuantity = reader.GetInt32("quantity_left")
                            };
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas pobierania przedmiotu ze sklepu po ID: {ex.Message}");
            }
        }
        return shopItem;
    }

    public bool UpdateStockQuantity(int itemId, int quantityChange, MySqlConnection conn, MySqlTransaction transaction = null)
    {
        string query = "UPDATE ItemsInShop SET quantity_left = quantity_left + @quantityChange WHERE id = @itemId";
        using (MySqlCommand command = new MySqlCommand(query, conn, transaction))
        {
            command.Parameters.AddWithValue("@quantityChange", quantityChange);
            command.Parameters.AddWithValue("@itemId", itemId);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    public bool PlaceOrder(int userId)
    {
        List<CartItem> itemsInCart = GetCartItems(userId);

        var cartInfo = GetCartInfo(userId);
        if (cartInfo == null || cartInfo.ItemCount == 0)
        {
            Console.WriteLine($"Koszyk dla użytkownika {userId} jest pusty lub nie znaleziono.");
            return false;
        }

        string updateCartStatusQuery = "UPDATE Carts SET status = 'Ordered' WHERE id = @cartId";
        string insertOrderQuery = "INSERT INTO Orders (userid, cartid, total_amount, order_date) VALUES (@userId, @cartId, @totalAmount, @orderDate); SELECT LAST_INSERT_ID();";

        using (MySqlConnection conn = GetConnection())
        {
            conn.Open();
            MySqlTransaction transaction = conn.BeginTransaction();
            try
            {
                int orderId;
                using (MySqlCommand insertOrderCommand = new MySqlCommand(insertOrderQuery, conn, transaction))
                {
                    insertOrderCommand.Parameters.AddWithValue("@userId", userId);
                    insertOrderCommand.Parameters.AddWithValue("@cartId", cartInfo.CartId);
                    insertOrderCommand.Parameters.AddWithValue("@totalAmount", cartInfo.TotalValue);
                    insertOrderCommand.Parameters.AddWithValue("@orderDate", DateTime.Now);
                    orderId = Convert.ToInt32(insertOrderCommand.ExecuteScalar());
                }

                using (MySqlCommand updateCartCommand = new MySqlCommand(updateCartStatusQuery, conn, transaction))
                {
                    updateCartCommand.Parameters.AddWithValue("@cartId", cartInfo.CartId);
                    updateCartCommand.ExecuteNonQuery();
                }

                string insertOrderItemQuery = "INSERT INTO OrderItems (orderid, itemid, quantity, price) VALUES (@orderId, @itemId, @quantity, @price)";
                foreach (var item in itemsInCart)
                {
                    using (MySqlCommand insertOrderItemCommand = new MySqlCommand(insertOrderItemQuery, conn, transaction))
                    {
                        insertOrderItemCommand.Parameters.AddWithValue("@orderId", orderId);
                        insertOrderItemCommand.Parameters.AddWithValue("@itemId", item.ItemId);
                        insertOrderItemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                        insertOrderItemCommand.Parameters.AddWithValue("@price", item.Price);
                        insertOrderItemCommand.ExecuteNonQuery();
                    }
                    UpdateStockQuantity(item.ItemId, -item.Quantity, conn, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch (MySqlException ex)
            {
                transaction.Rollback();
                Console.Error.WriteLine($"Błąd podczas składania zamówienia: {ex.Message}");
                return false;
            }
        }
    }

    public List<Order> GetUserOrders(int userId)
    {
        var orders = new List<Order>();
        string query = "SELECT id, userid, cartid, total_amount, order_date, status FROM Orders WHERE userid = @userId ORDER BY id DESC";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new Order
                            {
                                Id = reader.GetInt32("id"),
                                UserId = reader.GetInt32("userid"),
                                CartId = reader.GetInt32("cartid"),
                                TotalAmount = reader.GetDecimal("total_amount"),
                                OrderDate = reader.GetDateTime("order_date"),
                                Status = reader.GetString("status")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas pobierania zamówień użytkownika: {ex.Message}");
            }
        }
        return orders;
    }

    public List<OrderItem> GetOrderItems(int orderId)
    {
        var orderItems = new List<OrderItem>();
        string query = @"
            SELECT
            OI.itemid,
        IS.name AS product_name,
        OI.quantity,
        OI.price AS item_price,
        (OI.quantity * OI.price) AS total_item_price
            FROM
            OrderItems OI
            JOIN
            ItemsInShop IS ON OI.itemid = IS.id
            WHERE
            OI.orderid = @orderId";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@orderId", orderId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderItems.Add(new OrderItem
                            {
                                ItemId = reader.GetInt32("itemid"),
                                ProductName = reader.GetString("product_name"),
                                Quantity = reader.GetInt32("quantity"),
                                ItemPrice = reader.GetDecimal("item_price"),
                                TotalItemPrice = reader.GetDecimal("total_item_price")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.Error.WriteLine($"Błąd bazy danych podczas pobierania przedmiotów zamówienia: {ex.Message}");
            }
        }
        return orderItems;
    }
}
