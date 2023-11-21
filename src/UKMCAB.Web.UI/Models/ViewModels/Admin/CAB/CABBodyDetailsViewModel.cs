using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
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
        public List<string> TestingLocations { get; set; } = new();

        [CannotBeEmpty(ErrorMessage = "Select a body type")]
        public List<string> BodyTypes { get; set; } = new();

        public List<string> LegislativeAreas { get; set; } = new();

        public List<string>? ProductScheduleLegislativeAreas { get; set; }
        public string? Title => "Body details";
        public string[] FieldOrder => new[] { nameof(TestingLocations), nameof(BodyTypes), nameof(LegislativeAreas) };
    }
}
