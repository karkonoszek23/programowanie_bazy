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
                string login,
                string email,
                string password,
                string name,
                string lastname,
                string birthday,
                string gender,
                string phonenumber,
                string address)
        {
            string encryptedLogin = Md5.Encrypt(login);
            string encryptedPassword = Md5.Encrypt(password);
            this.Login = encryptedLogin;
            this.Email = email;
            this.Password = encryptedPassword;
            this.Name = name;
            this.LastName = lastname;
            this.Birthday = birthday;
            this.Gender = gender;
            this.PhoneNumber = phonenumber;
            this.Address = address;
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
