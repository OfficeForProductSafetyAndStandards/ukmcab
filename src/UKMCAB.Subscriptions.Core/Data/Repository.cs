using UKMCAB.Subscriptions.Core.Data.Models;

namespace UKMCAB.Subscriptions.Core.Data;

public class Repository<T> : IRepository where T : class, ITableEntity
{
    private readonly SubscriptionDbContext _context;
    private readonly string _tableKey;

    protected Repository(SubscriptionDbContext context, string tableKey)
    {
        _context = context;
        _tableKey = tableKey;
    }

    protected async Task AddAsync(ITableEntity entity)
    {
        entity.TableKey = _tableKey;
        await _context.Set<T>().AddAsync((T)entity);
        await _context.SaveChangesAsync();
    }

    protected async Task UpsertAsync(ITableEntity entity)
    {
        entity.TableKey = _tableKey;
        var exists = await _context.Set<T>().FindAsync(entity.TableKey, entity.PartitionKey, entity.RowKey);
        if (exists != null)
            _context.Entry(exists).CurrentValues.SetValues(entity);
        else
            await _context.Set<T>().AddAsync((T)entity);

        await _context.SaveChangesAsync();
    }

    protected async Task<T?> GetAsync<T>(Keys keys) where T : class, ITableEntity
    {
        return await _context.Set<T>().FindAsync(_tableKey, keys.PartitionKey, keys.RowKey);
    }

    public async Task<bool> ExistsAsync(Keys keys)
    {
        return await _context.Set<T>().FindAsync(_tableKey, keys.PartitionKey, keys.RowKey) != null;
    }

    public async Task<IAsyncEnumerable<T>> GetAllAsync<T>(string? partitionKey = null, int? skip = null, int? take = null) where T : class, ITableEntity
    {
        IQueryable<T> query = _context.Set<T>().Where(i => i.TableKey == _tableKey).AsQueryable();

        if (partitionKey is not null)
        {
            query = query.Where(x => x.PartitionKey == partitionKey);
        }

        if (skip is not null)
        {
            query = query.Skip(skip.Value);
        }

        if(take is not null)
        {
            query = query.Take(take.Value);
        }

        return query.ToAsyncEnumerable();
    }

    protected async Task UpdateAsync(ITableEntity entity)
    {
        entity.TableKey = _tableKey;
        _context.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Keys keys)
    {
        var entity = await _context.Set<T>().FindAsync(_tableKey, keys.PartitionKey, keys.RowKey);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllAsync()
    {
        _context.RemoveRange(_context.Set<T>().Where(i => i.TableKey == _tableKey));
        await _context.SaveChangesAsync();
    }
}

