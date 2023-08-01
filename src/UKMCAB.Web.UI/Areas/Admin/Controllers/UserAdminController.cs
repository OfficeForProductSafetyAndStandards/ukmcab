using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Account;
using UKMCAB.Web.UI.Models.ViewModels.Admin.User;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Route("user-admin"), Authorize(Policy = Policies.CabManagement)]
    public class UserAdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly TemplateOptions _templateOptions;
        public static class Routes
        {
            public const string UserList = "user-admin.list";
            public const string UserAccountRequestsList = "user-admin.account-requests.list";
            public const string ReviewAccountRequest = "user-admin.review-account-request";
            public const string RequestApproved = "user-admin.request-approved";
            public const string RequestRejected = "user-admin.request-rejected";
            public const string RejectRequest = "user-admin.reject-request";
        }

        public UserAdminController(IUserService userService, IAsyncNotificationClient notificationClient, IOptions<TemplateOptions> templateOptions)
        {
            _userService = userService;
            _notificationClient = notificationClient;
            _templateOptions = templateOptions.Value;
        }

        [Route("")]
        [HttpGet("list", Name = Routes.UserList)]
        public async Task<IActionResult> Index(int skip = 0)
        {
            var accounts = await _userService.ListAsync(false, skip);
            var pendingAccounts = await GetAllPendingRequests();

            return View(new UserAccountListViewModel
            {
                UserAccounts = accounts.ToList(),
                PendingAccountsCount = pendingAccounts.Count
            });
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
                return RedirectToAction("Index", "UserAdmin", new { Area="admin"});
            }

            if (submitType == Constants.SubmitType.Approve)
            {
                await _userService.ApproveAsync(account.Id);
                await _notificationClient.SendEmailAsync(account.EmailAddress, _templateOptions.AccountRequestApproved);

                return RedirectToAction("RequestApproved", "UserAdmin", new { Area = "admin", id=account.Id });
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
}
