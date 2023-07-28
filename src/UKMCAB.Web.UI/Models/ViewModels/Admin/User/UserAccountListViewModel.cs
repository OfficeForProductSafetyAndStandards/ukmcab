using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User
{
    public class UserAccountListViewModel : ILayoutModel
    {
        public string? Title => "User accounts";

        public List<UserAccount> UserAccounts { get; set; }
        public int PendingAccountsCount { get; set; }
    }
}
