
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Web.UI.Services.Subscriptions;

namespace UKMCAB.Web.UI.Services
{
    public class StatsRecorderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<StatsRecorderBackgroundService> _logger;

        public StatsRecorderBackgroundService(
            IServiceProvider serviceProvider,
            TelemetryClient telemetryClient,
            ILogger<StatsRecorderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stats Recorder started");

#if (DEBUG)
            var timeSpan = TimeSpan.FromSeconds(30);
#else
            var timeSpan = TimeSpan.FromDays(1);
#endif

            using PeriodicTimer timer = new(timeSpan);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await RecordStats();
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                    await _telemetryClient.FlushAsync(CancellationToken.None);
                    _logger.LogError(ex, "Error recording stats");
                }
            }
        }

        private async Task RecordStats()
        {
            var scope = _serviceProvider.CreateScope();
            var cabAdminService = scope.ServiceProvider.GetRequiredService<ICABAdminService>();
            var cabRepository = scope.ServiceProvider.GetRequiredService<ICABRepository>();
            var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();

            await cabAdminService.RecordStatsAsync();

            var pages1 = await subscriptionRepository.GetAllAsync();
            var subscriptions = await pages1.ToListAsync();

            var cabSubscriptionsCount = subscriptions.Count(x => x.SubscriptionType == SubscriptionType.Cab);
            var searchSubscriptionsCount = subscriptions.Count(x => x.SubscriptionType == SubscriptionType.Search);

            _telemetryClient.GetMetric(AiTracking.Metrics.CabSubscriptionsCount)
                .TrackValue(cabSubscriptionsCount);

            _telemetryClient.GetMetric(AiTracking.Metrics.SearchSubscriptionsCount)
                .TrackValue(searchSubscriptionsCount);

            var cabs = await cabRepository.GetItemLinqQueryable().AsAsyncEnumerable().ToListAsync();

            _telemetryClient.GetMetric(AiTracking.Metrics.CabsWithSchedules)
                .TrackValue(cabs.Count(x => (x.Schedules ?? new()).Count > 0));

            _telemetryClient.GetMetric(AiTracking.Metrics.CabsWithoutSchedules)
                .TrackValue(cabs.Count(x => (x.Schedules ?? new()).Count == 0));

            _logger.LogInformation($"Recorded stats successfully");
        }
    }
}
