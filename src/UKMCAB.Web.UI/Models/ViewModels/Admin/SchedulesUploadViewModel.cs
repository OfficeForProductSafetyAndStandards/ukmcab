namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class SchedulesUploadViewModel : ILayoutModel
    {
        public string? Title => "Upload the Schedule of Accreditation";

        public string  Id { get; set; }

        public List<string>? UploadedFiles { get; set; }

        public List<IFormFile>? Files { get; set; }
    }
}
