using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UKMCAB.Data.Interfaces.Services.WorkflowTask;

namespace UKMCAB.Data.PostgreSQL.Services.WorkflowTask;

public class PostgreWorkflowTaskRepository : IWorkflowTaskRepository
{
    private readonly ApplicationDataContext _dbContext;

    public PostgreWorkflowTaskRepository(ApplicationDataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Models.Workflow.WorkflowTask> CreateAsync(Models.Workflow.WorkflowTask task)
    {
        var createdTask = await _dbContext.WorkflowTasks.AddAsync(task);
        await _dbContext.SaveChangesAsync();
        return createdTask.Entity;
    }

    public async Task<Models.Workflow.WorkflowTask> GetAsync(string id)
    {
        return await _dbContext.WorkflowTasks.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Models.Workflow.WorkflowTask> PatchAsync<T>(string id, string fieldName, T value)
    {
        var task = await _dbContext.WorkflowTasks.FindAsync(id);
        if (task == null)
        {
            throw new KeyNotFoundException($"WorkflowTask with ID '{id}' not found.");
        }

        var property = typeof(Models.Workflow.WorkflowTask).GetProperty(fieldName);
        if (property == null || !property.CanWrite)
        {
            throw new ArgumentException($"Field '{fieldName}' is not valid or not writable.");
        }

        // Convert the value to the correct property type if necessary
        var convertedValue = Convert.ChangeType(value, property.PropertyType);
        property.SetValue(task, convertedValue);

        await _dbContext.SaveChangesAsync();

        return task;
    }

    public async Task<ICollection<Models.Workflow.WorkflowTask>> QueryAsync(Expression<Func<Models.Workflow.WorkflowTask, bool>> predicate)
    {
        return await _dbContext.WorkflowTasks.Where(predicate).ToListAsync();
    }

    public async Task<Models.Workflow.WorkflowTask> ReplaceAsync(Models.Workflow.WorkflowTask workflowTask)
    {
        var existingEntity = await _dbContext.WorkflowTasks.FindAsync(workflowTask.Id);
        if (existingEntity == null)
        {
            throw new Exception("Entity not found");
        }

        _dbContext.WorkflowTasks.Remove(existingEntity);

        var createdEntity = _dbContext.WorkflowTasks.Add(workflowTask);

        await _dbContext.SaveChangesAsync();

        return createdEntity.Entity;
    }
}
