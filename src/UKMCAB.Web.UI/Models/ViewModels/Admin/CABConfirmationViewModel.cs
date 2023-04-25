namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABConfirmationViewModel : ILayoutModel
    {
        public string CABId { get; set; }
        public string Name { get; set; }
        public string CABNumber { get; set; }
        public string? Title => "CAB confirmation";
    }
}
