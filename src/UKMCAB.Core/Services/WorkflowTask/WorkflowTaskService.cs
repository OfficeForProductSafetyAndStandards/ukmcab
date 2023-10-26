using UKMCAB.Core.Domain.WorkflowTask;
using UKMCAB.Core.Mappers;
using UKMCAB.Data.CosmosDb.Services.WorkflowTask;

namespace UKMCAB.Core.Services.WorkflowTask;

public class WorkflowTaskService : IWorkFlowTaskService
{
    private readonly IWorkflowTaskRepository _workflowTaskRepository;

    public WorkflowTaskService(IWorkflowTaskRepository workflowTaskRepository)
    {
        _workflowTaskRepository = workflowTaskRepository;
    }

    //todo - assigned user role
    public async Task<List<Domain.WorkflowTask.WorkflowTask>> GetBySubmittedUserRoleAndStatus(string submittedUserRole, TaskState taskState)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
                w.Submitter.Role != null && w.Submitter.Role.Equals(submittedUserRole, StringComparison.CurrentCultureIgnoreCase) && w.State.Equals(taskState.ToString())))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<Domain.WorkflowTask.WorkflowTask>> GetByAssignedUser(string userId)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
                w.Assignee.Id.Equals(userId, StringComparison.CurrentCultureIgnoreCase)))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<Domain.WorkflowTask.WorkflowTask> GetAsync(Domain.WorkflowTask.WorkflowTask workflowTask)
    {
        var task = await _workflowTaskRepository.GetAsync(workflowTask.Id);
        return task.MapToWorkflowTaskModel();
    }

    public async Task<Domain.WorkflowTask.WorkflowTask> CreateAsync(Domain.WorkflowTask.WorkflowTask workflowTask)
    {
        var task = workflowTask.MapToWorkflowTaskData();
        return (await _workflowTaskRepository.CreateAsync(task)).MapToWorkflowTaskModel();
    }

    public async Task<Domain.WorkflowTask.WorkflowTask> UpdateAsync(Domain.WorkflowTask.WorkflowTask workflowTask)
    {
        var task = workflowTask.MapToWorkflowTaskData();
        return (await _workflowTaskRepository.ReplaceAsync(task)).MapToWorkflowTaskModel();
    }
}

public interface IWorkFlowTaskService
{
    public Task<List<Domain.WorkflowTask.WorkflowTask>> GetBySubmittedUserRoleAndStatus(string userRole,
        TaskState taskState);

    public Task<List<Domain.WorkflowTask.WorkflowTask>> GetByAssignedUser(string userRole);
    public Task<Domain.WorkflowTask.WorkflowTask> GetAsync(Domain.WorkflowTask.WorkflowTask workflowTask);
    public Task<Domain.WorkflowTask.WorkflowTask> CreateAsync(Domain.WorkflowTask.WorkflowTask workflowTask);
    public Task<Domain.WorkflowTask.WorkflowTask> UpdateAsync(Domain.WorkflowTask.WorkflowTask workflowTask);
}