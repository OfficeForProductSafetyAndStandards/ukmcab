using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models.Workflow;

public record WorkflowTask(string Id, string TaskType, string State, UserAccount Submitter, UserAccount? Assignee, DateTime? Assigned,
    string Reason, DateTime SentOn, UserAccount LastUpdatedBy, DateTime LastUpdatedOn, bool? Approved,
    string? DeclineReason, bool Completed, Guid DocumentId);