using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Common.ConnectionStrings;

namespace UKMCAB.Data.CosmosDb.Services.WorkflowTask;

public class WorkflowTaskRepository : IWorkflowTaskRepository
{
    public const string ContainerId = "workflow-tasks";
    private Container _container;

    /// <summary>
    /// Repository for CRUD Workflow Tasks.
    /// </summary>
    /// <param name="connectionString"></param>
    public WorkflowTaskRepository(CosmosDbConnectionString connectionString)
    {
        var client = CosmosClientFactory.Create(connectionString);
        _container = client.GetContainer(DataConstants.CosmosDb.Database, ContainerId);
    }

    public async Task<ICollection<Models.Workflow.WorkflowTask>> QueryAsync(Expression<Func<Models.Workflow.WorkflowTask, bool>> predicate)
    {
        var query = _container.GetItemLinqQueryable<Models.Workflow.WorkflowTask>().Where(predicate).ToFeedIterator();
        var list = new List<Models.Workflow.WorkflowTask>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            list.AddRange(response.Resource.Select(r => r));
        }

        return list;
    }

    public async Task<Models.Workflow.WorkflowTask> CreateAsync(Models.Workflow.WorkflowTask task)
    {
        return await _container.CreateItemAsync(task);
    }

    public async Task<Models.Workflow.WorkflowTask> GetAsync(string id)
    {
        return await _container.ReadItemAsync<Models.Workflow.WorkflowTask>(id, new PartitionKey(id));
    }

    public async Task<Models.Workflow.WorkflowTask> PatchAsync<T>(string id, string fieldName, T value)
    {
        return await _container.PatchItemAsync<Models.Workflow.WorkflowTask>(id, new PartitionKey(id), new[]
        {
            PatchOperation.Set($"/{fieldName}", value)
        });
    }

    public async Task<Models.Workflow.WorkflowTask> ReplaceAsync(Models.Workflow.WorkflowTask workflowTask)
    {
        return await _container.ReplaceItemAsync(workflowTask, workflowTask.Id, new PartitionKey(workflowTask.Id));
    }
}