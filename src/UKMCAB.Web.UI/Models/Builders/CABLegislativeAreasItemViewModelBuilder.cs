using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Core.Extensions;
using UKMCAB.Common.Exceptions;

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
                RoleName = Roles.NameFor(documentLegislativeArea.RoleId) ?? throw new NotFoundException($"Role name could not be resolved for id: {documentLegislativeArea.RoleId}."),
                RoleId = documentLegislativeArea.RoleId,
                LegislativeAreaId = documentLegislativeArea.LegislativeAreaId
            };
            return this;
        }

        public ICabLegislativeAreasItemViewModelBuilder WithScopeOfAppointments(
            LegislativeAreaModel legislativeArea,
            List<DocumentScopeOfAppointment> documentScopeOfAppointments,
            List<PurposeOfAppointmentModel> purposeOfAppointments,
            List<CategoryModel> categories,
            List<SubCategoryModel> subCategories,
            List<ProductModel> products,
            List<ProcedureModel> procedures,
            List<DesignatedStandardModel> designatedStandards)
        {
            foreach (var documentScopeOfAppointment in documentScopeOfAppointments)
            {

                var purposeOfAppointment = purposeOfAppointments.FirstOrDefault(p => p.Id == documentScopeOfAppointment.PurposeOfAppointmentId);
                var subCategory = subCategories.FirstOrDefault(p => p.Id == documentScopeOfAppointment.SubCategoryId);

                var scopeOfAppointmentViewModelForCategories = CreateScopeOfAppointmentViewModelForCategories(
                    documentScopeOfAppointment.Id,
                    legislativeArea.Id,
                    legislativeArea.Name,
                    documentScopeOfAppointment.CategoryIdAndProcedureIds,
                    categories,
                    procedures,
                    subCategory?.Name,
                    purposeOfAppointment?.Name);

                _model.ScopeOfAppointments.AddRange(scopeOfAppointmentViewModelForCategories);

                var scopeOfAppointmentViewModelForProducts = CreateScopeOfAppointmentViewModelForProducts(
                    documentScopeOfAppointment.Id,
                    legislativeArea.Id,
                    legislativeArea.Name,
                    documentScopeOfAppointment.ProductIdAndProcedureIds,
                    products,
                    procedures,
                    subCategory?.Name,
                    purposeOfAppointment?.Name);

                _model.ScopeOfAppointments.AddRange(scopeOfAppointmentViewModelForProducts);

                // TODO: Remove hardcoded values once UKMCAB-1709 is complete
                documentScopeOfAppointment.DesignatedStandardIds = new List<Guid> { Guid.Parse("b4be7979-c764-4cc6-9db5-b333892bfc94"), Guid.Parse("afc862a2-24e1-46af-9ccc-39a63c487123") };

                if (documentScopeOfAppointment.DesignatedStandardIds.Any())
                {
                    var scopeOfAppointmentViewModelForDesignatedStandards = CreateScopeOfAppointmentViewModelForDesignatedStandards(
                    documentScopeOfAppointment.Id,
                    legislativeArea.Id,
                    legislativeArea.Name,
                    subCategory?.Name,
                    purposeOfAppointment?.Name,
                    documentScopeOfAppointment.DesignatedStandardIds,
                    designatedStandards);

                    _model.ScopeOfAppointments.Add(scopeOfAppointmentViewModelForDesignatedStandards);
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

        private List<LegislativeAreaListItemViewModel> CreateScopeOfAppointmentViewModelForProducts(
            Guid documentScopeOfAppointmentId,
            Guid legislativeAreaId,
            string legislativeAreaName,
            List<ProductAndProcedures> productIdAndProcedureIds,
            List<ProductModel> products,
            List<ProcedureModel> procedures,
            string? subCategoryName,
            string? purposeOfAppointmentName
            )
        {
            var scopeOfAppointments = new List<LegislativeAreaListItemViewModel>();
            foreach (var productAndProcedures in productIdAndProcedureIds)
            {
                var product = products.FirstOrDefault(p => p.Id == productAndProcedures.ProductId);
                var procedureNames = procedures.GetNamesByIds(productAndProcedures.ProcedureIds);

                var scopeOfAppointmentViewModel = new LegislativeAreaListItemViewModel(
                    legislativeAreaId,
                    legislativeAreaName,
                    purposeOfAppointmentName,
                    null,
                    subCategoryName,
                    documentScopeOfAppointmentId,
                    product?.Name,
                    procedureNames);

                _model.ScopeOfAppointments.Add(scopeOfAppointmentViewModel);
            }
            return scopeOfAppointments;
        }

        private List<LegislativeAreaListItemViewModel> CreateScopeOfAppointmentViewModelForCategories(
            Guid documentScopeOfAppointmentId,
            Guid legislativeAreaId,
            string legislativeAreaName,
            List<CategoryAndProcedures> categoryIdAndProcedureIds,
            List<CategoryModel> categories,
            List<ProcedureModel> procedures,
            string? subCategoryName,
            string? purposeOfAppointmentName
            )
        {
            var scopeOfAppointments = new List<LegislativeAreaListItemViewModel>();
            foreach (var categoryAndProcedures in categoryIdAndProcedureIds)
            {
                var category = categories.FirstOrDefault(p => p.Id == categoryAndProcedures.CategoryId);
                var procedureNames = procedures.GetNamesByIds(categoryAndProcedures.ProcedureIds);

                var model = new LegislativeAreaListItemViewModel(
                    legislativeAreaId,
                    legislativeAreaName,
                    purposeOfAppointmentName,
                    category?.Name,
                    subCategoryName,
                    documentScopeOfAppointmentId,
                    null,
                    procedureNames);

                scopeOfAppointments.Add(model);
            }
            return scopeOfAppointments;
        }

        private LegislativeAreaListItemViewModel CreateScopeOfAppointmentViewModelForDesignatedStandards(
            Guid documentScopeOfAppointmentId,
            Guid legislativeAreaId,
            string legislativeAreaName,
            string? subCategoryName,
            string? purposeOfAppointmentName,
            List<Guid> designatedStandardIds,
            List<DesignatedStandardModel> designatedStandards
            )
        {
            var soaDesignatedStandards = designatedStandards.Where(d => designatedStandardIds.Contains(d.Id));
            var soaDesignatedStandardsViewModels = soaDesignatedStandards.Select(d => new DesignatedStandardReadOnlyViewModel(d.Id, d.Name, d.ReferenceNumber, d.NoticeOfPublicationReference)).ToList();
                
            var model = new LegislativeAreaListItemViewModel(
                legislativeAreaId,
                legislativeAreaName,
                purposeOfAppointmentName,
                null,
                subCategoryName,
                documentScopeOfAppointmentId,
                null,
                null,
                designatedStandards: soaDesignatedStandardsViewModels);

            return model;
        }
    }
}
