using UKMCAB.Web.UI.Models.ViewModels.Search.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class CABLegislativeAreaPurposeOfAppointmentViewModel
{
    public string CabUrl { get; set; }
    public Guid LegislativeAreaId { get; set; }
    public string? LegislativeAreaName { get; set; } 
    public string? Regulation { get; set; }
    public List<StandardViewModel> PurposeOfAppointments { get; set; }
}