using System.ComponentModel.DataAnnotations;
using UKMCAB.Core.Domain;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Schedule
{
    public class RemoveScheduleViewModel : ILayoutModel
    {
        [Required(ErrorMessage = "Select an option")]
        public RemoveActionEnum? RemoveAction { get; set; }

        public string Title { get; set; } = string.Empty;

        public Guid CabId { get; set; }

        public string? ReturnUrl { get; set; }

        public FileUpload? FileUpload { get; set; }

        public bool LastSchedule { get; set; }
    }
}
