using UKMCAB.Web.UI.Models.ViewModels.Search;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.ServiceManagement
{
    public class InternalLandingPageViewModel : SearchViewModel
    {
        public override string? Title => Constants.PageTitle.Home;
        public int TotalDraftCABs { get; init; }
        public int TotalCABsPendingApproval { get; init; }
        public int TotalAccountRequests { get; init; }
        public int UnassignedNotification { get; init; }
        public int AssignedNotification { get; init; }
        public int AssignedToMeNotification { get; init; }
    }
}