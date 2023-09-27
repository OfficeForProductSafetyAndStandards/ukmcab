using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common;
using UKMCAB.Data.Domain;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users.Models;
using UKMCAB.Data.CosmosDb.Services.User;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Services.Users;

public class UserService : IUserService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserAccountRequestRepository _userAccountRequestRepository;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly IOptions<CoreEmailTemplateOptions> _templateOptions;

    public UserService(IUserAccountRepository userAccountRepository, IUserAccountRequestRepository userAccountRequestRepository, 
        IAsyncNotificationClient notificationClient, IOptions<CoreEmailTemplateOptions> templateOptions)
    {
        _userAccountRepository = userAccountRepository;
        _userAccountRequestRepository = userAccountRequestRepository;
        _notificationClient = notificationClient;
        _templateOptions = templateOptions;
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
            AuditLog = new List<Audit>
            {
                new Audit(model.SubjectId, $"{model.FirstName} {model.Surname}", string.Empty, DateTime.UtcNow, AuditUserActions.UserAccountRequest, model.Comments)
            }
        });
    }

    public Task<int> UserCountAsync(UserAccountLockReason? lockedReason = null, bool locked = false) => _userAccountRepository.UserCountAsync(lockedReason, locked);

    /// <inheritdoc />
    public Task<IEnumerable<UserAccount>> ListAsync(UserAccountListOptions options) 
        => _userAccountRepository.ListAsync(options);

    /// <inheritdoc />
    public async Task<IEnumerable<UserAccountRequest>> ListPendingAccountRequestsAsync(int skip = 0, int take = 20) 
        => await _userAccountRequestRepository.ListAsync(UserAccountRequestStatus.Pending, skip, take);

    /// <inheritdoc />
    public async Task<UserAccount?> GetAsync(string id) 
        => await _userAccountRepository.GetAsync(id).ConfigureAwait(false);

    public async Task<UserAccountRequest?> GetAccountRequestAsync(string id) 
        => await _userAccountRequestRepository.GetAsync(id).ConfigureAwait(false);


    /// <inheritdoc />
    public async Task ApproveAsync(string id, string role, UserAccount reviewer) => await ApproveRejectAsync(id, UserAccountRequestStatus.Approved, reviewer, role: role).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task RejectAsync(string id, string reason, UserAccount reviewer) => await ApproveRejectAsync(id, UserAccountRequestStatus.Rejected, reviewer, reason).ConfigureAwait(false);
    
    private async Task ApproveRejectAsync(string id, UserAccountRequestStatus status, UserAccount reviewer, string? reviewComments = null, string? role = null)
    {
        Guard.IsFalse(status == UserAccountRequestStatus.Pending, "You cannot assign a status of Pending");
        var request = await _userAccountRequestRepository.GetAsync(id).ConfigureAwait(false) ?? throw new NotFoundException("The user account request could not be found");
        Rule.IsTrue(request.Status == UserAccountRequestStatus.Pending, $"The request has already been reviewed and was {request.Status.ToString().ToLower()}");
        request.Status = status;
        request.ReviewComments = reviewComments;
        await _userAccountRequestRepository.UpdateAsync(request).ConfigureAwait(false);

        var account = await _userAccountRepository.GetAsync(request.SubjectId).ConfigureAwait(false);
        var audit = new Audit
        {
            DateTime = DateTime.UtcNow,
            UserName = $"{reviewer.FirstName} {reviewer.Surname}",
            UserId = reviewer.Id,
            UserRole = reviewer.Role,
            Comment = reviewComments
        };
        
        if (status == UserAccountRequestStatus.Approved)
        {
            Rule.IsTrue(role.EqualsAny(Roles.List.Select(x => x.Id).ToArray()), $"The supplied role id ('{role}') does not match any of the configured roles");


            if (account != null) // there's already an account for this fella
            {
                if (account.IsLocked)
                {
                    account.IsLocked = false;
                    account.LockReason = null;
                    account.Role = role;
                    await _userAccountRepository.UpdateAsync(account).ConfigureAwait(false);
                }
                // else: //noop - they already have an unlocked account anywayz.... 
            }
            else
            {
                audit.Action = AuditUserActions.ApproveAccountRequest;
                account = new UserAccount
                {
                    Id = request.SubjectId,
                    ContactEmailAddress = request.ContactEmailAddress,
                    EmailAddress = request.EmailAddress,
                    FirstName = request.FirstName,
                    OrganisationName = request.OrganisationName,
                    Surname = request.Surname,
                    Role = role,
                    AuditLog =request.AuditLog != null && request.AuditLog.Any() ? request.AuditLog : new List<Audit>(),
                };
                account.AuditLog.Add(audit);
                await _userAccountRepository.CreateAsync(account).ConfigureAwait(false);
            }

            var personalisation = new Dictionary<string, dynamic>
            {
                { "user-group", Roles.NameFor(role) ?? string.Empty}
            };
            await _notificationClient.SendEmailAsync(account.GetEmailAddress(), _templateOptions.Value.AccountRequestApproved, personalisation);
        }
        else
        {
            audit.Action = AuditUserActions.DeclineAccountRequest;
            if (request.AuditLog == null)
            {
                request.AuditLog = new List<Audit>();
            }
            request.AuditLog.Add(audit);
            await _userAccountRequestRepository.UpdateAsync(request);

            var personalisation = new Dictionary<string, dynamic>
            {
                {"rejection-reason", reviewComments ?? "" }
            };
            await _notificationClient.SendEmailAsync(request.GetEmailAddress(), _templateOptions.Value.AccountRequestRejected, personalisation);
        }
    }

    /// <inheritdoc />
    public async Task UpdateLastLogonDate(string id) => await _userAccountRepository.PatchAsync(id, UserAccount.LastLogonUtcFieldName, DateTime.UtcNow).ConfigureAwait(false);

    public async Task LockAccountAsync(string id, UserAccount reviewer, UserAccountLockReason reason, string? reasonDescription, string? internalNotes)
    {
        var account = await _userAccountRepository.GetAsync(id).ConfigureAwait(false);
        if (account != null) 
        {
            LockAccount(account, reason, reasonDescription, internalNotes);
            if (account.AuditLog == null)
            {
                account.AuditLog = new List<Audit>();
            }
            account.AuditLog.Add(new Audit
            {
                DateTime = DateTime.UtcNow,
                UserId = reviewer.Id,
                UserName = $"{reviewer.FirstName} {reviewer.Surname}",
                UserRole = reviewer.Role,
                Action =reason == UserAccountLockReason.Archived ? AuditUserActions.ArchiveAccountRequest : AuditUserActions.LockAccountRequest,
                Comment = internalNotes
            });
            await _userAccountRepository.UpdateAsync(account).ConfigureAwait(false);

            if(reason == UserAccountLockReason.Archived)
            {
                await _notificationClient.SendEmailAsync(account.GetEmailAddress(), _templateOptions.Value.AccountArchived);
            }
            else
            {
                await _notificationClient.SendEmailAsync(account.GetEmailAddress(), _templateOptions.Value.AccountLocked, new() { ["reason"] = reasonDescription });
            }
        }
        else
        {
            throw new NotFoundException($"User account for id '{id}' was not found");
        }
    }

    private void LockAccount(UserAccount account, UserAccountLockReason reason, string? reasonDescription, string? internalNotes)
    {
        if (!account.IsLocked)
        {
            account.IsLocked = true;
            account.LockReason = reason;
            account.LockReasonDescription = reasonDescription;
            account.LockInternalNotes = internalNotes;
        }
        else
        {
            throw new DomainException("The account is already locked");
        }
    }

    public async Task UnlockAccountAsync(string id, UserAccount reviewer, UserAccountUnlockReason reason, string? reasonDescription, string? internalNotes)
    {
        var account = await _userAccountRepository.GetAsync(id).ConfigureAwait(false);
        if (account != null)
        {
            var oldReason = UnlockAccount(account);
            if (account.AuditLog == null)
            {
                account.AuditLog = new List<Audit>();
            }
            account.AuditLog.Add(new Audit
            {
                DateTime = DateTime.UtcNow,
                UserId = reviewer.Id,
                UserName = $"{reviewer.FirstName} {reviewer.Surname}",
                UserRole = reviewer.Role,
                Action = reason == UserAccountUnlockReason.Unarchived ? AuditUserActions.UnarchiveAccountRequest : AuditUserActions.UnlockAccountRequest,
                Comment = internalNotes
            });
            await _userAccountRepository.UpdateAsync(account).ConfigureAwait(false);

            if (oldReason == UserAccountLockReason.Archived)
            {
                await _notificationClient.SendEmailAsync(account.GetEmailAddress(), _templateOptions.Value.AccountUnarchived);
            }
            else
            {
                await _notificationClient.SendEmailAsync(account.GetEmailAddress(), _templateOptions.Value.AccountUnlocked, new() { ["reason"] = reasonDescription });
            }
            //todo: record audit trail
        }
        else
        {
            throw new NotFoundException($"User account for id '{id}' was not found");
        }
    }

    private UserAccountLockReason UnlockAccount(UserAccount account)
    {
        if (account.IsLocked)
        {
            var rv = account.LockReason ?? throw new Exception("There must always be a lock reason if the account is locked");
            account.IsLocked = false;
            account.LockReason = null;
            account.LockReasonDescription = null;
            account.LockInternalNotes = null;
            return rv;
        }
        else
        {
            throw new DomainException("The account is already locked");
        }
    }
    
    public async Task UpdateUser(UserAccount user, UserAccount reviewer)
    {
        var existingAccount = await GetAsync(user.Id);
        var auditDate = DateTime.UtcNow;
        if (user.AuditLog == null)
        {
            user.AuditLog = new List<Audit>();
        }

        if (!existingAccount.ContactEmailAddress.Equals(user.ContactEmailAddress))
        {
            user.AuditLog.Add(new Audit
            {
                DateTime = auditDate,
                UserId = reviewer.Id,
                UserName = $"{reviewer.FirstName} {reviewer.Surname}",
                UserRole = reviewer.Role,
                Action = AuditUserActions.ChangeOfContactEmailAddress
            });
        }
        if (!existingAccount.OrganisationName.Equals(user.OrganisationName))
        {
            user.AuditLog.Add(new Audit
            {
                DateTime = auditDate,
                UserId = reviewer.Id,
                UserName = $"{reviewer.FirstName} {reviewer.Surname}",
                UserRole = reviewer.Role,
                Action = AuditUserActions.ChangeOfOrganisation
            });
        }
        if (!existingAccount.Role.Equals(user.Role))
        {
            user.AuditLog.Add(new Audit
            {
                DateTime = auditDate,
                UserId = reviewer.Id,
                UserName = $"{reviewer.FirstName} {reviewer.Surname}",
                UserRole = reviewer.Role,
                Action = AuditUserActions.ChangeOfRole
            });
        }

        await _userAccountRepository.UpdateAsync(user).ConfigureAwait(false);
    } 
}
