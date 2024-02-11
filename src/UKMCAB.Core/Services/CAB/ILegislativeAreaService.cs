using UKMCAB.Core.Domain.LegislativeAreas;

namespace UKMCAB.Core.Services.CAB;

public interface ILegislativeAreaService
{
    Task<IEnumerable<LegislativeAreaModel>> GetAllLegislativeAreasAsync();

    Task<IEnumerable<LegislativeAreaModel>> GetLegislativeAreasAsync(List<Guid?> excludeLegislativeAreaIds);

    Task<LegislativeAreaModel?> GetLegislativeAreaByIdAsync(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid purposeOfAppointmentId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid categoryId);  

    Task<ScopeOfAppointmentOptionsModel?> GetNextScopeOfAppointmentOptionsForProductAsync(Guid productId);

    Task<PurposeOfAppointmentModel?> GetPurposeOfAppointmentByIdAsync(Guid purposeOfAppointmentId);

    Task<CategoryModel?> GetCategoryByIdAsync(Guid categoryId);

    Task<ProductModel?> GetProductByIdAsync(Guid productId);

    Task<ProcedureModel?> GetProcedureByIdAsync(Guid procedureId);

    Task<SubCategoryModel?> GetSubCategoryByIdAsync(Guid subCategoryId);
}