namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaBaseViewModel : ILayoutModel
    {
        public LegislativeAreaBaseViewModel(string title, string subTitle)
        {
            Title = title;
            SubTitle = subTitle;
        }
        public Guid CABId { get; set; }

        public string? Title { get; set; }
        public string SubTitle { get; }
        public string? ReturnUrl { get; set; }

        public Guid ScopeId { get; set; }

        public bool IsFromSummary { get; set; }

        public LegislativeAreaBaseViewModel(string title, string subTitle, Guid cabId, Guid scopeId, bool isFromSummary)
        {
            Title = title;
            SubTitle = subTitle;
            CABId = cabId;
            ScopeId = scopeId;
            IsFromSummary = isFromSummary;
        }
    }
}
