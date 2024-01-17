namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class UserNoteViewModel : ILayoutModel
    {
        public string? Title { get; set; }

        public Guid Id { get; set; }
        public Guid CabDocumentId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserGroup { get; set; }
        public DateTime DateAndTime { get; set; }
        public string Note { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsOPSSOrInCreatorUserGroup { get; set; }
    }
}