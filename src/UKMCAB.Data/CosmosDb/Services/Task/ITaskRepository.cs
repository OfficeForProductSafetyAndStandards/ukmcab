using System.Linq.Expressions;

namespace UKMCAB.Data.CosmosDb.Services.Task;

public interface ITaskRepository
{
    Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate);

    Task<Data.Models> CreateAsync(Document document);

    [Obsolete("Use " + nameof(UpdateAsync))]
    Task<bool> Update(Document document);
        
    Task<bool> Delete(Document document);
    Task<bool> InitialiseAsync(bool force = false);
    IQueryable<Document> GetItemLinqQueryable();
    Task UpdateAsync(Document document);
}