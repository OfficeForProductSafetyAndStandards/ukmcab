namespace UKMCAB.Web.UI.Models.ViewModels.Account;

public class ApproveRequestViewModel : ILayoutModel
{
    public string? AccountId { get; set; }
    public string? Role { get; set; }
    public string? Title => "Approve account request";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
