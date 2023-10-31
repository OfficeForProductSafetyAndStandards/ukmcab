using System.Linq.Expressions;

namespace UKMCAB.Data.CosmosDb.Services.WorkflowTask;

public interface IWorkflowTaskRepository
{
    Task<ICollection<Models.Workflow.WorkflowTask>> QueryAsync(Expression<Func<Models.Workflow.WorkflowTask, bool>> predicate);
    Task<Models.Workflow.WorkflowTask> CreateAsync(Models.Workflow.WorkflowTask task);
    Task<Models.Workflow.WorkflowTask> GetAsync(string id);
    Task<Models.Workflow.WorkflowTask> PatchAsync<T>(string id, string fieldName, T value);
    Task<Models.Workflow.WorkflowTask> ReplaceAsync(Models.Workflow.WorkflowTask workflowTask);
}