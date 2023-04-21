using Humanizer;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
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
        #region Subscription request flow
        public const string Step0RequestSearchSubscription = "subscription.request.search";
        public const string Step0RequestCabSubscription = "subscription.request.cab";
        public const string Step1RequestConfirmSubscription = "subscription.request.confirm";
        public const string Step2RequestSubscriptionFrequency = "subscription.request.frequency";
        public const string Step3RequestSubscriptionEmailAddress = "subscription.request.emailaddress";
        public const string Step4RequestSubscription = "subscription.request";
        public const string Step5RequestedSubscription = "subscription.requested";
        #endregion

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

    #region Subscription request flow

    /*
     * SUBSCRIPTION REQUEST FLOW
     * A subscription request flow starts at the point where they click "Get emails" (on search results OR CAB profile) 
     * and _ENDS_ where the confirmation email has been sent (the 'check your inbox' page).
     * 
     */

    [HttpGet("subscribe/request/search", Name = Routes.Step0RequestSearchSubscription)]
    public IActionResult Step0RequestSearchSubscription()
    {
        var req = new SubscriptionRequestFlowModel
        {
            SubscriptionType = Models.SubscriptionType.Search,
            SearchQueryString = Request.QueryString.Value,
        };
        return RedirectToRoute(Routes.Step1RequestConfirmSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
    }

    [HttpGet("subscribe/request/cab/{id}", Name = Routes.Step0RequestCabSubscription)]
    public IActionResult Step0RequestCabSubscription(Guid id)
    {
        var req = new SubscriptionRequestFlowModel
        {
            SubscriptionType = Models.SubscriptionType.Cab,
            CabId = id,
        };
        return RedirectToRoute(Routes.Step1RequestConfirmSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
    }

    [Route("subscribe/request/confirm", Name = Routes.Step1RequestConfirmSubscription)]
    public IActionResult Step1RequestConfirmSubscription(string tok)
    {
        if(Request.Method == HttpMethod.Get.Method)
        {
            return View(new SubscriptionRequestFlowViewModel(JsonBase64UrlToken.Deserialize<SubscriptionRequestFlowModel>(tok)));
        }
        else
        {
            return RedirectToRoute(Routes.Step2RequestSubscriptionFrequency, new { tok });
        }
    }

    [Route("subscribe/request/frequency", Name = Routes.Step2RequestSubscriptionFrequency)]
    public IActionResult Step2RequestSubscriptionFrequency(string tok, [FromForm] string? frequency = null)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View();
        }
        else
        {
            tok = JsonBase64UrlToken.Pipe<SubscriptionRequestFlowModel>(tok, x => x.Frequency = Enum.Parse<Frequency>(frequency!, true));
            return RedirectToRoute(Routes.Step3RequestSubscriptionEmailAddress, new { tok });
        }
    }

    [Route("subscribe/request/email-address", Name = Routes.Step3RequestSubscriptionEmailAddress)]
    public IActionResult Step3RequestSubscriptionEmailAddress(string tok, [FromForm] string emailAddress)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View();
        }
        else
        {
            tok = JsonBase64UrlToken.Pipe<SubscriptionRequestFlowModel>(tok, x => x.EmailAddress = emailAddress);
            return RedirectToRoute(Routes.Step4RequestSubscription, new { tok });
        }
    }


    [Route("subscribe/request", Name = Routes.Step4RequestSubscription)]
    public async Task<IActionResult> Step4RequestSubscriptionAsync(string tok)
    {
        _ = new QueryCollection();

        if (Request.Method == HttpMethod.Get.Method)
        {
            return View();
        }
        else
        {
            var req = JsonBase64UrlToken.Deserialize<SubscriptionRequestFlowModel>(tok);
            if (req.SubscriptionType == Models.SubscriptionType.Search)
            {
                await _subscriptions.RequestSubscriptionAsync(new SearchSubscriptionRequest(req.EmailAddress, req.SearchQueryString, req.Frequency.Value));
            }
            else
            {
                await _subscriptions.RequestSubscriptionAsync(new CabSubscriptionRequest(req.EmailAddress, req.CabId ?? throw new Exception("CAB id should not be null"), req.Frequency.Value));
            }
            return RedirectToRoute(Routes.Step5RequestedSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
        }

    }

    [HttpGet("subscribe/requested", Name = Routes.Step5RequestedSubscription)]
    public IActionResult Step5RequestedSubscription(string tok)
    {
        var viewModel = JsonBase64UrlToken.Deserialize<SubscriptionRequestFlowModel>(tok);
        return View(new SubscriptionRequestFlowViewModel(viewModel));
    }

    #endregion




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