namespace UKMCAB.Data.Models.Users;

public class UserAccountRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SubjectId { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? Surname { get; set; }
    public string? OrganisationName { get; set; }
    public string? EmailAddress { get; set; }
    public string? ContactEmailAddress { get; set; }
    public string? GetEmailAddress() => ContactEmailAddress ?? EmailAddress;
    public string? Comments { get; set; }
    public UserAccountRequestStatus Status { get; set; } = UserAccountRequestStatus.Pending;
    public string? ReviewComments { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}
