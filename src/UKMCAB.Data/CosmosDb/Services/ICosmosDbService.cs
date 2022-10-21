using UKMCAB.Data.CosmosDb.Models;

namespace UKMCAB.Data.CosmosDb.Services
{
    public interface ICosmosDbService
    {
        Task<string> CreateAsync(CAB cab);
        Task<bool> UpdateAsync(CAB cab);
        Task<CAB?> GetByIdAsync(string id);
        Task<List<CAB>> GetPagedCABsAsync(int pageNumber, int pageCount );
        Task<int> GetCABCountAsync();
        Task<List<CAB>> Query(string text);
    }
}
