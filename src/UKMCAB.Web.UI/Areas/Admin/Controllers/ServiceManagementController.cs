using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services;
using UKMCAB.Web.UI.Models.ViewModels.Admin.ServiceManagement;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    //[Area("admin"), Authorize]
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


        [HttpGet("admin/service-management", Name = "service.management")]
        public async Task<IActionResult> ServiceManagement()
        {
            return View(new InternalLandingPageViewModel
            {
                TotalDraftCABs = await _cabAdminService.CABCountAsync(Status.Draft),
                TotalCABsPendingApproval = await _cabAdminService.CABCountAsync(SubStatus.PendingApproval),
                TotalAccountRequests = await _userService.CountRequestsAsync(UserAccountRequestStatus.Pending)
            }); 
        }
    }
}
