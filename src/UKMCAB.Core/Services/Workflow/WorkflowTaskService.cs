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
        var items = await _workflowTaskRepository.QueryAsync(w =>
            w.ForRoleId.ToLower() == roleId.ToLower() &&
            w.Assignee == null &&
            !w.Completed);
        return items.Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<WorkflowTask>> GetAssignedToGroupForRoleIdAsync(string roleId, string? userIdToExclude = null)
    {
        var items = await _workflowTaskRepository.QueryAsync(w =>
            w.ForRoleId.ToLower() == roleId.ToLower() && w.Assignee != null && w.Assignee.Id != userIdToExclude &&
            !w.Completed);

        return items.Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<WorkflowTask>> GetByAssignedUserAsync(string userId)
    {
        var items = await _workflowTaskRepository.QueryAsync(w =>
            w.Assignee != null && w.Assignee.Id == userId && !w.Completed);
        return items.Select(w => w.MapToWorkflowTaskModel()).ToList();
    }

    public async Task<List<WorkflowTask>> GetCompletedForRoleIdAsync(string roleId)
    {
        var items = await _workflowTaskRepository.QueryAsync(w => w.Completed && w.ForRoleId == roleId && w.LastUpdatedOn > DateTime.Now.AddMonths(-6));
        return items.Select(w => w.MapToWorkflowTaskModel()).ToList();
    }
    
    /// <inheritdoc />
    public async Task<List<WorkflowTask>> GetByCabIdAsync(Guid cabId)
    {
        var items = await _workflowTaskRepository.QueryAsync(w =>
            w.CabId.HasValue && w.CabId.Value == cabId);
        return items.Select(w => w.MapToWorkflowTaskModel()).OrderByDescending(t => t.LastUpdatedOn).ToList();
    }
    
    public async Task<List<WorkflowTask>> GetByCabIdAsync(Guid cabId, IEnumerable<TaskType> taskTypes)
    {
        var tasks = await GetByCabIdAsync(cabId);
        var items = tasks.Where(t => taskTypes.Contains(t.TaskType));
        return items.OrderByDescending(t => t.LastUpdatedOn).ToList();
    }
    
    public async Task<List<WorkflowTask>> GetByDocumentLAIdAsync(Guid docLaId)
    {
        var items = await _workflowTaskRepository.QueryAsync(w =>
            w.DocumentLAId.HasValue && w.DocumentLAId.Value == docLaId);
        return items.Select(w => w.MapToWorkflowTaskModel()).OrderByDescending(t => t.LastUpdatedOn).ToList();
    }
    
    public async Task<List<WorkflowTask>> GetByDocumentLAIdAsync(Guid docLaId, IEnumerable<TaskType> taskTypes)
    {
        var tasks = await GetByDocumentLAIdAsync(docLaId);
        var items = tasks.Where(t => taskTypes.Contains(t.TaskType));
        return items.OrderByDescending(t => t.LastUpdatedOn).ToList();
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

    public async Task MarkTaskAsCompletedAsync(Guid taskId, User userLastUpdatedBy)
    {
        var task = await GetAsync(taskId);
        task.LastUpdatedBy = userLastUpdatedBy;
        task.LastUpdatedOn = DateTime.Now;
        task.Completed = true;
        await UpdateAsync(task);
    }
}