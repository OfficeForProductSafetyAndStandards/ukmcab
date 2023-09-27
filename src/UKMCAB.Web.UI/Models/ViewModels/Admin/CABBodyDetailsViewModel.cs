using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

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
            ProductScheduleLegislativeAreas = document.Schedules?.Select(sch => sch.LegislativeArea).Distinct().ToList() ?? new List<string>();
            IsCompleted = TestingLocations.Any() && BodyTypes.Any();
        }

        public string? CABId { get; set; }

        [CannotBeEmpty(ErrorMessage = "Select a registered test location")]
        public List<string> TestingLocations { get; set; }
        [CannotBeEmpty(ErrorMessage = "Select a body type")]
        public List<string> BodyTypes { get; set; }
        public List<string> LegislativeAreas { get; set; }

        public List<string>? ProductScheduleLegislativeAreas { get; set; }
        public string? Title => "Body details";
        public string[] FieldOrder => new[] { nameof(TestingLocations), nameof(BodyTypes), nameof(LegislativeAreas) };
    }
}
