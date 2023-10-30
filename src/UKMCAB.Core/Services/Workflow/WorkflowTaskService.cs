using UKMCAB.Core.Domain.WorkflowTask;
using UKMCAB.Core.Mappers;
using UKMCAB.Data.CosmosDb.Services.WorkflowTask;

namespace UKMCAB.Core.Services.Workflow;

public class WorkflowTaskService : IWorkflowTaskService
{
    private readonly IWorkflowTaskRepository _workflowTaskRepository;

    public WorkflowTaskService(IWorkflowTaskRepository workflowTaskRepository)
    {
        _workflowTaskRepository = workflowTaskRepository;
    }

    public async Task<List<WorkflowTask>> GetUnassignedBySubmittedUserRole(string submitterUserRole)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
                w.Submitter.Role != null && w.Submitter.Role.Equals(submitterUserRole, StringComparison.CurrentCultureIgnoreCase)))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<WorkflowTask>> GetByAssignedUserRoleAndStatus(string assignedUserRole, TaskState taskState)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
                w.Assignee != null && w.Assignee.Role != null && w.Submitter.Role != null && w.Assignee.Role.Equals(assignedUserRole, StringComparison.CurrentCultureIgnoreCase) && w.State.Equals(taskState.ToString())))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<WorkflowTask>> GetByAssignedUser(string userId)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
                w.Assignee != null && w.Assignee.Id.Equals(userId, StringComparison.CurrentCultureIgnoreCase)))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<WorkflowTask> GetAsync(WorkflowTask workflowTask)
    {
        var task = await _workflowTaskRepository.GetAsync(workflowTask.Id.ToString());
        return task.MapToWorkflowTaskModel();
    }

    public async Task<WorkflowTask> CreateAsync(WorkflowTask workflowTask)
    {
        var task = workflowTask.MapToWorkflowTaskData();
        return (await _workflowTaskRepository.CreateAsync(task)).MapToWorkflowTaskModel();
    }

    public async Task<WorkflowTask> UpdateAsync(WorkflowTask workflowTask)
    {
        var task = workflowTask.MapToWorkflowTaskData();
        return (await _workflowTaskRepository.ReplaceAsync(task)).MapToWorkflowTaskModel();
    }
}