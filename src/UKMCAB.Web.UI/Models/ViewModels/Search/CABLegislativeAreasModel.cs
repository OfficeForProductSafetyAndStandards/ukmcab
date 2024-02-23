namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class CABLegislativeAreasModel
{
    public List<LegislativeAreasViewModel> LegislativeAreasModel { get; set; } = new();
    public string CabUrl { get; set; } = string.Empty;
    public Guid LegislativeAreaId { get; set; }
    public string? LegislativeAreaName { get; set; }
    public string? Regulation { get; set; }
    public List<(Guid Id, string Name)> PurposeOfAppointments { get; set; } = new();
    public (Guid? Id, string? Name) PurposeOfAppointment { get; set; }
    public (Guid? Id, string? Name) Category { get; set; }
    public (Guid? Id, string? Name) SubCategory { get; set; }
    public (Guid? Id, string? Name) Product { get; set; }
    public List<(Guid Id, string Name)> Categories { get; set; } = new();
    public List<(Guid Id, string Name)> SubCategories { get; set; } = new();
    public List<(Guid Id, string Name)> Products { get; set; } = new();
    public List<(Guid Id, string Name)> Procedures { get; set; } = new();
    public bool ShowLabels { get; set; } = true;
}