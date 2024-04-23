using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class ArchiveLegislativeAreaRequestController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;

    public ArchiveLegislativeAreaRequestController(
        ICABAdminService cabAdminService,
        IUserService userService) : base(userService)
    {
        _cabAdminService = cabAdminService;
    }

    public static class Routes
    {
        public const string ArchiveLegislativeArea = "archive.legislative.area";
    }

    [HttpGet("archive-request/{legislativeAreaId}", Name = Routes.ArchiveLegislativeArea)]
    public async Task<IActionResult> ArchiveAsync(Guid id, Guid legislativeAreaId)
    {
        var vm = new LegislativeAreaArchiveRequestViewModel(id, string.Empty);
        var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");

        vm.LegislativeAreaName = document.DocumentLegislativeAreas.Single(a => a.LegislativeAreaId == legislativeAreaId)
            .LegislativeAreaName;
        return View("~/Areas/Admin/Views/CAB/LegislativeArea/ArchiveLegislativeAreaReason.cshtml", vm);
    }

    [HttpPost("archive-request/{legislativeAreaId}", Name = Routes.ArchiveLegislativeArea)]
    public async Task<IActionResult> ArchiveAsync(Guid id, Guid legislativeAreaId,
        LegislativeAreaArchiveRequestViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/CAB/LegislativeArea/ArchiveLegislativeAreaRequest.cshtml", vm);
        }

        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var documentLegislativeArea =
            latestDocument.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == legislativeAreaId);
        documentLegislativeArea.Status = LAStatus.PendingSubmissionToArchiveAndArchiveSchedule; //todo: GG change based on model
        documentLegislativeArea.RequestReason = vm.ArchiveReason;

        await _cabAdminService.UpdateOrCreateDraftDocumentAsync((await _userService.GetAsync(User.GetUserId()!))!,
            latestDocument);
       return RedirectToRoute(CABController.Routes.CabSummary, new { id });
    }
}