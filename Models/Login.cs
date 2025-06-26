using System;
using MD5;

namespace Validation
{
    public class FetchUser
    {
        private String Login { get; set; }
        private String Password { get; set; }

        public FetchUser(LoginRequest req)
        {
            string encryptedLogin = Md5.Encrypt(req.Username);
            string encryptedPassword = Md5.Encrypt(req.Password);
            this.Login = encryptedLogin;
            this.Password = encryptedPassword;
        }

        public bool AmIGood()
        {
            // request do bazy
            return 1 == 1;
        }

    }
}
