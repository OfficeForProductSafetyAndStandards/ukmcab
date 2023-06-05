using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Search.Services;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Data
{
    public interface IInitialiseDataService
    {
        Task InitialiseAsync(bool force = false);
    }

    public class InitialiseDataService : IInitialiseDataService
    {
        private readonly ICABRepository _cabRepository;
        private readonly ISearchServiceManagment _searchServiceManagment;
        private readonly IDistCache _redisCache;

        public InitialiseDataService(ICABRepository cabRepository, ISearchServiceManagment searchServiceManagment, IDistCache redisCache)
        {
            _cabRepository = cabRepository;
            _searchServiceManagment = searchServiceManagment;
            _redisCache = redisCache;
        }

        public async Task InitialiseAsync(bool force = false)
        {
            await _cabRepository.InitialiseAsync(force);
            await _searchServiceManagment.InitialiseAsync(force);
            if (force)
            {
                await _redisCache.FlushAsync();
            }
        }
    }
}
