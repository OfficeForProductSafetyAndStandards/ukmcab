namespace UKMCAB.Core.Domain.Workflow;
//todo: This domain model should move within core and become shared and used for users also not just workflows.
public record User(string UserId, string? FirstName, string? Surname, string? Role, string EmailAddress)
{
    public string FirstAndLastName => $"{FirstName} {Surname}";
}