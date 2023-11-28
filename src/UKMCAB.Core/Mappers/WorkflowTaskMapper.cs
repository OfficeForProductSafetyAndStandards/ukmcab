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
            new User(source.Submitter.Id, source.Submitter.FirstName, source.Submitter.Surname, source.Submitter.RoleId, source.Submitter.EmailAddress),
            source.ForRoleId,
            source.Assignee != null
                ? new User(source.Assignee.Id, source.Assignee.FirstName, source.Assignee.Surname, source.Assignee.RoleId,source.Assignee.EmailAddress)
                : null,
            source.Assigned,
            source.Reason,
            source.SentOn,
            new User(source.LastUpdatedBy.Id, source.LastUpdatedBy.FirstName, source.LastUpdatedBy.Surname,
                source.LastUpdatedBy.RoleId, source.LastUpdatedBy.EmailAddress),
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
                RoleId = source.Submitter.RoleId,
                EmailAddress = source.Submitter.EmailAddress
            },
            source.ForRoleId,
            source.Assignee != null
                ? new UserAccount
                {
                    Id = source.Assignee.UserId,
                    FirstName = source.Assignee.FirstName,
                    Surname = source.Assignee.Surname,
                    RoleId = source.Assignee.RoleId,
                    EmailAddress = source.Assignee.EmailAddress
                }
                : null
            ,
            source.Assigned,
            source.Body,
            source.SentOn,
            new UserAccount
            {
                Id = source.LastUpdatedBy.UserId,
                FirstName = source.LastUpdatedBy.FirstName,
                Surname = source.LastUpdatedBy.Surname,
                RoleId = source.LastUpdatedBy.RoleId,
                EmailAddress = source.LastUpdatedBy.EmailAddress
            },
            source.LastUpdatedOn,
            source.Approved,
            source.DeclineReason,
            source.Completed,
            source.CABId);
        return task;
    }
}