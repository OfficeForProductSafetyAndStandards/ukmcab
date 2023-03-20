using Azure;
using Azure.Search.Documents.Indexes;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Data;

namespace UKMCAB.Web.UI.Services
{
    public class RandomSortGenerator : BackgroundService
    {
        private Timer _timer;
        private ICABRepository _repository;
        private SearchIndexerClient _searchIndexerClient;

        public RandomSortGenerator(ICABRepository cabRepository, CognitiveSearchConnectionString connectionString)
        {
            _repository = cabRepository;
            _searchIndexerClient = new SearchIndexerClient(new Uri(connectionString.Endpoint),
                new AzureKeyCredential(connectionString.ApiKey));

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var tomorrow = now.AddDays(1);
            var schedule = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 3, 0, 0);
            var difference = schedule - now;
            _timer = new Timer(RegenerateRandomSortValues, null, difference, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void RegenerateRandomSortValues(object? state)
        {
            var allCabs = await _repository.Query<Document>(d => d.IsPublished);
            foreach (var cab in allCabs)
            {
                cab.RandomSort = Guid.NewGuid().ToString();
                await _repository.Update(cab);
            }

            await _searchIndexerClient.RunIndexerAsync(DataConstants.Search.SEARCH_INDEXER);
        }
    }
}
