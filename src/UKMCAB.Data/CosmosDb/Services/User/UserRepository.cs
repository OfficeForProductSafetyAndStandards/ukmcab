using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Polly;
using Polly.Fallback;
using System.Text.Json;
using UKMCAB.Common.ConnectionStrings;

namespace UKMCAB.Data.CosmosDb.Services.User;

public enum UserAccountLockReason
{
    Archived,
    Other,
    None
}

public class UserAccount
{
    public string Id { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? Surname { get; set; }
    public string? EmailAddress { get; set; }
    public string? OrganisationName { get; set; }
    public string? ContactEmailAddress { get; set; }
    public bool IsLocked { get; set; }
    public UserAccountLockReason? LockReason { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}

public interface IUserAccountRepository
{
    Task CreateAsync(UserAccount userAccount);
    Task<UserAccount?> GetAsync(string id);
    Task InitialiseAsync();
    Task<IEnumerable<UserAccount>> ListAsync(bool? isLocked = false, int skip = 0, int take = 20);
    Task UpdateAsync(UserAccount userAccount);
}

public class UserAccountRepository : IUserAccountRepository
{
    public const string ContainerId = "user-accounts";
    private readonly Container _container;
    private readonly AsyncFallbackPolicy<UserAccount?> _getUserAccountPolicy;

    public UserAccountRepository(CosmosDbConnectionString cosmosDbConnectionString)
    {
        var client = CosmosClientFactory.Create(cosmosDbConnectionString);
        _container = client.GetContainer(DataConstants.CosmosDb.Database, ContainerId);
        _getUserAccountPolicy = Policy<UserAccount?>.Handle<CosmosException>(x => x.StatusCode == System.Net.HttpStatusCode.NotFound).FallbackAsync(null as UserAccount);
    }

    public async Task InitialiseAsync() => await _container.Database.CreateContainerIfNotExistsAsync(ContainerId, "/id");

    public async Task<UserAccount?> GetAsync(string id)
    {
        var retVal = await _getUserAccountPolicy.ExecuteAsync(async () => await _container.ReadItemAsync<UserAccount>(id, PartitionKey.None));
        return retVal;
    }

    public async Task CreateAsync(UserAccount userAccount)
    {
        await _container.CreateItemAsync(userAccount);
    }

    public async Task UpdateAsync(UserAccount userAccount)
    {
        await _container.ReplaceItemAsync(userAccount, userAccount.Id);
    }

    public async Task<IEnumerable<UserAccount>> ListAsync(bool? isLocked = false, int skip = 0, int take = 20)
    {
        var q = _container.GetItemLinqQueryable<UserAccount>().AsQueryable();

        if (isLocked.HasValue)
        {
            q = q.Where(x => x.IsLocked == isLocked);
        }

        var data = await q.OrderBy(x => x.Surname)
            .Skip(skip)
            .Take(take)
            .AsAsyncEnumerable()
            .ToListAsync();

        return data;
    }
}




public enum UserAccountRequestStatus
{
    Pending,
    Rejected,
    Approved,
}


public class UserAccountRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SubjectId { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? Surname { get; set; }
    public string? EmailAddress { get; set; }
    public string? OrganisationName { get; set; }
    public string? ContactEmailAddress { get; set; }
    public string? Comments { get; set; }
    public UserAccountRequestStatus Status { get; set; } = UserAccountRequestStatus.Pending;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}

public interface IUserAccountRequestRepository
{
    Task CreateAsync(UserAccountRequest userAccount);
    Task<UserAccountRequest?> GetPendingAsync(string subjectId);
    Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestStatus? status, int skip = 0, int take = 20);
    Task UpdateAsync(UserAccountRequest userAccount);
    Task InitialiseAsync();
}

public class UserAccountRequestRepository : IUserAccountRequestRepository
{
    public const string ContainerId = "user-account-requests";
    private readonly Container _container;

    public UserAccountRequestRepository(CosmosDbConnectionString cosmosDbConnectionString)
    {
        var client = CosmosClientFactory.Create(cosmosDbConnectionString);
        _container = client.GetContainer(DataConstants.CosmosDb.Database, ContainerId);
    }

    public async Task InitialiseAsync() => await _container.Database.CreateContainerIfNotExistsAsync(ContainerId, "/id");

    public async Task<UserAccountRequest?> GetPendingAsync(string subjectId)
    {
        var item = await _container.GetItemLinqQueryable<UserAccountRequest>().AsAsyncEnumerable().FirstOrDefaultAsync(x => x.SubjectId == subjectId && x.Status == UserAccountRequestStatus.Pending);
        return item;
    }

    public async Task CreateAsync(UserAccountRequest userAccount)
    {
        await _container.CreateItemAsync(userAccount);
    }

    public async Task UpdateAsync(UserAccountRequest userAccount)
    {
        await _container.ReplaceItemAsync(userAccount, userAccount.Id);
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
            .ToListAsync();

        return data;
    }
}
