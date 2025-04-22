using UKMCAB.Data.Domain;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Interfaces.Services.User;

public interface IUserAccountRequestRepository
{
    Task CreateAsync(UserAccountRequest userAccount);
    Task<UserAccountRequest?> GetPendingAsync(string subjectId);
    Task UpdateAsync(UserAccountRequest userAccount);
    Task InitialiseAsync();
    Task<UserAccountRequest?> GetAsync(string id);
    Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestListOptions options);
    Task<int> CountAsync(UserAccountRequestStatus? status = null);
}
