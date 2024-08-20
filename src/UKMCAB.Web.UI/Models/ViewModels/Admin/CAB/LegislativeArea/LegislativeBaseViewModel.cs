namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaBaseViewModel : ILayoutModel
    {
        public LegislativeAreaBaseViewModel(string title)
        {
            Title = title;
        }
        public Guid CABId { get; set; }

        public string? Title { get; set; }

        public string? ReturnUrl { get; set; }

        public Guid ScopeId { get; set; }

        public bool IsFromSummary { get; set; }

        public LegislativeAreaBaseViewModel(string title, Guid cabId, Guid scopeId, bool isFromSummary)
        {
            Title = title;
            CABId = cabId;
            ScopeId = scopeId;
            IsFromSummary = isFromSummary;
        }
    }
}
