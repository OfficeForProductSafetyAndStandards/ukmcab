using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Account;

public class AccountLockedViewModel : BasicPageModel
{
    public UserAccountLockReason? Reason { get; internal set; }
}
