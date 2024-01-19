using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class PublishCABViewModel : ILayoutModel
    {
        public string? CABId { get; set; }
        public string? CABName { get; set; }
        public string? ReturnURL { get; set; }
        
        [MaxLength(1000, ErrorMessage = "Maximum note length is 1000 characters")]
        public string? PublishInternalReason { get; set; }
        [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
        public string? PublishPublicReason { get; set; }

        public string? Title => "Publish CAB profile";
    }
}
