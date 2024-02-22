using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Web.UI.Services.LegislativeArea
{
    public class LegislativeAreaUtils: ILegislativeAreaUtils
    {
        private readonly IDistCache _distCache;
        private const string CacheKey = "soa_create_{0}";
        public LegislativeAreaUtils(IDistCache distCache)
        {
            _distCache = distCache;
        }
        public async Task CreateScopeOfAppointmentInCacheAsync(Guid scopeId, Guid legislativeAreaId)
        {
            var scopeOfAppointment = new DocumentScopeOfAppointment
            {
                Id = scopeId,
                LegislativeAreaId = legislativeAreaId
            };
            await _distCache.SetAsync(string.Format(CacheKey, scopeId.ToString()), scopeOfAppointment, TimeSpan.FromHours(1));
        }
    }
}
