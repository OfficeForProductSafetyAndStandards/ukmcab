using System.ComponentModel.DataAnnotations;
using UKMCAB.Core.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABBodyDetailsViewModel : CreateEditCABViewModel, ILayoutModel
    {
        public CABBodyDetailsViewModel() { }

        public CABBodyDetailsViewModel(Document document)
        {
            CABId = document.CABId;
            TestingLocations = document.TestingLocations ?? new List<string>();
            BodyTypes = document.BodyTypes ?? new List<string>();
            LegislativeAreas = document.LegislativeAreas ?? new List<string>();
        }

        public string? CABId { get; set; }

        public List<string> TestingLocations { get; set; }
        [Required(ErrorMessage = "Select a body type")]
        public List<string> BodyTypes { get; set; }
        [Required(ErrorMessage = "Select a legislative area")]
        public List<string> LegislativeAreas { get; set; }

        public string? Title => "Body details";

        public string[] FieldOrder => new[] { nameof(TestingLocations), nameof(BodyTypes), nameof(LegislativeAreas) };
    }
}
