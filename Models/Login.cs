using System;
using MD5;

namespace Validation
{
    public class FetchUser
    {
        private String Login { get; set; }
        private String Password { get; set; }

        public FetchUser(string login, string password)
        {
            string encryptedLogin = Md5.Encrypt(login);
            string encryptedPassword = Md5.Encrypt(password);
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
