namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class AuditHistoryItemViewModel:ILayoutModel
    {
        public string? Title => "History details";
        public string Date { get; set; }
        public string Time { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Usergroup { get; set; }
        public string Action { get; set; }
        public string InternalComment { get; set; }
        public string PublicComment { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsUserInputComment { get; set; }
    }
}
