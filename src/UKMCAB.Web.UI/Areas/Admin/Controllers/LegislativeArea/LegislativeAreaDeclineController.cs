using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/"), Authorize(Claims.LegislativeAreaApprove)]
public class LegislativeAreaDeclineController : UKMCAB.Web.UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;

    public static class Routes
    {
        public const string LegislativeAreaDecline = "legislative.area.decline";
    }

    public LegislativeAreaDeclineController(
        ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService,
        IWorkflowTaskService workflowTaskService,
        IAsyncNotificationClient notificationClient,
        IOptions<CoreEmailTemplateOptions> templateOptions) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _workflowTaskService = workflowTaskService;
        _legislativeAreaService = legislativeAreaService;
        _notificationClient = notificationClient;
        _templateOptions = templateOptions.Value;
    }

    [HttpGet("decline", Name = Routes.LegislativeAreaDecline)]
    public async Task<IActionResult> DeclineAsync(Guid id)
    {
        var la = (await GetLegislativeAreasForUserAsync()).First(); //todo multiples incoming for OPSS OGD
        var vm = new DeclineLAViewModel($"Decline Legislative area {la.Name}", id);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Decline.cshtml", vm);
    }

    [HttpPost("decline")]
    public async Task<IActionResult> DeclinePostAsync(Guid id,
        [Bind(nameof(DeclineLAViewModel.DeclineReason))]
        DeclineLAViewModel vm)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        var la = (await GetLegislativeAreasForUserAsync()).First(); //todo multiples incoming for OPSS OGD
        if (!document.DocumentLegislativeAreas.Select(l => l.LegislativeAreaId).Contains(la.Id))
        {
            throw new PermissionDeniedException("No legislative area on CAB owned by this OGD");
        }

        if (ModelState.IsValid)
        {
            await _cabAdminService.DeclineLegislativeAreaAsync((await _userService.GetAsync(User.GetUserId()!))!, id,
                la.Id, vm.DeclineReason);
            TempData.Add(Constants.DeclinedLA, true);
            
            // send legislative area decline notification
            await SendNotificationOfDeclineAsync(id, document.Name, la.Name, vm.DeclineReason);

            return RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true });
        }

        var viewModel = new DeclineLAViewModel($"Decline Legislative area {la.Name}", id);
        vm.DeclineReason = vm.DeclineReason;
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Decline.cshtml", viewModel);
    }

    private async Task<IList<LegislativeAreaModel>> GetLegislativeAreasForUserAsync()
    {
        return (await _legislativeAreaService.GetLegislativeAreasByRoleId(UserRoleId)).ToList();
    }

    /// <summary>
    /// Sends an email and notification for declined legislative area
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="legislativeAreaName">Name of legislative area</param>
    /// <param name="declineReason">Decline reason</param>
    private async Task SendNotificationOfDeclineAsync(Guid cabId, string? cabName, string legislativeAreaName, string declineReason)
    {
        if (cabName == null) throw new ArgumentNullException(nameof(cabName));       

        var approver = new User(CurrentUser.Id, CurrentUser.FirstName, CurrentUser.Surname,
            CurrentUser.Role ?? throw new InvalidOperationException(),
            CurrentUser.EmailAddress ?? throw new InvalidOperationException());

        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            {
                "CABUrl",
                UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(CABController.Routes.CabSummary, new { id = cabId }))
            },
            { "declineReason", declineReason },
            { "userGroup", approver.UserGroup },
            { "userName", approver.FirstAndLastName },
            { "legislativeAreaName", legislativeAreaName }
        };

        // send email to submitter group email 
        await _notificationClient.SendEmailAsync(_templateOptions.UkasGroupEmail,
            _templateOptions.NotificationLegislativeAreaDeclined, personalisation);      

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                TaskType.LegislativeAreaDeclined,
                approver,
                Roles.UKAS.Id,
                null,
                DateTime.Now,
                $"{approver.FirstAndLastName} from {approver.UserGroup} has declined the {legislativeAreaName} legislative area.",
                approver,
                DateTime.Now,
                false,
                declineReason,
                false,
                cabId));
    }
}