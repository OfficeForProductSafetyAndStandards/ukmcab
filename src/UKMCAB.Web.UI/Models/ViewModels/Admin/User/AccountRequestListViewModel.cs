using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User
{
    public class AccountRequestListViewModel : ILayoutModel
    {
        public List<UserAccountRequest> UserAccountRequests { get; set; }
        public string? Title => "Pending user account requests";
    }
}
