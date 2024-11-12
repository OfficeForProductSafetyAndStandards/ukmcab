using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaApproveViewModel : ILayoutModel
    {
        [Required(ErrorMessage = "Select an option")]
        public LegislativeAreaApproveActionEnum? LegislativeAreaApproveActionEnum { get; set; }

        public Guid CabId { get; set; }

        public CABLegislativeAreasItemViewModel LegislativeArea { get; set; } = new();

        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;

        public List<FileUpload> ProductSchedules { get; set; } = new();

        public LegislativeAreaReviewActionEnum ReviewActionEnum { get; set; }
    }
}
