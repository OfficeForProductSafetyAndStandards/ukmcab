using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Account;

public record AccountLockedViewModel : BasicPageModel
{
    public UserAccountLockReason? Reason { get; internal set; }
}
