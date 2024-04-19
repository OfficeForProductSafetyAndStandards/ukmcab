namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

public record ApprovalListViewModel : ILayoutModel
{
    public string? Title { get; set; } = "Review legislative areas";

    public Guid? CabId { get; set; }

    public List<(Guid, string)> LasToApprove { get; set; } = new();
    public string? SuccessBannerMessage { get; set; }
}
