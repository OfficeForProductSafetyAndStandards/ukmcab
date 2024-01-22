namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    using System.ComponentModel.DataAnnotations;

    public class UserNoteCreateViewModel : ILayoutModel
    {
        public string? Title { get; set; }

        public Guid CabDocumentId { get; set; }

        [Required(ErrorMessage = "Enter notes")]
        [MaxLength(1000, ErrorMessage = "Maximum note length is 1000 characters")]
        public string Note { get; set; }

        public string ReturnUrl { get; set; }
    }
}