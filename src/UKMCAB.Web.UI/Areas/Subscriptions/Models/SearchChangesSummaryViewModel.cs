using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public class SearchChangesSummaryViewModel : ILayoutModel
{
    public string? Title { get; set; } = "Search changes";
    public string SubscriptionId { get; internal set; }
    public SearchResultsChangesModel Changes { get; internal set; }
    public string SearchUrl { get; internal set; }

    public SearchChangesSummaryViewModel(string subscriptionId, SearchResultsChangesModel changes, string searchUrl)
    {
        SubscriptionId = subscriptionId;
        Changes = changes;
        SearchUrl = searchUrl;
    }
}