using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
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
    public IActionResult UnarchiveLegislativeAreaRequestReason(Guid id, Guid legislativeAreaId)
    {
        var vm = new UnarchiveLegislativeAreaRequestReasonViewModel();
        return View("~/Areas/Admin/views/CAB/LegislativeArea/UnarchiveLegislativeAreaRequestReason.cshtml", vm);
    }
}