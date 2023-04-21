using Microsoft.IdentityModel.Tokens;
using UKMCAB.Common.Security;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Services;
using UKMCAB.Web.UI.Areas.Subscriptions.Models;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Controllers;


[Area("subscriptions"), Route("subscriptions")]
public class SubscriptionsController : Controller
{
    private readonly ISubscriptionService _subscriptions;

    public static class Routes
    {

        public const string RequestSearchSubscription = "subscription.request.search";
        public const string RequestConfirmSubscription = "subscription.request.confirm";
        public const string RequestSubscriptionFrequency = "subscription.request.frequency";
        public const string RequestSubscriptionEmailAddress = "subscription.request.emailaddress";
        public const string RequestSubscription = "subscription.request";
        public const string RequestedSubscription = "subscription.requested";
        
        
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

    [HttpGet("subscribe/request/search", Name = Routes.RequestSearchSubscription)]
    public IActionResult RequestSearchSubscription()
    {
        var req = new SubscriptionRequestViewModel
        {
            SubscriptionType = Models.SubscriptionType.Search,
            SearchQueryString = Request.QueryString.Value,
        };

        return RedirectToRoute(Routes.RequestConfirmSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
    }

    [Route("subscribe/request/confirm", Name = Routes.RequestConfirmSubscription)]
    public IActionResult RequestConfirmSubscription(string tok)
    {
        if(Request.Method == HttpMethod.Get.Method)
        {
            return View();
        }
        else
        {
            return RedirectToRoute(Routes.RequestSubscriptionFrequency, new { tok });
        }
    }

    [Route("subscribe/request/frequency", Name = Routes.RequestSubscriptionFrequency)]
    public IActionResult RequestSubscriptionFrequency(string tok, [FromForm] string? frequency = null)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View();
        }
        else
        {
            tok = JsonBase64UrlToken.Pipe<SubscriptionRequestViewModel>(tok, x => x.Frequency = Enum.Parse<Frequency>(frequency!));
            return RedirectToRoute(Routes.RequestSubscriptionEmailAddress, new { tok });
        }
    }

    [HttpGet("subscribe/request/email-address", Name = Routes.RequestSubscriptionEmailAddress)]
    public IActionResult RequestSubscriptionEmailAddress(string tok) => View(tok);


    [HttpPost("subscribe/request", Name = Routes.RequestSubscription)]
    public async Task<IActionResult> RequestSubscriptionAsync(string tok, [FromForm] string? emailAddress)
    {
        var req = JsonBase64UrlToken.Deserialize<SubscriptionRequestViewModel>(tok)
            .Pipe(x => x.EmailAddress = emailAddress)
            ?? throw new Exception("Token deserialised to null");

        if (req.SubscriptionType == Models.SubscriptionType.Search)
        {
            await _subscriptions.RequestSubscriptionAsync(new SearchSubscriptionRequest(req.EmailAddress, req.SearchQueryString, req.Frequency.Value));
        }
        else
        {
            await _subscriptions.RequestSubscriptionAsync(new CabSubscriptionRequest(emailAddress, req.CabId ?? throw new Exception("CAB id should not be null"), req.Frequency.Value));
        }
        return RedirectToRoute(Routes.RequestedSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
    }

    [HttpGet("subscribe/requested", Name = Routes.RequestedSubscription)]
    public IActionResult RequestedSubscription(string tok)
    {
        var viewModel = JsonBase64UrlToken.Deserialize<SubscriptionRequestViewModel>(tok);
        return View(viewModel);
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