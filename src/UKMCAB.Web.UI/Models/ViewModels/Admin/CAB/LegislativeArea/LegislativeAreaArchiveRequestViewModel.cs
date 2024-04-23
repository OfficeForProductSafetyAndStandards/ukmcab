using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public record LegislativeAreaArchiveRequestViewModel(Guid CabId, string? Title) : ILayoutModel
    {
        [Required(ErrorMessage = "Enter reason")]
        [MaxLength(1000, ErrorMessage = "Maximum user notes length is 1000 characters")]
        public string? ArchiveReason { get; set; }

        public string? LegislativeAreaName { get; set; }
        public string? ReturnUrl { get; set; }
    }
}