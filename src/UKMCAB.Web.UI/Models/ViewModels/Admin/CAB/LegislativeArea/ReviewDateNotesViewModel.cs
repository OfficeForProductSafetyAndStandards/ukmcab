using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class ReviewDateNotesViewModel : ILayoutModel
    {
        public Guid? CabId { get; set; }
        public Guid? LegislativeAreaId { get; set; }
        public string LegislativeAreaName { get; set; }

        [DisplayName("Review date")]
        [Required]
        public DateTime? ReviewDate { get; set; }

        [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
        [Required(ErrorMessage = "Enter user notes")]
        public string? UserNotes { get; init; }
        [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
        public string? Reason { get; init; }

        public bool FromSummary { get; set; }
        public string Title => $"Legislative area {LegislativeAreaName} review date updated";
    }
}
