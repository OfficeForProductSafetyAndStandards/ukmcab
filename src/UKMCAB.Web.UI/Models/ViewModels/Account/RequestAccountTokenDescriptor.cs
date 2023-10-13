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