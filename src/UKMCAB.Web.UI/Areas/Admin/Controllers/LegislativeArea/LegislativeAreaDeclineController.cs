using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/"), Authorize]
public class LegislativeAreaDeclineController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;

    public static class Routes
    {
        public const string LegislativeAreaDecline = "legislative.area.decline";
    }

    public LegislativeAreaDeclineController(ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _userService = userService;
    }

    [HttpGet("decline", Name = Routes.LegislativeAreaDecline)]
    public async Task<IActionResult> DeclineAsync(Guid id)
    {
        var la = (await GetLegislativeAreasAsync()).First(); //todo multiples incoming for OPSS OGD
        var vm = new DeclineLAViewModel($"Decline Legislative area {la.Name}", id);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Decline.cshtml", vm);
    }
    
    [HttpPost("decline")]
    public async Task<IActionResult> DeclinePostAsync(Guid cabId,
        [Bind(nameof(DeclineLAViewModel.DeclineReason))]
        DeclineLAViewModel vm)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(cabId.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        var la = (await GetLegislativeAreasAsync()).First(); //todo multiples incoming for OPSS OGD
        
        if (ModelState.IsValid)
        {
            var docLa = document.DocumentLegislativeAreas.First(l => l.LegislativeAreaId == la.Id);
            docLa.Status = LAStatus.Declined;
            return RedirectToRoute(CABController.Routes.CabSummary, new { id = cabId, subSectionEditAllowed = true });
        }
        
        var viewModel = new DeclineLAViewModel($"Decline Legislative area {la.Name}", cabId);
        vm.DeclineReason = vm.DeclineReason;
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Decline.cshtml", viewModel);

    }
    
    private async Task<IList<LegislativeAreaModel>> GetLegislativeAreasAsync()
    {
        var currentUser = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ??
                          throw new InvalidOperationException();
        var userRoleId = Roles.List.First(r =>
            r.Label != null && r.Label.Equals(currentUser.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
        return (await _legislativeAreaService.GetLegislativeAreasByRoleId(userRoleId)).ToList();
    }
    
}