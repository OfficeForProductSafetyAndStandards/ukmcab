namespace UKMCAB.Web.UI.Services.Subscriptions;

public class SubscriptionsBackgroundService : BackgroundService
{
    private readonly ISubscriptionEngineCoordinator _subscriptionEngineCoordinator;

    public bool IsEnabled { get; set; } = true;

    public SubscriptionsBackgroundService(ISubscriptionEngineCoordinator subscriptionEngineCoordinator) => _subscriptionEngineCoordinator = subscriptionEngineCoordinator;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
#if (DEBUG)
            const int Interval = 30_000;
#else
            const int Interval = 60_000 * 10;
#endif
            if(IsEnabled)
            {
                var result = await _subscriptionEngineCoordinator.RequestProcessAsync(stoppingToken).ConfigureAwait(false);
            }

            var delay = Interval + RandomNumber.Next(-1000, 1000);
            await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
        }
    }
}
