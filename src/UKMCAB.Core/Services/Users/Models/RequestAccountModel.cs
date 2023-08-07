namespace UKMCAB.Core.Services.Users.Models;

public class RequestAccountModel
{
    public string SubjectId { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Organisation { get; set; } = null!;
    public string ContactEmailAddress { get; set; } = null!;
    public string Comments { get; set; } = null!;
}
