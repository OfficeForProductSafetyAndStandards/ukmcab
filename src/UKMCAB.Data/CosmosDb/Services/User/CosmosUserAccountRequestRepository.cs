using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Polly;
using Polly.Fallback;
using System.Linq.Dynamic.Core;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public class CosmosUserAccountRequestRepository : IUserAccountRequestRepository
{
    public const string ContainerId = "user-account-requests";
    private readonly Container _container;
    private readonly AsyncFallbackPolicy<UserAccountRequest?> _getUserAccountRequestPolicy;

    public CosmosUserAccountRequestRepository(CosmosDbConnectionString cosmosDbConnectionString)
    {
        var client = CosmosClientFactory.Create(cosmosDbConnectionString);
        _container = client.GetContainer(DataConstants.CosmosDb.Database, ContainerId);
        _getUserAccountRequestPolicy = Policy<UserAccountRequest?>.Handle<CosmosException>(x => x.StatusCode == System.Net.HttpStatusCode.NotFound).FallbackAsync(null as UserAccountRequest);
    }

    public async Task InitialiseAsync() => await _container.Database.CreateContainerIfNotExistsAsync(ContainerId, "/id").ConfigureAwait(false);

    public async Task<UserAccountRequest?> GetPendingAsync(string subjectId)
    {
        var item = await _container.GetItemLinqQueryable<UserAccountRequest>().AsAsyncEnumerable().FirstOrDefaultAsync(x => x.SubjectId == subjectId && x.Status == UserAccountRequestStatus.Pending);
        return item;
    }

    public async Task<UserAccountRequest?> GetAsync(string id)
    {
        var retVal = await _getUserAccountRequestPolicy.ExecuteAsync(async () => await _container.ReadItemAsync<UserAccountRequest>(id, new PartitionKey(id)).ConfigureAwait(false)).ConfigureAwait(false);
        return retVal;
    }

    public async Task CreateAsync(UserAccountRequest userAccount)
    {
        await _container.CreateItemAsync(userAccount).ConfigureAwait(false);
    }

    public async Task UpdateAsync(UserAccountRequest userAccount)
    {
        await _container.ReplaceItemAsync(userAccount, userAccount.Id).ConfigureAwait(false);
    }

    public async Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestListOptions options)
    {
        var query = _container.GetItemLinqQueryable<UserAccountRequest>().AsQueryable();
        if (options.Status.HasValue)
        {
            query = query.Where(x => x.Status == options.Status);
        }

        var set = (await query.AsAsyncEnumerable().ToListAsync()).AsQueryable();

        var data = set.OrderBy(options.SortBy.Expression(nameof(UserAccountRequest.CreatedUtc)))
            .Skip(options.SkipTake.Skip)
            .Take(options.SkipTake.Take)
            .ToList();

        return data;
    }

    public async Task<int> CountAsync(UserAccountRequestStatus? status = null)
    {
        var q = _container.GetItemLinqQueryable<UserAccountRequest>().AsQueryable();
        if (status.HasValue)
        {
            q = q.Where(x => x.Status == status);
        }
        var rv = await q.CountAsync().ConfigureAwait(false);
        return rv;
    }
}
