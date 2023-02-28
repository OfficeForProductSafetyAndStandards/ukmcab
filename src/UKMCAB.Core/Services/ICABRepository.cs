using System.Linq.Expressions;
using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public interface ICABRepository
    {
        Task<Document> GetByIdAsync(string id);
        Task<List<Document>> Query(string whereClause);
        Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate);
        Task<Document> CreateAsync(Document document);
        Task<bool> Updated(Document document);

        Task<bool> Update(dynamic document);
    }
}
