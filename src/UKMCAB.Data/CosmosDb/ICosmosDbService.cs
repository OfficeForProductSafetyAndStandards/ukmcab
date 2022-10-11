using UKMCAB.Data.CosmosDb.Models;

namespace UKMCAB.Data.CosmosDb
{
    public interface ICosmosDbService
    {
        Task<string> CreateAsync(CAB cab);
        Task<bool> UpdateAsync(CAB cab);
        Task<CAB> GetByIdAsync(string id);
        Task<List<CAB>> GetAllAsync();
    }
}
