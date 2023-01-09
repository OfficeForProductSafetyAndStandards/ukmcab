using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CreateCABViewModel: ILayoutModel
    {
        [Required]
        public string Name { get; set; }
        public string? UKASReference { get; set; }
        [Required]
        public string Address { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? RegisteredOfficeLocation { get; set; }
        public List<string>? BodyTypes { get; set; }
        public List<string>? Regulations { get; set; }

        public List<string>? CountryList { get; set; }
        public List<string>? BodyTypeList { get; set; }
        public List<string>? RegulationList { get; set; }
        public bool IsUKASUser { get; set; }

        public string? Title => "Create a new CAB";
    }
}
