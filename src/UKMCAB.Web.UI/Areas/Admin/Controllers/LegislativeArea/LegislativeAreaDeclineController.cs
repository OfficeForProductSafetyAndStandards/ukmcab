using Microsoft.AspNetCore.Authorization;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/"), Authorize(Claims.LegislativeAreaApprove)]
public class LegislativeAreaDeclineController : UKMCAB.Web.UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;

    public static class Routes
    {
        public const string LegislativeAreaDecline = "legislative.area.decline";
    }

    public LegislativeAreaDeclineController(ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService, IUserService userService) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
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
}