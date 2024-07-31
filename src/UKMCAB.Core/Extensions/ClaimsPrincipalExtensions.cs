using System.Security.Claims;
using UKMCAB.Core.Extensions;
using UKMCAB.Core.Security;

namespace UKMCAB.Common.Extensions
{
    public static class ClaimsPrincipalExtensions 
    {
        public static bool HasOgdRole(this ClaimsPrincipal principal) => Roles.OgdRolesList.Contains(principal.GetRoleId());
    }
}
