using UKMCAB.Subscriptions.Core.Data.Models;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public class SubscriptionsDiagnosticsSubscriptionListViewModel : ILayoutModel
{
    public string? Title => "Subscriptions list";

    public string? Skip { get; internal set; }
    public List<SubscriptionEntity>? List { get; internal set; }
}