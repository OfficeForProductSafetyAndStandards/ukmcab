using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class ForgotPasswordViewModel: ILayoutModel
    {
        [Required]
        [RegularExpression("^.+(?:@ukas\\.com|@.+\\.gov\\.uk)$", ErrorMessage = "Only ukas.com or .gov.uk email addresses are valid")]
        [EmailAddress]
        public string Email { get; set; }

        public string? Title => "Forgot password";
    }
}
