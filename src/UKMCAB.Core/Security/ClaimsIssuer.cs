using System.Security.Claims;

namespace UKMCAB.Core.Security;

public static class ClaimsIssuer
{
    /// <summary>
    /// Given a role, this will supply a list of granular claims.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static IEnumerable<Claim> Get(string role)
    {
        if (role == Roles.OPSS.Id)
        {
            return new Claim[]
            {
                new(Claims.CabEdit, "*"), // can edit any cab
                new(Claims.CabCanApprove, "*"), // can approve any cab
                new(Claims.CabManagement, string.Empty), // can manage all cabs
                new(Claims.CabGovernmentUserNotes, string.Empty), // can view/add Government user notes on cabs
                new(Claims.UserManagement, string.Empty), // can manage users
            };
        }

        if (role == Roles.UKAS.Id)
        {
            return new Claim[]
            {
                new(Claims.CabEdit, "*"), // can edit any cab
                new(Claims.CabCreateDraft, string.Empty), // can create drafts of CABs
                new(Claims.CabManagement, string.Empty), // can manage all cabs
            };
        }

        //TODO : Permission will be assigned later
        if (Roles.OgdRolesList.Contains(role))
        {
            return new Claim[]
            {
                new(Claims.IsOneLoginUser, "*"),
                new(Claims.CabEdit, "*"), // can edit any cab
                //new(Claims.CabManagement, string.Empty), // can manage all cabs
            };
        }

        throw new NotSupportedException($"Role '{role}' not supported");
    }
}