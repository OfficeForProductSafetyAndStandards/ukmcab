using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class LoginViewModel : ILayoutModel
    {
        public string? Title => "Login";
        public string ReturnURL { get; set; }

        [Required(ErrorMessage = "Enter email address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
