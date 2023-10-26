using System.Linq.Expressions;

namespace UKMCAB.Data.CosmosDb.Services.WorkflowTask;
using Models.WorkflowTask;

public interface IWorkflowTaskRepository
{
    Task<ICollection<WorkflowTask>> QueryAsync(Expression<Func<WorkflowTask, bool>> predicate);
    Task<WorkflowTask> CreateAsync(WorkflowTask task);
    Task<WorkflowTask> GetAsync(string id);
    Task<WorkflowTask> PatchAsync<T>(string id, string fieldName, T value);
    Task<WorkflowTask> ReplaceAsync(WorkflowTask workflowTask);
}