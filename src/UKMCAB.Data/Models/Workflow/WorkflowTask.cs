using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models.Workflow;

public record WorkflowTask(
    string Id, 
    string TaskType, 
    UserAccount Submitter, 
    string ForRoleId, 
    UserAccount? Assignee, 
    DateTime? Assigned,
    string Reason, 
    DateTime SentOn, 
    UserAccount LastUpdatedBy, 
    DateTime LastUpdatedOn, 
    bool? Approved,
    string? DeclineReason, 
    bool Completed, 
    Guid? CabId,
    Guid? DocumentLAId);