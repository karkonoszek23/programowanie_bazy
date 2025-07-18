using System;
using MD5;
using System.Text.RegularExpressions;
using Requests;

namespace Validation
{
    public class UserRegistration
    {
        private string Username { get; set; }
        private string Email { get; set; }
        private string Password { get; set; }
        private string Name { get; set; }
        private string LastName { get; set; }
        private string Birthday { get; set; }
        private string Gender { get; set; }
        private string PhoneNumber { get; set; }
        private string Address { get; set; }

        public UserRegistration(RegisterRequest req)
        {
            this.Username = req.Username;
            this.Password = req.Password;
            this.Email = req.Email;
            this.Name = req.Name;
            this.LastName = req.LastName;
            this.Birthday = req.Birthday;
            this.Gender = req.Gender;
            this.PhoneNumber = req.PhoneNumber;
            this.Address = req.Address;
            Hash();
        }

        public bool IsLoginPasswdCorrectLength()
        {
            return Username.Length > 6 && Password.Length > 8;
        }

        public bool IsEmailCorrect()
        {
            Regex email = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return email.IsMatch(Email);
        }

        public bool IsGenderCorrect()
        {
            return Gender == "K" || Gender == "M" || Gender == "N";
        }

        public bool IsPhoneNumberCorrect()
        {
            return PhoneNumber.Length == 9 && PhoneNumber is not null;
        }

        public int AmIGood()
        {
            if (!IsLoginPasswdCorrectLength())
            {
                return 1;
            }
            if (!IsEmailCorrect())
            {
                return 2;
            }
            if (!IsGenderCorrect())
            {
                return 3;
            }
            if (!IsPhoneNumberCorrect())
            {
                return 4;
            }
            return 0;
        }

        private void Hash()
        {
            string encryptedLogin = Md5.Encrypt(Username);
            string encryptedPassword = Md5.Encrypt(Password);
            Username = encryptedLogin;
            Password = encryptedPassword;
        }

        public string[] FetchFields()
        {
            return [Username, Password, Email, Name, LastName, Birthday, Gender,
                                                        PhoneNumber, Address];
        }
    }
}
