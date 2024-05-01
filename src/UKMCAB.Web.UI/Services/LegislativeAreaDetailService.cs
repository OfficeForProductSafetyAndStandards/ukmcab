using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using System.Security.Claims;

namespace UKMCAB.Web.UI.Services
{
    public class LegislativeAreaDetailService : ILegislativeAreaDetailService
    {
        private readonly ILegislativeAreaService _legislativeAreaService;

        public LegislativeAreaDetailService (ILegislativeAreaService legislativeAreaService)
        {
            _legislativeAreaService = legislativeAreaService;
        }

        public async Task<CABLegislativeAreasItemViewModel> PopulateCABLegislativeAreasItemViewModelAsync(Document? cab,
       Guid LegislativeAreaId)
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
                AppointmentDate = documentLegislativeArea.AppointmentDate,
                ReviewDate = documentLegislativeArea.ReviewDate,
                Reason = documentLegislativeArea.Reason,
                CanChooseScopeOfAppointment = legislativeArea.HasDataModel,
                LegislativeAreaId = legislativeArea.Id
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
                    var soaViewModel = new LegislativeAreaListItemViewModel()
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

            return legislativeAreaViewModel;
        }


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
