using UKMCAB.Web.UI.Models.ViewModels.Search.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class CABLegislativeAreasModel
{
    public List<LegislativeAreasViewModel> LegislativeAreasModel { get; set; } = new();
    public string CabUrl { get; set; } = string.Empty;
    public Guid LegislativeAreaId { get; set; }
    public string? LegislativeAreaName { get; set; } 
    public string? Regulation { get; set; }
    public List<StandardViewModel> PurposeOfAppointments { get; set; } = new();
    public StandardViewModel PurposeOfAppointment { get; set; } = new();
    public StandardViewModel Category { get; set; } = new();
    public StandardViewModel SubCategory { get; set; } = new();
    public List<StandardViewModel> Categories { get; set; } = new();
    public List<StandardViewModel> SubCategories { get; set; } = new();
    public List<StandardViewModel> Products { get; set; } = new();
}