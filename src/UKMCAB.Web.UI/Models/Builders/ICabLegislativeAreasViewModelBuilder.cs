using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Models.Builders
{
    public interface ICabLegislativeAreasViewModelBuilder
    {
        ICabLegislativeAreasViewModelBuilder WithDocumentLegislativeAreas(List<DocumentLegislativeArea> documentLegislativeAreas, List<LegislativeAreaModel> legislativeAreas, List<DocumentScopeOfAppointment> scopeOfAppointments,
            List<PurposeOfAppointmentModel> purposeOfAppointments, List<CategoryModel> categories, List<SubCategoryModel> subCategories, List<ProductModel> products, List<ProcedureModel> procedures, List<DesignatedStandardModel> designatedStandards);
        CABLegislativeAreasViewModel Build();
    }
}
