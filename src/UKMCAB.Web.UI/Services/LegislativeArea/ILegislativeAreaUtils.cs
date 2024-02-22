namespace UKMCAB.Web.UI.Services.LegislativeArea
{
    public interface ILegislativeAreaUtils
    {
        Task CreateScopeOfAppointmentInCacheAsync(Guid scopeId, Guid legislativeAreaId);
    }
}