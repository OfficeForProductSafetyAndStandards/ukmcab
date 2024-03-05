namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class LegislativeAreaViewModel
{
    public Guid LegislativeAreaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Regulation { get; set; }
    public bool HasDataModel { get; set; }
    public bool IsProvisional { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? Reason { get; set; }
    public bool IsArchived { get; set; }

    public string? PointOfContactName { get; set; }
    public string? PointOfContactEmail { get; set; }
    public string? PointOfContactPhone { get; set; }
    public bool? IsPointOfContactPublicDisplay { get; set; }
}