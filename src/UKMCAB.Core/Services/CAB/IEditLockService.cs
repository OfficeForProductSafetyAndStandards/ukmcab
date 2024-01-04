namespace UKMCAB.Core.Services.CAB;

public interface IEditLockService
{
    /// <summary>
    /// Returns the user id if any of the cab edit lock
    /// </summary>
    /// <param name="cabId">cab to check</param>
    /// <returns>UserId with lock</returns>
    Task<string?> LockExistsForCabAsync(string cabId);
    Task SetAsync(string cabId, string userId);
    public Task RemoveEditLockForUserAsync(string userId);
}