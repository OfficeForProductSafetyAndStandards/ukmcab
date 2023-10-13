using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public interface IUserAccountRequestRepository
{
    Task CreateAsync(UserAccountRequest userAccount);
    Task<UserAccountRequest?> GetPendingAsync(string subjectId);
    Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestStatus? status, int skip = 0, int take = 20);
    Task UpdateAsync(UserAccountRequest userAccount);
    Task InitialiseAsync();
    Task<UserAccountRequest?> GetAsync(string id);
}
