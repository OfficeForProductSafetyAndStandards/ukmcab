using UKMCAB.Core.Domain.Workflow;

namespace UKMCAB.Core.Services.Workflow;

public interface IWorkflowTaskService
{
    public Task<List<WorkflowTask>> GetUnassignedBySubmittedUserRoleAsync(string userRole);
    public Task<List<WorkflowTask>> GetByAssignedUserRoleAndStatusAsync(string userRole,
        TaskState taskState);

    public Task<List<WorkflowTask>> GetByAssignedUserAsync(string userRole);
    public Task<WorkflowTask> GetAsync(Guid id);
    public Task<WorkflowTask> CreateAsync(WorkflowTask workflowTask);
    public Task<WorkflowTask> UpdateAsync(WorkflowTask workflowTask);
}