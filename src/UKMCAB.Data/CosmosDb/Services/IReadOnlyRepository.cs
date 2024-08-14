using System.Linq.Expressions;
using UKMCAB.Data.Pagination;

namespace UKMCAB.Data.CosmosDb.Services;

public interface IReadOnlyRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate);
    Task<(IEnumerable<U> Results, PaginationInfo PaginationInfo)> PaginatedQueryAsync<U>(Expression<Func<U, bool>> predicate, int pageIndex, int pageSize = 20) where U : ISortable;
    Task<T> GetAsync(string id);
}