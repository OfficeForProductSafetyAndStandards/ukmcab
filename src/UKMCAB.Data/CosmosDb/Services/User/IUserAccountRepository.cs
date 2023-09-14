using UKMCAB.Common.Domain;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public interface IUserAccountRepository
{
    Task CreateAsync(UserAccount userAccount);
    Task<UserAccount?> GetAsync(string id);
    Task InitialiseAsync();
    Task<int> UserCountAsync(int? lockReason = null, bool locked = false);
    Task<IEnumerable<UserAccount>> ListAsync(UserAccountListOptions options);
    Task PatchAsync<T>(string id, string fieldName, T value);
    Task UpdateAsync(UserAccount userAccount);
}
