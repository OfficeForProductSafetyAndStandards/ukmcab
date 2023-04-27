using Microsoft.IdentityModel.Tokens;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Services;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Areas.Subscriptions.Models;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Controllers;


[Area("subscriptions"), Route("subscriptions")]
public class SubscriptionsController : Controller
{
    private readonly ISubscriptionService _subscriptions;
    private readonly ICachedPublishedCABService _cachedPublishedCabService;

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
        public const string SearchChangesSummary = "subscription.search.changes-summary";
        public const string ChangeFrequency = "subscription.manage.change-frequency";
        public const string FrequencyChanged = "subscription.manage.frequency-changed";
        public const string Unsubscribe = "subscription.manage.unsubscribe";
        public const string UnsubscribeAll = "subscriptions.unsubscribe-all";
        public const string UnsubscribedAll = "subscriptions.unsubscribed-all";

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

        public static class Manage
        {
            private const string _base = "Manage/";
            public const string ManageSubscription = $"{_base}ManageSubscription";
            public const string SearchChangesSummary = $"{_base}SearchChangesSummary";
            public const string RequestUpdateEmailAddress = $"{_base}RequestUpdateEmailAddress";
            public const string RequestedUpdateEmailAddress = $"{_base}RequestedUpdateEmailAddress";
            public const string Unsubscribed = $"{_base}Unsubscribed";
            public const string Unsubscribe = $"{_base}Unsubscribe";
            public const string UnsubscribeAll = $"{_base}UnsubscribeAll";
            public const string UnsubscribedAll = $"{_base}UnsubscribedAll";

            public const string ChangeFrequency = $"{_base}ChangeFrequency";
        }
    }

    public SubscriptionsController(ISubscriptionService subscriptions, ICachedPublishedCABService cachedPublishedCabService)
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
        var queryString = Request.QueryString.Value;
        var req = new SubscriptionRequestFlowModel
        {
            SubscriptionType = SubscriptionType.Search,
            SearchQueryString = Request.QueryString.Value,
        };
        return RedirectToRoute(Routes.Step1RequestConfirmSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
    }

    [HttpGet("subscribe/request/cab/{id}", Name = Routes.Step0RequestCabSubscription)]
    public async Task<IActionResult> Step0RequestCabSubscriptionAsync(Guid id)
    {
        var req = new SubscriptionRequestFlowModel
        {
            SubscriptionType = SubscriptionType.Cab,
            CabId = id,
            CabName = await GetCabNameAsync(id),
        };
        return RedirectToRoute(Routes.Step1RequestConfirmSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
    }

    [Route("subscribe/request/confirm", Name = Routes.Step1RequestConfirmSubscription)]
    public IActionResult Step1RequestConfirmSubscription(string tok)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View(Views.RequestFlow.Step1RequestConfirmSubscription, new SubscriptionRequestFlowViewModel(JsonBase64UrlToken.Deserialize<SubscriptionRequestFlowModel>(tok)));
        }
        else
        {
            return RedirectToRoute(Routes.Step2RequestSubscriptionFrequency, new { tok });
        }
    }

    [Route("subscribe/request/frequency", Name = Routes.Step2RequestSubscriptionFrequency)]
    public IActionResult Step2RequestSubscriptionFrequency(string tok, [FromForm] string? frequency = null)
    {
        if (Request.Method == HttpMethod.Post.Method)
        {
            if (Enum.TryParse<Frequency>(frequency!, true, out var f))
            {
                tok = JsonBase64UrlToken.Pipe<SubscriptionRequestFlowModel>(tok, x => x.Frequency = f);
                return RedirectToRoute(Routes.Step3RequestSubscriptionEmailAddress, new { tok });
            }
            else
            {
                ModelState.AddModelError("", "Choose how often you want to get emails");
            }
        }
        return View(Views.RequestFlow.Step2RequestSubscriptionFrequency);
    }

    [Route("subscribe/request/email-address", Name = Routes.Step3RequestSubscriptionEmailAddress)]
    public IActionResult Step3RequestSubscriptionEmailAddress(string tok, [FromForm] string? emailAddress)
    {
        if (Request.Method == HttpMethod.Post.Method)
        {
            if (emailAddress.IsValidEmail())
            {
                tok = JsonBase64UrlToken.Pipe<SubscriptionRequestFlowModel>(tok, x => x.EmailAddress = emailAddress);
                return RedirectToRoute(Routes.Step4RequestSubscription, new { tok });
            }
            else
            {
                ModelState.AddModelError("", $"Enter a{(emailAddress?.Clean() == null ? "n" : " valid")} email address");
            }
        }
        return View(Views.RequestFlow.Step3RequestSubscriptionEmailAddress);
    }


    [Route("subscribe/request", Name = Routes.Step4RequestSubscription)]
    public async Task<IActionResult> Step4RequestSubscriptionAsync(string tok)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View(Views.RequestFlow.Step4RequestSubscription);
        }
        else
        {
            var req = JsonBase64UrlToken.Deserialize<SubscriptionRequestFlowModel>(tok);
            if (req.SubscriptionType == SubscriptionType.Search)
            {
                var result = await _subscriptions.RequestSubscriptionAsync(new SearchSubscriptionRequest(req.EmailAddress, req.SearchQueryString, req.Frequency.Value));
                if (result.ValidationResult == SubscriptionService.ValidationResult.AlreadySubscribed)
                {
                    throw new DomainException("You are already subscribed to this search");
                }
                else if (result.ValidationResult == SubscriptionService.ValidationResult.EmailBlocked)
                {
                    throw new DomainException("Your email address is on a no-send list");
                }
            }
            else
            {
                var result = await _subscriptions.RequestSubscriptionAsync(new CabSubscriptionRequest(req.EmailAddress, req.CabId ?? throw new Exception("CAB id should not be null"), req.Frequency.Value));
                if (result.ValidationResult == SubscriptionService.ValidationResult.AlreadySubscribed)
                {
                    throw new DomainException("You are already subscribed to this CAB");
                }
                else if (result.ValidationResult == SubscriptionService.ValidationResult.EmailBlocked)
                {
                    throw new DomainException("Your email address is on a no-send list");
                }
            }
            return RedirectToRoute(Routes.Step5RequestedSubscription, new { tok = JsonBase64UrlToken.Serialize(req) });
        }

    }

    [HttpGet("subscribe/requested", Name = Routes.Step5RequestedSubscription)]
    public IActionResult Step5RequestedSubscription(string tok)
    {
        var viewModel = JsonBase64UrlToken.Deserialize<SubscriptionRequestFlowModel>(tok);
        return View(Views.RequestFlow.Step5RequestedSubscription, new SubscriptionRequestFlowViewModel(viewModel));
    }

    #endregion

    #region Confirmation

    [HttpGet("subscribe/search/confirm", Name = Routes.ConfirmSearchSubscription)]
    public async Task<IActionResult> ConfirmSearchSubscriptionAsync(string token)
    {
        var result = await _subscriptions.ConfirmSearchSubscriptionAsync(token).ConfigureAwait(false);
        return RedirectToRoute(Routes.ManageSubscription, new { id = result.SubscriptionId, smsg = Base64UrlEncoder.Encode("You've subscribed to emails about UKMCAB search results") });
    }

    [HttpGet("subscribe/cab/confirm", Name = Routes.ConfirmCabSubscription)]
    public async Task<IActionResult> ConfirmCabSubscriptionAsync(string token)
    {
        var result = await _subscriptions.ConfirmCabSubscriptionAsync(token).ConfigureAwait(false);

        if (result.ValidationResult == SubscriptionService.ValidationResult.Success)
        {
            var cabName = await GetCabNameAsync(result.CabSubscriptionRequest.CabId);
            return RedirectToRoute(Routes.ManageSubscription, new { id = result.SubscriptionId, smsg = Base64UrlEncoder.Encode($"You've subscribed to emails about UKMCAB profile for '{cabName}'") });
        }
        else if (result.ValidationResult == SubscriptionService.ValidationResult.EmailBlocked)
        {
            throw new DomainException("Unable to fulfil this request");
        }
        else if (result.ValidationResult == SubscriptionService.ValidationResult.AlreadySubscribed)
        {
            throw new DomainException("You are already subscribed");
        }
        else
        {
            throw new NotSupportedException("The validation result " + result.ValidationResult.ToString() + " is not supported");
        }


    }

    [HttpGet("update/email-address/confirm", Name = Routes.ConfirmUpdatedEmailAddress)]
    public async Task<IActionResult> ConfirmUpdateEmailAddressAsync(string token)
    {
        var subscriptionId = await _subscriptions.ConfirmUpdateEmailAddressAsync(token).ConfigureAwait(false);
        return RedirectToRoute(Routes.ManageSubscription, new { id = subscriptionId, smsg = Base64UrlEncoder.Encode("You have successfully updated your email address.") });
    }

    #endregion


    [HttpGet("{subscriptionId}/search/changes/{changesDescriptorId}", Name = Routes.SearchChangesSummary)]
    public async Task<IActionResult> SearchChangesSummaryAsync(string subscriptionId, string changesDescriptorId)
    {
        var subscription = (await _subscriptions.GetSubscriptionAsync(subscriptionId).ConfigureAwait(false)) ?? throw new DomainException("Subscription not found");
        var changes = await _subscriptions.GetSearchResultsChangesAsync(changesDescriptorId).ConfigureAwait(false) ?? throw new DomainException("Search result changes information could not be found");
        var searchActionUrl = Url.Action(nameof(SearchController.Index), nameof(SearchController).ControllerName(), new { area = "search" }) ?? throw new Exception("Search action url could not be resolved");
        var searchUrl = string.IsNullOrEmpty(subscription?.SearchQueryString) ? searchActionUrl : string.Concat(searchActionUrl, (subscription?.SearchQueryString?.EnsureStartsWith("?")));
        var viewModel = new SearchChangesSummaryViewModel(subscriptionId, changes, searchUrl);
        return View(Views.Manage.SearchChangesSummary, viewModel);
    }

    [HttpGet("manage/{id}", Name = Routes.ManageSubscription)]
    public async Task<IActionResult> ManageSubscriptionAsync(string id, string? smsg)
    {
        var viewModel = await GetSubscriptionViewModelAsync(id, "Manage subscription");
        viewModel.SuccessBannerMessage = smsg != null ? Base64UrlEncoder.Decode(smsg) : null;

        if (viewModel.Subscription.SubscriptionType == SubscriptionType.Cab)
        {
            viewModel.CabName = await GetCabNameAsync(viewModel.Subscription.CabId!.Value);
        }

        return View(Views.Manage.ManageSubscription, viewModel);
    }

    #region Unsubscribe

    [Route("unsubscribe/{id}", Name = Routes.Unsubscribe)]
    public async Task<IActionResult> UnsubscribeAsync(string id)
    {
        var viewModel = await GetSubscriptionViewModelAsync(id, "Unsubscribe");
        if (Request.Method == HttpMethod.Get.Method)
        {
            return View(Views.Manage.Unsubscribe, viewModel);
        }
        else
        {
            var done = await _subscriptions.UnsubscribeAsync(id).ConfigureAwait(false);
            return View(Views.Manage.Unsubscribed, viewModel);
        }
    }

    [Route("unsubscribe-all", Name = Routes.UnsubscribeAll)]
    public async Task<IActionResult> UnsubscribeAllAsync(UnsubscribeAllViewModel viewModel)
    {
        if (Request.Method == HttpMethod.Post.Method)
        {
            if (viewModel.EmailAddress.IsValidEmail())
            {
                _ = await _subscriptions.UnsubscribeAllAsync(viewModel.EmailAddress).ConfigureAwait(false);
                return RedirectToRoute(Routes.UnsubscribedAll);
            }
            else
            {
                ModelState.AddModelError("", $"Enter a{(viewModel.EmailAddress?.Clean() == null ? "n" : " valid")} email address");
            }
        }
        return View(Views.Manage.UnsubscribeAll, viewModel);
    }

    [Route("unsubscribed-all", Name = Routes.UnsubscribedAll)]
    public async Task<IActionResult> UnsubscribedAll() => View(Views.Manage.UnsubscribedAll);

    #endregion

    #region Update email address

    [Route("manage/{id}/update-email-address/request", Name = Routes.RequestUpdateEmailAddress)]
    public async Task<IActionResult> RequestUpdateEmailAddressAsync(string id, [FromForm] string? emailAddress)
    {
        if (Request.Method == HttpMethod.Post.Method)
        {
            if (emailAddress.IsValidEmail())
            {
                await _subscriptions.RequestUpdateEmailAddressAsync(new SubscriptionService.UpdateEmailAddressOptions(id, emailAddress));
                return RedirectToRoute(Routes.RequestedUpdateEmailAddress, new { id, email = Base64UrlEncoder.Encode(emailAddress) });
            }
            else
            {
                ModelState.AddModelError("", $"Enter a{(emailAddress?.Clean() == null ? "n" : " valid")} email address");
            }
        }
        var viewModel = await GetSubscriptionViewModelAsync(id, "").ConfigureAwait(false);
        return View(Views.Manage.RequestUpdateEmailAddress, viewModel);
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
    public async Task<IActionResult> ChangeFrequencyAsync(string id, [FromForm] string? frequency)
    {
        var viewModel = await GetSubscriptionViewModelAsync(id, "Change frequency");
        if (Request.Method == HttpMethod.Post.Method)
        {
            if (Enum.TryParse<Frequency>(frequency!, true, out var f))
            {
                await _subscriptions.UpdateFrequencyAsync(id, f);
                return RedirectToRoute(Routes.FrequencyChanged, new { id });
            }
            else
            {
                ModelState.AddModelError("", "Choose how often you want to get emails");
            }
        }
        return View(Views.Manage.ChangeFrequency, viewModel);
    }

    [HttpGet("manage/{id}/frequency-changed", Name = Routes.FrequencyChanged)]
    public IActionResult FrequencyChanged(string id)
        => RedirectToRoute(Routes.ManageSubscription, new { id, smsg = Base64UrlEncoder.Encode("You have successfully updated the email frequency.") });

    #endregion


    private async Task<string> GetCabNameAsync(Guid id)
    {
        var cab = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id.ToString());
        if (cab != null)
        {
            return cab.Name;
        }
        else
        {
            return string.Empty;
        }
    }

    private async Task<SubscriptionViewModel> GetSubscriptionViewModelAsync(string id, string title)
    {
        var model = await _subscriptions.GetSubscriptionAsync(id).ConfigureAwait(false) ?? throw new DomainException("Subscription not found");
        var viewModel = new SubscriptionViewModel(model, title);
        return viewModel;
    }

}

