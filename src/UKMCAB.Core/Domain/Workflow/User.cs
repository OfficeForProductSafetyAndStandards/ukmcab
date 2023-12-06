using UKMCAB.Core.Security;

namespace UKMCAB.Core.Domain.Workflow;
//todo: This domain model should move within core and become shared and used for users also not just workflows.
public record User(string UserId, string? FirstName, string? Surname, string RoleId, string EmailAddress)
{
    public string FirstAndLastName => $"{FirstName} {Surname}";
    public string UserGroup => Roles.NameFor(RoleId)!;
}