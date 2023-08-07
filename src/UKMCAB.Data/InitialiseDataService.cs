using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.CosmosDb.Services.User;
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
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserAccountRequestRepository _userAccountRequestRepository;

        public InitialiseDataService(ICABRepository cabRepository, ISearchServiceManagment searchServiceManagment, IDistCache redisCache, IUserAccountRepository userAccountRepository, IUserAccountRequestRepository userAccountRequestRepository)
        {
            _cabRepository = cabRepository;
            _searchServiceManagment = searchServiceManagment;
            _redisCache = redisCache;
            _userAccountRepository = userAccountRepository;
            _userAccountRequestRepository = userAccountRequestRepository;
        }

        public async Task InitialiseAsync(bool force = false)
        {
            await _userAccountRepository.InitialiseAsync().ConfigureAwait(false);
            await _userAccountRequestRepository.InitialiseAsync().ConfigureAwait(false);

            force = await _cabRepository.InitialiseAsync(force);
            await _searchServiceManagment.InitialiseAsync(force);
            if (force)
            {
                await _redisCache.FlushAsync();
            }
        }
    }
}
