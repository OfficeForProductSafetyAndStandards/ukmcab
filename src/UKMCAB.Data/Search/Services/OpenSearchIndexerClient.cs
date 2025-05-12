using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    /// <summary>
    /// TEMPORARY Open Search Indexer implementation (update as required)
    /// </summary>
    public class OpenSearchIndexerClient : IOpenSearchIndexerClient
    {
        private readonly IOpenSearchClient _openSearchClient;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;

        public OpenSearchIndexerClient(IOpenSearchClient openSearchClient, IMapper mapper, IServiceProvider serviceProvider)
        {
            _openSearchClient = openSearchClient;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }
        public async Task<bool> IndexExistsAsync(string indexerName)
        {
            var response = await _openSearchClient.Indices.ExistsAsync(indexerName);
            return response.Exists; 
        }


        public async Task CreateIndexAsync(string indexerName)
        {
            var response = await _openSearchClient.Indices.CreateAsync(indexerName, c => 
                c.Map<CABIndexItem>(m => m.AutoMap())
            );

            if (!response.IsValid)
            {
                throw new Exception($"Failed to create index '{indexerName}': {response.DebugInformation}.");
            }
        }

        public async Task DeleteIndexAsync(string indexerName)
        {
            await _openSearchClient.Indices.DeleteAsync(indexerName);
        }

        public async Task BulkIndexAsync(string indexerName, IEnumerable<CABIndexItemOpenSearch> documents)
        {
            var response = await _openSearchClient.BulkAsync(b => b
                .Index(indexerName)
                .IndexMany(documents)
            );

            if (response.Errors)
            {
                var errors = string.Join("\n", response.ItemsWithErrors.Select(e => e.Error?.Reason));
                throw new Exception($"Bulk indexing failed: \n{ errors }");
            }           

        }

        public async Task RunIndexerAsync(string indexerName, CancellationToken token = default)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var cabRepository = scope.ServiceProvider.GetRequiredService<ICABRepository>();

            var docs = cabRepository.GetItemLinqQueryable()
                     .Where(d => d.StatusValue != Status.Historical)
                     .ToList();

            var cabIndexItems = _mapper.Map<List<CABIndexItemOpenSearch>>(docs);

            await BulkIndexAsync(DataConstants.Search.SEARCH_INDEX, cabIndexItems);
        }
    }
}
