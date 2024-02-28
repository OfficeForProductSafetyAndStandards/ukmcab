using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public class CABAuditLogHistoryViewModel : ILayoutModel
{
    public string? Title { get; set; } = "History";

    public AuditLogHistoryViewModel AuditLogHistory { get; set; }

    public string? ReturnUrl { get; set; }
}
