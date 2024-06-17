using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaRemoveRequestViewModel : ILayoutModel
    {
        public Guid CabId { get; set; }

        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter reason")]
        [MaxLength(1000, ErrorMessage = "Maximum user notes length is 1000 characters")]
        public string? UserNotes { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
