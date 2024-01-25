using UKMCAB.Web.UI.Models.ViewModels.Shared;
namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public class CABConfirmationViewModel : ILayoutModel
{
    public string? Name { get; set; }
    public string URLSlug { get; set; } = string.Empty;
    public string CABNumber { get; set; } = string.Empty;
    public string? Title => "CAB confirmation";
}

public class CabSubmittedForApprovalConfirmationViewModel : ConfirmationViewModel
{
    public string? Name { get; init; }
    public string? Id { get; init; }

    public ConfirmationViewModel ConfirmationViewModel => new()
    {
        Title = $"CAB {Name} has been submitted for approval",
        Body = Body
    };
}
