using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User;

public class UserAccountViewModel : ILayoutModel
{
    public string? Title => "User account";

    public UserAccount UserAccount { get; set; }
}

public enum UserAccountLockToggleUIMode
{
    Lock,
    Unlock,
    Archive,
    Unarchive,
}

public class UserAccountLockUnlockViewModel : ILayoutModel
{
    public UserAccountLockToggleUIMode Mode { get; set; }

    public string? Title => "Lock user account";

    public string VerbBaseForm => Mode switch
    {
        UserAccountLockToggleUIMode.Lock => "lock",
        UserAccountLockToggleUIMode.Unlock => "unlock",
        UserAccountLockToggleUIMode.Archive => "archive",
        UserAccountLockToggleUIMode.Unarchive => "unarchive",
        _ => throw new NotImplementedException(),
    };

    public string VerbPastTenseForm => Mode switch
    {
        UserAccountLockToggleUIMode.Lock => "locked",
        UserAccountLockToggleUIMode.Unlock => "unlocked",
        UserAccountLockToggleUIMode.Archive => "archived",
        UserAccountLockToggleUIMode.Unarchive => "unarchived",
        _ => throw new NotImplementedException(),
    };

    public string VerbPresentParticiple => Mode switch
    {
        UserAccountLockToggleUIMode.Lock => "locking",
        UserAccountLockToggleUIMode.Unlock => "unlocking",
        UserAccountLockToggleUIMode.Archive => "archiving",
        UserAccountLockToggleUIMode.Unarchive => "unarchiving",
        _ => throw new NotImplementedException(),
    };

    [Required(ErrorMessage = "Enter a reason")]
    public string? Reason { get; set; }

    [Required(ErrorMessage = "Enter notes")]
    public string? Notes { get; set; }
}


