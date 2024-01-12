namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    using System.ComponentModel.DataAnnotations;

    public class UserNoteCreateViewModel : ILayoutModel
    {
        public string? Title { get; set; }

        public Guid CabDocumentId { get; set; }

        [Required(ErrorMessage = "Enter notes")]
        public string Note { get; set; }

        public string ReturnUrl { get; set; }
    }
}