using UKMCAB.Data.Domain;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.CosmosDb.Services.User;

public interface IUserAccountRequestRepository
{
    System.Threading.Tasks.Task CreateAsync(UserAccountRequest userAccount);
    Task<UserAccountRequest?> GetPendingAsync(string subjectId);
    System.Threading.Tasks.Task UpdateAsync(UserAccountRequest userAccount);
    System.Threading.Tasks.Task InitialiseAsync();
    Task<UserAccountRequest?> GetAsync(string id);
    Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestListOptions options);
    Task<int> CountAsync(UserAccountRequestStatus? status = null);
}
