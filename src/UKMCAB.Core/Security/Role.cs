namespace UKMCAB.Core.Security;

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