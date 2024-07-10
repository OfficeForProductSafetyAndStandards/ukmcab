using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public abstract class CreateEditCABViewModel
    {
        public bool IsFromSummary { get; set; }
        public string? ReturnUrl { get; set; }
        public Status DocumentStatus { get; set; }
        public bool IsCompleted { get; set; }
        public bool ShowSubsectionEditAction { get; set; } //new
        public string SubsectionActionName { get; set; } = string.Empty; //new
    }
}
