using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class UnarchiveLegislativeAreaRequestReasonViewModel : ILayoutModel
    {
        public Guid CabId { get; set; }
        public Guid LegislativeAreaId { get; set; }

        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter reason")]
        [MaxLength(1000, ErrorMessage = "Maximum user notes length is 1000 characters")]
        public string? UserNotes { get; set; }

        [MaxLength(1000, ErrorMessage = "Maximum user notes length is 1000 characters")]
        public string? PublicUserNotes { get; set; }

        public bool FromSummary { get; set; }
    }
}
