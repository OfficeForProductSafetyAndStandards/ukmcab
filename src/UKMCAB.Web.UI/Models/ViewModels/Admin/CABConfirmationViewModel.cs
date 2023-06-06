namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABConfirmationViewModel : ILayoutModel
    {
        public string Name { get; set; }
        public string URLSlug { get; set; }
        public string CABNumber { get; set; }
        public string? Title => "CAB confirmation";
    }
}
