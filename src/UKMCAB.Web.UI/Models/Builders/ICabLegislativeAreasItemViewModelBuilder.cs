using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Web.UI.Models.Builders
{
    public interface ICabLegislativeAreasItemViewModelBuilder
    {
        ICabLegislativeAreasItemViewModelBuilder WithDocumentLegislativeAreaDetails(LegislativeAreaModel legislativeArea, DocumentLegislativeArea documentLegislativeArea);
        ICabLegislativeAreasItemViewModelBuilder WithScopeOfAppointments(LegislativeAreaModel legislativeArea, List<DocumentScopeOfAppointment> documentScopeOfAppointments,
            List<PurposeOfAppointmentModel> purposeOfAppointments, List<CategoryModel> categories, List<SubCategoryModel> subCategories, List<ProductModel> products, List<ProcedureModel> procedures, 
            List<DesignatedStandardModel> designatedStandards, List<PpeProductTypeModel> ppeProductTypes, List<ProtectionAgainstRiskModel> ProtectionAgainstRisks, List<AreaOfCompetencyModel> areaOfCompetencies);
        ICabLegislativeAreasItemViewModelBuilder WithNoOfProductsInScopeOfAppointment();
        CABLegislativeAreasItemViewModel Build();
    }
}
