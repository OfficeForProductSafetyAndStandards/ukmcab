namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea.Review
{    public class ReviewLegislativeAreasViewModel : ILayoutModel
    {
        public Guid CABId { get; set; }
        public string? ReturnUrl { get; set; }
        public List<CABLegislativeAreasItemViewModel> ActiveLAItems { get; set; } = new();
        public List<CABLegislativeAreasItemViewModel> ArchivedLAItems { get; set; } = new();
        public string? Title => "Legislative areas";
        public string? SuccessBannerMessage { get; set; } 
        public string? BannerContent { get; set; } 
        public string? ErrorLink { get; set; }
        public bool FromSummary { get; set; }
        public bool ShowArchiveLegislativeAreaAction { get; set; }
        public bool ShowAddRemoveLegislativeAreaActions { get; set; }
    }
}
