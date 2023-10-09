namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public record UnsubscribeAllViewModel : BasicPageModel
{
    public string? EmailAddress { get; set; }
}