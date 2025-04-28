using Polly;
using Polly.Fallback;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.PostgreSQL.Services.User;

public class PostgreUserAccountRequestRepository : IUserAccountRequestRepository
{
    private readonly ApplicationDataContext _dbContext;
    private readonly AsyncFallbackPolicy<UserAccountRequest?> _getUserAccountRequestPolicy;

    public PostgreUserAccountRequestRepository(ApplicationDataContext dbContext)
    {
        _dbContext = dbContext;
        _getUserAccountRequestPolicy = Policy<UserAccountRequest?>.Handle<Exception>().FallbackAsync(null as UserAccountRequest);
    }

    public async Task<int> CountAsync(UserAccountRequestStatus? status = null)
    {
        var q = _dbContext.UserAccountRequests.AsQueryable();
        if (status.HasValue)
        {
            q = q.Where(x => x.Status == status);
        }
        var rv = q.Count();
        return rv;
    }

    public async Task CreateAsync(UserAccountRequest userAccount)
    {
        await _dbContext.AddAsync(userAccount);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserAccountRequest?> GetAsync(string id)
    {
        var retVal = await _getUserAccountRequestPolicy.ExecuteAsync(async () =>
            _dbContext.UserAccountRequests.FirstOrDefault(x => x.Id == id)).ConfigureAwait(false);
        return retVal;
    }

    public async Task<UserAccountRequest?> GetPendingAsync(string subjectId)
    {
        var item = _dbContext.UserAccountRequests.FirstOrDefault(x =>
            x.SubjectId == subjectId &&
            x.Status == UserAccountRequestStatus.Pending
        );
        return item;
    }

    public Task InitialiseAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestListOptions options)
    {
        var query = _dbContext.UserAccountRequests.AsQueryable();
        if (options.Status.HasValue)
        {
            query = query.Where(x => x.Status == options.Status);
        }

        var set = (await query.AsAsyncEnumerable().ToListAsync()).AsQueryable();

        var data = set.OrderBy(i => i.CreatedUtc)
            .Skip(options.SkipTake.Skip)
            .Take(options.SkipTake.Take)
            .ToList();

        return data;
    }

    public async Task UpdateAsync(UserAccountRequest userAccount)
    {
        _dbContext.UserAccountRequests.Update(userAccount);
        await _dbContext.SaveChangesAsync();
    }
}
