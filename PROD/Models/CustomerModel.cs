using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROD.Models 
{
    [Table("Customers")]
    public class CustomerModel
    {
        [Key]
        public int CustomerID { get; set; }

        [Required]
        [StringLength(255)]
        public string CustomerName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"(?=^.{8,}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$", ErrorMessage = "Password should contain 8 characters,one uppercase,one lowercase,one special characters atleast")]
        public string Password { get; set; }

        [Required]
        [NotMapped] // Does not effect with your database
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get;set; }

        // Default value is managed by database schema
        public int LoyaltyPoints { get; set; }

        [Required(ErrorMessage = "Please enter the CAPTCHA.")]
        [Display(Name = "CAPTCHA")]
        public string Captcha { get; set; }
    }

    public class ForgotPassword
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [RegularExpression(@"(?=^.{8,}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$", ErrorMessage = "Password should contain 8 characters,one uppercase,one lowercase,one special characters atleast")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [NotMapped]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
