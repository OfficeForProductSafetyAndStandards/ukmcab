using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Services;
using UKMCAB.Web.UI.Areas.Subscriptions.Models;
using UKMCAB.Web.UI.Services.Subscriptions;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Controllers;


[Area("subscriptions"), Route("subscriptions/diagnostics")]
public class SubscriptionsDiagnosticsController : Controller
{
    private readonly ISubscriptionService _subscriptions;
    private readonly ISubscriptionEngine _engine;
    private readonly ISubscriptionsDateTimeProvider _subscriptionsDateTimeProvider;
    private readonly IOutboundEmailSender _outboundEmailSender;
    private readonly ISubscriptionEngineCoordinator _subscriptionEngineCoordinator;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly SubscriptionsBackgroundService _subscriptionsBackgroundService;

    public SubscriptionsDiagnosticsController(ISubscriptionService subscriptions, ISubscriptionEngine subscriptionEngine, 
        ISubscriptionsDateTimeProvider subscriptionsDateTimeProvider, IOutboundEmailSender outboundEmailSender,
        ISubscriptionEngineCoordinator subscriptionEngineCoordinator, ISubscriptionRepository subscriptionRepository,
        IWebHostEnvironment webHostEnvironment, SubscriptionsBackgroundService subscriptionsBackgroundService)
    {
        _subscriptions = subscriptions;
        _engine = subscriptionEngine;
        _subscriptionsDateTimeProvider = subscriptionsDateTimeProvider;
        _outboundEmailSender = outboundEmailSender;
        _subscriptionEngineCoordinator = subscriptionEngineCoordinator;
        _subscriptionRepository = subscriptionRepository;
        _webHostEnvironment = webHostEnvironment;
        _subscriptionsBackgroundService = subscriptionsBackgroundService;
    }

    public class Routes
    {
        public const string Home = "subscriptions.diagnostics.home";
        public const string SentEmails = "subscriptions.diagnostics.sent-emails";
        public const string ClearAllData = "subscriptions.diagnostics.data.clear";
        public const string ClearSentEmails = "subscriptions.diagnostics.sent-emails.clear";
        public const string RequestProcess = "subscriptions.diagnostics.request-process";
        public const string FakeDateTimeSet = "subscriptions.diagnostics.fake-datetime";
        public const string FakeDateTimeClear = "subscriptions.diagnostics.fake-datetime.clear";
        public const string SubscriptionList = "subscriptions.diagnostics.subscriptions-list";
        public const string SubscriptionPoke = "subscriptions.diagnostics.subscription.poke";
        public const string ToggleBackgroundServiceIsEnabled = "subscriptions.diagnostics.background-service.is-enabled.toggle";
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_webHostEnvironment.IsDevelopment())
        {
            context.Result = new NotFoundResult();
        }
        else
        {
            base.OnActionExecuting(context);
        }
    }

    [HttpGet(Name = Routes.Home)]
    public IActionResult Index(string? m = null)
    {
        var model = new SubscriptionsDiagnosticsViewModel
        {
            DateTimeEnvelope = _subscriptionsDateTimeProvider.Get(),
            OutboundSenderMode = _outboundEmailSender.Mode,
            SentEmails = _outboundEmailSender.Requests,
            SuccessBannerMessage = m != null ? Base64UrlEncoder.Decode(m) : null,
            IsBackgroundServiceEnabled = _subscriptionsBackgroundService.IsEnabled,
        };

        model.LastConfirmationLink = model.SentEmails.OrderBy(x=>x.Timestamp).LastOrDefault()?.Replacements.GetValueOrDefault(EmailPlaceholders.ConfirmLink);
        return View(model);
    }

    [HttpGet("list", Name = Routes.SubscriptionList)]
    public async Task<IActionResult> ListAsync(int? skip = null)
    {
        var page = await _subscriptionRepository.GetAllAsync(skip:skip);
        

        var vm = new SubscriptionsDiagnosticsSubscriptionListViewModel
        {
            Skip = (skip ?? 0) + (await page.CountAsync()),
            List = await page.ToListAsync(),
        };

        return View("Subscriptions", vm);
    }

    [HttpPost("poke/{id}", Name = Routes.SubscriptionPoke)]
    public async Task<IActionResult> PokeSubscriptionAsync(string id, string returnurl)
    {
        var e = await _subscriptionRepository.GetAsync(new SubscriptionKey(id))??throw new DomainException("Subscription not found");
        e.LastThumbprint = Guid.NewGuid().ToString();
        await _subscriptionRepository.UpsertAsync(e);
        return Redirect(QueryHelpers.AddQueryString(returnurl, "p", "1"));
    }

    [HttpPost("background-service/toggle", Name = Routes.ToggleBackgroundServiceIsEnabled)]
    public IActionResult ToggleBackgroundServiceIsEnabled()
    {
        if(_subscriptionsBackgroundService.IsEnabled)
        {
            _subscriptionsBackgroundService.IsEnabled = false;
            return RedirectToAction(nameof(Index), new { m = Base64UrlEncoder.Encode("Background service has been disabled") });
        }
        else
        {
            _subscriptionsBackgroundService.IsEnabled = true;
            return RedirectToAction(nameof(Index), new { m = Base64UrlEncoder.Encode("Background service has been enabled") });
        }
    }


    [HttpGet("sent-emails", Name = Routes.SentEmails)]
    public IActionResult SentEmails()
    {
        var model = new SubscriptionsDiagnosticsViewModel
        {
            SentEmails = _outboundEmailSender.Requests,
        };
        return View(model);
    }

    [HttpPost("sent-emails/clear", Name = Routes.ClearSentEmails)]
    public IActionResult ClearSentEmails()
    {
        _outboundEmailSender.Requests.Clear();
        return RedirectToAction(nameof(Index), new { m = Base64UrlEncoder.Encode("Sent emails cleared successfully") });
    }

    [HttpPost("fake-datetime", Name = Routes.FakeDateTimeSet)]
    public IActionResult FakeDateTimeSet([FromForm] string when, [FromForm] int expiryInHours)
    {
        if (DateTime.TryParseExact(when, SubscriptionsDiagnosticsViewModel.FakeDateTimeFormat, null, DateTimeStyles.None, out var d))
        {
            _subscriptionsDateTimeProvider.Set(new SubscriptionsDateTimeProvider.SetPayload { DateTime = d, ExpiryHours = expiryInHours });
            return RedirectToAction(nameof(Index), new { m = Base64UrlEncoder.Encode("Fake date/time has been set") });
        }
        else
        {
            throw new Common.Exceptions.DomainException($"Date time format for '{when}' not recognised; it should be {SubscriptionsDiagnosticsViewModel.FakeDateTimeFormat}");
        }

    }

    [HttpPost("fake-datetime/clear", Name = Routes.FakeDateTimeClear)]
    public IActionResult FakeDateTimeClear()
    {
        _subscriptionsDateTimeProvider.Clear();
        return RedirectToAction(nameof(Index), new { m = Base64UrlEncoder.Encode("Fake date/time has been cleared") });
    }

    [HttpPost("process", Name = Routes.RequestProcess)]
    public async Task<IActionResult> ProcessAsync()
    {
        var result = await _subscriptionEngineCoordinator.RequestProcessAsync(CancellationToken.None);
        
        if(result.Exception != null)
        {
            return Content("Error: " + result.Exception.ToString());
        }
        else
        {
            return RedirectToAction(nameof(Index), new { m = Base64UrlEncoder.Encode("Request process gave result: " + result.Status), stat = JsonBase64UrlToken.Serialize(result.Stats) });
        }
        
    }

    [HttpPost("data/clear", Name = Routes.ClearAllData)]
    public async Task<IActionResult> ClearAllDataAsync()
    {
        var eng = (IClearable)_engine;
        var subs = (IClearable)_subscriptions;
        await eng.ClearDataAsync();
        await subs.ClearDataAsync();
        return RedirectToAction(nameof(Index), new { m = Base64UrlEncoder.Encode("All subscriptions data cleared successfully") });
    }


}

