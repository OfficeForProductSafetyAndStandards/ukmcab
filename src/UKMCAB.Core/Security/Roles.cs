namespace UKMCAB.Core.Security;

public static class Roles
{
    public static Role DFTP { get; } = new("dftp", "DFTP");
    public static Role DFTR { get; } = new("dftr", "DFTR");
    public static Role DLUHC { get; } = new("dluhc", "DLUHC");
    public static Role MCGA { get; } = new("mcga", "MCGA");
    public static Role MHRA { get; } = new("mhra", "MHRA");
    public static Role OPSS { get; } = new("opss", "OPSS");
    public static Role OPSS_OGD { get; } = new("opss_ogd", "OPSS (OGD)");
    public static Role UKAS { get; } = new("ukas", "UKAS");

    public static IEnumerable<string> OgdRolesList { get; } =
        new List<string> { DFTP.Id, DFTR.Id, DLUHC.Id, MCGA.Id, MHRA.Id, OPSS_OGD.Id };

    public static IEnumerable<Role> List { get; } =
        new List<Role> { DFTP, DFTR, DLUHC, MCGA, MHRA, OPSS, OPSS_OGD, UKAS };

    public static string? NameFor(string? roleId) => List.FirstOrDefault(x => x.Id == roleId)?.Label;

    public static string RoleId(string? role)
    {
        var roleId = List.First(r =>
                r.Label != null &&
                r.Label.Equals(role, StringComparison.CurrentCultureIgnoreCase))
            .Id;
        return roleId;
    }
}