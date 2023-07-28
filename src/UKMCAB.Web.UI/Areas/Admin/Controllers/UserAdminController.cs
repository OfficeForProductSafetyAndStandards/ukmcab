using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Admin.User;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Route("user-admin"), Authorize(Policy = Policies.CabManagement)]
    public class UserAdminController : Controller
    {
        private readonly IUserService _userService;
        public static class Routes
        {
            public const string UserList = "user-admin.list";
            public const string UserAccountRequestsList = "user-admin.account-requests.list";
            public const string ReviewAccountRequest = "user-admin.review-account-request";
        }

        public UserAdminController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous] // TODO: added to allow dev testing, needs to be removed
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

        [AllowAnonymous] // TODO: added to allow dev testing, needs to be removed
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

        [AllowAnonymous] // TODO: added to allow dev testing, needs to be removed
        [HttpGet("review-account-request/{id}", Name = Routes.ReviewAccountRequest)]
        public async Task<IActionResult> ReviewAccountRequest(string id)
        {
            var account = await _userService.GetAccountRequestAsync(id);
            return View(new ReviewAccountRequestViewModel
            {
                UserAccountRequest = account
            });
        }
    }
}
