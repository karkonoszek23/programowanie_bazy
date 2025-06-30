// UserLogin.cs
using System;
using MD5;
using Requests; // Add this using directive

namespace Validation
{
    public class UserLogin
    {
        private String Username { get; set; } // Changed Login to Username
        private String Password { get; set; }

        public UserLogin(LoginRequest req)
        {
            string encryptedLogin = Md5.Encrypt(req.Username); // Uses req.Username
            string encryptedPassword = Md5.Encrypt(req.Password);
            this.Username = encryptedLogin;
            this.Password = encryptedPassword;
        }

        public string[] FetchFields()
        {
            return [Username, Password]; // Returns Username
        }
    }
}
