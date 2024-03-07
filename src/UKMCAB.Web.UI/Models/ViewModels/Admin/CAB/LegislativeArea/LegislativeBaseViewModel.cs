namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaBaseViewModel : ILayoutModel
    {
        public Guid CABId { get; set; }

        public string? Title { get; set; }

        public string? ReturnUrl { get; set; }

        public Guid ScopeId { get; set; }

        public bool IsFromSummary { get; set; }
    }
}
