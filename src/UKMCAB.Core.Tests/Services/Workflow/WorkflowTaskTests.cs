using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bogus;
using Moq;
using NUnit.Framework;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Mappers;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.CosmosDb.Services.WorkflowTask;

namespace UKMCAB.Core.Tests.Services.Workflow;

public class WorkflowTaskServiceTests
{
    private Mock<IWorkflowTaskRepository> _mockWorkflowTaskRepository = null!;
    private WorkflowTaskService _sut = null!;
    private readonly Faker _faker = new();

    [SetUp]
    public void Setup()
    {
        _mockWorkflowTaskRepository = new Mock<IWorkflowTaskRepository>();
        _sut = new WorkflowTaskService(_mockWorkflowTaskRepository.Object);
    }

    [Test]
    public async Task ValidId_GetAsync_ReturnsTask()
    {
        // Arrange
        var guid = _faker.Random.Guid();
        var task = new WorkflowTask(guid, TaskType.UserAccountRequest, CreateFakeOpssUser(),
            Roles.OPSS.Id, null,
            null, _faker.Random.Words(), _faker.Date.Past(), CreateFakeOpssUser(), _faker.Date.Past(), null, null,
            false,
            _faker.Random.Guid()
        );
        _mockWorkflowTaskRepository.Setup(r => r.GetAsync(guid.ToString()))
            .ReturnsAsync(task.MapToWorkflowTaskData());

        // Act
        var result = await _sut.GetAsync(guid);

        // Assert
        Assert.AreEqual(task, result);
    }

    [Test]
    public void InvalidId_GetAsync_ThrowsNotFoundException()
    {
        // Arrange
        var guid = _faker.Random.Guid();
        _mockWorkflowTaskRepository.Setup(r => r.GetAsync(guid.ToString()))
            .ThrowsAsync(new NotFoundException("Workflow Task not found"));

        // Act and Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _sut.GetAsync(guid));
    }

    [Test]
    public async Task ValidRequest_CreateAsync_ReturnsCreatedTask()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = CreateValidTask(ukasUser, Roles.OPSS.Id, null!);
        var dataTask = task.MapToWorkflowTaskData();
        _mockWorkflowTaskRepository.Setup(r => r.CreateAsync(It.IsAny<Data.Models.Workflow.WorkflowTask>()))
            .ReturnsAsync(dataTask);

        // Act
        var returnedTask = await _sut.CreateAsync(task);

        // Assert
        Assert.AreEqual(task, returnedTask);
    }

    private WorkflowTask CreateValidTask(User userSubmitter, string forRoleId, User userAssigned, Guid? cabId = null)
    {
        var task = new WorkflowTask(_faker.Random.Guid(), TaskType.RequestToPublish, userSubmitter,
            forRoleId, userAssigned,
            DateTime.UtcNow, _faker.Random.Words(), _faker.Date.Past(), userSubmitter, _faker.Date.Past(), null, null,
            false,
            cabId ?? _faker.Random.Guid()
        );
        return task;
    }

    [Test]
    public void InvalidRequest_CreateAsync_ThrowsInvalidOperationException()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = new WorkflowTask(Guid.Empty, TaskType.RequestToPublish, ukasUser, Roles.OPSS.Id,
            null,
            null, _faker.Random.Words(), _faker.Date.Past(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );
        _mockWorkflowTaskRepository.Setup(r => r.CreateAsync(It.IsAny<Data.Models.Workflow.WorkflowTask>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act and Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(task));
    }

    [Test]
    public async Task ValidUpdateTask_UpdateAsync_ReturnsUpdatedTask()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = new WorkflowTask(_faker.Random.Guid(), TaskType.RequestToPublish, ukasUser,
            Roles.OPSS.Id, null,
            null, _faker.Random.Words(), _faker.Date.Past(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );

        var newTask = task;
        newTask.Completed = true;
        newTask.Assignee = CreateFakeOpssUser();
        newTask.Assigned = DateTime.UtcNow.AddDays(-1);
        newTask.Approved = true;
        newTask.Reason = "approved";
        newTask.LastUpdatedBy = CreateFakeOpssUser();
        newTask.LastUpdatedOn = DateTime.UtcNow;
        _mockWorkflowTaskRepository.Setup(r => r.ReplaceAsync(It.IsAny<Data.Models.Workflow.WorkflowTask>()))
            .ReturnsAsync(newTask.MapToWorkflowTaskData());

        // Act
        var returnedTask = await _sut.UpdateAsync(task);

        // Assert
        Assert.AreEqual(newTask.Completed, returnedTask.Completed);
        Assert.AreEqual(newTask.ForRoleId, returnedTask.ForRoleId);
        Assert.AreEqual(newTask.Assignee, returnedTask.Assignee);
        Assert.AreEqual(newTask.Assigned, returnedTask.Assigned);
        Assert.AreEqual(newTask.Approved, returnedTask.Approved);
        Assert.AreEqual(newTask.Reason, returnedTask.Reason);
        Assert.AreEqual(newTask.LastUpdatedBy, returnedTask.LastUpdatedBy);
        Assert.AreEqual(newTask.LastUpdatedOn, returnedTask.LastUpdatedOn);
    }

    [Test]
    public void InvalidUpdateTask_UpdateAsync_ThrowsInvalidOperationException()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = new WorkflowTask(Guid.Empty, TaskType.RequestToPublish, ukasUser, Roles.UKAS.Id,
            null,
            null, _faker.Random.Words(), _faker.Date.Past(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );
        _mockWorkflowTaskRepository.Setup(r => r.ReplaceAsync(It.IsAny<Data.Models.Workflow.WorkflowTask>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act and Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.UpdateAsync(task));
    }

    [Test]
    public async Task TasksFound_GetByAssignedUserAsync_ReturnsTasks()
    {
        // Arrange
        var userAssigned = CreateFakeOpssUser();
        _mockWorkflowTaskRepository.Setup(r =>
                r.QueryAsync(It.IsAny<Expression<Func<Data.Models.Workflow.WorkflowTask, bool>>>()))
            .ReturnsAsync(new List<Data.Models.Workflow.WorkflowTask>
            {
                CreateValidTask(CreateFakeOpssUser(), Roles.OPSS.Id, userAssigned).MapToWorkflowTaskData(),
                CreateValidTask(CreateFakeUkasUser(), Roles.UKAS.Id, userAssigned).MapToWorkflowTaskData(),
            });

        // Act
        var result = await _sut.GetByAssignedUserAsync(userAssigned.UserId);

        // Arrange
        Assert.AreEqual(2, result.Count);
        foreach (var task in result)
        {
            Assert.AreEqual(userAssigned, task.Assignee);
        }
    }

    [Test]
    public async Task TasksFound_GetByAssignedRoleAndStatusAsync_ReturnsTasks()
    {
        // Arrange
        var userAssigned = CreateFakeOpssUser();
        _mockWorkflowTaskRepository.Setup(r =>
                r.QueryAsync(It.IsAny<Expression<Func<Data.Models.Workflow.WorkflowTask, bool>>>()))
            .ReturnsAsync(new List<Data.Models.Workflow.WorkflowTask>
            {
                CreateValidTask(CreateFakeOpssUser(), Roles.OPSS.Id, userAssigned)
                    .MapToWorkflowTaskData(),
                CreateValidTask(CreateFakeUkasUser(), Roles.UKAS.Id, userAssigned)
                    .MapToWorkflowTaskData(),
            });

        // Act
        var result = await _sut.GetByForRoleAndCompletedAsync(userAssigned.UserId, true);

        // Arrange
        Assert.AreEqual(2, result.Count);
        foreach (var task in result)
        {
            Assert.AreEqual(userAssigned, task.Assignee);
        }
    }

    [Test]
    public async Task TasksFound_GetUnassignedBySubmittedUserRoleAsync_ReturnsTasks()
    {
        // Arrange
        var userSubmitter = CreateFakeUkasUser();
        _mockWorkflowTaskRepository.Setup(r =>
                r.QueryAsync(It.IsAny<Expression<Func<Data.Models.Workflow.WorkflowTask, bool>>>()))
            .ReturnsAsync(new List<Data.Models.Workflow.WorkflowTask>
            {
                CreateValidTask(userSubmitter, Roles.OPSS.Id, CreateFakeOpssUser())
                    .MapToWorkflowTaskData(),
                CreateValidTask(userSubmitter, Roles.OPSS.Id, CreateFakeOpssUser())
                    .MapToWorkflowTaskData(),
            });

        // Act
        var result = await _sut.GetByForRoleAndCompletedAsync(userSubmitter.Role!, true);

        // Arrange
        Assert.AreEqual(2, result.Count);
        foreach (var task in result)
        {
            Assert.AreEqual(userSubmitter, task.Submitter);
        }
    }

    [Test]
    public async Task TasksFound_GetByCabIdAsync_ReturnsTasks()
    {
        // Arrange
        var cabId = _faker.Random.Guid();
        var userAssigned = CreateFakeOpssUser();
        _mockWorkflowTaskRepository.Setup(r =>
                r.QueryAsync(It.IsAny<Expression<Func<Data.Models.Workflow.WorkflowTask, bool>>>()))
            .ReturnsAsync(new List<Data.Models.Workflow.WorkflowTask>
            {
                CreateValidTask(CreateFakeOpssUser(), Roles.OPSS.Id, userAssigned, cabId).MapToWorkflowTaskData(),
                CreateValidTask(CreateFakeUkasUser(), Roles.UKAS.Id, userAssigned, cabId).MapToWorkflowTaskData(),
            });

        // Act
        var result = await _sut.GetByCabIdAsync(cabId);

        // Arrange
        Assert.AreEqual(2, result.Count);
        foreach (var task in result)
        {
            Assert.AreEqual(userAssigned, task.Assignee);
            Assert.AreEqual(cabId, task.CABId);
        }
    }

    private User CreateFakeUkasUser()
    {
        var ukasUser = new User(_faker.Random.Word(), _faker.Random.Word(), _faker.Random.Word(),
            Roles.UKAS.ToString(), _faker.Internet.Email());
        return ukasUser;
    }

    private User CreateFakeOpssUser()
    {
        var ukasUser = new User(_faker.Random.Word(), _faker.Random.Word(), _faker.Random.Word(),
            Roles.OPSS.ToString(), _faker.Internet.Email());
        return ukasUser;
    }
}