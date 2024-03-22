using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaRemoveWithOptionViewModel : LegislativeAreaRemoveViewModel
    {
        [Required(ErrorMessage = "Select an option for the product schedules")]
        public RemoveActionEnum? ProductScheduleAction { get; set; }

        public List<FileUpload> ProductSchedules { get; set; } = new();
    }
}
