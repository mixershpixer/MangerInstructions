using System;
using System.ComponentModel.DataAnnotations;

namespace MangerInstructions.ViewModel
{
    public class LoginUser
    {
        private String password;
        private String email;

        [Required(ErrorMessage = "PressEmail")]
        public String Email
        {
            get => email;
            set => email = value.ToLower();
        }

        [Required(ErrorMessage = "PressPassword")]
        [DataType(DataType.Password)]
        public String Password
        {
            get => password;
            set
            {
                password = HashPassword.GetHash(value);
            }
        }
    }
}
