using System.Security.Claims;

namespace UKMCAB.Core.Security.Extensions
{
    public static class ClaimsExtensions
    {
        public static string GetNameIdentifier(this IEnumerable<Claim> claims)
            => claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value;
    }
}
