using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Mappers;

public static class WorkflowTaskMapper
{
    public static WorkflowTask MapToWorkflowTaskModel(this Data.Models.Workflow.WorkflowTask source)
    {
        TaskType taskType = Enum.Parse<TaskType>(source.TaskType);
        WorkflowTask task = new WorkflowTask(
            Guid.Parse(source.Id),
            taskType, 
            new User(source.Submitter.Id, source.Submitter.FirstName, source.Submitter.Surname, source.Submitter.Role),
            source.ForRoleId,
            source.Assignee != null
                ? new User(source.Assignee.Id, source.Assignee.FirstName, source.Assignee.Surname, source.Assignee.Role)
                : null,
            source.Assigned,
            source.Reason,
            source.SentOn,
            new User(source.LastUpdatedBy.Id, source.LastUpdatedBy.FirstName, source.LastUpdatedBy.Surname,
                source.LastUpdatedBy.Role),
            source.LastUpdatedOn,
            source.Approved,
            source.DeclineReason,
            source.Completed,
            source.CabId);
        return task;
    }

    public static Data.Models.Workflow.WorkflowTask MapToWorkflowTaskData(this WorkflowTask source)
    {
        Data.Models.Workflow.WorkflowTask task = new Data.Models.Workflow.WorkflowTask(
            source.Id.ToString(),
            source.TaskType.ToString(),
            new UserAccount
            {
                Id = source.Submitter.UserId,
                FirstName = source.Submitter.FirstName,
                Surname = source.Submitter.Surname,
                Role = source.Submitter.Role
            },
            source.ForRoleId,
            source.Assignee != null
                ? new UserAccount
                {
                    Id = source.Assignee.UserId,
                    FirstName = source.Assignee.FirstName,
                    Surname = source.Assignee.Surname,
                    Role = source.Assignee.Role
                }
                : null
            ,
            source.Assigned,
            source.Reason,
            source.SentOn,
            new UserAccount
            {
                Id = source.LastUpdatedBy.UserId,
                FirstName = source.LastUpdatedBy.FirstName,
                Surname = source.LastUpdatedBy.Surname,
                Role = source.LastUpdatedBy.Role
            },
            source.LastUpdatedOn,
            source.Approved,
            source.DeclineReason,
            source.Completed,
            source.CABId);
        return task;
    }
}