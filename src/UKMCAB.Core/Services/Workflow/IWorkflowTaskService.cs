using UKMCAB.Core.Domain.Workflow;

namespace UKMCAB.Core.Services.Workflow;

public interface IWorkflowTaskService
{
    public Task<List<WorkflowTask>> GetUnassignedBySubmittedUserRoleAsync(string userRole);
    public Task<List<WorkflowTask>> GetByAssignedUserRoleAndCompletedAsync(string assignedUserRole,
        bool completed = false);

    public Task<List<WorkflowTask>> GetByAssignedUserAsync(string userRole);
    public Task<WorkflowTask> GetAsync(Guid id);
    public Task<WorkflowTask> CreateAsync(WorkflowTask workflowTask);
    public Task<WorkflowTask> UpdateAsync(WorkflowTask workflowTask);
}