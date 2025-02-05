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
                new(Claims.CabCreateDraft, string.Empty), // can create drafts of CABs
                new(Claims.CabEdit, "*"), // can edit any cab
                new(Claims.CabCanApprove, "*"), // can approve any cab
                new(Claims.CanRequest, "*"), // can make requests on CABs and LAs
                new(Claims.CabManagement, string.Empty), // can manage all cabs
                new(Claims.CabGovernmentUserNotes, string.Empty), // can view/add Government user notes on cabs
                new(Claims.UserManagement, string.Empty), // can manage users
                new (Claims.LegislativeAreaApprove, string.Empty), // Opss admin approve LAs after OGDs
                new (Claims.LegislativeAreaManage, string.Empty) // Opss admin can manage LAs
            };
        }

        if (role == Roles.UKAS.Id)
        {
            return new Claim[]
            {
                new(Claims.CabEdit, "*"), // can edit any cab
                new(Claims.CabCreateDraft, string.Empty), // can create drafts of CABs
                new(Claims.CanRequest, "*"), // can make requests on CABs and LAs
                new(Claims.CabManagement, string.Empty), // can manage all cabs
                new (Claims.LegislativeAreaManage, string.Empty) // can manage LAs
            };
        }
        
        if (Roles.OgdRolesList.Contains(role))
        {
            return new Claim[]
            {
                new(Claims.LegislativeAreaApprove, role), // Approve/Decline Legislative Area
                new(Claims.CabGovernmentUserNotes, string.Empty), // can view/add Government user notes on cabs
                new(Claims.CabManagement, string.Empty), // can manage all cabs
                new (Claims.LegislativeAreaManage, string.Empty) // can manage LAs
            };
        }

        throw new NotSupportedException($"Role '{role}' not supported");
    }
}