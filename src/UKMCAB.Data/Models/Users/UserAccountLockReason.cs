namespace UKMCAB.Data.Models.Users;

public enum UserAccountLockReason
{
    /// <summary>
    /// It's locked cos it's locked.
    /// </summary>
    None,

    /// <summary>
    /// It's locked cos it's archived.
    /// </summary>
    Archived,
}

public enum UserAccountUnlockReason
{
    /// <summary>
    /// It's unlocked cos it's unlocked.
    /// </summary>
    None,

    /// <summary>
    /// It's unlocked cos it's unarchived.
    /// </summary>
    Unarchived,
}
