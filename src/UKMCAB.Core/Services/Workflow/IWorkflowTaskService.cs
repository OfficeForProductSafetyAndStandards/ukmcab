using UKMCAB.Core.Domain.Workflow;

namespace UKMCAB.Core.Services.Workflow;

public interface IWorkflowTaskService
{
    public Task<List<WorkflowTask>> GetUnassignedByForRoleIdAsync(string roleId);
    public Task<List<WorkflowTask>> GetByForRoleAndCompletedAsync(string assignedUserRole, bool completed = false);

    public Task<List<WorkflowTask>> GetByAssignedUserAsync(string userId);
    public Task<WorkflowTask> GetAsync(Guid id);
    public Task<WorkflowTask> CreateAsync(WorkflowTask workflowTask);
    public Task<WorkflowTask> UpdateAsync(WorkflowTask workflowTask);
}