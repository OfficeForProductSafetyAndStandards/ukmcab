using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Core.Services.CAB;

public interface ILegislativeAreaService
{
    Task<IEnumerable<LegislativeAreaModel>> GetAllLegislativeAreasAsync();

    Task<IEnumerable<LegislativeAreaModel>> GetLegislativeAreasAsync(List<Guid> excludeLegislativeAreaIds);
    Task<IEnumerable<LegislativeAreaModel>> GetLegislativeAreasByRoleId(string roleId);
    
    Task<LegislativeAreaModel> GetLegislativeAreaByIdAsync(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid legislativeAreaId, int? pageNumber = null, string? searchTerm = null, int pageSize = 20, List<Guid>? designatedStandardIds = null, bool isShowAllSelectedDesignatedRequest = false, bool ShowAllSelectedIsOn = false);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid purposeOfAppointmentId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid categoryId);  
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForSubCategoryAsync(Guid subCategoryId);  
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForProductAsync(Guid productId);
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPpeProductTypeAsync();
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForProtectionAgainstRiskAsync();
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPpeCategoryAsync(Guid ppeCategoryId);
    Task<ScopeOfAppointmentOptionsModel> GetPpeProductTypeScopeOfAppointmentOptionsAsync();
    Task<ScopeOfAppointmentOptionsModel> GetProtectionAgainstRiskScopeOfAppointmentOptionsAsync();
    Task<ScopeOfAppointmentOptionsModel> GetAreaOfCompetencyScopeOfAppointmentOptionsAsync();
    Task<ScopeOfAppointmentOptionsModel> GetPpeProcedureScopeOfAppointmentOptionsAsync();
    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForAreaOfCompetencyAsync(Guid areaOfCompetencyId);
    Task<PurposeOfAppointmentModel?> GetPurposeOfAppointmentByIdAsync(Guid purposeOfAppointmentId);

    Task<CategoryModel?> GetCategoryByIdAsync(Guid categoryId);

    Task<ProductModel?> GetProductByIdAsync(Guid productId);

    Task<ProcedureModel?> GetProcedureByIdAsync(Guid procedureId);

    Task<SubCategoryModel?> GetSubCategoryByIdAsync(Guid subCategoryId);
    Task<PpeCategoryModel?> GetPpeCategoryByIdAsync(Guid ppeCategoryId);
    Task<PpeProductTypeModel?> GetPpeProductTypeByIdAsync(Guid ppeProductTypeId);
    Task<ProtectionAgainstRiskModel?> GetProtectionAgainstRiskByIdAsync(Guid protectionAgainstRiskId);
    Task<AreaOfCompetencyModel?> GetAreaOfCompetencyByIdAsync(Guid areaOfCompetencyId);
    Task<DesignatedStandardModel?> GetDesignatedStandardByIdAsync(Guid designatedStandardId);
    Task<List<LegislativeAreaModel>> GetLegislativeAreasForDocumentAsync(Document document);
    Task<List<PurposeOfAppointmentModel>> GetPurposeOfAppointmentsForDocumentAsync(Document document);
    Task<List<CategoryModel>> GetCategoriesForDocumentAsync(Document document);
    Task<List<SubCategoryModel>> GetSubCategoriesForDocumentAsync(Document document);
    Task<List<ProductModel>> GetProductsForDocumentAsync(Document document);
    Task<List<ProcedureModel>> GetProceduresForDocumentAsync(Document document);
    Task<List<DesignatedStandardModel>> GetDesignatedStandardsForDocumentAsync(Document document);
    Task<List<PpeProductTypeModel>> GetPpeProductTypesForDocumentAsync(Document document);
    Task<List<ProtectionAgainstRiskModel>> GetProtectionAgainstRisksForDocumentAsync(Document document);
    Task<List<AreaOfCompetencyModel>> GetAreaOfCompetenciesForDocumentAsync(Document document);
}