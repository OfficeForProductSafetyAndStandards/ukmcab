using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<UKMCABUser> _userManager;

        public AdminService(UserManager<UKMCABUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> IsInRoleAsync(ClaimsPrincipal claimsPrincipal, string roleName)
        {
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            if (user == null)
            {
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null)
            {
                return false;
            }

            return userRoles.Any(ur => ur.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
