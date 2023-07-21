using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public interface IUserAccountRepository
{
    Task CreateAsync(UserAccount userAccount);
    Task<UserAccount?> GetAsync(string id);
    Task InitialiseAsync();
    Task<IEnumerable<UserAccount>> ListAsync(bool? isLocked = false, int skip = 0, int take = 20);
    Task PatchAsync<T>(string id, string fieldName, T value);
    Task UpdateAsync(UserAccount userAccount);
}
