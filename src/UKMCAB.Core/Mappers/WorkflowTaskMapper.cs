using UKMCAB.Core.Domain.WorkflowTask;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Mappers;

public static class WorkflowTaskMapper
{
    public static WorkflowTask MapToWorkflowTaskModel(this Data.Models.WorkflowTask.WorkflowTask source)
    {
        TaskType taskType = Enum.Parse<TaskType>(source.TaskType);
        TaskState taskState = Enum.Parse<TaskState>(source.State);
        WorkflowTask task = new WorkflowTask(
            source.Id,
            taskType, taskState,
            new User(source.Submitter.Id, source.Submitter.FirstName, source.Submitter.Surname, source.Submitter.Role),
            new User(source.Assignee.Id, source.Assignee.FirstName, source.Assignee.Surname, source.Assignee.Role),
            source.Assigned,
            source.Reason,
            source.SentOn,
            new User(source.LastUpdatedBy.Id, source.LastUpdatedBy.FirstName, source.LastUpdatedBy.Surname, source.LastUpdatedBy.Role),
            source.LastUpdatedOn,
            source.Approved,
            source.DeclineReason,
            source.Completed,
            source.documentId);
        return task;
    }

    public static Data.Models.WorkflowTask.WorkflowTask MapToWorkflowTaskData(this WorkflowTask source)
    {
        Data.Models.WorkflowTask.WorkflowTask task = new Data.Models.WorkflowTask.WorkflowTask(
            source.Id,
            source.TaskType.ToString(),
            source.State.ToString(),
            new UserAccount
            {
                Id = source.Submitter.UserID,
                FirstName = source.Submitter.FirstName,
                Surname = source.Submitter.Surname,
                Role = source.Submitter.Role
            },
            new UserAccount
            {
                Id = source.Assignee.UserID,
                FirstName = source.Assignee.FirstName,
                Surname = source.Assignee.Surname,
                Role = source.Assignee.Role
            },
            source.Assigned,
            source.Reason,
            source.SentOn,
            new UserAccount
            {
                Id = source.Assignee.UserID,
                FirstName = source.Assignee.FirstName,
                Surname = source.Assignee.Surname,
                Role = source.Assignee.Role
            },
            source.LastUpdatedOn,
            source.Approved,
            source.DeclineReason,
            source.Completed,
            source.documentId);
        return task;
    }
}