using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/review-legislative-areas"), Authorize]
public class LegislativeAreaReviewController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;

    public LegislativeAreaReviewController(
        ICABAdminService cabAdminService, 
        ILegislativeAreaService legislativeAreaService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
    }

    public static class Routes
    {
        public const string LegislativeAreaSelected = "legislative.area.selected";
    }

    [HttpGet(Name = Routes.LegislativeAreaSelected)]
    public async Task<IActionResult> ReviewLegislativeAreas(Guid id, string? returnUrl)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

        // Implies no document or archived
        if (latestDocument == null)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }

        var scopeOfAppointments = latestDocument.ScopeOfAppointments;
        var selectedLAs = new List<LegislativeAreaListItemViewModel>();      

        foreach (var sa in scopeOfAppointments)
        {
            var products = new List<string>();            
            var procedures = new List<string>();

            var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync((Guid)sa.LegislativeAreaId);

            var purpose = sa.PurposeOfAppointmentId != null
            ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)sa.PurposeOfAppointmentId)
            : null;

            var category = sa.CategoryId != null
            ? await _legislativeAreaService.GetCategoryByIdAsync((Guid)sa.CategoryId)
            : null;

            var subCategory = sa.SubCategoryId != null
            ? await _legislativeAreaService.GetSubCategoryByIdAsync((Guid)sa.SubCategoryId)
            : null;

            if (sa.ProductIds.Any())
            {
                foreach (var productId in sa.ProductIds)
                {
                    var prod = await _legislativeAreaService.GetProductByIdAsync(productId);
                    if (prod != null) products.Add(prod.Name);
                }
            }

            if (sa.ProcedureIds.Any())
            {
                foreach (var procedureId in sa.ProcedureIds)
                {
                    var proc = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                    if (proc?.Name != null) procedures.Add(proc.Name);
                }
            }

            var laItem = new LegislativeAreaListItemViewModel
            {
                LegislativeArea = new ListItem { Id = sa.LegislativeAreaId, Title = legislativeArea.Name },
                PurposeOfAppointment = purpose?.Name ?? string.Empty,
                Category = category?.Name ?? string.Empty,
                SubCategory = subCategory?.Name ?? string.Empty,
                Products = products,
                Procedures = procedures 
            };
            selectedLAs.Add(laItem);
        }

        var groupedSelectedLAs = selectedLAs.GroupBy(la => la.LegislativeArea?.Title).Select(group => new SelectedLegislativeAreaViewModel
        {
            LegislativeAreaName = group.Key,
            LegislativeAreaDetails = group.Select(laDetails => new LegislativeAreaListItemViewModel
            {
                PurposeOfAppointment = laDetails.PurposeOfAppointment,
                Category = laDetails.Category,
                SubCategory = laDetails.SubCategory,
                Products = laDetails.Products,
                Procedures = laDetails.Procedures
            }).ToList()
        }).ToList();

        var vm = new SelectedLegislativeAreasViewModel
        {
            ReturnUrl = returnUrl ?? "/",
            SelectedLegislativeAreas = groupedSelectedLAs
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", vm);
    }
}