using UKMCAB.Data.Domain;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public interface IUserAccountRepository
{
    System.Threading.Tasks.Task CreateAsync(UserAccount userAccount);
    Task<UserAccount?> GetAsync(string id);
    System.Threading.Tasks.Task InitialiseAsync();
    Task<int> UserCountAsync(UserAccountLockReason? lockReason = null, bool locked = false);
    Task<IEnumerable<UserAccount>> ListAsync(UserAccountListOptions options);
    System.Threading.Tasks.Task PatchAsync<T>(string id, string fieldName, T value);
    System.Threading.Tasks.Task UpdateAsync(UserAccount userAccount);
}
