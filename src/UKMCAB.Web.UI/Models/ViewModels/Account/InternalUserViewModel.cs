using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class InternalUserViewModel : ILayoutModel
    {
        [Required(ErrorMessage = "Enter email address")]
        [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9._%+-]+\\.gov\\.uk$", ErrorMessage = "Only .gov.uk email addresses are acceptable")]
        [Display(Name = "Email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Enter first name")]
        [Display(Name = "First name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Enter last name")]
        [Display(Name = "Last name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Enter password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm password")]
        [Compare("Password", ErrorMessage = "Password does not match, try again.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string? ConfirmPassword { get; set; }

        public bool UserCreated { get; set; }

        public string? Title => "Internal user";

        public string[] FieldOrder => new[] { nameof(Email), nameof(FirstName), nameof(LastName), nameof(Password), nameof(ConfirmPassword) };
    }
}
