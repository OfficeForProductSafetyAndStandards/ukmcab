using System.ComponentModel.DataAnnotations;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class LegislativeAreaRemoveViewModel : ILayoutModel
    {
        [Required(ErrorMessage = "Select an option")]
        public RemoveActionEnum Action { get; set; }

        public Guid CabId { get; set; }

        public CABLegislativeAreasItemViewModel LegislativeArea { get; set; } = new();

        public List<FileUpload> ProductSchedules { get; set; } = new();

        public string Title { get; set; } = string.Empty;

        public bool ShowArchiveOption { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
