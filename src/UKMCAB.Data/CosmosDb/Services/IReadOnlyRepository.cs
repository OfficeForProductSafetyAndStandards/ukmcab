using System.Linq.Expressions;

namespace UKMCAB.Data.CosmosDb.Services;

public interface IReadOnlyRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate);

    Task<T> GetAsync(string id);
}