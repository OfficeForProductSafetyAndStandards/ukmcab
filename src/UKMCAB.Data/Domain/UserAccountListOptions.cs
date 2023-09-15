using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Domain;
public record UserAccountListOptions(bool? IsLocked = false, UserAccountLockReason? LockReason = null, int Skip = 0, int Take = 20, string? ExcludeId = null, string? SortField = null, string? SortDirection = null);