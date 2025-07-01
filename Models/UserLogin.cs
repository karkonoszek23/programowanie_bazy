using System;
using MD5;
using Requests;

namespace Validation
{
    public class UserLogin
    {
        private String Username { get; set; }
        private String Password { get; set; }

        public UserLogin(LoginRequest req)
        {
            string encryptedLogin = Md5.Encrypt(req.Username);
            string encryptedPassword = Md5.Encrypt(req.Password);
            this.Username = encryptedLogin;
            this.Password = encryptedPassword;
        }

        public string[] FetchFields()
        {
            return [Username, Password];
        }
    }
}
