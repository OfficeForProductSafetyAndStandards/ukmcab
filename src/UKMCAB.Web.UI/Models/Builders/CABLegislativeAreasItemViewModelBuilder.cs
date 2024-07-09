using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Core.Domain.LegislativeAreas;

namespace UKMCAB.Web.UI.Models.Builders
{
    public class CabLegislativeAreasItemViewModelBuilder : ICabLegislativeAreasItemViewModelBuilder
    {
        private CABLegislativeAreasItemViewModel _model;

        public CabLegislativeAreasItemViewModelBuilder()
        {
            _model = new CABLegislativeAreasItemViewModel();
        }

        public CABLegislativeAreasItemViewModel Build()
        {
            var model = _model;
            _model = new CABLegislativeAreasItemViewModel();
            return model;
        }

        public ICabLegislativeAreasItemViewModelBuilder WithDocumentLegislativeAreaDetails(LegislativeAreaModel legislativeArea, DocumentLegislativeArea documentLegislativeArea)
        {
            _model = new CABLegislativeAreasItemViewModel
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
                LegislativeAreaId = documentLegislativeArea.LegislativeAreaId
            };
            return this;
        }

        // TODO: Need to split into multiple methods
        public ICabLegislativeAreasItemViewModelBuilder WithScopeOfAppointments(
            LegislativeAreaModel legislativeArea,
            List<DocumentScopeOfAppointment> documentScopeOfAppointments,
            List<PurposeOfAppointmentModel> purposeOfAppointments,
            List<CategoryModel> categories,
            List<SubCategoryModel> subCategories,
            List<ProductModel> products,
            List<ProcedureModel> procedures)
        {
            foreach (var scopeOfAppointment in documentScopeOfAppointments)
            {
                var purposeOfAppointment = purposeOfAppointments.FirstOrDefault(p => p.Id == scopeOfAppointment.PurposeOfAppointmentId);
                //var category = categories.FirstOrDefault(p => p.Id == scopeOfAppointment.CategoryId);
                var subCategory = subCategories.FirstOrDefault(p => p.Id == scopeOfAppointment.SubCategoryId);

                foreach (var categoryIdAndProcedureIds in scopeOfAppointment.CategoryIdAndProcedureIds)
                {
                    var category = categories.FirstOrDefault(p => p.Id == categoryIdAndProcedureIds.CategoryId);

                    var procedureNames = new List<string>();
                    foreach (var procedureId in categoryIdAndProcedureIds.ProcedureIds)
                    {
                        var procedure = procedures.First(p => p.Id == procedureId);
                        procedureNames.Add(procedure.Name);
                    }

                    var scopeOfAppointmentViewModel = new LegislativeAreaListItemViewModel(
                        legislativeArea.Id,
                        legislativeArea.Name,
                        purposeOfAppointment?.Name,
                        category?.Name,
                        subCategory?.Name,
                        scopeOfAppointment.Id,
                        null,
                        procedureNames);

                    _model.ScopeOfAppointments.Add(scopeOfAppointmentViewModel);
                }

                foreach (var productAndProcedures in scopeOfAppointment.ProductIdAndProcedureIds)
                {
                    var product = products.FirstOrDefault(p => p.Id == productAndProcedures.ProductId);

                    var procedureNames = new List<string>();
                    foreach (var procedureId in productAndProcedures.ProcedureIds)
                    {
                        var procedure = procedures.First(p => p.Id == procedureId);
                        procedureNames.Add(procedure.Name);
                    }

                    var scopeOfAppointmentViewModel = new LegislativeAreaListItemViewModel(
                        legislativeArea.Id,
                        legislativeArea.Name,
                        purposeOfAppointment?.Name,
                        null,
                        subCategory?.Name,
                        scopeOfAppointment.Id,
                        product?.Name,
                        procedureNames);

                    _model.ScopeOfAppointments.Add(scopeOfAppointmentViewModel);
                }
            }
            return this;
        }

        public ICabLegislativeAreasItemViewModelBuilder WithNoOfProductsInScopeOfAppointment()
        {
            var distinctScopeOfAppointments = _model.ScopeOfAppointments.GroupBy(s => s.ScopeId).ToList();
            foreach (var scopeOfAppointmentGroup in distinctScopeOfAppointments)
            {
                var scopeOfAppointment = _model.ScopeOfAppointments.First(s => s.ScopeId == scopeOfAppointmentGroup.Key);
                scopeOfAppointment.NoOfProductsInScopeOfAppointment = scopeOfAppointmentGroup.Count();
            }
            return this;
        }
    }
}
