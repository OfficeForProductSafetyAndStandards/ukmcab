using Microsoft.IdentityModel.Tokens;
using UKMCAB.Common.Security;
using UKMCAB.Core.Services;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Services;
using UKMCAB.Web.UI.Areas.Subscriptions.Models;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Controllers;


[Area("subscriptions"), Route("subscriptions")]
public class SubscriptionsController : Controller
{
    private readonly ISubscriptionService _subscriptions;
    private readonly ICachedPublishedCabService _cachedPublishedCabService;

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

        #region Confirmation
        public const string ConfirmSearchSubscription = "subscription.confirm.search";
        public const string ConfirmCabSubscription = "subscription.confirm.cab";
        public const string ConfirmUpdatedEmailAddress = "subscription.confirm.update-email-address";
        #endregion

        public const string ManageSubscription = "subscription.manage";
        public const string ChangeFrequency = "subscription.manage.change-frequency";
        public const string FrequencyChanged = "subscription.manage.frequency-changed";
        public const string Unsubscribe = "subscription.manage.unsubscribe";
        public const string UnsubscribeAll = "subscription.unsubscribe-all";

        #region Update email address flow
        public const string RequestUpdateEmailAddress = "subscription.manage.request.update-email-address"; 
        public const string RequestedUpdateEmailAddress = "subscription.manage.requested.update-email-address";
        #endregion
    }

    public class Views
    {
        public static class RequestFlow
        {
            private const string _base = "RequestFlow/";
            public const string Step1RequestConfirmSubscription = $"{_base}Step1RequestConfirmSubscription";
            public const string Step2RequestSubscriptionFrequency = $"{_base}Step2RequestSubscriptionFrequency";
            public const string Step3RequestSubscriptionEmailAddress = $"{_base}Step3RequestSubscriptionEmailAddress";
            public const string Step4RequestSubscription = $"{_base}Step4RequestSubscription";
            public const string Step5RequestedSubscription = $"{_base}Step5RequestedSubscription";
        }

        public static class Confirmation
        {
            private const string _base = "Confirmation/";
            public const string ConfirmedSearchSubscription = $"{_base}ConfirmedSearchSubscription";
            public const string ConfirmedCabSubscription = $"{_base}ConfirmedCabSubscription";
            public const string ConfirmedUpdatedEmailAddress = $"{_base}ConfirmedUpdatedEmailAddress";
        }


        public static class Manage
        {
            private const string _base = "Manage/";
            public const string ManageSubscription = $"{_base}ManageSubscription";
            public const string RequestUpdateEmailAddress = $"{_base}RequestUpdateEmailAddress";
            public const string RequestedUpdateEmailAddress = $"{_base}RequestedUpdateEmailAddress";
            public const string Unsubscribed = $"{_base}Unsubscribe";
            public const string Unsubscribe = $"{_base}Unsubscribed";
            public const string UnsubscribeAll = $"{_base}UnsubscribeAll";
            public const string UnsubscribedAll = $"{_base}UnsubscribedAll";

            public const string ChangeFrequency = $"{_base}ChangeFrequency";
            public const string FrequencyChanged = $"{_base}FrequencyChanged";
        }
    }

    public SubscriptionsController(ISubscriptionService subscriptions, ICachedPublishedCabService cachedPublishedCabService)
    {
        _subscriptions = subscriptions;
        _cachedPublishedCabService = cachedPublishedCabService;
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
            SubscriptionType = SubscriptionType.Search,
            SearchQueryString = Request.QueryString.Value,
        };
        return RedirectToRoute(Routes.Step1RequestConfirmSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
    }

    [HttpGet("subscribe/request/cab/{id}", Name = Routes.Step0RequestCabSubscription)]
    public IActionResult Step0RequestCabSubscription(Guid id)
    {
        var req = new SubscriptionRequestFlowModel
        {
            SubscriptionType = SubscriptionType.Cab,
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
        else // todo: VALIDATION
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
        else // todo: VALIDATION
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
            if (req.SubscriptionType == SubscriptionType.Search)
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

    #region Confirmation

    [HttpGet("subscribe/search/confirm", Name = Routes.ConfirmSearchSubscription)]
    public async Task<IActionResult> ConfirmSearchSubscriptionAsync(string token)
    {
        var result = await _subscriptions.ConfirmSearchSubscriptionAsync(token).ConfigureAwait(false);
        return View(Views.Confirmation.ConfirmedSearchSubscription);
    }

    [HttpGet("subscribe/cab/confirm", Name = Routes.ConfirmCabSubscription)]
    public async Task<IActionResult> ConfirmCabSubscriptionAsync(string token)
    {
        var result = await _subscriptions.ConfirmCabSubscriptionAsync(token).ConfigureAwait(false);
        ViewBag.CabName = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(result.CabSubscriptionRequest.CabId.ToString());
        return View(Views.Confirmation.ConfirmedCabSubscription);
    }

    [HttpGet("update/email-address/confirm", Name = Routes.ConfirmUpdatedEmailAddress)]
    public async Task<IActionResult> ConfirmUpdateEmailAddressAsync(string token)
    {
        var result = await _subscriptions.ConfirmUpdateEmailAddressAsync(token).ConfigureAwait(false);
        return View(Views.Confirmation.ConfirmedUpdatedEmailAddress);
    }

    #endregion

    [HttpGet("manage/{id}", Name = Routes.ManageSubscription)]
    public async Task<IActionResult> ManageSubscriptionAsync(string id)
    {
        return View(Views.Manage.ManageSubscription, new SubscriptionViewModel(await _subscriptions.GetSubscriptionAsync(id).ConfigureAwait(false)));
    }

    #region Unsubscribe

    [Route("unsubscribe/{id}", Name = Routes.Unsubscribe)]
    public async Task<IActionResult> UnsubscribeAsync(string id)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View(Views.Manage.Unsubscribe, new SubscriptionViewModel(await _subscriptions.GetSubscriptionAsync(id).ConfigureAwait(false)));
        }
        else
        {
            var done = await _subscriptions.UnsubscribeAsync(id).ConfigureAwait(false);
            return View(Views.Manage.Unsubscribed);
        }
    }

    [Route("unsubscribe-all", Name = Routes.UnsubscribeAll)]
    public async Task<IActionResult> UnsubscribeAllAsync([FromForm] string? emailAddress = null)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View(Views.Manage.UnsubscribeAll);
        }
        else
        {
            var done = await _subscriptions.UnsubscribeAllAsync(emailAddress).ConfigureAwait(false);
            return View(Views.Manage.UnsubscribedAll);
        }
    }

    #endregion

    #region Update email address

    [Route("manage/{id}/update-email-address/request", Name = Routes.RequestUpdateEmailAddress)]
    public async Task<IActionResult> RequestUpdateEmailAddressAsync(string id, [FromForm] string emailAddress)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View();
        }
        else // todo: VALIDATION
        {
            await _subscriptions.RequestUpdateEmailAddressAsync(new SubscriptionService.UpdateEmailAddressOptions(id, emailAddress));
            return RedirectToRoute(Routes.RequestedUpdateEmailAddress, new { id, email = Base64UrlEncoder.Encode(emailAddress)});
        }
    }

    [HttpGet("manage/{id}/update-email-address/requested", Name = Routes.RequestedUpdateEmailAddress)]
    public IActionResult RequestedUpdateEmailAddress(string id, string email)
    {
        ViewBag.Email = Base64UrlEncoder.Decode(email);
        return View(Views.Manage.RequestedUpdateEmailAddress);
    }

    #endregion

    #region Change frequency

    [Route("manage/{id}/change-frequency", Name = Routes.ChangeFrequency)]
    public async Task<IActionResult> ChangeFrequencyAsync(string id, [FromForm] Frequency frequency)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View(Views.Manage.ChangeFrequency);
        }
        else // todo: VALIDATION
        {
            await _subscriptions.UpdateFrequencyAsync(id, frequency);
            return RedirectToRoute(Routes.FrequencyChanged, new { id });
        }
    }

    [HttpGet("manage/{id}/frequency-changed", Name = Routes.FrequencyChanged)]
    public IActionResult FrequencyChanged(string id)
    {
        return View(Views.Manage.FrequencyChanged);
    }

    #endregion

}