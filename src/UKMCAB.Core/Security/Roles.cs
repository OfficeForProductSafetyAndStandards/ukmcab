namespace UKMCAB.Core.Security;

public static class Roles
{
    public static Role OPSS { get; } = new Role("opss", "OPSS");
    public static Role UKAS { get; } = new Role("ukas", "UKAS");
    public static Role[] List { get; } = new Role[] { OPSS, UKAS };

    public static string? NameFor(string? roleId) => List.FirstOrDefault(x => x.Id == roleId)?.Label;
}
