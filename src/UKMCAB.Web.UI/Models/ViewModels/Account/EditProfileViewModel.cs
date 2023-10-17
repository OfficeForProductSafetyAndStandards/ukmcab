using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class EditProfileViewModel : ILayoutModel
    {
        public string? Title => "Edit my details";
        
        [Required(ErrorMessage = "Enter a first name")]
        public string? FirstName { get; set; }
        
        [Required(ErrorMessage = "Enter a last name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Enter an email address")]
        [RegularExpression("^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        public string? ContactEmailAddress { get; set; }

        public string[] FieldOrder => new[] { nameof(FirstName), nameof(LastName), nameof(ContactEmailAddress) };

        public UserAccount GetUserAccount() => new UserAccount { FirstName = FirstName, Surname = LastName, ContactEmailAddress = ContactEmailAddress };
    }
}
