namespace UKMCAB.Data.Models.Users;

public class UserAccount
{

    public string Id { get; set; } = null!;
    public List<Audit> AuditLog { get; set; }

    public string? FirstName { get; set; }
    
    private string? _surname;
    public string? Surname
    {
        get 
        { 
            return _surname; 
        }
        set
        {
            _surname = value;
            SurnameNormalized = value?.ToLowerInvariant();
        }
    }
    public string? SurnameNormalized { get; set; }
    public string? OrganisationName { get; set; }
    public string? EmailAddress { get; set; }
    public string? ContactEmailAddress { get; set; }
    public bool IsLocked { get; set; }
    public bool IsArchived => LockReason == UserAccountLockReason.Archived;
    public UserAccountLockReason? LockReason { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    public const string LastLogonUtcFieldName = "lastLogonUtc";

    public DateTime? LastLogonUtc { get; set; }
    public string? LockReasonDescription { get; set; }
    public string? LockInternalNotes { get; set; }
    
    /// <summary>
    /// The Role Label from Roles.
    /// </summary>
    public string? Role { get; set; }

    public string? GetEmailAddress() => ContactEmailAddress ?? EmailAddress;
    public string Status
    {
        get
        {
            if (IsArchived)
            {
                return "Archived";
            }
            else if (IsLocked)
            {
                return "Locked";
            }
            return "Active";
        }
    }
}
