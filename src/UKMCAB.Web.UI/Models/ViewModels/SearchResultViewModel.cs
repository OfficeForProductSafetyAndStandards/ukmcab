
namespace UKMCAB.Web.UI.Models.ViewModels;

public class SearchResultViewModel
{
    public string id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Website { get; set; }
    public string WebsiteURL => Website.StartsWith("http") ? Website : $"https://{Website}"; 
    public string Regulations { get; set; }
}
