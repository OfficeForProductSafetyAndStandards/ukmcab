using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class UnarchiveLegislativeAreaRequestViewModel : ILayoutModel
    {
        public Guid CabId { get; set; }

        public CABLegislativeAreasItemViewModel LegislativeArea { get; set; } = new();

        public string Title { get; set; } = string.Empty;

        public List<FileUpload> ActiveProductSchedules { get; set; } = new();
        
    }
}
