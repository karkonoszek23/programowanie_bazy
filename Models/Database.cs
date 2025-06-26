using MySql.Data.MySqlClient;
namespace Database
{
    public class DBConnection
    {
        const string DbAddress = "127.0.0.1";
        const string Port = "3306";
        const string DbName = "shop";
        private string UserName { get; set; }
        private string Password { get; set; }
        private string connInfo { get; set; }

        public DBConnection(string user, string passwd)
        {
            UserName = user;
            Password = passwd;
            connInfo = $"Server={DbAddress};Port={Port};Database={DbName};Uid={UserName};Pwd={Password};";
        }

        public void MakeQuery(string query)
        {
            using (MySqlConnection conn = new MySqlConnection(connInfo))
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
                                Console.WriteLine($"Id: {reader["id"]}, Name: {reader["name"]}");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
        }
    }
}
