using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Home
{
    public class UpdatesViewModel : ILayoutModel
    {
        public string? Title => Constants.PageTitle.Updates;
        public FeedLinksViewModel? FeedLinksViewModel { get; set; }
    }
}
