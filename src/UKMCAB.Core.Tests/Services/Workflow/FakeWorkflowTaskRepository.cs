using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.CosmosDb.Services.WorkflowTask;

namespace UKMCAB.Core.Tests.Services.Workflow;

public class FakeWorkflowTaskRepository : IWorkflowTaskRepository
{
    private readonly Dictionary<string, Data.Models.Workflow.WorkflowTask> _tasks = new();

    public async Task<ICollection<Data.Models.Workflow.WorkflowTask>> QueryAsync(
        Expression<Func<Data.Models.Workflow.WorkflowTask, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<Data.Models.Workflow.WorkflowTask> CreateAsync(Data.Models.Workflow.WorkflowTask task)
    {
        _tasks.Add(task.Id, task);
        return Task.FromResult(task);
    }

    public Task<Data.Models.Workflow.WorkflowTask> GetAsync(string id)
    {
        return Task.FromResult(_tasks[id] ?? throw new InvalidOperationException());
    }

    public async Task<Data.Models.Workflow.WorkflowTask> PatchAsync<T>(string id, string fieldName, T value)
    {
        var task = await GetAsync(id);
        return task;
    }

    public Task<Data.Models.Workflow.WorkflowTask> ReplaceAsync(
        Data.Models.Workflow.WorkflowTask workflowTask)
    {
        var id = workflowTask.Id;
        _tasks.Remove(id);
        _tasks.Add(id, workflowTask);
        return Task.FromResult(_tasks[id]);
    }
}