using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models.WorkflowTask;

namespace UKMCAB.Data.CosmosDb.Services.Task;

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

    public async Task<WorkflowTask> CreateAsync(WorkflowTask task)
    {
        return await _container.CreateItemAsync(task);
    }

    public async Task<WorkflowTask> GetAsync(string id)
    {
        return await _container.ReadItemAsync<WorkflowTask>(id, new PartitionKey(id));
    }

    public async Task<WorkflowTask> PatchAsync<T>(string id, string fieldName, T value)
    {
        return await _container.PatchItemAsync<WorkflowTask>(id, new PartitionKey(id), new[]
        {
            PatchOperation.Set($"/{fieldName}", value)
        });
    }

    public async Task<WorkflowTask> ReplaceAsync(WorkflowTask workflowTask)
    {
        return await _container.ReplaceItemAsync(workflowTask, workflowTask.Id, new PartitionKey(workflowTask.Id));
    }
}