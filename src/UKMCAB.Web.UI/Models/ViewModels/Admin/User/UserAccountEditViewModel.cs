using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User
{
    public class UserAccountEditViewModel : ILayoutModel
    {
        public string? Title => "Edit user account";
        public UserAccount? UserAccount { get; set; }

        [Required(ErrorMessage = "Enter a contact email address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Enter an organisation")]
        public string Organisation { get; set; }
        [Required(ErrorMessage = "Enter a role")]
        public string UserGroup { get; set; }

        public string? ReturnURL { get; set; }

        public string[] FieldOrder => new[] { nameof(Email), nameof(Organisation), nameof(UserGroup) };

    }
}
