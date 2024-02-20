namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea.Review
{
    public class ReviewLegislativeAreasViewModel: ILayoutModel
    {
        public Guid CABId { get; set; }
        public string? ReturnUrl { get; set; }
        public List<CABLegislativeAreasItemViewModel> LAItems { get; set; } = new();
        public string? Title => "Legislative areas added";
    }
}
