using Polly;
using Polly.Fallback;
using UKMCAB.Common;
using UKMCAB.Common.Exceptions;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.PostgreSQL.Services.User;

public class PostgreUserAccountRepository : IUserAccountRepository
{
    private readonly ApplicationDataContext _dbContext;
    private readonly AsyncFallbackPolicy<UserAccount?> _getUserAccountPolicy;

    public PostgreUserAccountRepository(ApplicationDataContext dbContext)
    {
        _dbContext = dbContext;
        _getUserAccountPolicy = Policy<UserAccount?>.Handle<NotFoundException>().FallbackAsync(null as UserAccount);
    }

    public async Task CreateAsync(UserAccount userAccount)
    {
        await _dbContext.AddAsync(userAccount);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserAccount?> GetAsync(string id)
    {
        var retVal = await _getUserAccountPolicy.ExecuteAsync(async () =>
            _dbContext.UserAccounts.FirstOrDefault(x => x.Id == id)).ConfigureAwait(false);
        return retVal;
    }

    public async Task InitialiseAsync()
    { }

    public async Task<IEnumerable<UserAccount>> ListAsync(UserAccountListOptions options)
    {
        var q = (await _dbContext.UserAccounts.AsAsyncEnumerable().ToListAsync()).AsQueryable();

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

        var data = q.OrderBy(i => i.LastLogonUtc)
            .Skip(options.SkipTake.Skip)
            .Take(options.SkipTake.Take)
            .ToList();

        return data;
    }

    public async Task PatchAsync<T>(string id, string fieldName, T value)
    {
        var userAccount = await _dbContext.UserAccounts.FindAsync(id);
        if (userAccount == null)
        {
            throw new KeyNotFoundException($"UserAccount with ID '{id}' not found.");
        }

        var property = typeof(Models.Users.UserAccount).GetProperty(fieldName);
        if (property == null || !property.CanWrite)
        {
            throw new ArgumentException($"Field '{fieldName}' is not valid or not writable.");
        }

        property.SetValue(userAccount, value);

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserAccount userAccount)
    {
        _dbContext.UserAccounts.Update(userAccount);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> UserCountAsync(UserAccountLockReason? lockReason = null, bool locked = false)
    {
        if (lockReason == null)
        {
            return _dbContext.UserAccounts.AsQueryable().Where(x => x.IsLocked == locked).Count();
        }
        else
        {
            return _dbContext.UserAccounts.AsQueryable().Where(x => x.IsLocked == locked && x.LockReason == (UserAccountLockReason)lockReason).Count();
        }
    }
}
