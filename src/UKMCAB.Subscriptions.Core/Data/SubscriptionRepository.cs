using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data;

public interface ISubscriptionRepository : IRepository
{
    Task UpsertAsync(SubscriptionEntity entity);
    Task<SubscriptionEntity?> GetAsync(SubscriptionKey key);
    Task<IAsyncEnumerable<SubscriptionEntity>> GetAllAsync(string? partitionKey = null, int? skip = null, int? take = null);
}

/// <summary>
/// Store subscription data in Azure Table storage
/// </summary>
public class SubscriptionRepository : Repository<SubscriptionEntity>, ISubscriptionRepository
{
    public SubscriptionRepository(SubscriptionDbContext dataContext) : base(dataContext, $"{SubscriptionsCoreServicesOptions.TableNamePrefix}subscriptions") { }

    public async Task UpsertAsync(SubscriptionEntity entity)
    {
        await base.UpsertAsync(entity);
    }

    public async Task<SubscriptionEntity?> GetAsync(SubscriptionKey key)
    {
        return await GetAsync<SubscriptionEntity>(key);
    }

    public async Task<IAsyncEnumerable<SubscriptionEntity>> GetAllAsync(string? partitionKey = null, int? skip = null, int? take = null)
    {
        return await GetAllAsync<SubscriptionEntity>(partitionKey, skip, take);
    }
}

