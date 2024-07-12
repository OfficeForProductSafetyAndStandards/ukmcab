namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABGovernmentUserNotesViewModel : CreateEditCABViewModel
    {
        public DateTime? LastGovernmentUserNoteDate { get; set; }
        public string? CABId { get; set; }
        public string? Id { get; set; }
        public int GovernmentUserNoteCount { get; set; }
        public string? ReturnUrl { get; set; }
    }
}

