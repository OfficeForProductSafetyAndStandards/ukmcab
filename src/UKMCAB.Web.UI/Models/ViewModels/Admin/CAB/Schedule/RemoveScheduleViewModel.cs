using System.ComponentModel.DataAnnotations;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Schedule
{
    public class RemoveScheduleViewModel : ILayoutModel
    {
        [Required(ErrorMessage = "Select an option")]
        public RemoveActionEnum Action { get; set; }

        public string Title { get; set; } = string.Empty;

        public Guid CabId { get; set; }

        public List<string> ScheduleFileLabelList { get; set; } = new();

        public string? ReturnUrl { get; set; }
    }
}
