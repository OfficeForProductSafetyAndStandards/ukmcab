using System.ComponentModel.DataAnnotations;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaDeclineReasonViewModel : ILayoutModel
    {
        public Guid CabId { get; set; }

        public string LegislativeAreaName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;      

        public LegislativeAreaReviewActionEnum ReviewActionEnum { get; set; }

        [Required(ErrorMessage = "Enter reason", AllowEmptyStrings = false)]
        public string DeclineReason { get; set; } = null!;
    }
}
