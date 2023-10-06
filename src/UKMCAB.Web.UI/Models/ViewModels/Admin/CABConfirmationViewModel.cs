namespace UKMCAB.Web.UI.Models.ViewModels.Admin;

public class CABConfirmationViewModel : ILayoutModel
{
    public string Name { get; set; }
    public string URLSlug { get; set; }
    public string CABNumber { get; set; }
    public string? Title => "CAB confirmation";
}

public class CabSubmittedForApprovalConfirmationViewModel : ILayoutModel
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Title => $"CAB {Name} has been submitted for approval";
}
