namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class FeedLinksViewModel
    {
        public string Title { get; set; }
        public string FeedUrl { get; set; }
        public string EmailUrl { get; set; }
        public string ShareUrl { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string Endpoint { get; set; }
        public string? CABName { get; set; } = string.Empty;
        public string SearchKeyword { get; set; } = string.Empty;
    }
}
