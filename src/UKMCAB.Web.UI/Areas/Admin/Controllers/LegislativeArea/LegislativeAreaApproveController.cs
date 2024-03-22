using Microsoft.AspNetCore.Authorization;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/"), Authorize]
public class LegislativeAreaApproveController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;

    public static class Routes
    {
        public const string LegislativeAreaApprove = "legislative.area.approve";
    }

    public LegislativeAreaApproveController(ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
    }

    [HttpPost("approve", Name = Routes.LegislativeAreaApprove)]
    public async Task<IActionResult> ApprovePostAsync(Guid id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        var la = (await GetLegislativeAreasForUserAsync()).First(); //todo multiples incoming for OPSS OGD
        if (!document.DocumentLegislativeAreas.Select(l => l.LegislativeAreaId).Contains(la.Id))
        {
            throw new PermissionDeniedException("No legislative area on CAB owned by this OGD");
        }
        
        await _cabAdminService.ApproveLegislativeAreaAsync((await _userService.GetAsync(User.GetUserId()!))!, id, la.Id);
        TempData.Add(Constants.ApprovedLA, true);
        return RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true });
    }

    private async Task<IList<LegislativeAreaModel>> GetLegislativeAreasForUserAsync()
    {
        return (await _legislativeAreaService.GetLegislativeAreasByRoleId(UserRoleId)).ToList();
    }

}