using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.PublishApproval;

public record ApproveCABViewModel(Guid CabId, string CabName,string? ReturnUrl, string? Title) : UserNotesReasonViewModel(CabId, CabName, ReturnUrl, Title)
{
    [Required(ErrorMessage = "Enter a CAB number", AllowEmptyStrings = false)]
    [RegularExpression(@"^[\w\d\s(),-]*$", ErrorMessage = "Enter a CAB number using only numbers and letters")]
    public string CABNumber { get; set; } = null!;

    [Required(ErrorMessage = "Select who should see the CAB number", AllowEmptyStrings = false)]
    public string CabNumberVisibility { get; set; } = null!;
}
