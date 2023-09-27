using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Domain;

public record SortBy(string? Name, string? Direction);
public record UserAccountListOptions(SkipTake SkipTake, SortBy SortBy,  bool? IsLocked = false, UserAccountLockReason? LockReason = null, string? ExcludeId = null);
public record UserAccountRequestListOptions(SkipTake SkipTake, SortBy SortBy, UserAccountRequestStatus? Status = null);

public record SkipTake
{
    public SkipTake(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    public int Skip { get; }
    public int Take { get; }

    public static SkipTake FromPage(int pageIndex, int pageSize)
    {
        return new SkipTake(pageIndex * pageSize, pageSize);
    }
}

