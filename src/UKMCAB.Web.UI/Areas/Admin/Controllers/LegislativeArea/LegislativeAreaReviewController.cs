using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea.Review;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/review-legislative-areas"), Authorize]
public class LegislativeAreaReviewController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;

    public LegislativeAreaReviewController(
        ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
    }

    public static class Routes
    {
        public const string LegislativeAreaSelected = "legislative.area.selected";
    }

    [HttpGet(Name = Routes.LegislativeAreaSelected)]
    public async Task<IActionResult> ReviewLegislativeAreas(Guid id, string? returnUrl, LegislativeAreaActionMessageEnum? actionType, bool fromSummary)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        // Implies no document or archived
        if (latestDocument == null)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }

        var isOgdUser = Roles.OgdRolesList.Contains(UserRoleId);
        if (isOgdUser)
        {
            await _cabAdminService.FilterCabContentsByLaIfPendingOgdApproval(latestDocument, UserRoleId);
        }
        var vm = await PopulateCABLegislativeAreasViewModelAsync(latestDocument);
        var singleDraftDoc = await _cabAdminService.IsSingleDraftDocAsync(id);
        vm.ShowArchiveLegislativeAreaAction = !singleDraftDoc;

        if (actionType.HasValue)
        {
            vm.SuccessBannerMessage = AlertMessagesUtils.LegislativeAreaActionMessages[actionType.Value];
        }
        vm.FromSummary = fromSummary;
        vm.ReturnUrl = returnUrl;

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", vm);
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

        if(submitType == "AddLegislativeArea")
        {
            var cabLegislativeAreaIds = latestDocument.DocumentLegislativeAreas.Select(n => n.LegislativeAreaId).ToList();
            var remainingLegislativeAreasToSelect = await _legislativeAreaService.GetLegislativeAreasAsync(cabLegislativeAreaIds);

            // if all legislative area added and none left to add
            if (remainingLegislativeAreasToSelect.Count() == 0)
            {
                ModelState.AddModelError("AddLegislativeArea", "All legislative areas have already been added to this CAB profile.");
                var vm = await PopulateCABLegislativeAreasViewModelAsync(latestDocument);                
                return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", vm);
            }
            else
            {
                return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddLegislativeArea, new { id, fromSummary = reviewLaVM.FromSummary });
            }
        }

        var laIdOrLaName = string.Empty;

        if (submitType.StartsWith("Add-"))
        {
            laIdOrLaName = submitType[4..];
            if (Guid.TryParse(laIdOrLaName, out Guid laId))
            {
                return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddPurposeOfAppointment, new { id, scopeId = Guid.Empty, legislativeAreaId = laId, fromSummary = reviewLaVM.FromSummary });
            }
        }
        else if (submitType.StartsWith("Edit-"))
        {
            laIdOrLaName = submitType[5..];
        }
        else if (submitType.StartsWith("Remove-"))
        {
            laIdOrLaName = submitType[7..];
        }

        var cabLaOfSelectedScopeofAppointment = reviewLaVM.ActiveLAItems.FirstOrDefault(la => la.SelectedScopeOfAppointmentId != null && la.Name == laIdOrLaName);
        if (cabLaOfSelectedScopeofAppointment == null)
        {
            ModelState.AddModelError(laIdOrLaName, "Select a scope of appointment");
            var isOgdUser = Roles.OgdRolesList.Contains(UserRoleId);
            if (isOgdUser)
            {
                await _cabAdminService.FilterCabContentsByLaIfPendingOgdApproval(latestDocument, UserRoleId);
            }
            var vm = await PopulateCABLegislativeAreasViewModelAsync(latestDocument);
            vm.ErrorLink = laIdOrLaName;
            return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", vm);
        }

        var laOfSelectedSoa = cabLaOfSelectedScopeofAppointment.ScopeOfAppointments.First(soa => soa.ScopeId == cabLaOfSelectedScopeofAppointment.SelectedScopeOfAppointmentId);
        var legislativeAreaId = laOfSelectedSoa.LegislativeArea.Id;
        var selectedScopeOfAppointmentId = laOfSelectedSoa.ScopeId;
        Guard.IsTrue(selectedScopeOfAppointmentId != Guid.Empty, "Scope Id Guid cannot be empty");

        if (ModelState.IsValid)
        {
            if (submitType.StartsWith(Constants.SubmitType.Edit))
            {
                return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddPurposeOfAppointment, new { id, scopeId = Guid.Empty, compareScopeId = selectedScopeOfAppointmentId, legislativeAreaId, fromSummary = reviewLaVM.FromSummary });
            }
            if (submitType.StartsWith(Constants.SubmitType.Remove))
            {
                var soaToRemove = latestDocument.ScopeOfAppointments.First(s => s.Id == selectedScopeOfAppointmentId);
                if (soaToRemove != null)
                {
                    var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                    latestDocument.ScopeOfAppointments.Remove(soaToRemove);

                    var documentLegislativeArea = latestDocument.DocumentLegislativeAreas.FirstOrDefault(la => la.LegislativeAreaId == legislativeAreaId);
                    if (latestDocument.StatusValue == Status.Draft && latestDocument.SubStatus == SubStatus.None &&
                        (documentLegislativeArea.Status == LAStatus.Published || documentLegislativeArea.Status == LAStatus.Declined || documentLegislativeArea.Status == LAStatus.DeclinedByOpssAdmin))
                    {
                        documentLegislativeArea.Status = LAStatus.Draft;
                    }

                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);                    
                    return RedirectToRoute(Routes.LegislativeAreaSelected, new { id, actionType = LegislativeAreaActionMessageEnum.AssessmentProcedureRemoved, fromSummary = reviewLaVM.FromSummary });
                }
            }
        }

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", new ReviewLegislativeAreasViewModel());
    }

    private async Task<ReviewLegislativeAreasViewModel> PopulateCABLegislativeAreasViewModelAsync(Document cab)
    {
        var viewModel = new ReviewLegislativeAreasViewModel
        {
            CABId = Guid.Parse(cab.CABId),
            ShowAddRemoveLegislativeAreaActions = !cab.IsPendingOgdApproval,
        };

        foreach (var documentLegislativeArea in cab.DocumentLegislativeAreas)
        {
            var legislativeArea =
                await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentLegislativeArea
                    .LegislativeAreaId);

            var legislativeAreaViewModel = new CABLegislativeAreasItemViewModel
            {
                LegislativeAreaId = legislativeArea.Id,
                Name = legislativeArea.Name,
                Status = documentLegislativeArea.Status,
                RoleName = Roles.NameFor(documentLegislativeArea.RoleId),
                IsProvisional = documentLegislativeArea.IsProvisional,
                AppointmentDate = documentLegislativeArea.AppointmentDate,
                ReviewDate = documentLegislativeArea.ReviewDate,
                Reason = documentLegislativeArea.Reason,
                CanChooseScopeOfAppointment = legislativeArea.HasDataModel,
                IsArchived = documentLegislativeArea.Archived,
                ShowEditActions = documentLegislativeArea.Status == LAStatus.Draft ||
                                  documentLegislativeArea.Status == LAStatus.PendingApproval && UserRoleId == documentLegislativeArea.RoleId ||
                                  documentLegislativeArea.Status == LAStatus.Approved && UserRoleId == Roles.OPSS.Id ||
                                  cab.StatusValue == Status.Draft && cab.SubStatus == SubStatus.None,
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

            var distinctSoa = legislativeAreaViewModel.ScopeOfAppointments.GroupBy(s => s.ScopeId).ToList();
            foreach (var item in distinctSoa)
            {
                var scopeOfApps = legislativeAreaViewModel.ScopeOfAppointments;
                scopeOfApps.First(soa => soa.ScopeId == item.Key).NoOfProductsInScopeOfAppointment = scopeOfApps.Count(soa => soa.ScopeId == item.Key);
            }
            if (legislativeAreaViewModel.IsArchived != true)
            {
                viewModel.ActiveLAItems.Add(legislativeAreaViewModel);
            }
            else
            {
                viewModel.ArchivedLAItems.Add(legislativeAreaViewModel);
            }
        };

        return viewModel;
    }
}