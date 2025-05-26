using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "The Name field is required.")]
        [StringLength(50, ErrorMessage = "The Name cannot be longer than 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Password field is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*]).+$", ErrorMessage = "The password must contain at least one capital letter, one number, and one special character (!@#$%^&*).")]
        public string Password { get; set; }

        //[Required(ErrorMessage = "The Role field is required.")]
        //public string Role { get; set; }
    }
}