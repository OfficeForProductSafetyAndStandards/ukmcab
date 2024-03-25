using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class CABProfileViewModel : ILayoutModel
    {
        public string? Title => $"CAB profile - {Name}";
        public string? ReturnUrl { get; set; }
        public string? Status { get; set; }
        public string? StatusCssStyle { get; init; }
        public string? SubStatus { get; set; }
        public string? ArchivedBy { get; set; }
        public string? ArchivedDate { get; set; }
        public string? ArchiveReason { get; set; }
        public bool IsArchived { get; set; }
        public bool IsUnarchivedRequest { get; set; }
        public bool ShowRequestToUnarchive { get; set; }
        public bool ShowRequestToUnpublish { get; set; }
        public bool IsPublished { get; set; }
        public bool HasDraft { get; set; }
        public AuditLogHistoryViewModel? AuditLogHistory { get; set; }

        public string CABId { get; set; }
        public string CABUrl { get; set; }

        // Publish attributes
        public DateTime? PublishedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // About
        public string? Name { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        
        public bool IsOPSSUser { get; init; }

        public string? UKASReferenceNumber { get; set; }

        // Contact details
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PointOfContactName { get; set; }
        public string? PointOfContactEmail { get; set; }
        public string? PointOfContactPhone { get; set; }
        public bool? IsPointOfContactPublicDisplay { get; set; }

        public string? RegisteredOfficeLocation { get; set; }
        // Body details
        public List<string> RegisteredTestLocations { get; set; } = new();
        public string BodyNumber { get; set; }
        public string? CabNumberVisibility { get;  set; }
        public List<string> BodyTypes { get; set; } = new();
        public List<string> LegislativeAreas { get; set; } = new();
        public CABDocumentsViewModel ProductSchedules { get; set; } = new();
        public CABLegislativeAreasModel CabLegislativeAreas { get; set; } = new();
        public CABDocumentsViewModel SupportingDocuments { get; set; } = new();
        public FeedLinksViewModel FeedLinksViewModel { get; set; } = new();
        public UserNoteListViewModel? GovernmentUserNotes { get; set; } 
    
        public string? RequestFirstAndLastName { get; set; }
        public string? RequestUserGroup { get; set; }
        public string? RequestReasonSummary { get; set; }
        public string? RequestReason { get; set; }
        
        public TaskType? RequestTaskType { get; set; }
        
    }
}
