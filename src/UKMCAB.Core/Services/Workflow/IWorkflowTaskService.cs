using UKMCAB.Core.Domain.WorkflowTask;

namespace UKMCAB.Core.Services.Workflow;

public interface IWorkflowTaskService
{
    public Task<List<Domain.WorkflowTask.WorkflowTask>> GetUnassignedBySubmittedUserRole(string userRole);
    public Task<List<Domain.WorkflowTask.WorkflowTask>> GetByAssignedUserRoleAndStatus(string userRole,
        TaskState taskState);

    public Task<List<Domain.WorkflowTask.WorkflowTask>> GetByAssignedUser(string userRole);
    public Task<Domain.WorkflowTask.WorkflowTask> GetAsync(Domain.WorkflowTask.WorkflowTask workflowTask);
    public Task<Domain.WorkflowTask.WorkflowTask> CreateAsync(Domain.WorkflowTask.WorkflowTask workflowTask);
    public Task<Domain.WorkflowTask.WorkflowTask> UpdateAsync(Domain.WorkflowTask.WorkflowTask workflowTask);
}