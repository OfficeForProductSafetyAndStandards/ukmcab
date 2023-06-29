using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Domain.Emails.Uris;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Areas.Subscriptions.Controllers;

namespace UKMCAB.Web.UI.Services.Subscriptions;

public class SubscriptionsConfiguratorHostedService : IHostedService
{
    private readonly IEmailTemplatesService _emailTemplatesService;
    private readonly LinkGenerator _linkGenerator;
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<SubscriptionsConfiguratorHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServer _server;

    public SubscriptionsConfiguratorHostedService(IEmailTemplatesService emailTemplatesService, LinkGenerator linkGenerator, 
        TelemetryClient telemetry, ILogger<SubscriptionsConfiguratorHostedService> logger, IConfiguration configuration, IHostApplicationLifetime hostApplicationLifetime, IServer server)
    {
        _emailTemplatesService = emailTemplatesService;
        _linkGenerator = linkGenerator;
        _telemetry = telemetry;
        _logger = logger;
        _configuration = configuration;
        _hostApplicationLifetime = hostApplicationLifetime;
        _server = server;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
        _hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        _hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    private void OnStarted()
    {
        try
        {
            Initialise();
        } 
        catch (Exception ex)
        {
            _telemetry.TrackException(ex);
            _telemetry.TrackEvent(AiTracking.Events.SubscriptionsInitialise+"_FAIL");
            _logger.LogError(ex, ex.Message);
        }
    }

    private void OnStopping()
    {
    }

    private void OnStopped()
    {
    }


    public void Initialise()
    {
        var defaultAppAddresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses.ToArray() ?? Array.Empty<string>();
        var defaultAppAddress = defaultAppAddresses.FirstOrDefault(x => x.StartsWith("https"));
        var configuredAppAddress = _configuration["AppHostName"].PrependIf("https://");
        var baseAddress = configuredAppAddress ?? defaultAppAddress ?? throw new Exception("No web addresses obtainable");
        var @base = new Uri(baseAddress);

        var options = new UriTemplateOptions
        {
            BaseUri = @base,

            CabDetails = new("@cabid", _linkGenerator.GetPathByRouteValues(CABProfileController.Routes.TrackInboundLinkCabDetails, new { id = "@cabid" })
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

        _telemetry.TrackEvent(AiTracking.Events.SubscriptionsInitialise, new Dictionary<string, string>()
        {
            [nameof(options)] = options.Serialize() ?? "",
            [nameof(defaultAppAddresses)] = defaultAppAddresses.Serialize() ?? "",
            [nameof(defaultAppAddress)] = defaultAppAddress ?? "",
            [nameof(baseAddress)] = baseAddress ?? "",
        });

        _logger.LogInformation($"{nameof(SubscriptionsConfiguratorHostedService)} initialised successfully");
    }
}


