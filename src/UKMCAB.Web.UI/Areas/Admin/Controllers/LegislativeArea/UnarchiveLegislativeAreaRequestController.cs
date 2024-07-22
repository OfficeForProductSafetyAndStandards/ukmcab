using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Asn1.Ocsp;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class UnarchiveLegislativeAreaRequestController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IDistCache _distCache;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly ILegislativeAreaDetailService _legislativeAreaDetailService;
    private const string CacheKey = "soa_create_{0}";

    public static class Routes
    {
        public const string UnarchiveLegislativeAreaRequest =
            "legislative.area.unarchive-legislativearea-request";

        public const string UnarchiveLegislativeAreaRequestReason =
            "legislative.area.unarchive-legislativearea-request-reason";
    }

    public UnarchiveLegislativeAreaRequestController(
        ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        ILegislativeAreaDetailService legislativeAreaDetailService,
        IUserService userService,
        IDistCache distCache) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _distCache = distCache;
        _legislativeAreaDetailService = legislativeAreaDetailService;
    }

    [HttpGet("unarchive-request/{legislativeAreaId}", Name = Routes.UnarchiveLegislativeAreaRequest)]
    public async Task<IActionResult> UnarchiveLegislativeAreaRequest(Guid id, Guid legislativeAreaId, bool fromSummary)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var vm = new UnarchiveLegislativeAreaRequestViewModel
        {
            CabId = id,
            LegislativeArea =
                await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument,
                    legislativeAreaId),
            FromSummary = fromSummary
        };
        return View("~/Areas/Admin/views/CAB/LegislativeArea/UnarchiveLegislativeAreaRequest.cshtml", vm);
    }

    [HttpGet("unarchive-request-reason/{legislativeAreaId}", Name = Routes.UnarchiveLegislativeAreaRequestReason)]
    public async Task<IActionResult> UnarchiveLegislativeAreaRequestReason(Guid id, Guid legislativeAreaId, bool fromSummary)
    {
        var la = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);
        var vm = new UnarchiveLegislativeAreaRequestReasonViewModel
        {
            CabId = id,
            LegislativeAreaId = legislativeAreaId,
            Title = la.Name,
            FromSummary = fromSummary
        };
        return View("~/Areas/Admin/views/CAB/LegislativeArea/UnarchiveLegislativeAreaRequestReason.cshtml", vm);
    }

    [HttpPost("unarchive-request-reason/{legislativeAreaId}")]
    public async Task<IActionResult> UnarchiveLegislativeAreaRequestReasonPost(Guid id, UnarchiveLegislativeAreaRequestReasonViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString());           

            // if opss user
            if (UserRoleId == Roles.OPSS.Id)
            {               
                await _cabAdminService.UnArchiveLegislativeAreaAsync(CurrentUser, id, vm.LegislativeAreaId, vm.UserNotes, vm.PublicUserNotes);

                return RedirectToAction("ReviewLegislativeAreas", "LegislativeAreaReview", new { Area = "admin", id, vm.FromSummary });

            }
            else
            {   
                var la = document!.DocumentLegislativeAreas.First(d => d.LegislativeAreaId == vm.LegislativeAreaId);
                la.Status = LAStatus.PendingSubmissionToUnarchive;
                la.RequestReason = vm.UserNotes;
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(CurrentUser, document);
                return RedirectToRoute(CABController.Routes.CabSummary, new { id, revealEditActions = true });                
            }               
        }
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/UnarchiveLegislativeAreaRequestReason.cshtml", vm);
    }
}