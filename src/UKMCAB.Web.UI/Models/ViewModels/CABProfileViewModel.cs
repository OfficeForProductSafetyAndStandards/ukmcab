namespace UKMCAB.Web.UI.Models.ViewModels;

public class CABProfileViewModel : ILayoutModel
{
    public string Id { get; set; }
    public string Name { get; set; }

    // Dates
    public string Published { get; set; }
    public string LastUpdated { get; set; }
    
    // Contact details
    public string Address { get; set; }
    public string Website { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

    // Details of appointment
    public List<RegulationViewModel> Regulations { get; set; }
    
    public string BodyNumber { get; set; }
    public string BodyType { get; set; }
    public string RegisteredOfficeLocation { get; set; }
    public string TestingLocations { get; set; }

    public List<string> CertificationActivityLocations { get; set; }

    public List<AppointmentRevisionViewModel> AppointmentRevisions { get; set; }

    public string? Title => Name;

    public bool IsAdmin { get; set; }
}