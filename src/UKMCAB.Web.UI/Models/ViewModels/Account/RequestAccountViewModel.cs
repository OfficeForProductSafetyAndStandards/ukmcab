using System.ComponentModel.DataAnnotations;
using static UKMCAB.Web.UI.Constants;

namespace UKMCAB.Web.UI.Models.ViewModels.Account;

public class RequestAccountViewModel : ILayoutModel
{
    public string Token { get; set; }

    [Required(ErrorMessage = "Enter a first name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Enter a last name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Enter your organisation name")]
    public string Organisation { get; set; }

    [Required(ErrorMessage = "Enter email address")]
    [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", ErrorMessage = "Enter a valid email address")]
    public string ContactEmailAddress { get; set; }

    [Required(ErrorMessage = "Enter a reason for your request")]
    public string Comments { get; set; }

    public string? Title => "Request user account";

    public string[] FieldOrder => new[] { nameof(FirstName), nameof(LastName), nameof(Organisation), nameof(ContactEmailAddress), nameof(Comments) };

}
