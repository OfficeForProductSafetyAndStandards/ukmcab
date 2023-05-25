using System.Linq.Expressions;
using UKMCAB.Data.Models;

namespace UKMCAB.Data.CosmosDb.Services
{
    public interface ICABRepository
    {
        Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate);

        Task<Document> CreateAsync(Document document);

        Task<bool> Update(Document document);
        
        Task<bool> Delete(Document document);
        Task InitialiseAsync(bool force = false);
        IQueryable<Document> GetItemLinqQueryable();
    }
}
