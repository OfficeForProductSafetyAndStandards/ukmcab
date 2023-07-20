namespace UKMCAB.Web.UI.Models.ViewModels.Account;

public class RequestAccountTokenDescriptor
{
    public string SubjectId { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public RequestAccountTokenDescriptor() { }
    public RequestAccountTokenDescriptor(string subjectId, string emailAddress)
    {
        SubjectId = subjectId;
        EmailAddress = emailAddress;
    }
}

public class RequestAccountViewModel : ILayoutModel
{
    public string Token { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string Organisation { get; set; }
    public string ContactEmailAddress { get; set; }
    public string Comments { get; set; }

    public string? Title => "Request user account";
}
