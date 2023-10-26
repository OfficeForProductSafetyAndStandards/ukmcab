namespace UKMCAB.Core.Domain.WorkflowTask;
//todo: This domain model should move within core and become shared and used for users also not just workflows.
public record User(string UserID, string? FirstName, string? Surname, string? Role);