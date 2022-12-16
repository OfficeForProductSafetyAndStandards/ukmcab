using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class PendingAccountsViewModel: ILayoutModel
    {
        public string? Title => "Pending account requests";
        public List<UKMCABUser> PendingUsers { get; set; }
    }
}
