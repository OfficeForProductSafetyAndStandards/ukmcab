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
    public class ServiceManagementController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IUserService _userService;
        private readonly IWorkflowTaskService _workflowTaskService;

        public static class Routes
        {
            public const string ServiceManagement = "service.management";
        }

        public ServiceManagementController(ICABAdminService cabAdminService, IUserService userService,
            IWorkflowTaskService workflowTaskService)
        {
            _cabAdminService = cabAdminService;
            _userService = userService;
            _workflowTaskService = workflowTaskService;
        }


        [HttpGet("service-management", Name = "service.management")]
        public async Task<IActionResult> ServiceManagement()
        {
            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                    .Value) ?? throw new InvalidOperationException("User account not found");

            var role = userAccount.Role == Roles.OPSS.Id ? null : userAccount.Role;
            var docs = await _cabAdminService.FindAllCABManagementQueueDocumentsForUserRole(role);
            var userRole = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS : Roles.UKAS;
            var unassignedNotifications = await _workflowTaskService.GetUnassignedByForRoleIdAsync(userRole.Id);
            var assignedNotifications =
                await _workflowTaskService.GetAssignedToGroupForRoleIdAsync(userRole.Id, userAccount.Id);
            var assignedNotificationToSpecificUser = await _workflowTaskService.GetByAssignedUserAsync(userAccount.Id);
            
            return View(new InternalLandingPageViewModel
            {
                TotalDraftCABs = docs.Where(d => d.StatusValue == Status.Draft).Count(),
                TotalCABsPendingApproval = docs.Where(d => d.SubStatus == SubStatus.PendingApproval).Count(),
                TotalAccountRequests = await _userService.CountRequestsAsync(UserAccountRequestStatus.Pending),
                UnassignedNotification = unassignedNotifications.Count,
                AssignedNotification = assignedNotifications.Count,
                AssignedToMeNotification = assignedNotificationToSpecificUser.Count,
                UserRoleLabel = userRole.Label
            });
        }
    }
}