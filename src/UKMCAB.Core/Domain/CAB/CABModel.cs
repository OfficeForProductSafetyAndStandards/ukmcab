using UKMCAB.Data.Models;

namespace UKMCAB.Core.Domain.CAB;

public class CabModel
{
    public Guid Id { get; set; }
    public Status StatusValue { get; set; }
    public SubStatus SubStatus { get; internal set; }
    public Guid CABId { get; set; }
    public string? Name { get; set; } = null!;
    public string? CABNumber { get; set; }
    public string? CabNumberVisibility { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? UKASReference { get; set; }


    public GeoAddress? Address { get; set; }
    public OrganisationContactDetails? OrganisationContactDetails { get; set; }
    public PointOfContact? PointOfContact { get; set; } 

    public string? RegisteredOfficeLocation { get; set; }

    public List<string> TestingLocations { get; set; } = new();
    public List<string> BodyTypes { get; set; } = new();
    public List<string> LegislativeAreas { get; set; } = new();

    public List<FileUpload> Schedules { get; set; } = new();

    public string ScheduleLabels => string.Join(", ", Schedules?.Select(sch => sch.Label) ?? new List<string>());

    public List<FileUpload> SupportingDocuments { get; set; } = new();

    public string DocumentLabels =>
        string.Join(", ", SupportingDocuments?.Select(doc => doc.Label) ?? new List<string>());

    public string? HiddenText { get; set; } = null!;
    public string? RandomSort { get; set; } = null!;

    public string URLSlug { get; set; } = string.Empty;
 
    public string Status => StatusValue.ToString();

    // New audit
    public List<Audit> AuditLog { get; set; } = new();

    // Used by the search index, saves a lot of effort to flatten the model in the data source
    public DateTime LastUpdatedDate => AuditLog.Any() ? AuditLog.Max(al => al.DateTime) : DateTime.MinValue;

}