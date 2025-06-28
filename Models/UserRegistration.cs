using System;
using MD5;
using System.Text.RegularExpressions;
namespace Validation
{
    public class UserRegistration
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

        public UserRegistration(RegisterRequest req)
        {
            this.Login = req.Username;
            this.Password = req.Password;
            this.Email = req.Email;
            this.Name = req.Name;
            this.LastName = req.LastName;
            this.Birthday = req.Birthday;
            this.Gender = req.Gender;
            this.PhoneNumber = req.PhoneNumber;
            this.Address = req.Address;
            if (AmIGood().Equals(0))
                Hash();
        }

        public bool IsLoginPasswdCorrectLength()
        {
            return Login.Length > 6 && Password.Length > 8;
        }

        public bool IsEmailCorrect()
        {
            Regex email = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return email.IsMatch(Email);
        }

        public bool IsGenderCorrect()
        {
            return Gender.Equals('F') || Gender.Equals('M') || Gender.Equals('N');
        }

        public bool IsPhoneNumberCorrect()
        {
            return PhoneNumber.Length == 9;
        }

        public int AmIGood()
        {
            // Zwraca kod, wiadomosc zbindowa bedzie w view modelu,
            // jesli mamy blad.
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
            string encryptedLogin = Md5.Encrypt(Login);
            string encryptedPassword = Md5.Encrypt(Password);
            Login = encryptedLogin;
            Password = encryptedPassword;
        }

        public string[] FetchFields()
        {
            return [Login, Password, Email, Name, LastName, Birthday, Gender,
                                                        PhoneNumber, Address];
        }

    }
}
