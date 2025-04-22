using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Polly;
using Polly.Fallback;
using System.Linq.Dynamic.Core;
using UKMCAB.Common;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public class CosmosUserAccountRepository : IUserAccountRepository
{

    public const string ContainerId = "user-accounts";
    private readonly Container _container;
    private readonly AsyncFallbackPolicy<UserAccount?> _getUserAccountPolicy;

    public CosmosUserAccountRepository(CosmosDbConnectionString cosmosDbConnectionString)
    {
        var client = CosmosClientFactory.Create(cosmosDbConnectionString);
        _container = client.GetContainer(DataConstants.CosmosDb.Database, ContainerId);
        _getUserAccountPolicy = Policy<UserAccount?>.Handle<CosmosException>(x => x.StatusCode == System.Net.HttpStatusCode.NotFound).FallbackAsync(null as UserAccount);
    }

    public async Task InitialiseAsync() => await _container.Database.CreateContainerIfNotExistsAsync(ContainerId, "/id");


    public async Task<UserAccount?> GetAsync(string id)
    {
        var retVal = await _getUserAccountPolicy.ExecuteAsync(async () => await _container.ReadItemAsync<UserAccount>(id, new PartitionKey(id)).ConfigureAwait(false)).ConfigureAwait(false);
        return retVal;
    }

    public async Task CreateAsync(UserAccount userAccount) => await _container.CreateItemAsync(userAccount).ConfigureAwait(false);

    public async Task UpdateAsync(UserAccount userAccount) => await _container.ReplaceItemAsync(userAccount, userAccount.Id, new PartitionKey(userAccount.Id)).ConfigureAwait(false);

    public async Task<int> UserCountAsync(UserAccountLockReason? lockReason = null, bool locked = false) {        

        if (lockReason == null)
        {
            return await _container.GetItemLinqQueryable<UserAccount>().AsQueryable().Where(x => x.IsLocked == locked).CountAsync();
        }
        else
        {
            return await _container.GetItemLinqQueryable<UserAccount>().AsQueryable().Where(x => x.IsLocked == locked && x.LockReason == (UserAccountLockReason)lockReason).CountAsync();
        }
    }

    public async Task<IEnumerable<UserAccount>> ListAsync(UserAccountListOptions options)
    {
        var q = (await _container.GetItemLinqQueryable<UserAccount>().AsAsyncEnumerable().ToListAsync()).AsQueryable();

        if (options.ExcludeId.IsNotNullOrEmpty())
        {
            q = q.Where(x => x.Id != options.ExcludeId);
        }

        if (options.IsLocked.HasValue)
        {
            q = q.Where(x => x.IsLocked == options.IsLocked);
            if (options.IsLocked.Value && options.LockReason != null) 
            {
                q = q.Where(x => x.LockReason == options.LockReason);
            }
        }

        var data = q.OrderBy(options.SortBy.Expression(nameof(UserAccount.LastLogonUtc)))
            .Skip(options.SkipTake.Skip)
            .Take(options.SkipTake.Take)
            .ToList();

        return data;
    }

    public async Task PatchAsync<T>(string id, string fieldName, T value)
    {
        await _container.PatchItemAsync<UserAccount>(id, new PartitionKey(id), new[]
        {
            PatchOperation.Set($"/{fieldName}", value)
        });
    }
}
