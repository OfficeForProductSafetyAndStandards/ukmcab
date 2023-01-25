namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class SchedulesListViewModel: ILayoutModel
    {
        public string Id { get; set; }
        public List<string>? UploadedFiles { get; set; }
        public string? Title => "Schedule of Accreditation uploaded";
    }
}
