using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User;

public class ReviewAccountRequestViewModel : ILayoutModel
{
    public string? Title => "Review user account request";

    public UserAccountRequest UserAccountRequest { get; set; }
}
