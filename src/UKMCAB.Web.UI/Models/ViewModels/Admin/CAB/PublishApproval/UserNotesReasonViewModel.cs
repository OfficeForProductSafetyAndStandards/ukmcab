using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.PublishApproval;

public record UserNotesReasonViewModel(Guid CabId, string CabName, string? ReturnUrl, string? Title) : BasicPageModel(Title)
{
    public Guid CabId { get; set; } = CabId;

    public string CabName { get; set; } = CabName;

    public string? ReturnURL { get; set; } = ReturnUrl;

    [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
    public string? Reason { get; set; }

    [MaxLength(1000, ErrorMessage = "Maximum user notes length is 1000 characters")]
    public string? UserNotes { get; set; }

}
