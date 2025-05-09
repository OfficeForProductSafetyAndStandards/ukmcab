
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Web.UI.Services
{
    public class StatsRecorderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<StatsRecorderBackgroundService> _logger;

        public StatsRecorderBackgroundService(
            IServiceProvider serviceProvider,
            ISubscriptionRepository subscriptionRepository,
            TelemetryClient telemetryClient,
            ILogger<StatsRecorderBackgroundService> logger) 
        { 
            _serviceProvider = serviceProvider;
            _subscriptionRepository = subscriptionRepository;
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
            var _cabAdminService = scope.ServiceProvider.GetRequiredService<ICABAdminService>();
            var _cabRepository = scope.ServiceProvider.GetRequiredService<ICABRepository>();

            await _cabAdminService.RecordStatsAsync();

            var pages1 = await _subscriptionRepository.GetAllAsync(take: 1);
            var pages2 = await pages1.ToListAsync();
            var subscriptions = pages2.SelectMany(x => x.Values).ToList();

            var cabSubscriptionsCount = subscriptions.Count(x => x.SubscriptionType == SubscriptionType.Cab);
            var searchSubscriptionsCount = subscriptions.Count(x => x.SubscriptionType == SubscriptionType.Search);

            _telemetryClient.GetMetric(AiTracking.Metrics.CabSubscriptionsCount)
                .TrackValue(cabSubscriptionsCount);

            _telemetryClient.GetMetric(AiTracking.Metrics.SearchSubscriptionsCount)
                .TrackValue(searchSubscriptionsCount);

            var cabs = await _cabRepository.GetItemLinqQueryable().AsAsyncEnumerable().ToListAsync();

            _telemetryClient.GetMetric(AiTracking.Metrics.CabsWithSchedules)
                .TrackValue(cabs.Count(x => (x.Schedules ?? new()).Count > 0));

            _telemetryClient.GetMetric(AiTracking.Metrics.CabsWithoutSchedules)
                .TrackValue(cabs.Count(x => (x.Schedules ?? new()).Count == 0));

            _logger.LogInformation($"Recorded stats successfully");
        }
    }
}
