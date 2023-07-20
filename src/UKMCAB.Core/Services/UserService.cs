using UKMCAB.Common;
using UKMCAB.Data.CosmosDb.Services.User;

namespace UKMCAB.Core.Services;

public enum UserAccountStatus
{
    /// <summary>
    /// No user account and no pending user account request
    /// </summary>
    Unknown,
    PendingUserAccountRequest,
    UserAccountLocked,
    Active,
}

public class RequestAccountModel
{
    public string SubjectId { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Organisation { get; set; } = null!;
    public string ContactEmailAddress { get; set; } = null!;
    public string Comments { get; set; } = null!;
}

public interface IUserService
{
    Task<UserService.UserStatus> GetUserAccountStatusAsync(string id);
    Task SubmitRequestAccountAsync(RequestAccountModel model);
}

public class UserService : IUserService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserAccountRequestRepository _userAccountRequestRepository;

    public UserService(IUserAccountRepository userAccountRepository, IUserAccountRequestRepository userAccountRequestRepository)
    {
        _userAccountRepository = userAccountRepository;
        _userAccountRequestRepository = userAccountRequestRepository;
    }

    public record UserStatus(UserAccountStatus Status, UserAccountLockReason? UserAccountLockReason = null);

    public async Task<UserStatus> GetUserAccountStatusAsync(string id)
    {
        var account = await _userAccountRepository.GetAsync(id);
        if (account == null)
        {
            var pendingRequest = await _userAccountRequestRepository.GetPendingAsync(id).ConfigureAwait(false);
            if (pendingRequest != null)
            {
                return new UserStatus(UserAccountStatus.PendingUserAccountRequest);
            }
            else
            {
                return new UserStatus(UserAccountStatus.Unknown);
            }
        }
        else if (account.IsLocked)
        {
            return new UserStatus(UserAccountStatus.UserAccountLocked, account.LockReason);
        }
        else
        {
            return new UserStatus(UserAccountStatus.Active);
        }
    }

    public async Task SubmitRequestAccountAsync(RequestAccountModel model)
    {
        var pendingRequest = await _userAccountRequestRepository.GetPendingAsync(model.SubjectId).ConfigureAwait(false);
        Rule.IsTrue(pendingRequest == null, "There is already a pending user account request. You will be emailed once it has been reviewed.");
        await _userAccountRequestRepository.CreateAsync(new UserAccountRequest
        {
            SubjectId = model.SubjectId,
            ContactEmailAddress = model.ContactEmailAddress,
            EmailAddress = model.EmailAddress,
            FirstName = model.FirstName,
            OrganisationName = model.Organisation,
            Surname = model.Surname,
            Comments = model.Comments,
        });
    }
}
