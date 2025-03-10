﻿using UKMCAB.Data.Models;

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
            IsCompleted = TestingLocations.Any() && BodyTypes.Any();
        }

        public string? CABId { get; set; }

        [CannotBeEmpty(ErrorMessage = "Select a registered test location")]
        public List<string> TestingLocations { get; set; } = new();

        [CannotBeEmpty(ErrorMessage = "Select a body type")]
        public List<string> BodyTypes { get; set; } = new();

        public bool isMRA => BodyTypes.Any(t => t.Equals("UK body designated under MRA"));
        public List<string> BodyTypesSummary => BodyTypes.Where(t => !t.Equals("UK body designated under MRA")).ToList();

        public string? Title => "Body details";
        public string? SubTitle { get; set; }
        public string[] FieldOrder => new[] { nameof(TestingLocations), nameof(BodyTypes) };
    }
}
