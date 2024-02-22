using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea.Review;
using UKMCAB.Web.UI.Services.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/review-legislative-areas"), Authorize]
public class LegislativeAreaReviewController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;
    private readonly ILegislativeAreaUtils _legislativeAreaUtils;

    public LegislativeAreaReviewController(
        ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService,
        ILegislativeAreaUtils legislativeAreaUtils)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _userService = userService;
        _legislativeAreaUtils = legislativeAreaUtils;
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

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", await PopulateCABLegislativeAreasViewModelAsync(latestDocument));
    }

    [HttpPost(Name = Routes.LegislativeAreaSelected)]
    public async Task<IActionResult> ReviewLegislativeAreas(Guid id, string submitType, ReviewLegislativeAreasViewModel reviewLaVM)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

        // Implies no document or archived
        if (latestDocument == null)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }

        if (submitType != null && submitType.StartsWith("Add-"))
        {
            var scopeOfAppointmentId = Guid.NewGuid();
            var legislativeAreaIdString = submitType.Substring(4);
            if (Guid.TryParse(legislativeAreaIdString, out Guid legislativeAreaId))
            {
                await _legislativeAreaUtils.CreateScopeOfAppointmentInCacheAsync(scopeOfAppointmentId, legislativeAreaId);
            }
            return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddPurposeOfAppointment, new { id, scopeId = scopeOfAppointmentId });
        }

        var cabLaOfSelectedScopeofAppointment = reviewLaVM.LAItems.FirstOrDefault(la => la.ScopeOfAppointments.Any(soa => soa.IsSelected == true));
        if (cabLaOfSelectedScopeofAppointment == null)
        {
            ModelState.AddModelError("ScopeOfAppointment", "Select a scope of apppointment to edit");
            return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", await PopulateCABLegislativeAreasViewModelAsync(latestDocument));
        }

        var laOfSelectedSoa = cabLaOfSelectedScopeofAppointment.ScopeOfAppointments.First(la => la.IsSelected == true);
        var laId = laOfSelectedSoa.LegislativeArea.Id;
        var selectedScopeOfAppointmentId = laOfSelectedSoa.ScopeId;
        Guard.IsTrue(selectedScopeOfAppointmentId != Guid.Empty, "Scope Id Guid cannot be empty");


        if (ModelState.IsValid)
        {
            // TODO: Handle the different submit types
            if (submitType == Constants.SubmitType.Edit)
            {

            }
            if (submitType == Constants.SubmitType.Remove)
            {

            }
        }

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", new ReviewLegislativeAreasViewModel());
    }

    private async Task<ReviewLegislativeAreasViewModel> PopulateCABLegislativeAreasViewModelAsync(Document cab)
    {
        var viewModel = new ReviewLegislativeAreasViewModel
        {
            CABId = Guid.Parse(cab.CABId)
        };

        foreach (var documentLegislativeArea in cab.DocumentLegislativeAreas)
        {
            var legislativeArea =
                await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentLegislativeArea
                    .LegislativeAreaId);

            var legislativeAreaViewModel = new CABLegislativeAreasItemViewModel
            {
                Name = legislativeArea.Name,
                LegislativeAreaId = legislativeArea.Id,
                IsProvisional = documentLegislativeArea.IsProvisional,
                AppointmentDate = documentLegislativeArea.AppointmentDate,
                ReviewDate = documentLegislativeArea.ReviewDate,
                Reason = documentLegislativeArea.Reason,
                CanChooseScopeOfAppointment = legislativeArea.HasDataModel,
            };

            var scopeOfAppointments = cab.ScopeOfAppointments.Where(x => x.LegislativeAreaId == legislativeArea.Id);
            foreach (var scopeOfAppointment in scopeOfAppointments)
            {
                var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId.HasValue
                    ? (await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(scopeOfAppointment
                        .PurposeOfAppointmentId.Value))?.Name
                    : null;

                var category = scopeOfAppointment.CategoryId.HasValue
                    ? (await _legislativeAreaService.GetCategoryByIdAsync(scopeOfAppointment.CategoryId.Value))
                    ?.Name
                    : null;

                var subCategory = scopeOfAppointment.SubCategoryId.HasValue
                    ? (await _legislativeAreaService.GetSubCategoryByIdAsync(scopeOfAppointment.SubCategoryId
                        .Value))?.Name
                    : null;

                foreach (var productProcedure in scopeOfAppointment.ProductIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        Category = category,
                        SubCategory = subCategory,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (productProcedure.ProductId.HasValue)
                    {
                        var product =
                            await _legislativeAreaService.GetProductByIdAsync(productProcedure.ProductId.Value);
                        soaViewModel.Product = product!.Name;
                    }

                    foreach (var procedureId in productProcedure.ProcedureIds)
                    {
                        var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                        soaViewModel.Procedures?.Add(procedure!.Name);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }
            }

            viewModel.LAItems.Add(legislativeAreaViewModel);
        }

        return viewModel;
    }
}