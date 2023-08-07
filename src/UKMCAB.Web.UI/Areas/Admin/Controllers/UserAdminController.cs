using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security.Tokens;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Areas.Home.Controllers;
using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Models.ViewModels.Account;
using UKMCAB.Web.UI.Models.ViewModels.Admin.User;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("user-admin"), Authorize]
public class UserAdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly ISecureTokenProcessor _secureTokenProcessor;
    private readonly TemplateOptions _templateOptions;
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
    }

    public UserAdminController(IUserService userService, IAsyncNotificationClient notificationClient, IOptions<TemplateOptions> templateOptions, ISecureTokenProcessor secureTokenProcessor)
    {
        _userService = userService;
        _notificationClient = notificationClient;
        _secureTokenProcessor = secureTokenProcessor;
        _templateOptions = templateOptions.Value;
    }

    [HttpGet("list", Name = Routes.UserList)]
    public async Task<IActionResult> UserListAsync(int skip = 0) => await UserListAsync(skip, false, "User accounts");

    [HttpGet("list/locked", Name = Routes.UserListLocked)]
    public async Task<IActionResult> UserListLockedAsync(int skip = 0) => await UserListAsync(skip, true, "Locked/archived user accounts");

    private async Task<IActionResult> UserListAsync(int skip, bool isLocked, string title)
    {
        var accounts = await _userService.ListAsync(isLocked, skip);
        var pendingAccounts = await GetAllPendingRequests();
        return View("UserList", new UserAccountListViewModel
        {
            UserAccounts = accounts.ToList(),
            PendingAccountsCount = pendingAccounts.Count,
            Title = title,
            LockedOnly = isLocked,
        });
    }

    [HttpGet("{id}", Name = Routes.UserAccount)]
    public async Task<IActionResult> UserAccountAsync(string id)
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
            return View(ViewName, new UserAccountLockUnlockViewModel() { Mode = mode });
        }
        else if (Request.Method == HttpMethod.Post.Method && model != null)
        {
            model.Pipe(x => x.Mode = mode);
            if (ModelState.IsValid)
            {
                if(mode.EqualsAny(UserAccountLockToggleUIMode.Lock, UserAccountLockToggleUIMode.Archive))
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
    public async Task<IActionResult> AccountRequestList()
    {
        var pendingAccounts = await GetAllPendingRequests();
        return View(new AccountRequestListViewModel
        {
            UserAccountRequests = pendingAccounts.OrderByDescending(pa => pa.CreatedUtc).ToList()
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
            return RedirectToAction("Index", "UserAdmin", new { Area = "admin" });
        }

        return View(new ReviewAccountRequestViewModel
        {
            UserAccountRequest = account
        });
    }


    [HttpPost("review-account-request/{id}", Name = Routes.ReviewAccountRequest)]
    public async Task<IActionResult> ReviewAccountRequest(string id, string submitType)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToAction("Index", "UserAdmin", new { Area = "admin" });
        }

        if (submitType == Constants.SubmitType.Approve)
        {
            await _userService.ApproveAsync(account.Id);
            await _notificationClient.SendEmailAsync(account.EmailAddress, _templateOptions.AccountRequestApproved);

            return RedirectToAction("RequestApproved", "UserAdmin", new { Area = "admin", id = account.Id });
        }
        return RedirectToAction("RejectRequest", "UserAdmin", new { Area = "admin", id = account.Id });
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

    [HttpGet("reject-request/{id}", Name = Routes.RejectRequest)]
    public async Task<IActionResult> RejectRequest(string id)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToAction("Index", "UserAdmin", new { Area = "admin" });
        }
        return View(new RejectRequestViewModel
        {
            AccountId = account.Id
        });
    }

    [HttpPost("reject-request/{id}", Name = Routes.RejectRequest)]
    public async Task<IActionResult> RejectRequest(string id, RejectRequestViewModel model)
    {
        var account = await _userService.GetAccountRequestAsync(id);
        if (account == null)
        {
            return RedirectToAction("Index", "UserAdmin", new { Area = "admin" });
        }

        if (ModelState.IsValid)
        {
            await _userService.RejectAsync(account.Id);
            var personalisation = new Dictionary<string, dynamic>
            {
                {"rejection-reason", model.Reason }
            };
            await _notificationClient.SendEmailAsync(account.EmailAddress, _templateOptions.AccountRequestRejected, personalisation);

            return RedirectToAction("RequestRejected", "UserAdmin", new { Area = "admin", id = account.Id });
        }

        model.AccountId = id;
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
