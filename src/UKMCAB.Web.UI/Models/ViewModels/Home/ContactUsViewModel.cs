
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Home
{
    public class ContactUsViewModel : ILayoutModel
    {
        public string? Title => "Contact us";

        [Required(ErrorMessage = "Enter a name")]
        [MaxLength(50, ErrorMessage = "Maximum name length is 50 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Enter an email address")]
        [MaxLength(50, ErrorMessage = "Maximum email address length is 50 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Enter a subject")]
        [MaxLength(100, ErrorMessage = "Maximum subject length is 100 characters")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Enter a message")]
        [MaxLength(1000, ErrorMessage = "Maximum message length is 1000 characters")]
        public string? Message { get; set; }

        public string[] FieldOrder => new[] { nameof(Name), nameof(Email), nameof(Subject), nameof(Message) };

    }
}
