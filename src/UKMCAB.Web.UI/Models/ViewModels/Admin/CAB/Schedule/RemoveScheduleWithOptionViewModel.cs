using System.ComponentModel.DataAnnotations;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Schedule
{
    public class RemoveScheduleWithOptionViewModel : RemoveScheduleViewModel
    {
        [Required(ErrorMessage = "Select an option for the legislative area")]
        public LegislativeAreaActionEnum? RemoveLegislativeAction { get; set; }
    }
}
