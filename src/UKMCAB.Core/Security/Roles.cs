namespace UKMCAB.Core.Security;

public static class Roles
{
    public static Role OPSS { get; } = new Role("opss", "OPSS");
    public static Role UKAS { get; } = new Role("ukas", "UKAS");
    public static Role[] List { get; } = { OPSS, UKAS };

    public static string? NameFor(string? roleId) => List.FirstOrDefault(x => x.Id == roleId)?.Label;

    public static string RoleId(string? role)
    {
        var roleId = List.First(r =>
                r.Label != null &&
                r.Label.Equals(role, StringComparison.CurrentCultureIgnoreCase))
            .Id;
        return roleId;
    }

    public static bool IsOpssUser(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return string.Equals(role, OPSS.Label, StringComparison.CurrentCultureIgnoreCase)
               || string.Equals(role, OPSS.Id, StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool IsUkasUser(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return string.Equals(role, UKAS.Label, StringComparison.CurrentCultureIgnoreCase)
               || string.Equals(role, UKAS.Id, StringComparison.CurrentCultureIgnoreCase);
    }
}