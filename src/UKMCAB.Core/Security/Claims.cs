namespace UKMCAB.Core.Security;
public class Claims
{
    public const string CabEdit = "cab.edit";
    public const string Organisation = "org";
}

public static class Roles
{
    public static Role OPSS { get; } = new Role("opss", "OPSS");
    public static Role UKAS { get; } = new Role("ukas", "UKAS");
    public static Role[] List { get; } = new Role[] { OPSS, UKAS };

    public static string? NameFor(string? roleId) => List.FirstOrDefault(x => x.Id == roleId)?.Label;
}

public class Role
{
    public string Id { get; set; }
    public string Label { get; set; }
    public Role(string id, string label)
    {
        Id = id;
        Label = label;
    }
}