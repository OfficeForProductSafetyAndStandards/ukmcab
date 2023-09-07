using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Common.Domain;
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

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("user-admin"), Authorize(Policy = Policies.UserManagement)]
public class UserAdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ISecureTokenProcessor _secureTokenProcessor;
    public static class Routes
    {
        public const string UserList = "user-admin.list";
        public const string UserListLocked = "user-admin.list.locked";
        public const string UserAccount = "user-admin.user-account.view";
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
    public async Task<IActionResult> UserListAsync(int pageNumber = 1) => await UserListAsync(pageNumber, false, "User accounts");

    [HttpGet("list/locked", Name = Routes.UserListLocked)]
    public async Task<IActionResult> UserListLockedAsync(int pageNumber = 1) => await UserListAsync(pageNumber, true, "Locked/archived user accounts");

    private async Task<IActionResult> UserListAsync(int page, bool isLocked, string title)
    {
        const int take = 20;
        var pageIndex = page - 1;
        var skip = pageIndex * take;

        var accounts = await _userService.ListAsync(new UserAccountListOptions(Skip: skip, Take: take, IsLocked: isLocked, ExcludeId: User.FindFirstValue(ClaimTypes.NameIdentifier)));
        var pendingAccounts = await GetAllPendingRequests();
        return View("UserList", new UserAccountListViewModel
        {
            UserAccounts = accounts.ToList(),
            PendingAccountsCount = pendingAccounts.Count,
            Title = title,
            LockedOnly = isLocked,
            Pagination = new PaginationViewModel
            {
                PageNumber = page,
                ResultsPerPage = 20,
                ResultType = string.Empty,
                Total = await _userService.UserCountAsync(isLocked)
            }
        });
    }

    [HttpGet("{id}", Name = Routes.UserAccount)]
    public async Task<IActionResult> UserAccountAsync(string id)
    {
        Rule.IsTrue(id != User.FindFirstValue(ClaimTypes.NameIdentifier), "One cannot manage one's own profile");
        var account = await _userService.GetAsync(id);
        if (account == null)
        {
            return NotFound();
        }
        else
        {
            return View(new UserAccountViewModel
            {
                UserAccount = account
            });
        }
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
                    await _userService.LockAccountAsync(id, reason, model.Reason, model.Notes).ConfigureAwait(false);
                }
                else
                {
                    await _userService.UnlockAccountAsync(id, model.Reason, model.Notes).ConfigureAwait(false);
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
    public async Task<IActionResult> AccountRequestList(int pageNumber = 1)
    {
        var pendingAccounts = await GetAllPendingRequests();
        var total = pendingAccounts.Count;
        var skip = pageNumber - 1;
        if (skip * 20 >= total)
        {
            skip = 0;
        }

        pendingAccounts = pendingAccounts.OrderByDescending(pa => pa.CreatedUtc).Skip(skip).Take(20).ToList();

        return View(new AccountRequestListViewModel
        {
            UserAccountRequests = pendingAccounts,
            Pagination = new PaginationViewModel
            {
                Total = total,
                PageNumber = pageNumber,
                ResultType = string.Empty,
                ResultsPerPage = 20
            }
        });
    }

    private async Task<List<UserAccountRequest>> GetAllPendingRequests()
    {
        var list = new List<UserAccountRequest>();
        var request = await _userService.ListPendingAccountRequestsAsync();
        list.AddRange(request);
        var page = 1;
        while (request.Count() == 20)
        {
            request = await _userService.ListPendingAccountRequestsAsync(page * 20);
            list.AddRange(request);
            page++;
        }

        return list;
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

        await _userService.ApproveAsync(account.Id, role); // todo!!!!!!!!!!!!!! add role in service

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
            await _userService.RejectAsync(account.Id, model.Reason ?? string.Empty);
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
