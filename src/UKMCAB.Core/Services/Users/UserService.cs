using UKMCAB.Common;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Services.Users.Models;
using UKMCAB.Data.CosmosDb.Services.User;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Services.Users;

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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task<IEnumerable<UserAccount>> ListAsync(bool? isLocked = false, int skip = 0, int take = 20)
    {
        return _userAccountRepository.ListAsync(isLocked, skip, take);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserAccountRequest>> ListPendingAccountRequestsAsync(int skip = 0, int take = 20)
    {
        return await _userAccountRequestRepository.ListAsync(UserAccountRequestStatus.Pending, skip, take);
    }

    /// <inheritdoc />
    public async Task<UserAccount?> GetAsync(string id) => await _userAccountRepository.GetAsync(id).ConfigureAwait(false);

    public async Task<UserAccountRequest?> GetAccountRequestAsync(string id) => await _userAccountRequestRepository.GetAsync(id).ConfigureAwait(false);


    /// <inheritdoc />
    public async Task ApproveAsync(string id) => await ApproveRejectAsync(id, UserAccountRequestStatus.Approved).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task RejectAsync(string id) => await ApproveRejectAsync(id, UserAccountRequestStatus.Rejected).ConfigureAwait(false);
    
    private async Task ApproveRejectAsync(string id, UserAccountRequestStatus status)
    {
        Guard.IsFalse(status == UserAccountRequestStatus.Pending, "You cannot assign a status of Pending");
        var request = await _userAccountRequestRepository.GetAsync(id).ConfigureAwait(false) ?? throw new NotFoundException("The user account request could not be found");
        Rule.IsTrue(request.Status == UserAccountRequestStatus.Pending, $"The request has already been reviewed ({request.Status})");
        request.Status = status;
        await _userAccountRequestRepository.UpdateAsync(request).ConfigureAwait(false);

        if (status == UserAccountRequestStatus.Approved)
        {
            var account = await _userAccountRepository.GetAsync(request.SubjectId).ConfigureAwait(false);
            if (account != null) // there's already an account for this fella
            {
                if (account.IsLocked)
                {
                    account.IsLocked = false;
                    account.LockReason = null;
                }
                // else: //noop - they already have an unlocked account anywayz.... 
            }
            else
            {
                account = new UserAccount
                {
                    Id = request.SubjectId,
                    ContactEmailAddress = request.ContactEmailAddress,
                    EmailAddress = request.EmailAddress,
                    FirstName = request.FirstName,
                    OrganisationName = request.OrganisationName,
                    Surname = request.Surname,
                };
                await _userAccountRepository.CreateAsync(account).ConfigureAwait(false);

                //todo: email the person using 'ContactEmailAddress'
            }
        }
    }

    /// <inheritdoc />
    public async Task UpdateLastLogonDate(string id) => await _userAccountRepository.PatchAsync(id, UserAccount.LastLogonUtcFieldName, DateTime.UtcNow).ConfigureAwait(false);
}
