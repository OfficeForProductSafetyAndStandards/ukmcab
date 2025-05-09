using AutoMapper;
using UKMCAB.Data.Interfaces.Services.CAB;

namespace UKMCAB.Data.Search.Services
{
    public class PostgreSearchServiceManagment : ISearchServiceManagment
    {
        private readonly IOpenSearchIndexerClient _openSearchIndexerClient;
        private readonly ICABRepository _cabRepository;
        private readonly IMapper _mapper;
        public PostgreSearchServiceManagment(IOpenSearchIndexerClient openSearchIndexerClient, ICABRepository cabRepository, IMapper mapper)
        {
            _openSearchIndexerClient = openSearchIndexerClient;
            _cabRepository = cabRepository;
            _mapper = mapper;
        }
        public async Task InitialiseAsync(bool force = false)
        {
            var indexExists = await _openSearchIndexerClient.IndexExistsAsync(DataConstants.Search.SEARCH_INDEX);
            if (!indexExists || force)
            {
                if (indexExists)
                {
                    await _openSearchIndexerClient.DeleteIndexAsync(DataConstants.Search.SEARCH_INDEX);
                }

                await _openSearchIndexerClient.CreateIndexAsync(DataConstants.Search.SEARCH_INDEX);

                await _openSearchIndexerClient.RunIndexerAsync(DataConstants.Search.SEARCH_INDEX);
            }
        }
    }
}
