using System.Linq.Expressions;
using UKMCAB.Data.Pagination;

namespace UKMCAB.Data.Interfaces.Services;

public interface IReadOnlyRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate);
    Task<(IEnumerable<O> Results, PaginationInfo PaginationInfo)> PaginatedQueryAsync<O>(Expression<Func<O, bool>> predicate, int pageNumber, string? searchTerm = null, int pageSize = 20) where O : class, IOrderable;
    Task<T> GetAsync(string id);
}