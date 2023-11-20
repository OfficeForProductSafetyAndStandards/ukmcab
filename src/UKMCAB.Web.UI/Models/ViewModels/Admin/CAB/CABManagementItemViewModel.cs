namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABManagementItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string URLSlug { get; set; }= string.Empty;
        public string CABNumber { get; set; }= string.Empty;
        public string? CabNumberVisibility { get; set; }
        public string Status { get; set; }= string.Empty;
        public DateTime LastUpdated { get; set; }
        
    }
}
