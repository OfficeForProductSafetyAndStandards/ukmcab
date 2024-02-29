using UKMCAB.Core.Domain;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Schedule
{
    public class RemoveScheduleViewModel : ILayoutModel
    {
        public string Title { get; set; } = string.Empty;

        public Guid CabId { get; set; }

        public FileUpload? FileUpload { get; set; }

        public RemoveActionEnum? RemoveScheduleAction { get; set; }
    }
}
