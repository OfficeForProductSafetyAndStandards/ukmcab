using System.Security.Claims;
using UKMCAB.Common;
using UKMCAB.Core.Security;
using UKMCAB.Data;

namespace UKMCAB.Core.Domain;

public static class CabNumberVisibility
{
    public static CabNumberVisibilityOption Default { get; } = new(DataConstants.CabNumberVisibilityOptions.Default, "Select an option");
    public static CabNumberVisibilityOption Public { get; } = new(DataConstants.CabNumberVisibilityOptions.Public, "Display for all users");
    public static CabNumberVisibilityOption Internal { get; } = new(DataConstants.CabNumberVisibilityOptions.Internal, "Display for internal users");
    public static CabNumberVisibilityOption Private { get; } = new(DataConstants.CabNumberVisibilityOptions.Private, "Display for internal users excluding UKAS");
    public static CabNumberVisibilityOption[] Options { get; } = new[] { Default, Public, Internal, Private };
    public static CabNumberVisibilityOption Get(string? id) => Options.FirstOrDefault(x => x.Id == id) ?? Public;

    public static string Display(string? optionId, ClaimsPrincipal principal, string? cabNumber) => CanDisplay(optionId, principal) ? cabNumber.Clean() : null;

    public static bool CanDisplay(string? optionId, ClaimsPrincipal principal)
    {
        var option = Get(optionId);
        return option == Public
            || (option == Internal && (principal.Identity?.IsAuthenticated ?? false))
            || (option == Private && principal.IsInRole(Roles.OPSS.Id));
    }
}

public class CabNumberVisibilityOption
{
    internal CabNumberVisibilityOption(string? id, string label)
    {
        Id = id;
        Label = label;
    }

    public string? Id { get; }
    public string Label { get; }
}
