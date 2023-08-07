namespace UKMCAB.Data.Models.Users;

public class UserAccount
{
    public string Id { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? Surname { get; set; }
    public string? EmailAddress { get; set; }
    public string? OrganisationName { get; set; }
    public string? ContactEmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsLocked { get; set; }
    public UserAccountLockReason? LockReason { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    public const string LastLogonUtcFieldName = "lastLogonUtc";
    public DateTime? LastLogonUtc { get; set; }
    public string? LockReasonDescription { get; set; }
    public string? LockInternalNotes { get; set; }
}
