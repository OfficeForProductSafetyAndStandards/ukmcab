using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class EditProfileViewModel : ILayoutModel
    {
        public string? Title => "Edit profile";
        
        [Required(ErrorMessage = "Enter a first name")]
        public string? FirstName { get; set; }
        
        [Required(ErrorMessage = "Enter a last name")]
        public string? LastName { get; set; }
        
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Enter an email address")]
        [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", ErrorMessage = "Enter a valid email address")]
        public string? ContactEmailAddress { get; set; }

        public string[] FieldOrder => new[] { nameof(FirstName), nameof(LastName), nameof(PhoneNumber), nameof(ContactEmailAddress) };

        public UserAccount GetUserAccount() => new UserAccount { FirstName = FirstName, Surname = LastName, PhoneNumber = PhoneNumber, ContactEmailAddress = ContactEmailAddress };
    }
}
