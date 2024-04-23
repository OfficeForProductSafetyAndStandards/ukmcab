using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.ServiceManagement;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class ServiceManagementController : UI.Controllers.ControllerBase
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IWorkflowTaskService _workflowTaskService;
        private readonly IEditLockService _editLockService;

        public static class Routes
        {
            public const string ServiceManagement = "service.management";
        }

        public ServiceManagementController(ICABAdminService cabAdminService, IUserService userService,
            IWorkflowTaskService workflowTaskService, IEditLockService editLockService) : base(userService)
        {
            _cabAdminService = cabAdminService;
            _workflowTaskService = workflowTaskService;
            _editLockService = editLockService;
        }


        [HttpGet("service-management", Name = "service.management")]
        public async Task<IActionResult> ServiceManagement()
        {
            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                    .Value) ?? throw new InvalidOperationException("User account not found");
            await _editLockService.RemoveEditLockForUserAsync(userAccount.Id);
            var cabs = await _cabAdminService.FindAllCABManagementQueueDocumentsForUserRole(UserRoleId);
            var unassignedNotifications = await _workflowTaskService.GetUnassignedByForRoleIdAsync(UserRoleId);
            var assignedNotifications =
                await _workflowTaskService.GetAssignedToGroupForRoleIdAsync(UserRoleId, userAccount.Id);
            var assignedNotificationToSpecificUser = await _workflowTaskService.GetByAssignedUserAsync(userAccount.Id);
            
            return View(new InternalLandingPageViewModel
            {
                TotalDraftCABs = cabs.DraftCabs.Count(),
                TotalPendingDraftCABs = cabs.PendingDraftCabs.Count(),
                TotalPendingPublishCABs = cabs.PendingPublishCabs.Count(),
                TotalPendingArchiveCABs = cabs.PendingArchiveCabs.Count(),
                TotalAccountRequests = await _userService.CountRequestsAsync(UserAccountRequestStatus.Pending),
                UnassignedNotification = unassignedNotifications.Count,
                AssignedNotification = assignedNotifications.Count,
                AssignedToMeNotification = assignedNotificationToSpecificUser.Count,
                UserRoleLabel = Roles.NameFor(UserRoleId) 
            });
        }
    }
}