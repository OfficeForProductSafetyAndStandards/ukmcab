namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class WorkQueueViewModel : ILayoutModel
    {
        public string? Title => "Admin";
        public string Filter { get; set; }
        public string Sort { get; set; }
        public List<WorkQueueItemViewModel> WorkQueueItems { get; set; }
    }
}
