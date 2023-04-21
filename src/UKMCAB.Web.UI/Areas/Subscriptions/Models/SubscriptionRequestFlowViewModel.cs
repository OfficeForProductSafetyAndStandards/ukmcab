namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public class SubscriptionRequestFlowViewModel : ILayoutModel
{
    public string? Title => "Subscriptions";

    public SubscriptionRequestFlowModel Flow { get; set; }

    public SubscriptionRequestFlowViewModel(SubscriptionRequestFlowModel flow)
    {
        Flow = flow;
    }
}