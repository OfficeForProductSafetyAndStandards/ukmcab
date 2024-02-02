using UKMCAB.Core.Domain.LegislativeAreas;

namespace UKMCAB.Core.Services.CAB;

public interface ILegislativeAreaService
{
    Task<IEnumerable<LegislativeAreaModel>> GetAllLegislativeAreas();

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForLegislativeArea(Guid legislativeAreaId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPurposeOfAppointment(Guid purposeOfAppointmentId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForCategory(Guid categoryId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForSubcategory(Guid categoryId);

    Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForProduct(Guid productId);
}