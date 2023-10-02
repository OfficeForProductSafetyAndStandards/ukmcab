using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public abstract class CreateEditCABViewModel
    {
        public bool IsFromSummary { get; set; }
        public Status DocumentStatus { get; set; }
        public bool IsCompleted { get; set; }
    }
}
