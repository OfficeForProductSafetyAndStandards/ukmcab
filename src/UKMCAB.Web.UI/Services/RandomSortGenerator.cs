using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.ApplicationInsights;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Infrastructure.Logging.Models;

namespace UKMCAB.Web.UI.Services
{
    public class RandomSortGenerator : BackgroundService
    {
        private ICABRepository _repository;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILoggingService _loggingService;
        private readonly IDistCache _distCache;
        private SearchIndexerClient _searchIndexerClient;

        public RandomSortGenerator(ICABRepository cabRepository, CognitiveSearchConnectionString connectionString, TelemetryClient telemetryClient, ILoggingService loggingService, IDistCache distCache)
        {
            _repository = cabRepository;
            _telemetryClient = telemetryClient;
            _loggingService = loggingService;
            _distCache = distCache;
            _searchIndexerClient = new SearchIndexerClient(new Uri(connectionString.Endpoint),
                new AzureKeyCredential(connectionString.ApiKey));

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var lockName = StringExt.Keyify(nameof(RandomSortGenerator), nameof(ExecuteAsync));
                var lockOwner = LockOwner.Create();
                try
                {
                    // Wait unit next 3 a.m.
                    var now = DateTime.UtcNow;
                    var nextRun = now.Hour < 3 ? now : now.AddDays(1);
                    var schedule = new DateTime(nextRun.Year, nextRun.Month, nextRun.Day, 3, 0, 0);
                    var difference = schedule - now;
                    await Task.Delay(difference);

                    var got = await _distCache.LockTakeAsync(lockName, lockOwner, TimeSpan.FromMinutes(1));
                    if (got)
                    {
                        await RegenerateRandomSortValues();
                    }
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                    _loggingService.Log(new LogEntry(ex));
                }
                finally
                {
                    await _distCache.LockReleaseAsync(lockName, lockOwner);
                }
            }
        }

        private async Task RegenerateRandomSortValues()
        {
            var allCabs = await _repository.Query<Document>(d => d.StatusValue == Status.Published);
            foreach (var cab in allCabs)
            {
                cab.RandomSort = Guid.NewGuid().ToString();
                await _repository.Update(cab);
            }

            await _searchIndexerClient.RunIndexerAsync(DataConstants.Search.SEARCH_INDEXER);
        }
    }
}
