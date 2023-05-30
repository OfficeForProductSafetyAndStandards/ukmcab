using Microsoft.ApplicationInsights;
using UKMCAB.Common;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Domain.Emails.Uris;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Areas.Subscriptions.Controllers;

namespace UKMCAB.Web.UI.Services.Subscriptions;

public class SubscriptionsUriTemplatesConfiguratorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEmailTemplatesService _emailTemplatesService;
    private readonly LinkGenerator _linkGenerator;
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<SubscriptionsUriTemplatesConfiguratorMiddleware> _logger;

    public SubscriptionsUriTemplatesConfiguratorMiddleware(RequestDelegate next, IEmailTemplatesService emailTemplatesService, LinkGenerator linkGenerator, TelemetryClient telemetry, ILogger<SubscriptionsUriTemplatesConfiguratorMiddleware> logger)
    {
        _next = next;
        _emailTemplatesService = emailTemplatesService;
        _linkGenerator = linkGenerator;
        _telemetry = telemetry;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context?.Request?.Path.Value ?? string.Empty;
        if (!path.StartsWith("/robots933456.txt"))
        {
            if (!_emailTemplatesService.IsConfigured() || context.Request.Query["subscriptionsuritemplatesconfiguratormiddlewareinit"] == "1")
            {
                Initialise(context);
            }
            await _next(context);
        }
    }

    private void Initialise(HttpContext context)
    {
        var options = new UriTemplateOptions
        {
            BaseUri = context.Request.GetRequestUri(),

            CabDetails = new("@cabid", _linkGenerator.GetPathByRouteValues(CABProfileController.Routes.CabDetails, new { id = "@cabid" })
                                ?? throw new Exception("Cab details route not found")),

            Search = new(_linkGenerator.GetPathByAction(nameof(SearchController.Index), nameof(SearchController).ControllerName())
                                ?? throw new Exception("Search CABs route not found")),

            ConfirmCabSubscription = new("@token",
                                _linkGenerator.GetPathByRouteValues(SubscriptionsController.Routes.ConfirmCabSubscription, new { token = "@token" })
                                ?? throw new Exception("Confirm cab subscription route not found")),

            ConfirmSearchSubscription = new("@token",
                                _linkGenerator.GetPathByRouteValues(SubscriptionsController.Routes.ConfirmSearchSubscription, new { token = "@token" })
                                ?? throw new Exception("Confirm search subscription route not found")),

            ConfirmUpdateEmailAddress = new("@token",
                                _linkGenerator.GetPathByRouteValues(SubscriptionsController.Routes.ConfirmUpdatedEmailAddress, new { token = "@token" })
                                ?? throw new Exception("Confirm updated email address route not found")),

            ManageSubscription = new("@subscriptionid",
                                _linkGenerator.GetPathByRouteValues(SubscriptionsController.Routes.ManageSubscription, new { id = "@subscriptionid" })
                                ?? throw new Exception("Manage subscription route not found")),

            Unsubscribe = new("@subscriptionid",
                                _linkGenerator.GetPathByRouteValues(SubscriptionsController.Routes.Unsubscribe, new { id = "@subscriptionid" })
                                ?? throw new Exception("Unsubscribe route not found")),

            UnsubscribeAll = new("@emailaddress",
                                _linkGenerator.GetPathByRouteValues(SubscriptionsController.Routes.UnsubscribeAll, new { emailAddress = "@emailaddress" })
                                ?? throw new Exception("Unsubscribe-all route not found")),

            SearchChangesSummary = new("@subscriptionid", "@changesdescriptorid",
                                _linkGenerator.GetPathByRouteValues(SubscriptionsController.Routes.SearchChangesSummary, new { subscriptionId = "@subscriptionid", changesDescriptorId = "@changesdescriptorid" })
                                ?? throw new Exception("Search-changes-summary route not found"))
        };
        _emailTemplatesService.Configure(options);


        var d = new Dictionary<string, string>
        {
            [nameof(UriTemplateOptions)] = options.Serialize()
        };

        foreach (var item in context.Request.Headers)
        {
            var key = $"hdr_{item.Key}";
            if (!d.ContainsKey(key) && item.Key.DoesNotEqual("cookie"))
            {
                d[key] = item.Value.ToString();
            }
        }
        _telemetry.TrackEvent(AiTracking.Events.SubscriptionsInitialise, context.ToTrackingMetadata(d));

        _logger.LogInformation($"{nameof(SubscriptionsUriTemplatesConfiguratorMiddleware)} initialised successfully");
    }
}

public static class SubscriptionsUriTemplatesConfiguratorMiddlewareExtensions
{
    public static IApplicationBuilder UseSubscriptionsUriTemplatesConfiguratorMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<SubscriptionsUriTemplatesConfiguratorMiddleware>();
        return app;
    }
}