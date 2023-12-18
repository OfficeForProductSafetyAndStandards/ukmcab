using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class UnarchiveCABViewModel : ILayoutModel
    {
        public string? CABId { get; set; }
        public string? CABName { get; set; }
        public string? ReturnURL { get; set; }

        [Required(ErrorMessage = "State the reason for unarchiving this CAB record")]
        [MaxLength(1000, ErrorMessage = "Maximum note length is 1000 characters")]
        public string? UnarchiveInternalReason { get; set; }
        [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
        public string? UnarchivePublicReason { get; set; }

        public string? Title => "Unarchive CAB profile";
    }
}
