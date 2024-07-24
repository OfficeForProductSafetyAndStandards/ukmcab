namespace UKMCAB.Core.Services.CAB;

public interface IEditLockService
{
    /// <summary>
    /// Returns true if the CAB does not have a edit lock matching the user
    /// Returns false if the CAB not found or if CAB has a edit locked matching the user
    /// </summary>
    /// <param name="cabId">cab to check</param>
    /// <param name="userId">id of user</param>
    /// <returns>UserId with lock</returns>
    Task<bool> IsCabLockedForUser(string cabId, string userId);
    
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
    /// Remove cab from edit lock
    /// </summary>
    /// <param name="cabId">Cab to clear lock for</param>
    /// <returns></returns>
    Task RemoveEditLockForCabAsync(string cabId);
}