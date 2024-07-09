using System.Security.Claims;

namespace UKMCAB.Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetRoleId(this ClaimsPrincipal principal) => principal.Claims.First(c => c.Type.Equals(ClaimTypes.Role)).Value;
    }
}
