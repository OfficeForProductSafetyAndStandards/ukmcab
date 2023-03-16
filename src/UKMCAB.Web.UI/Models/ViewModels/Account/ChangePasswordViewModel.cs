using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class ChangePasswordViewModel: ILayoutModel
    {
        [Required(ErrorMessage = "Enter the current password")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Enter the new password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Re-enter the new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password does not match, try again.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }

        public bool PasswordChanged { get; set; }

        public string? Title => "Change password";
    }
}
