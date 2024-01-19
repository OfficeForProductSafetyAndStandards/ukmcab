namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Unpublish;

public record ApproveUnpublishCABViewModel(
    string? Title,
    string CABName,
    string CabUrl,
    Guid CabId,
    string SubmitterGroup,
    string SubmitterFirstAndLastName,
    bool UnpublishRequested) : BasicPageModel(Title)
{
    public string? Reason { get; set; }
}