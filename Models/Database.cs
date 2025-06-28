using MySql.Data.MySqlClient;
using Validation;
namespace Database
{
    public class DBConnection
    {
        const string DbAddress = "127.0.0.1";
        const string Port = "3306";
        const string DbName = "shop";
        const string UserTable = "Users";
        const string UserLogins = "UserLogIns";

        private string UserName { get; set; }
        private string Password { get; set; }
        private string connInfo { get; set; }

        public DBConnection(string user, string passwd)
        {
            UserName = user;
            Password = passwd;
            connInfo = $"Server={DbAddress};Port={Port};Database={DbName};Uid={UserName};Pwd={Password};";
        }

        public void LoginUser(string[] userData)
        {
            string userLoginQuery = String.Format(
                    "SELECT user_in_db({0}, {1})", userData[0], userData[1]
                    );
            MakeQuery(userLoginQuery);
        }

        public void RegisterUser(string[] userData)
        {
            string userAddQuery = String.Format(
                    "CALL add_user_full_info({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                    userData[0],
                    userData[1],
                    userData[2],
                    userData[3],
                    userData[4],
      userData[5],
                    userData[6],
                    userData[7],
                    userData[8]);
            MakeQuery(userAddQuery);
        }

        private void MakeQuery(string query)
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
