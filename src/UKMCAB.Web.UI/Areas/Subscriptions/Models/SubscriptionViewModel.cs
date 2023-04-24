using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public class SubscriptionViewModel
{
    public SubscriptionModel Subscription { get; set; }

    public SubscriptionViewModel(SubscriptionModel subscription)
    {
        Subscription = subscription;
    }
}