using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABBodyDetailsViewModel : ILayoutModel
    {
        public string? CABId { get; set; }

        public List<string> TestingLocations { get; set; }
        [Required(ErrorMessage = "Select a body type")]
        public List<string> BodyTypes { get; set; }
        [Required(ErrorMessage = "Select a legislative area")]
        public List<string> LegislativeAreas { get; set; }

        public string? Title => "Body details";
    }
}
