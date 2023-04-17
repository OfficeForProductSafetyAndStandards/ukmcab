using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABSummaryViewModel: ILayoutModel //IValidatableObject, ILayoutModel
    {
        public string? CABId { get; set; }
        public CABDetailsViewModel? CabDetailsViewModel { get; set; }
        public CABContactViewModel? CabContactViewModel { get; set; }
        public CABBodyDetailsViewModel? CabBodyDetailsViewModel { get; set; }
        // Schedules of accreditation
        public List<FileUpload>? Schedules { get; set; }
        // Supporting documents
        public List<FileUpload>? Documents { get; set; }



        public bool ValidCAB { get; set; }
        public bool ShowError { get; set; }
        public string? Title => "Check details before publishing";
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Schedules == null || !Schedules.Any())
        //    {
        //        yield return new ValidationResult("At least one schedule of accreditation is required");
        //    }
        //}
    }
}
