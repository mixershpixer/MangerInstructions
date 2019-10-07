using System;
using System.ComponentModel.DataAnnotations;

namespace MangerInstructions.ViewModel
{
    public class RegisterUser
    {
        private String password;
        private String email;

        public String Id { get; set; }

        [Required(ErrorMessage = "EnterName")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "LengthSetting")]
        public String Name { get; set; }

        [EmailAddress(ErrorMessage = "EmailIsIncorrect")]
        [Required(ErrorMessage = "EnterEmail")]
        public String Email
        {
            get => email;
            set => email = value.ToLower();
        }

        [Required(ErrorMessage = "EnterPassword")]
        [StringLength(128, MinimumLength = 3, ErrorMessage = "PasswordSetting")]
        [DataType(DataType.Password)]
        public String Password
        {
            get => password;
            set => password = HashPassword.GetHash(value);
        }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "IncorrectPassword")]
        public String ConfirmPassword
        {
            get => password;
            set => password = HashPassword.GetHash(value);
        }
    }
}
