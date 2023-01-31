using System.Security.Claims;

namespace UKMCAB.Web.UI.Services
{
    public interface IAdminService
    {
        public Task<bool> IsInRoleAsync(ClaimsPrincipal claimsPrincipal, IEnumerable<string> roleNames);
    }
}
