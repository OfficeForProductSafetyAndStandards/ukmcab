using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABGovernmentUserNotesViewModel : CreateEditCABViewModel
    {
        public CABGovernmentUserNotesViewModel(Document latest, string? returnUrl)
        {
            Id = latest.id;
            CABId = latest.CABId;
            GovernmentUserNoteCount = latest.GovernmentUserNotes.Count;
            LastGovernmentUserNoteDate = Enumerable.MaxBy(latest.GovernmentUserNotes, u => u.DateTime)?.DateTime;
            ReturnUrl = returnUrl;
        }
        public DateTime? LastGovernmentUserNoteDate { get; set; }
        public string? CABId { get; set; }
        public string? Id { get; set; }
        public int GovernmentUserNoteCount { get; set; }
    }
}

