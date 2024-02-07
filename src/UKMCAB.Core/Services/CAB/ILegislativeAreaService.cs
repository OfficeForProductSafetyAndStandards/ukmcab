using UKMCAB.Core.Domain.LegislativeAreas;

namespace UKMCAB.Core.Services.CAB;

public interface ILegislativeAreaService
{
    Task<IEnumerable<LegislativeAreaModel>> GetAllLegislativeAreasAsync();

    Task<LegislativeAreaModel?> GetLegislativeAreaByIdAsync(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid purposeOfAppointmentId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid categoryId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForSubcategoryAsync(Guid categoryId);

    Task<ScopeOfAppointmentOptionsModel?> GetNextScopeOfAppointmentOptionsForProductAsync(Guid productId);

    Task<PurposeOfAppointmentModel?> GetPurposeOfAppointmentByIdAsync(Guid purposeOfAppointmentId);

    Task<CategoryModel?> GetCategoryByIdAsync(Guid categoryId);
}