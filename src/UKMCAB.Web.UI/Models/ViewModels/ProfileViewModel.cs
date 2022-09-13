using System.Security.Principal;

namespace UKMCAB.Web.UI.Models.ViewModels;

public class ProfileViewModel
{
    public string Name { get; set; }

    public string Published { get; set; }
    public string LastUpdated { get; set; }

    public string Address { get; set; }
    public string Website { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

    public List<DetailsOfAppointmentViewModel> DetailsOfAppointments { get; set; }
    
    public string BodyNumber { get; set; }
    public string BodyType { get; set; }
    public string RegisteredOfficeLocation { get; set; }
    public string TestingLocations { get; set; }
    public string AccreditaionBody { get; set; }
    public string AccreditationStandard { get; set; }
    public string AppointmentDetails { get; set; }

    public string Locations { get; set; }

    public List<AppointmentRevisionViewModel> AppointmentRevisions { get; set; }

}