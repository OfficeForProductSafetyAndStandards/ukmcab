using UKMCAB.Core.Services.CAB;

namespace UKMCAB.Web.UI.Services.Subscriptions;

public class SubscriptionsBackgroundService : BackgroundService
{
    private readonly ISubscriptionEngineCoordinator _subscriptionEngineCoordinator;
    private readonly IConfiguration _configuration;

#if (DEBUG)
    public bool IsEnabled { get; set; } = false; // in debug-mode, default to disabled.
#else
    public bool IsEnabled { get; set; } = true;  // in release-mode, assume it's deployed to a server and default to enabled.
#endif

    public SubscriptionsBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var scope = serviceProvider.CreateScope();
        _subscriptionEngineCoordinator = scope.ServiceProvider.GetRequiredService<ISubscriptionEngineCoordinator>();
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_configuration["DisableSubscriptionsEngine"] != "true")
        {
            while (!stoppingToken.IsCancellationRequested)
            {
#if (DEBUG)
                const int Interval = 30_000;
#else
            const int Interval = 60_000 * 10;
#endif
                if (IsEnabled)
                {
                    var result = await _subscriptionEngineCoordinator.RequestProcessAsync(stoppingToken).ConfigureAwait(false);
                }

                var delay = Interval + RandomNumber.Next(-1000, 1000);
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
