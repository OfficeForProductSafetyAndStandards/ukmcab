using System;
using System.Threading.Tasks;
using Bogus;
using NUnit.Framework;
using UKMCAB.Core.Domain.WorkflowTask;
using UKMCAB.Core.Mappers;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.CosmosDb.Services.WorkflowTask;

namespace UKMCAB.Core.Tests.Services.Workflow;

public class TaskTests
{
    private IWorkflowTaskRepository _workflowTaskRepository = null!;
    private WorkflowTaskService _sut = null!;
    private Faker _faker = new();

    [SetUp]
    public void Setup()
    {
        _workflowTaskRepository = new FakeWorkflowTaskRepository();
        _sut = new WorkflowTaskService(_workflowTaskRepository);
    }

    [Test]
    public async Task ValidId_GetAsync_ReturnsTask()
    {
        // Arrange
        var guid = _faker.Random.Guid();
        var task = new WorkflowTask(guid, TaskType.UserAccountRequest, TaskState.ToDo, CreateFakeOpssUser(), null,
            null, _faker.Random.Words(), _faker.Date.Past(), CreateFakeOpssUser(), _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );
        await _workflowTaskRepository.CreateAsync(task.MapToWorkflowTaskData());
        
        // Act
        var result = await _sut.GetAsync(task);
        
        // Assert
        Assert.AreEqual(task, result);
    }

    [Test]
    public async Task ValidRequest_CreateAsync_ReturnsCreatedTask()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = new WorkflowTask(_faker.Random.Guid(), TaskType.RequestToPublish, TaskState.ToDo, ukasUser, null,
            null, _faker.Random.Words(), _faker.Date.Past(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );
        // Act
        var returnedTask = await _sut.CreateAsync(task);

        // Assert
        Assert.AreEqual(task, returnedTask);
    }
    
    [Test]
    public async Task UpdateTask_UpdateAsync_ReturnsUpdatedTask()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = new WorkflowTask(_faker.Random.Guid(), TaskType.RequestToPublish, TaskState.ToDo, ukasUser, null,
            null, _faker.Random.Words(), _faker.Date.Past(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );
        await _sut.CreateAsync(task);
        var newTask = task;
        newTask.Completed = true;
        newTask.Assignee = CreateFakeOpssUser();
        newTask.Assigned = DateTime.UtcNow.AddDays(-1);
        newTask.Approved = true;
        newTask.Reason = "approved";
        newTask.State = TaskState.Done;
        newTask.LastUpdatedBy = CreateFakeOpssUser();
        newTask.LastUpdatedOn = DateTime.UtcNow;
        
        // Act
        var returnedTask = await _sut.UpdateAsync(task);

        // Assert
        Assert.AreEqual(newTask.Completed, returnedTask.Completed);
        Assert.AreEqual(newTask.Assignee, returnedTask.Assignee);
        Assert.AreEqual(newTask.Assigned, returnedTask.Assigned);
        Assert.AreEqual(newTask.Approved, returnedTask.Approved);
        Assert.AreEqual(newTask.Reason, returnedTask.Reason);
        Assert.AreEqual(newTask.State, returnedTask.State); 
        Assert.AreEqual(newTask.LastUpdatedBy, returnedTask.LastUpdatedBy); 
        Assert.AreEqual(newTask.LastUpdatedOn, returnedTask.LastUpdatedOn); 
    }

    private User CreateFakeUkasUser()
    {
        var ukasUser = new User(_faker.Random.Word(), _faker.Random.Word(), _faker.Random.Word(),
            Roles.UKAS.ToString());
        return ukasUser;
    }
    private User CreateFakeOpssUser()
    {
        var ukasUser = new User(_faker.Random.Word(), _faker.Random.Word(), _faker.Random.Word(),
            Roles.OPSS.ToString());
        return ukasUser;
    }
}