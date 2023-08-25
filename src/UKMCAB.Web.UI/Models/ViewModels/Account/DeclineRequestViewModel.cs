
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Account;

public class DeclineRequestViewModel : ILayoutModel
{
    public string? AccountId { get; set; }

    [Required(ErrorMessage = "Enter the reason for declining this account request")]
    public string? Reason { get; set; }
    public string? Title => "Reject account request";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
