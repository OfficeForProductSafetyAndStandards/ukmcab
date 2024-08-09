using System.Linq.Expressions;
using UKMCAB.Data.Models;

namespace UKMCAB.Data.CosmosDb.Services.CAB
{
    public interface ICABRepository
    {
        Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate);

        Task<Document> CreateAsync(Document document, DateTime lastUpdatedDateTime);
        Task<int> GetCABCountByStatusAsync(Status status);
        Task<int> GetCABCountBySubStatusAsync(SubStatus subStatus);
        
        Task<bool> DeleteAsync(Document document);
        Task<bool> InitialiseAsync(bool force = false);
        IQueryable<Document> GetItemLinqQueryable();
        Task UpdateAsync(Document document, DateTime? lastUpdatedDate = default);
    }
}
