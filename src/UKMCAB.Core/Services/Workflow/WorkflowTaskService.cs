using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Mappers;
using UKMCAB.Data.CosmosDb.Services.WorkflowTask;
// ReSharper disable SpecifyStringComparison

namespace UKMCAB.Core.Services.Workflow;

public class WorkflowTaskService : IWorkflowTaskService
{
    private readonly IWorkflowTaskRepository _workflowTaskRepository;

    public WorkflowTaskService(IWorkflowTaskRepository workflowTaskRepository)
    {
        _workflowTaskRepository = workflowTaskRepository;
    }

    
    public async Task<List<WorkflowTask>> GetUnassignedByForRoleIdAsync(string roleId)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
               w.ForRoleId.ToLower() == roleId.ToLower() &&
                w.Assignee == null && 
                !w.Completed))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<WorkflowTask>> GetByAssignedUserRoleAndCompletedAsync(string assignedUserRole,
        bool completed = false)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
                w.Assignee != null && 
                w.Assignee.Role != null &&
                w.Assignee.Role.ToLower() == assignedUserRole.ToLower() &&
                w.Completed == completed))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<WorkflowTask>> GetByAssignedUserAsync(string userId)
    {
        return (await _workflowTaskRepository.QueryAsync(w =>
                w.Assignee != null && w.Assignee.Id == userId))
            .Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<WorkflowTask> GetAsync(Guid id)
    {
        var task = await _workflowTaskRepository.GetAsync(id.ToString());
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