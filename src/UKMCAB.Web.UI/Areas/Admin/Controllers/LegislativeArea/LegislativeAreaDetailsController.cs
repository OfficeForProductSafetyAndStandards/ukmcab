using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Data.Models;
using System.Security.Claims;
using UKMCAB.Core.Services.Users;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;


[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IEditLockService _editLockService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;

    public static class Routes
    {
        public const string LegislativeAreaAdd = "legislative.area.add-legislativearea";
        public const string PurposeOfAppointment = "legislative.area.add-purpose-of-appointment";
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
    

    [HttpGet("add", Name = Routes.LegislativeAreaAdd)]
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

    [HttpPost("add", Name = Routes.LegislativeAreaAdd)]
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
                    return RedirectToRoute(Routes.PurposeOfAppointment, new { id, scopeId = scopeOfAppointmentId });
                }
                // save additional info
                else if (submitType == Constants.SubmitType.AdditionalInfo)
                {
                    return RedirectToRoute( LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation , new { id, laId = documentLegislativeAreaId });
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
    
    
    [HttpGet("add-purpose-of-appointment/{scopeId}", Name = Routes.PurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, Guid scopeId)
    {
        //todo get scope of appointment
        var laId = Guid.Parse("e840c3d0-6153-4baa-82ab-0374b81d46fe");
        var options = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(laId);
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(laId);
        if (legislativeArea == null)
        {
            throw new InvalidOperationException($"Legislative Area not found for {laId}");
        }
            
        var selectListItems = options.PurposeOfAppointments.Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();
        var vm = new PurposeOfAppointmentViewModel
        {
            Title = "Select purpose of appointment",
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointments = selectListItems,
            CabId = id
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }
    
    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync()
    {
        var legislativeAreas = await _legislativeAreaService.GetAllLegislativeAreasAsync();
        return legislativeAreas.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
    }
}