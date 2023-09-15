using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User
{
    public class UserAccountListViewModel : ILayoutModel
    {
        public string? Title { get; set; }

        public List<UserAccount> UserAccounts { get; set; }
        public int PendingAccountsCount { get; set; }

        public bool LockedOnly { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public string? SortField { get; set; }

        public string? SortDirection { get; set; }
    }
}
