using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.Security;
public class SignInHelper
{
    public static void AddClaims(UserAccount account, ClaimsIdentity identity)
    {
        identity.AddClaim(new Claim(ClaimTypes.Email, account.ContactEmailAddress ?? account.EmailAddress ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.GivenName, account.FirstName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.Surname, account.Surname ?? string.Empty));
        identity.AddClaim(new Claim(Claims.Organisation, account.OrganisationName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.Role, account.RoleId ?? string.Empty));
        identity.AddClaims(ClaimsIssuer.Get(account.RoleId));
    }
}
