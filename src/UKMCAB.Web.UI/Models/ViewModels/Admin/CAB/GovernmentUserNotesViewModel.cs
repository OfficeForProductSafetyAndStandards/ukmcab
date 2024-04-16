namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    using UKMCAB.Web.UI.Models.ViewModels.Shared;

    public class GovernmentUserNotesViewModel : ILayoutModel
    {
        public string? Title => "Government user notes";

        public Guid CABId { get; set; }

        public UserNoteListViewModel GovernmentUserNotes { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
