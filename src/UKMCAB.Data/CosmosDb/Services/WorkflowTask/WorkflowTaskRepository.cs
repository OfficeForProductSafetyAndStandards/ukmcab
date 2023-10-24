using Microsoft.Azure.Cosmos;
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
    public WorkflowTaskRepository(ConnectionString connectionString)
    {
        var client = CosmosClientFactory.Create(connectionString);
        _container = client.GetContainer(DataConstants.CosmosDb.Database, ContainerId);
    }

    public async Task<Models.WorkflowTask.WorkflowTask> CreateAsync(Models.WorkflowTask.WorkflowTask task)
    {
        return await _container.CreateItemAsync(task);
    }

    public async Task<Models.WorkflowTask.WorkflowTask> GetAsync(string id)
    {
        return await _container.ReadItemAsync<Models.WorkflowTask.WorkflowTask>(id, new PartitionKey(id));
    }

    public async Task<Models.WorkflowTask.WorkflowTask> PatchAsync<T>(string id, string fieldName, T value)
    {
        return await _container.PatchItemAsync<Models.WorkflowTask.WorkflowTask>(id, new PartitionKey(id), new[]
        {
            PatchOperation.Set($"/{fieldName}", value)
        });
    }

    public async Task<Models.WorkflowTask.WorkflowTask> ReplaceAsync(Models.WorkflowTask.WorkflowTask workflowTask)
    {
        return await _container.ReplaceItemAsync(workflowTask, workflowTask.Id, new PartitionKey(workflowTask.Id));
    }
}