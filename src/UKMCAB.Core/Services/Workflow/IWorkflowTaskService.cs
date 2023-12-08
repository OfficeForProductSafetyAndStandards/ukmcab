using UKMCAB.Core.Domain.Workflow;

namespace UKMCAB.Core.Services.Workflow;

public interface IWorkflowTaskService
{
    public Task<List<WorkflowTask>> GetUnassignedByForRoleIdAsync(string roleId);
    public Task<List<WorkflowTask>> GetAssignedToGroupForRoleIdAsync(string roleId, string? userIdToExclude = null);
    public Task<List<WorkflowTask>> GetByAssignedUserAsync(string userId);
    public Task<List<WorkflowTask>> GetCompletedForRoleIdAsync(string roleId);

    /// <summary>
    /// Get tasks by CabId and order by last updated date descending
    /// </summary>
    /// <param name="cabId">cab to search for</param>
    /// <returns>Ordered tasks found</returns>
    public Task<List<WorkflowTask>> GetByCabIdAsync(Guid cabId);

    public Task<WorkflowTask> GetAsync(Guid id);
    public Task<WorkflowTask> CreateAsync(WorkflowTask workflowTask);
    public Task<WorkflowTask> UpdateAsync(WorkflowTask workflowTask);
    public Task MarkTaskAsCompletedAsync(Guid taskId, User userLastUpdatedBy);
}