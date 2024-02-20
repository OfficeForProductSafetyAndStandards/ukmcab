using UKMCAB.Web.UI.Models.ViewModels.Search.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class CABLegislativeAreasModel
{
    public List<LegislativeAreasViewModel> LegislativeAreasModel { get; set; }
    public string CabUrl { get; set; }
    public CABLegislativeAreaView CABLegislativeAreaView { get; set; }
    public CABLaPurposeOfAppointmentViewModel? PurposeOfAppointments { get; set; }
    public CABLaPoaCategoriesViewModel? PoaCategories { get; set; }
}