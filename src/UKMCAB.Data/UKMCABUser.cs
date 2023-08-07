namespace UKMCAB.Data;

[Obsolete("Need to migrate to claims principal")]
public class UKMCABUser
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<string> Regulations { get; set; } = new List<string>();
    public string? RequestReason { get; set; }
    public bool RequestApproved { get; set; }
}
