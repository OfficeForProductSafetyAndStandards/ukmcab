using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public class SubscriptionViewModel : ILayoutModel
{
    public SubscriptionModel Subscription { get; set; }

    public string? Title { get; set; }

    public string? SuccessBannerMessage { get; set; }

    public SubscriptionViewModel(SubscriptionModel subscription, string? title)
    {
        Subscription = subscription;
        Title = title;
    }

    public string FrequencyDescription => Subscription.Frequency switch
    {
        Frequency.Realtime => Subscription.SubscriptionType == SubscriptionType.Search ? "as soon as the search is updated" : "as soon as the CAB is updated",
        Frequency.Daily => "daily",
        Frequency.Weekly => "weekly",
        _ => throw new NotImplementedException(),
    };

    public string CabName { get; internal set; }
    public string SearchUrl { get; internal set; }
    public string? CabProfileUrl { get; internal set; }
}
