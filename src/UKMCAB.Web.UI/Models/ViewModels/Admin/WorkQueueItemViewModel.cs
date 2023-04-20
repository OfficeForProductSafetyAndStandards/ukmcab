namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class WorkQueueItemViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CABNumber { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
