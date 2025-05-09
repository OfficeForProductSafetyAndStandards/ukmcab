using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using UKMCAB.Subscriptions.Core.Data.Models;

namespace UKMCAB.Subscriptions.Core.Data;

public class SubscriptionDbContext : DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options)
        : base(options)
    {
    }

    public DbSet<TableEntity> TableEntities => Set<TableEntity>();
    public DbSet<SubscriptionEntity> SubscriptionEntities => Set<SubscriptionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionEntity>()
            .HasKey(e => new { e.TableKey, e.PartitionKey, e.RowKey });
    }
}

public interface IRepository
{
    Task DeleteAllAsync();
    Task DeleteAsync(Keys keys);
    Task<bool> ExistsAsync(Keys keys);
}

public class Repository<T> : IRepository where T : class, ITableEntity
{
    private readonly SubscriptionDbContext _context;
    private readonly string _tableKey;
    private readonly DbSet<T> _dbSet;

    protected Repository(SubscriptionDbContext context, string tableKey)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _tableKey = tableKey;
    }

    protected async Task AddAsync(ITableEntity entity)
    {
        entity.TableKey = _tableKey;
        await _dbSet.AddAsync((T)entity);
        await _context.SaveChangesAsync();
    }

    protected async Task UpsertAsync(ITableEntity entity)
    {
        entity.TableKey = _tableKey;
        var exists = await _dbSet.FindAsync(entity.PartitionKey, entity.RowKey);
        if (exists != null)
            _context.Entry(exists).CurrentValues.SetValues(entity);
        else
            await _dbSet.AddAsync((T)entity);

        await _context.SaveChangesAsync();
    }

    protected async Task<T?> GetAsync<T>(Keys keys) where T : class, ITableEntity
    {
        return await _dbSet.FindAsync(keys.PartitionKey, keys.RowKey, _tableKey) as T;
    }

    public async Task<bool> ExistsAsync(Keys keys)
    {
        return await _dbSet.FindAsync(keys.PartitionKey, keys.RowKey, _tableKey) != null;
    }

    public async Task<IAsyncEnumerable<Page<T>>> GetAllAsync<T>(string? partitionKey = null, string? continuationToken = null, int? take = null) where T : class, ITableEntity
    {
        IQueryable<T> query = (IQueryable<T>)_dbSet.Where(i => i.TableKey == _tableKey).AsQueryable();

        if (partitionKey == null)
        {
            return query.QueryAsync<T>(maxPerPage: take).AsPages(take, continuationToken);
        }
        else
        {
            return query.QueryAsync<T>(x => x.PartitionKey == partitionKey, take).AsPages(take, continuationToken);
        }
    }

    protected async Task UpdateAsync(ITableEntity entity)
    {
        entity.TableKey = _tableKey;
        _context.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Keys keys)
    {
        var entity = await _dbSet.FindAsync(keys.PartitionKey, keys.RowKey, _tableKey);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllAsync()
    {
        _context.RemoveRange(_dbSet.Where(i => i.TableKey == _tableKey));
        await _context.SaveChangesAsync();
    }
}

