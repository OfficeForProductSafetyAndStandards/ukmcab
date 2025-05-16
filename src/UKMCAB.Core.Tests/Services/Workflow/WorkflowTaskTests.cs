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
using UKMCAB.Data.Interfaces.Services.WorkflowTask;

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
        var task = new WorkflowTask(TaskType.UserAccountRequest, CreateFakeOpssUser(),
            Roles.OPSS.Id, null,
            null, _faker.Random.Words(), CreateFakeOpssUser(), _faker.Date.Past().ToUniversalTime(), null, null,
            false,
            _faker.Random.Guid()
        );
        task.SentOn = DateTime.UtcNow;

        _mockWorkflowTaskRepository.Setup(r => r.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(task.MapToWorkflowTaskData());

        // Act
        var result = await _sut.GetAsync(Guid.NewGuid());

        // Assert
        Assert.That(task, Is.EqualTo(result));
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
        Assert.That(task, Is.EqualTo(returnedTask));
    }

    private WorkflowTask CreateValidTask(User userSubmitter, string forRoleId, User? userAssigned = null,
        Guid? cabId = null, bool completed = false, Guid? documentLAId = null)
    {
        var task = new WorkflowTask(TaskType.RequestToPublish, userSubmitter,
            forRoleId, userAssigned,
            DateTime.UtcNow, _faker.Random.Words(), userSubmitter, _faker.Date.Past().ToUniversalTime(), null, null,
            completed,
            cabId ?? _faker.Random.Guid(),
            documentLAId ?? _faker.Random.Guid()
        )
        {
            SentOn = DateTime.UtcNow.AddDays(-1)
        };
        return task;
    }

    [Test]
    public void InvalidRequest_CreateAsync_ThrowsInvalidOperationException()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = new WorkflowTask(TaskType.RequestToPublish, ukasUser, Roles.OPSS.Id,
            null,
            null, _faker.Random.Words(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );
        task.Id = Guid.Empty;
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
        var task = new WorkflowTask(TaskType.RequestToPublish, ukasUser,
            Roles.OPSS.Id, null,
            null, _faker.Random.Words(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        );

        var newTask = task;
        newTask.Completed = true;
        newTask.Assignee = CreateFakeOpssUser();
        newTask.Assigned = DateTime.UtcNow.AddDays(-1);
        newTask.Approved = true;
        newTask.Body = "approved";
        newTask.LastUpdatedBy = CreateFakeOpssUser();
        newTask.LastUpdatedOn = DateTime.UtcNow;
        _mockWorkflowTaskRepository.Setup(r => r.ReplaceAsync(It.IsAny<Data.Models.Workflow.WorkflowTask>()))
            .ReturnsAsync(newTask.MapToWorkflowTaskData());

        // Act
        var returnedTask = await _sut.UpdateAsync(task);

        // Assert
        Assert.That(newTask.Completed, Is.EqualTo(returnedTask.Completed));
        Assert.That(newTask.ForRoleId, Is.EqualTo(returnedTask.ForRoleId));
        Assert.That(newTask.Assignee, Is.EqualTo(returnedTask.Assignee));
        Assert.That(newTask.Assigned, Is.EqualTo(returnedTask.Assigned));
        Assert.That(newTask.Approved, Is.EqualTo(returnedTask.Approved));
        Assert.That(newTask.Body, Is.EqualTo(returnedTask.Body));
        Assert.That(newTask.LastUpdatedBy, Is.EqualTo(returnedTask.LastUpdatedBy));
        Assert.That(newTask.LastUpdatedOn, Is.EqualTo(returnedTask.LastUpdatedOn));
    }

    [Test]
    public void InvalidUpdateTask_UpdateAsync_ThrowsInvalidOperationException()
    {
        // Arrange
        var ukasUser = CreateFakeUkasUser();
        var task = new WorkflowTask(TaskType.RequestToPublish, ukasUser, Roles.UKAS.Id,
            null,
            null, _faker.Random.Words(), ukasUser, _faker.Date.Past(), null, null, false,
            _faker.Random.Guid()
        )
        {
            Id = Guid.Empty
        };
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
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(userAssigned, Is.EqualTo(task.Assignee));
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
        var result = await _sut.GetAssignedToGroupForRoleIdAsync(userAssigned.UserId);

        // Arrange
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(userAssigned, Is.EqualTo(task.Assignee));
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
        var result = await _sut.GetAssignedToGroupForRoleIdAsync(userSubmitter.RoleId!);

        // Arrange
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(userSubmitter, Is.EqualTo(task.Submitter));
        }
    }

    [Test]
    public async Task TasksFound_GetCompletedForRoleIdAsync_ReturnsTasks()
    {
        // Arrange
        var userSubmitter = CreateFakeUkasUser();
        _mockWorkflowTaskRepository.Setup(r =>
                r.QueryAsync(It.IsAny<Expression<Func<Data.Models.Workflow.WorkflowTask, bool>>>()))
            .ReturnsAsync(new List<Data.Models.Workflow.WorkflowTask>
            {
                CreateValidTask(userSubmitter, Roles.OPSS.Id, CreateFakeOpssUser(), null, true)
                    .MapToWorkflowTaskData(),
                CreateValidTask(userSubmitter, Roles.OPSS.Id, null, Guid.NewGuid(), true)
                    .MapToWorkflowTaskData(),
            });

        // Act
        var result = await _sut.GetCompletedForRoleIdAsync(userSubmitter.RoleId!);

        // Arrange
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(userSubmitter, Is.EqualTo(task.Submitter));
            Assert.That(Roles.OPSS.Id, Is.EqualTo(task.ForRoleId));
            Assert.That(task.Completed, Is.True);
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
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(cabId, Is.EqualTo(task.CABId));
        }
    }

    [Test]
    public async Task TasksFound_GetByCabIdAsync_ReturnsTasksFilteredByType()
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
        var result = await _sut.GetByCabIdAsync(cabId, new List<TaskType> { TaskType.RequestToPublish });

        // Arrange
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(TaskType.RequestToPublish, Is.EqualTo(task.TaskType));
            Assert.That(cabId, Is.EqualTo(task.CABId));
        }
    }

    [Test]
    public async Task TasksFound_GetByDocumentLAIdAsync_ReturnsTasks()
    {
        // Arrange
        var documentLAId = _faker.Random.Guid();
        var userAssigned = CreateFakeOpssUser();
        _mockWorkflowTaskRepository.Setup(r =>
                r.QueryAsync(It.IsAny<Expression<Func<Data.Models.Workflow.WorkflowTask, bool>>>()))
            .ReturnsAsync(new List<Data.Models.Workflow.WorkflowTask>
            {
                CreateValidTask(CreateFakeOpssUser(), Roles.OPSS.Id, userAssigned, documentLAId: documentLAId).MapToWorkflowTaskData(),
                CreateValidTask(CreateFakeUkasUser(), Roles.UKAS.Id, userAssigned, documentLAId: documentLAId).MapToWorkflowTaskData(),
            });

        // Act
        var result = await _sut.GetByDocumentLAIdAsync(documentLAId);

        // Arrange
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(documentLAId, Is.EqualTo(task.DocumentLAId));
        }
    }
    
    [Test]
    public async Task TasksFound_GetByDocumentLAIdAsync_ReturnsTasksFilteredByType()
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
        var result = await _sut.GetByDocumentLAIdAsync(cabId, new List<TaskType> { TaskType.RequestToPublish });

        // Arrange
        Assert.That(2, Is.EqualTo(result.Count));
        foreach (var task in result)
        {
            Assert.That(TaskType.RequestToPublish, Is.EqualTo(task.TaskType));
            Assert.That(cabId, Is.EqualTo(task.CABId));
        }
    }

    
    private User CreateFakeUkasUser()
    {
        var ukasUser = new User(_faker.Random.Word(), _faker.Random.Word(), _faker.Random.Word(),
            Roles.UKAS.ToString()!, _faker.Internet.Email());
        return ukasUser;
    }

    private User CreateFakeOpssUser()
    {
        var ukasUser = new User(_faker.Random.Word(), _faker.Random.Word(), _faker.Random.Word(),
            Roles.OPSS.ToString()!, _faker.Internet.Email());
        return ukasUser;
    }
}