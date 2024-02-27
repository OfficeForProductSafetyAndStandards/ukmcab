using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class ScheduleFileUploadViewModel : CreateEditCABViewModel, ILayoutModel 
    {
        public string? Title { get; set; }
        public string? CABId { get; set; }
        public List<FileViewModel>? UnArchivedFiles { get; set; }       
        public string? SelectedScheduleId { get; set; }
        public string? SelectedArchivedScheduleId { get; set; }
    }
}
