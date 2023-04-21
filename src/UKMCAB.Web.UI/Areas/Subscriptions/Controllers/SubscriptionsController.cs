using UKMCAB.Subscriptions.Core.Services;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Controllers;


[Area("subscriptions"), Route("subscriptions")]
public class SubscriptionsController : Controller
{
    private readonly ISubscriptionService _subscriptions;

    public static class Routes
    {
        public const string ConfirmSearchSubscription = "subscription.confirm.search";
        public const string ConfirmCabSubscription = "subscription.confirm.cab";
        public const string ConfirmUpdatedEmailAddress = "subscription.confirm.update-email-address";
        public const string ManageSubscription = "subscription.manage";
        public const string Unsubscribe = "subscription.unsubscribe";
        public const string UnsubscribeAll = "subscription.unsubscribe-all";
    }

    public SubscriptionsController(ISubscriptionService subscriptions)
    {
        _subscriptions = subscriptions;
    }

    [HttpGet("subscribe/search/confirm", Name = Routes.ConfirmSearchSubscription)]
    public async Task<IActionResult> ConfirmSearchSubscriptionAsync(string token)
    {
        var result = await _subscriptions.ConfirmSearchSubscriptionAsync(token).ConfigureAwait(false);
        return Content("ok; search sub confirmed");
    }

    [HttpGet("subscribe/cab/confirm", Name = Routes.ConfirmCabSubscription)]
    public async Task<IActionResult> ConfirmCabSubscriptionAsync(string token)
    {
        var result = await _subscriptions.ConfirmCabSubscriptionAsync(token).ConfigureAwait(false);
        return Content("ok; cab sub confirmed");
    }

    [HttpGet("update/email-address", Name = Routes.ConfirmUpdatedEmailAddress)]
    public async Task<IActionResult> ConfirmUpdateEmailAddressAsync(string token)
    {
        var result = await _subscriptions.ConfirmUpdateEmailAddressAsync(token).ConfigureAwait(false);
        return Content("ok; email address updated: " + result);
    }

    [HttpGet("manage/{id}", Name = Routes.ManageSubscription)]
    public async Task<IActionResult> ManageSubscriptionAsync(string id)
    {
        var sub = await _subscriptions.GetSubscriptionAsync(id).ConfigureAwait(false);
        return Content("todo - manage sub");
    }

    [HttpGet("unsubscribe/{id}", Name = Routes.Unsubscribe)]
    public async Task<IActionResult> UnsubscribeAsync(string id)
    {
        var done = await _subscriptions.UnsubscribeAsync(id).ConfigureAwait(false);
        return Content("unsubscribed: "+done);
    }

    [HttpGet("unsubscribe/all", Name = Routes.UnsubscribeAll)]
    public async Task<IActionResult> UnsubscribeAllAsync(string emailAddress)
    {
        var count = await _subscriptions.UnsubscribeAllAsync(emailAddress).ConfigureAwait(false);
        return Content("unsubscribed all; count=" + count);
    }

}