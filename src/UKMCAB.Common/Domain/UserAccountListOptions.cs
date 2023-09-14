namespace UKMCAB.Common.Domain;
public record UserAccountListOptions(bool? IsLocked = false, int? LockReason = null, int Skip = 0, int Take = 20, string? ExcludeId = null);