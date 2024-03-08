using System.ComponentModel.DataAnnotations;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class LegislativeAreaRemoveWithOptionViewModel : LegislativeAreaRemoveViewModel
    {
        [Required(ErrorMessage = "Select an option for the product schedules")]
        public RemoveActionEnum? ProductScheduleAction { get; set; }

        public List<FileUpload> ProductSchedules { get; set; } = new();
    }
}
