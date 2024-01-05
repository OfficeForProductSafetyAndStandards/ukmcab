namespace UKMCAB.Core.Services.CAB;

public interface IEditLockService
{
    /// <summary>
    /// Returns the user id if any of the cab edit lock
    /// </summary>
    /// <param name="cabId">cab to check</param>
    /// <returns>UserId with lock</returns>
    Task<string?> LockExistsForCabAsync(string cabId);
    
    /// <summary>
    /// Adds Cab to cache for edit lock with associated user
    /// </summary>
    /// <param name="cabId">cab to lock</param>
    /// <param name="userId">user with lock</param>
    /// <returns></returns>
    Task SetAsync(string cabId, string userId);
    
    /// <summary>
    /// Remove user from edit lock
    /// </summary>
    /// <param name="userId">User to clear lock for</param>
    /// <returns></returns>
    Task RemoveEditLockForUserAsync(string userId);
    
    /// <summary>
    /// Remove cabId from edit lock
    /// </summary>
    /// <param name="cabId">Cab to clear lock for</param>
    /// <returns></returns>
    Task RemoveEditLockForCabAsync(string cabId);
}