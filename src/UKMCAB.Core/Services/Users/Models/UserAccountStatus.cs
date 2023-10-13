namespace UKMCAB.Core.Services.Users.Models;

public enum UserAccountStatus
{
    /// <summary>
    /// No user account and no pending user account request
    /// </summary>
    Unknown,
    PendingUserAccountRequest,
    UserAccountLocked,
    Active,
}
