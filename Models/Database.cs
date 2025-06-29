using MySql.Data.MySqlClient;
using System;

namespace Database
{
    public class DBConnection
    {
        const string DbAddress = "localhost";
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

        public bool LoginUser(string username, string password)
        {
            string query = "SELECT user_in_db(@p_username, @p_password)";

            using (MySqlConnection conn = new MySqlConnection(connInfo))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@p_username", username);
                        command.Parameters.AddWithValue("@p_password", password);


                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result) == 1;
                        }
                        return false;
                    }
                }
                catch (MySqlException ex)
                {
                    Console.Error.WriteLine($"Database login error for user '{username}': {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An unexpected error occurred during login for user '{username}': {ex.Message}");
                    return false;
                }
            }
        }

        public void RegisterUser(string[] userData)
        {
            string query = "CALL add_user_full_info(@param0, @param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";

            using (MySqlConnection conn = new MySqlConnection(connInfo))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand command = new MySqlCommand(query, conn))
                    {
                        for (int i = 0; i < userData.Length; i++)
                        {
                            command.Parameters.AddWithValue($"@param{i}", userData[i]);
                        }

                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"User registration procedure executed. Rows affected (if any from procedure): {rowsAffected}");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.Error.WriteLine($"Database registration error: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An unexpected error occurred during user registration: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
