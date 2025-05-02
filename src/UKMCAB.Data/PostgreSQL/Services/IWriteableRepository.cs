namespace UKMCAB.Data.PostgreSQL.Services;

public interface IWriteableRepository<T> where T : class
{
    Task CreateAsync(T entity);
}

public class PostgreWritableRepository<T> : IWriteableRepository<T> where T : class
{
    private readonly ApplicationDataContext _dbContext;

    public PostgreWritableRepository(ApplicationDataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(T entity)
    {
        var doc = _dbContext.Set<T>().Add(entity);
        await _dbContext.SaveChangesAsync();
    }
}