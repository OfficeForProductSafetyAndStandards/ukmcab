using UKMCAB.Data.Models;
using UKMCAB.Core.Extensions;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABGovernmentUserNotesViewModel : CreateEditCABViewModel
    {
        public CABGovernmentUserNotesViewModel()
        {
        }

        public CABGovernmentUserNotesViewModel(Document latest, string? returnUrl)
        {
            Id = latest.id;
            CABId = latest.CABId;
            GovernmentUserNoteCount = latest.GovernmentUserNotes.Count;
            LastGovernmentUserNoteDate = latest.LastGovernmentUserNoteDate();
            ReturnUrl = returnUrl;
        }
        public DateTime? LastGovernmentUserNoteDate { get; private set; }
        public string? CABId { get; private set; }
        public string? Id { get; private set; }
        public int GovernmentUserNoteCount { get; private set; }
    }
}

