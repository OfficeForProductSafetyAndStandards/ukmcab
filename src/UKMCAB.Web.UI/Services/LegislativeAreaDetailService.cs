using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Web.UI.Helpers;

namespace UKMCAB.Web.UI.Services
{
    [Obsolete("This class is obsolete. Use UKMCAB.Core.Extensions.DocumentLegislativeAreaExtensions instead.")]
    public class LegislativeAreaDetailService : ILegislativeAreaDetailService
    {
        private readonly ILegislativeAreaService _legislativeAreaService;

        public LegislativeAreaDetailService(ILegislativeAreaService legislativeAreaService)
        {
            _legislativeAreaService = legislativeAreaService;
        }

        [Obsolete("This method is obsolete. Use UKMCAB.Web.UI.Models.Builders CabLegislativeAreasItemViewModelBuilder instead.")]
        public async Task<CABLegislativeAreasItemViewModel> PopulateCABLegislativeAreasItemViewModelAsync(Document? cab, Guid LegislativeAreaId)
        {
            var documentLegislativeArea =
                cab.DocumentLegislativeAreas.Where(n => n.LegislativeAreaId == LegislativeAreaId).First() ??
                throw new InvalidOperationException("document legislative area not found");

            var legislativeArea =
                    await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentLegislativeArea
                        .LegislativeAreaId);

            var legislativeAreaViewModel = new CABLegislativeAreasItemViewModel
            {
                Name = legislativeArea.Name,
                IsProvisional = documentLegislativeArea.IsProvisional,
                IsArchived = documentLegislativeArea.Archived,
                AppointmentDate = documentLegislativeArea.AppointmentDate,
                ReviewDate = documentLegislativeArea.ReviewDate,
                Reason = documentLegislativeArea.Reason,
                RequestReason = documentLegislativeArea.RequestReason,
                PointOfContactName = documentLegislativeArea.PointOfContactName,
                PointOfContactEmail = documentLegislativeArea.PointOfContactEmail,
                PointOfContactPhone = documentLegislativeArea.PointOfContactPhone,
                IsPointOfContactPublicDisplay = documentLegislativeArea.IsPointOfContactPublicDisplay,
                CanChooseScopeOfAppointment = legislativeArea.HasDataModel,
                Status = documentLegislativeArea.Status,
                StatusCssStyle = CssClassUtils.LAStatusStyle(documentLegislativeArea.Status),
                RoleName = Roles.NameFor(documentLegislativeArea.RoleId),
                RoleId = documentLegislativeArea.RoleId,
                LegislativeAreaId = documentLegislativeArea.LegislativeAreaId,
                NewlyCreated = documentLegislativeArea.NewlyCreated,
                MRABypass = documentLegislativeArea.MRABypass
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
                    : null;

                var subCategory = scopeOfAppointment.SubCategoryId.HasValue
                    ? (await _legislativeAreaService.GetSubCategoryByIdAsync(scopeOfAppointment.SubCategoryId
                        .Value))?.Name
                    : null;

                var ppeProductType = scopeOfAppointment.PpeProductTypeId.HasValue
                ? (await _legislativeAreaService.GetPpeProductTypeByIdAsync(scopeOfAppointment.PpeProductTypeId
                    .Value))?.Name
                : null;

                var protectionAgainstRisk = scopeOfAppointment.ProtectionAgainstRiskId.HasValue
                    ? (await _legislativeAreaService.GetProtectionAgainstRiskByIdAsync(scopeOfAppointment.ProtectionAgainstRiskId
                        .Value))?.Name
                    : null;

                foreach (var productProcedure in scopeOfAppointment.ProductIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel()
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        Category = category?.Name,
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

                foreach (var categoryProcedure in scopeOfAppointment.CategoryIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel()
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        Category = category?.Name,
                        SubCategory = subCategory,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (categoryProcedure.CategoryId.HasValue)
                    {
                        category = await _legislativeAreaService.GetCategoryByIdAsync(categoryProcedure.CategoryId.Value);
                        soaViewModel.Category = category!.Name;
                    }

                    foreach (var procedureId in categoryProcedure.ProcedureIds)
                    {
                        var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                        soaViewModel.Procedures?.Add(procedure!.Name);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }

                foreach (var areaOfCompetencyProcedure in scopeOfAppointment.AreaOfCompetencyIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        PpeProductType = ppeProductType,
                        ProtectionAgainstRisk = protectionAgainstRisk,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (areaOfCompetencyProcedure.AreaOfCompetencyId.HasValue)
                    {
                        var areaOfCompetency =
                            await _legislativeAreaService.GetAreaOfCompetencyByIdAsync(areaOfCompetencyProcedure.AreaOfCompetencyId.Value);
                        soaViewModel.AreaOfCompetency = areaOfCompetency!.Name;
                    }

                    foreach (var procedureId in areaOfCompetencyProcedure.ProcedureIds)
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

            return legislativeAreaViewModel;
        }

        [Obsolete("This method is obsolete. Use UKMCAB.Core.Extensions GetLegislativeAreasPendingApprovalByOgd instead.")]
        public List<DocumentLegislativeArea> GetPendingApprovalDocumentLegislativeAreaList(Document document, ClaimsPrincipal user)
        {
            return document.DocumentLegislativeAreas.Where(dla =>
                    (dla.Status == LAStatus.PendingApproval ||
                     dla.Status == LAStatus.PendingApprovalToRemove ||
                     dla.Status == LAStatus.PendingApprovalToArchiveAndArchiveSchedule ||
                     dla.Status == LAStatus.PendingApprovalToArchiveAndRemoveSchedule ||
                     dla.Status == LAStatus.PendingApprovalToUnarchive)
                    && user.IsInRole(dla.RoleId)).ToList();
        }
    }
}
