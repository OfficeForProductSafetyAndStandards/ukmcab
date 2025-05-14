using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Mappers;

public static class WorkflowTaskMapper
{
    public static WorkflowTask MapToWorkflowTaskModel(this Data.Models.Workflow.WorkflowTask source)
    {
        var taskType = Enum.Parse<TaskType>(source.TaskType);
        var task = new WorkflowTask(
            taskType, 
            new User(source.Submitter.Id, source.Submitter.FirstName, source.Submitter.Surname, source.Submitter.Role, source.Submitter.EmailAddress),
            source.ForRoleId,
            source.Assignee != null
                ? new User(source.Assignee.Id, source.Assignee.FirstName, source.Assignee.Surname, source.Assignee.Role,source.Assignee.EmailAddress)
                : null,
            source.Assigned,
            source.Reason,
            new User(source.LastUpdatedBy.Id, source.LastUpdatedBy.FirstName, source.LastUpdatedBy.Surname,
                source.LastUpdatedBy.Role, source.LastUpdatedBy.EmailAddress),
            source.LastUpdatedOn,
            source.Approved,
            source.DeclineReason,
            source.Completed,
            source.CabId,
            source.DocumentLAId
            )
        {
            Id = Guid.Parse(source.Id),
            SentOn = source.SentOn
        };
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
                Role = source.Submitter.RoleId,
                EmailAddress = source.Submitter.EmailAddress
            },
            source.ForRoleId,
            source.Assignee != null
                ? new UserAccount
                {
                    Id = source.Assignee.UserId,
                    FirstName = source.Assignee.FirstName,
                    Surname = source.Assignee.Surname,
                    Role = source.Assignee.RoleId,
                    EmailAddress = source.Assignee.EmailAddress
                }
                : null
            ,
            source.Assigned?.ToUniversalTime(),
            source.Body,
            source.SentOn.ToUniversalTime(),
            new UserAccount
            {
                Id = source.LastUpdatedBy.UserId,
                FirstName = source.LastUpdatedBy.FirstName,
                Surname = source.LastUpdatedBy.Surname,
                Role = source.LastUpdatedBy.RoleId,
                EmailAddress = source.LastUpdatedBy.EmailAddress
            },
            source.LastUpdatedOn.ToUniversalTime(),
            source.Approved,
            source.DeclineReason,
            source.Completed,
            source.CABId,
            source.DocumentLAId);
        return task;
    }
}