using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CsvHelper.Configuration.Attributes;
using UKMCAB.Data.Domain;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security.Tokens;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Areas.Home.Controllers;
using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Models.ViewModels.Account;
using UKMCAB.Web.UI.Models.ViewModels.Admin.User;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Areas.Account.Controllers;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("user-admin"), Authorize(Policy = Policies.UserManagement)]
public class UserAdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ISecureTokenProcessor _secureTokenProcessor;
    public static class Routes
    {
        public const string UserList = "user-admin.list";
        public const string UserListArchived = "user-admin.list.archived";
        public const string UserListLocked = "user-admin.list.locked";
        public const string UserAccount = "user-admin.user-account.view";
        public const string UserAccountEdit = "user-admin.user-account.edit";
        public const string UserAccountLock = "user-admin.user-account.lock";
        public const string UserAccountUnlock = "user-admin.user-account.unlock";
        public const string UserAccountArchive = "user-admin.user-account.archive";
        public const string UserAccountUnarchive = "user-admin.user-account.unarchive";
        public const string UserAccountRequestsList = "user-admin.account-requests.list";        
        public const string ReviewAccountRequest = "user-admin.review-account-request";
        public const string RequestApproved = "user-admin.request-approved";
        public const string RequestRejected = "user-admin.request-rejected";
        public const string RejectRequest = "user-admin.reject-request";
        public const string ApproveRequest = "user-admin.approve-request";
    }

    public UserAdminController(IUserService userService, ISecureTokenProcessor secureTokenProcessor)
    {
        _userService = userService;
        _secureTokenProcessor = secureTokenProcessor;
    }

    [HttpGet("list", Name = Routes.UserList)]
    public async Task<IActionResult> UserListAsync(int pageNumber = 1, [FromQuery(Name = "sf")] string? sortField = null, [FromQuery(Name = "sd")] string? sortDirection = null) 
        => await UserListAsync(pageNumber, false, null, "User accounts", sortField, sortDirection);

    [HttpGet("list/archived", Name = Routes.UserListArchived)]
    public async Task<IActionResult> UserListArchivedAsync(int pageNumber = 1, [FromQuery(Name = "sf")] string? sortField = null, [FromQuery(Name = "sd")] string? sortDirection = null)
        => await UserListAsync(pageNumber, true, UserAccountLockReason.Archived, "Archived user accounts", sortField, sortDirection);    
    
    [HttpGet("list/locked", Name = Routes.UserListLocked)]
    public async Task<IActionResult> UserListLockedAsync(int pageNumber = 1, [FromQuery(Name = "sf")] string? sortField = null, [FromQuery(Name = "sd")] string? sortDirection = null) 
        => await UserListAsync(pageNumber, true, UserAccountLockReason.None, "Locked user accounts", sortField, sortDirection);

    private async Task<IActionResult> UserListAsync(int page, bool isLocked, UserAccountLockReason? lockReason, string title, string? sortField = null, string? sortDirection = null)
    {
        var options = new UserAccountListOptions(SkipTake.FromPage(page-1,20), new SortBy(sortField, sortDirection ?? SortDirectionHelper.Descending), isLocked, lockReason, null);
        var accounts = await _userService.ListAsync(options);
        var pendingAccountsCount = await _userService.CountRequestsAsync(UserAccountRequestStatus.Pending);

        var viewName = string.Empty;
        if (title.Equals("User accounts"))
        {
            viewName = "UserList";
        }
        else if(title.Equals("Locked user accounts"))
        {
            viewName = "UserLockedList";
        }
        else if (title.Equals("Archived user accounts"))
        {
            viewName = "UserArchivedList";
        }

        return View(viewName, new UserAccountListViewModel
        {
            UserAccounts = accounts.ToList(),
            PendingAccountsCount = pendingAccountsCount,
            ActiveUsersCount = await _userService.UserCountAsync(null, false),
            ArchivedUsersCount = await _userService.UserCountAsync(UserAccountLockReason.Archived, true),
            LockedUsersCount = await _userService.UserCountAsync(UserAccountLockReason.None, true),
            Title = "User accounts",
            LockedOnly = isLocked,
            SortField = sortField ?? nameof(UserAccount.LastLogonUtc),
            SortDirection = sortDirection ?? SortDirectionHelper.Descending,
            Pagination = new PaginationViewModel
            {
                PageNumber = page,
                ResultsPerPage = 20,
                ResultType = string.Empty,
                Total = await _userService.UserCountAsync(lockReason, isLocked)
            }
        });
    }

    [HttpGet("{id}", Name = Routes.UserAccount)]
    public async Task<IActionResult> UserAccountAsync(string id, string? returnUrl, int pagenumber = 1)
    {
        var account = await _userService.GetAsync(id);
        if (account == null)
        {
            return NotFound();
        }
        else
        {
            return View(new UserAccountViewModel
            {
                UserAccount = account,
                IsMyOwnAccount = User.FindFirstValue(ClaimTypes.NameIdentifier) == account.Id,
                AuditLogHistoryViewModel = new AuditLogHistoryViewModel(account.AuditLog, pagenumber),
                ReturnURL = returnUrl
            });
        }
    }

    [HttpGet("{id}/edit", Name = Routes.UserAccountEdit)]
    public async Task<IActionResult> UserAccountEditAsync(string id, string? returnUrl)
    {
        Rule.IsFalse(User.FindFirstValue(ClaimTypes.NameIdentifier) == id, "User cannot edit their own account.");
        var account = await _userService.GetAsync(id);
        if (account == null)
        {
            return NotFound();
        }
        else
        {
            return View(new UserAccountEditViewModel
            {
                UserAccount = account,
                Email = account.ContactEmailAddress,
                Organisation = account.OrganisationName,
                UserGroup = account.RoleId,
                ReturnURL = returnUrl
            });
        }
    }

    [HttpPost("{id}/edit", Name = Routes.UserAccountEdit)]
    public async Task<IActionResult> UserAccountEditAsync(string id, UserAccountEditViewModel model)
    {
        Rule.IsFalse(User.FindFirstValue(ClaimTypes.NameIdentifier) == id, "User cannot edit their own account.");
        var account = await _userService.GetAsync(id);
        if (account == null)
        {
            return NotFound();
        }
        if(ModelState.IsValid)
        {
            if (!account.ContactEmailAddress.Equals(model.Email) ||
                !account.OrganisationName.Equals(model.Organisation) ||
                !account.RoleId.Equals(model.UserGroup))
            {
                var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var reviewer = await _userService.GetAsync(reviewerId);
                account.ContactEmailAddress = model.Email;
                account.OrganisationName = model.Organisation;
                account.RoleId = model.UserGroup;
                await _userService.UpdateUser(account, reviewer);
            }
            return RedirectToAction("UserAccount", new { id, returnUrl = model.ReturnURL });
        }

        model.UserAccount = account;
        return View(model);
    }



    [Route("{id}/lock", Name = Routes.UserAccountLock)]
    public async Task<IActionResult> UserAccountLockAsync(string id, [FromForm] UserAccountLockUnlockViewModel? model = null) => await UserAccountLockToggleAsync(UserAccountLockToggleUIMode.Lock, id, model);

    [Route("{id}/unlock", Name = Routes.UserAccountUnlock)]
    public async Task<IActionResult> UserAccountUnlockAsync(string id, [FromForm] UserAccountLockUnlockViewModel? model = null) => await UserAccountLockToggleAsync(UserAccountLockToggleUIMode.Unlock, id, model);

    [Route("{id}/archive", Name = Routes.UserAccountArchive)]
    public async Task<IActionResult> UserAccountArchiveAsync(string id, [FromForm] UserAccountLockUnlockViewModel? model = null) => await UserAccountLockToggleAsync(UserAccountLockToggleUIMode.Archive, id, model);

    [Route("{id}/unarchive", Name = Routes.UserAccountUnarchive)]
    public async Task<IActionResult> UserAccountUnarchiveAsync(string id, [FromForm] UserAccountLockUnlockViewModel? model = null) => await UserAccountLockToggleAsync(UserAccountLockToggleUIMode.Unarchive, id, model);

    private async Task<IActionResult> UserAccountLockToggleAsync(UserAccountLockToggleUIMode mode, string id, UserAccountLockUnlockViewModel? model)
    {
        const string ViewName = "UserAccountLockUnlock";
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reviewer = await _userService.GetAsync(reviewerId);

        if (Request.Method == HttpMethod.Get.Method)
        {
            ModelState.Clear();
            return View(ViewName, new UserAccountLockUnlockViewModel() { Mode = mode });
        }
        else if (Request.Method == HttpMethod.Post.Method && model != null)
        {
            model.Pipe(x => x.Mode = mode);
            if (ModelState.IsValid)
            {
                if (mode.EqualsAny(UserAccountLockToggleUIMode.Lock, UserAccountLockToggleUIMode.Archive))
                {
                    var reason = mode == UserAccountLockToggleUIMode.Archive ? UserAccountLockReason.Archived : UserAccountLockReason.None;
                    await _userService.LockAccountAsync(id, reviewer, reason, model.Reason, model.Notes).ConfigureAwait(false);
                }
                else
                {
                    var reason = mode == UserAccountLockToggleUIMode.Unarchive ? UserAccountUnlockReason.Unarchived : UserAccountUnlockReason.None;

                    await _userService.UnlockAccountAsync(id, reviewer, reason, model.Reason, model.Notes).ConfigureAwait(false);
                }

                return RedirectToRoute(HomeController.Routes.Message, new
                {
                    token = _secureTokenProcessor.Enclose(new MessageViewModel
                    {
                        Title = $"Account {model.VerbPastTenseForm}",
                        Heading = $"Account {model.VerbPastTenseForm}",
                        Body = $"You have {model.VerbPastTenseForm} the user account",
                        LinkLabel = "Continue",
                        LinkUrl = Url.RouteUrl(Routes.UserAccount, new { id }),
                        ViewName = "Panel"
                    })
                });
            }
            else
            {
                return View(ViewName, model);
            }
        }
        else
        {
            throw new DomainException("Either the http method was incorrect or the model was not set.");
        }
    }



    [HttpGet("account-requests", Name = Routes.UserAccountRequestsList)]
    public async Task<IActionResult> AccountRequestList(int pageNumber = 1, [FromQuery(Name = "sf")] string? sortField = null, [FromQuery(Name = "sd")] string? sortDirection = null)
    {
        var count = await _userService.CountRequestsAsync(UserAccountRequestStatus.Pending);
        var pendingAccounts = await _userService.ListRequestsAsync(new UserAccountRequestListOptions(SkipTake.FromPage(pageNumber - 1, 20), new SortBy(sortField, sortDirection ?? SortDirectionHelper.Descending), UserAccountRequestStatus.Pending));
        return View(new AccountRequestListViewModel
        {
            UserAccountRequests = pendingAccounts.ToList(),
            ActiveUsersCount = await _userService.UserCountAsync(null, false),
            ArchivedUsersCount = await _userService.UserCountAsync(UserAccountLockReason.Archived, true),
            LockedUsersCount = await _userService.UserCountAsync(UserAccountLockReason.None, true),
            Pagination = new PaginationViewModel
            {
                Total = count,
                PageNumber = pageNumber,
                ResultType = string.Empty,
                ResultsPerPage = 20
            },
            SortField = sortField ?? nameof(UserAccountRequest.CreatedUtc),
            SortDirection = sortDirection ?? SortDirectionHelper.Descending,
        });
    }


    [HttpGet("review-account-request/{id}", Name = Routes.ReviewAccountRequest)]
    public async Task<IActionResult> ReviewAccountRequest(string id)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToRoute(Routes.UserList);
        }

        return View(new ReviewAccountRequestViewModel
        {
            UserAccountRequest = account
        });
    }


    [HttpPost("review-account-request/{id}", Name = Routes.ReviewAccountRequest)]
    public async Task<IActionResult> ReviewAccountRequest(string id, string submitType, [FromForm] string? role = null)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToRoute(Routes.UserAccountRequestsList);
        }

        if (submitType == Constants.SubmitType.Approve)
        {
            if (role.IsNullOrEmpty())
            {
                ModelState.AddModelError("", "Select a user group");
                return await ReviewAccountRequest(id);
            }
            else
            {
                return RedirectToRoute(Routes.ApproveRequest, new { account.Id, role });
            }
        }
        return RedirectToRoute(Routes.RejectRequest, new { account.Id });
    }

    [HttpGet("approve-request/{id}", Name = Routes.ApproveRequest)]
    public async Task<IActionResult> ApproveRequest(string id, [FromQuery] string role)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToRoute(Routes.UserList);
        }
        return View(new ApproveRequestViewModel
        {
            AccountId = account.Id,
            Role = role,
            FirstName = account.FirstName,
            LastName = account.Surname,
        });
    }

    [HttpPost("approve-request/{id}", Name = Routes.ApproveRequest)]
    public async Task<IActionResult> ApproveRequestPost(string id, [FromQuery] string role)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToRoute(Routes.UserList);
        }
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reviewer = await _userService.GetAsync(reviewerId);

        await _userService.ApproveAsync(account.Id, role, reviewer); // todo!!!!!!!!!!!!!! add role in service

        return RedirectToRoute(Routes.UserAccountRequestsList); //RedirectToRoute(Routes.RequestApproved, new { account.Id });
    }

    [HttpGet("request-approved/{id}", Name = Routes.RequestApproved)]
    public async Task<IActionResult> RequestApproved(string id)
    {
        var model = new BasicPageModel
        {
            Title = "Request approved"
        };
        return View(model);
    }

    [HttpGet("decline-request/{id}", Name = Routes.RejectRequest)]
    public async Task<IActionResult> DeclineRequest(string id)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToRoute(Routes.UserList);
        }
        return View(new DeclineRequestViewModel
        {
            AccountId = account.Id,
            FirstName = account.FirstName,
            LastName = account.Surname
        });
    }

    [HttpPost("decline-request/{id}", Name = Routes.RejectRequest)]
    public async Task<IActionResult> DeclineRequest(string id, DeclineRequestViewModel model)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToRoute(Routes.UserList);
        }

        if (ModelState.IsValid)
        {
            var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reviewer = await _userService.GetAsync(reviewerId);
            await _userService.RejectAsync(account.Id, model.Reason ?? string.Empty, reviewer);
            return RedirectToRoute(Routes.UserAccountRequestsList);
        }

        model.AccountId = id;
        model.FirstName = account.FirstName;
        model.LastName = account.Surname;
        return View(model);
    }

    [HttpGet("request-rejected/{id}", Name = Routes.RequestRejected)]
    public async Task<IActionResult> RequestRejected(string id)
    {
        var model = new BasicPageModel
        {
            Title = "Request rejected"
        };
        return View(model);
    }
}
