using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Services.CAB;

public interface ILegislativeAreaService
{
    Task<IEnumerable<LegislativeAreaModel>> GetAllLegislativeAreasAsync();

    Task<IEnumerable<LegislativeAreaModel>> GetLegislativeAreasAsync(List<Guid> excludeLegislativeAreaIds);
    Task<IEnumerable<LegislativeAreaModel>> GetLegislativeAreasByRoleId(string roleId);
    
    Task<LegislativeAreaModel> GetLegislativeAreaByIdAsync(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid purposeOfAppointmentId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid categoryId);  
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForSubCategoryAsync(Guid subCategoryId);  
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForProductAsync(Guid productId);

    Task<PurposeOfAppointmentModel?> GetPurposeOfAppointmentByIdAsync(Guid purposeOfAppointmentId);

    Task<CategoryModel?> GetCategoryByIdAsync(Guid categoryId);

    Task<ProductModel?> GetProductByIdAsync(Guid productId);

    Task<ProcedureModel?> GetProcedureByIdAsync(Guid procedureId);

    Task<SubCategoryModel?> GetSubCategoryByIdAsync(Guid subCategoryId);
    Task<DesignatedStandardModel?> GetDesignatedStandardByIdAsync(Guid designatedStandardId);
    Task<List<LegislativeAreaModel>> GetLegislativeAreasForDocumentAsync(Document document);
    Task<List<PurposeOfAppointmentModel>> GetPurposeOfAppointmentsForDocumentAsync(Document document);
    Task<List<CategoryModel>> GetCategoriesForDocumentAsync(Document document);
    Task<List<SubCategoryModel>> GetSubCategoriesForDocumentAsync(Document document);
    Task<List<ProductModel>> GetProductsForDocumentAsync(Document document);
    Task<List<ProcedureModel>> GetProceduresForDocumentAsync(Document document);
}