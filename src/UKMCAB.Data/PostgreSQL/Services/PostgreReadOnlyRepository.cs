using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UKMCAB.Data.Interfaces.Services;
using UKMCAB.Data.Pagination;

namespace UKMCAB.Data.PostgreSQL.Services;

public class PostgreReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class
{
    private readonly ApplicationDataContext _dbContext;

    public PostgreReadOnlyRepository(ApplicationDataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Task.FromResult(_dbContext.Set<T>().AsEnumerable());
    }

    public async Task<T> GetAsync(string id)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        var guidId = Guid.Parse(id);

        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == guidId).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<O> Results, PaginationInfo PaginationInfo)> PaginatedQueryAsync<O>(Expression<Func<O, bool>> predicate, int pageNumber, string? searchTerm = null, int pageSize = 20) where O : class, IOrderable
    {
        IQueryable<T> query = _dbContext.Set<T>();
        query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => EF.Property<string>(x, "Name").Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || EF.Property<List<string>>(x, "ReferenceNumber").Any(refNum => refNum.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        var queryCount = await query.CountAsync();

        var paginationInfo = new PaginationInfo(pageNumber, queryCount);

        query = query.OrderBy(x => EF.Property<string>(x, "Name")).Skip(paginationInfo.Skip).Take(paginationInfo.Take);

        var list = new List<O>();
        list.AddRange(query.ToList().Select(i => i as O));

        return (list, paginationInfo);
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate)
    {
        return _dbContext.Set<T>().Where(predicate);
    }
}
