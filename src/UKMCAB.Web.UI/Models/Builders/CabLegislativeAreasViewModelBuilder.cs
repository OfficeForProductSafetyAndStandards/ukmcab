using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Models.Builders
{
    public class CabLegislativeAreasViewModelBuilder : ICabLegislativeAreasViewModelBuilder
    {
        private readonly ICabLegislativeAreasItemViewModelBuilder _cabLegislativeAreasItemViewModelBuilder;
        private CABLegislativeAreasViewModel _model;

        public CabLegislativeAreasViewModelBuilder(ICabLegislativeAreasItemViewModelBuilder cabLegislativeAreasItemViewModelBuilder)
        {
            _cabLegislativeAreasItemViewModelBuilder = cabLegislativeAreasItemViewModelBuilder;
            _model = new CABLegislativeAreasViewModel();
        }

        public CABLegislativeAreasViewModel Build()
        {
            var model = _model;
            _model = new CABLegislativeAreasViewModel();
            return model;
        }

        public ICabLegislativeAreasViewModelBuilder WithDocumentLegislativeAreas(
            List<DocumentLegislativeArea> documentLegislativeAreas, 
            List<LegislativeAreaModel> legislativeAreas, 
            List<DocumentScopeOfAppointment> scopeOfAppointments,
            List<PurposeOfAppointmentModel> purposeOfAppointments,
            List<CategoryModel> categories,
            List<SubCategoryModel> subCategories,
            List<ProductModel> products,
            List<ProcedureModel> procedures,
            List<DesignatedStandardModel> designatedStandards)
        {
            foreach (var documentLegislativeArea in documentLegislativeAreas)
            {
                var legislativeArea = legislativeAreas.Single(la => la.Id == documentLegislativeArea.LegislativeAreaId);
                var documentScopeOfAppointments = scopeOfAppointments.Where(s => s.LegislativeAreaId == legislativeArea.Id).ToList();

                var viewModel = _cabLegislativeAreasItemViewModelBuilder
                    .WithDocumentLegislativeAreaDetails(legislativeArea, documentLegislativeArea)
                    .WithScopeOfAppointments(
                        legislativeArea,
                        documentScopeOfAppointments,
                        purposeOfAppointments,
                        categories,
                        subCategories,
                        products,
                        procedures,
                        designatedStandards)
                    .WithNoOfProductsInScopeOfAppointment()
                    .Build();

                if (viewModel.IsArchived == true)
                {
                    _model.ArchivedLegislativeAreas.Add(viewModel);
                }
                else
                {
                    _model.ActiveLegislativeAreas.Add(viewModel);
                }
            }
            return this;
        }
    }
}
