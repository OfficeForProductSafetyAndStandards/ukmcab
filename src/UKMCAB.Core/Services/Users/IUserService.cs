using UKMCAB.Core.Services.Users.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Services.Users;

public interface IUserService
{
    /// <summary>
    /// Approves a user account request
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task ApproveAsync(string id);
    Task<UserAccount?> GetAsync(string id);

    Task<UserAccountRequest?> GetAccountRequestAsync(string id);

    /// <summary>
    /// Gets the status of a user account / user account request
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<UserService.UserStatus> GetUserAccountStatusAsync(string id);

    /// <summary>
    /// Lists user accounts
    /// </summary>
    /// <param name="isLocked"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<IEnumerable<UserAccount>> ListAsync(bool? isLocked = false, int skip = 0, int take = 20);

    /// <summary>
    /// Lists user account requests
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<IEnumerable<UserAccountRequest>> ListPendingAccountRequestsAsync(int skip = 0, int take = 20);

    /// <summary>
    /// Rejects a user account request
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task RejectAsync(string id);

    /// <summary>
    /// Submits a user account request
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task SubmitRequestAccountAsync(RequestAccountModel model);

    /// <summary>
    /// Updates the last logon date to the current date time utc for the user account whose id matches the supplied id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task UpdateLastLogonDate(string id);
}
