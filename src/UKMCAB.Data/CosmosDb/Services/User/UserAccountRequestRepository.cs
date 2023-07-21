using Microsoft.Azure.Cosmos;
using Polly;
using Polly.Fallback;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public class UserAccountRequestRepository : IUserAccountRequestRepository
{
    public const string ContainerId = "user-account-requests";
    private readonly Container _container;
    private readonly AsyncFallbackPolicy<UserAccountRequest?> _getUserAccountRequestPolicy;

    public UserAccountRequestRepository(CosmosDbConnectionString cosmosDbConnectionString)
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
        var retVal = await _getUserAccountRequestPolicy.ExecuteAsync(async () => await _container.ReadItemAsync<UserAccountRequest>(id, PartitionKey.None).ConfigureAwait(false)).ConfigureAwait(false);
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

    public async Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestStatus? status, int skip = 0, int take = 20)
    {
        var q = _container.GetItemLinqQueryable<UserAccountRequest>().AsQueryable();

        if (status.HasValue)
        {
            q = q.Where(x => x.Status == status);
        }

        var data = await q.OrderByDescending(x => x.CreatedUtc)
            .Skip(skip)
            .Take(take)
            .AsAsyncEnumerable()
            .ToListAsync()
            .ConfigureAwait(false);

        return data;
    }
}
