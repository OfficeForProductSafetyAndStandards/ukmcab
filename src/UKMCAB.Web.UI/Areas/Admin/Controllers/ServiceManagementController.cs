using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.ServiceManagement;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using System.Security.Claims;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class ServiceManagementController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IUserService _userService;

        public static class Routes
        {
            public const string ServiceManagement = "service.management";
        }

        public ServiceManagementController(ICABAdminService cabAdminService, IUserService userService)
        {
            _cabAdminService = cabAdminService;
            _userService = userService;
        }


        [HttpGet("service-management", Name = "service.management")]
        public async Task<IActionResult> ServiceManagement()
        {
            var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value) ?? throw new InvalidOperationException("User account not found");

            var docs = await _cabAdminService.FindAllCABManagementQueueDocuments(userAccount);
            return View(new InternalLandingPageViewModel
            {
                //TotalDraftCABs = await _cabAdminService.GetCABCountForStatusAsync(userAccount, Status.Draft),
                TotalDraftCABs = docs.Where(d => d.StatusValue == Status.Draft).Count(),
                //TotalCABsPendingApproval = await _cabAdminService.GetCABCountForSubStatusAsync(SubStatus.PendingApproval),
                TotalCABsPendingApproval = docs.Where(d => d.SubStatus == SubStatus.PendingApproval).Count(),
                TotalAccountRequests = await _userService.CountRequestsAsync(UserAccountRequestStatus.Pending)
            }); 
        }
    }
}
