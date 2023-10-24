namespace UKMCAB.Data.CosmosDb.Services.WorkflowTask;
using Models.WorkflowTask;

public interface IWorkflowTaskRepository
{
    Task<WorkflowTask> CreateAsync(WorkflowTask task);
    Task<WorkflowTask> GetAsync(string id);
    Task<WorkflowTask> PatchAsync<T>(string id, string fieldName, T value);
    Task<WorkflowTask> ReplaceAsync(WorkflowTask workflowTask);
}