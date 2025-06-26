using System;
using MD5;

namespace Validation
{
    public class CreateUser
    {
        private string Login { get; set; }
        private string Email { get; set; }
        private string Password { get; set; }
        private string Name { get; set; }
        private string LastName { get; set; }
        private string Birthday { get; set; }
        private string Gender { get; set; }
        private string PhoneNumber { get; set; }
        private string Address { get; set; }

        public CreateUser(
                RegisterRequest req)
        {
            string encryptedLogin = Md5.Encrypt(req.Username);
            string encryptedPassword = Md5.Encrypt(req.Password);
            this.Login = encryptedLogin;
            this.Password = encryptedPassword;
            this.Email = req.Email;
            this.Name = req.Name;
            this.LastName = req.LastName;
            this.Birthday = req.Birthday;
            this.Gender = req.Gender;
            this.PhoneNumber = req.PhoneNumber;
            this.Address = req.Address;
        }

        public void Push()
        {
            Console.WriteLine("do bazy");
        }

        public bool AmIGood()
        {
            // request do bazy
            return 1 == 1;
        }

    }
}
