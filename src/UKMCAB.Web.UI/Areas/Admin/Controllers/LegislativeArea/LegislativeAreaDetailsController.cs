using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Data.Models;
using System.Security.Claims;
using UKMCAB.Core.Services.Users;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IEditLockService _editLockService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;

    public static class Routes
    {
        public const string LegislativeAreaDetails = "legislative.area.details";
        public const string LegislativeAreaAdd = "legislative.area.add-legislativearea";
        public const string PurposeOfAppointmentAdd = "legislative.area.add-purposeofappointment";
    }
    public LegislativeAreaDetailsController(
        ICABAdminService cabAdminService,            
        IEditLockService editLockService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService
    )
    {
        _cabAdminService = cabAdminService;        
        _editLockService = editLockService;
        _legislativeAreaService = legislativeAreaService;
        _userService = userService;
    }
    
    [HttpGet("admin/cab/{id}/legislative-area/details/{documentLegislativeAreaId}", Name = Routes.LegislativeAreaDetails)]
    public async Task<IActionResult> Details(Guid id, Guid documentLegislativeAreaId)
    {
        var vm = new LegislativeAreaDetailViewModel(Title: "Legislative area details");
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Details.cshtml", vm);
    }

    [HttpGet("admin/cab/{id}/legislative-area/add", Name = Routes.LegislativeAreaAdd)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, string? returnUrl)
    {
        var vm = new LegislativeAreaViewModel
        {
            CABId = id,
            LegislativeAreas = await this.GetLegislativeSelectListItemsAsync(),
            ReturnUrl = returnUrl,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    [HttpPost("admin/cab/{id}/legislative-area/add", Name = Routes.LegislativeAreaAdd)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, LegislativeAreaViewModel vm, string submitType)
    {
        if (ModelState.IsValid)
        {   
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

            // Implies no document or archived
            if (latestDocument == null)
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            else
            {
                // add  document new legislative area;
                var documentLegislativeAreaId = Guid.NewGuid();

                var documentLegislativeArea = new DocumentLegislativeArea
                {
                    Id = documentLegislativeAreaId,
                    LegislativeAreaId = vm.SelectedLegislativeAreaId
                };

                latestDocument.DocumentLegislativeAreas.Add(documentLegislativeArea);

                // add new document scope of appointment;
                var scopeOfAppointmentId = Guid.NewGuid();

                var scopeOfAppointment = new DocumentScopeOfAppointment
                {
                    Id = scopeOfAppointmentId,
                    LegislativeAreaId = vm.SelectedLegislativeAreaId
                };

                latestDocument.ScopeOfAppointments.Add(scopeOfAppointment);

                var userAccount =
                  await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);

                if (submitType == Constants.SubmitType.Continue)
                {
                    return RedirectToRoute(Routes.PurposeOfAppointmentAdd, new { id, scopeId = scopeOfAppointmentId });
                }
                // save additional info
                else if (submitType == Constants.SubmitType.AdditionalInfo)
                {
                    return RedirectToRoute(Routes.LegislativeAreaDetails, new { id, documentLegislativeAreaId });
                }
                // save as draft
                else
                {
                    return RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true });
                }
            }            
        }
        else
        {
            vm.LegislativeAreas = await this.GetLegislativeSelectListItemsAsync();
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
        }
    }

    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync()
    {
        var legislativeareas = await _legislativeAreaService.GetAllLegislativeAreas();
        return legislativeareas.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
    }
}