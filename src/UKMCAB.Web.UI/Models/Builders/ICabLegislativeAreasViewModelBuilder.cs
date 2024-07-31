using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Models.Builders
{
    public interface ICabLegislativeAreasViewModelBuilder
    {
        ICabLegislativeAreasViewModelBuilder WithDocumentLegislativeAreas(List<DocumentLegislativeArea> documentLegislativeAreas, List<LegislativeAreaModel> legislativeAreas, List<DocumentScopeOfAppointment> scopeOfAppointments,
            List<PurposeOfAppointmentModel> purposeOfAppointments, List<CategoryModel> categories, List<SubCategoryModel> subCategories, List<ProductModel> products, List<ProcedureModel> procedures);
        CABLegislativeAreasViewModel Build();
    }
}
