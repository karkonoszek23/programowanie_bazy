using System;
using MD5;

namespace Validation
{
    public class UserLogin
    {
        private String Login { get; set; }
        private String Password { get; set; }

        public UserLogin(LoginRequest req)
        {
            string encryptedLogin = Md5.Encrypt(req.Username);
            string encryptedPassword = Md5.Encrypt(req.Password);
            this.Login = encryptedLogin;
            this.Password = encryptedPassword;
        }

        public string[] FetchFields()
        {
            return [Login, Password];

        }

    }
}
