using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User
{
    public class AccountRequestListViewModel : ILayoutModel
    {
        public List<UserAccountRequest> UserAccountRequests { get; set; }
        public string? Title => "User account requests";
        public PaginationViewModel Pagination { get; set; }
    }
}
