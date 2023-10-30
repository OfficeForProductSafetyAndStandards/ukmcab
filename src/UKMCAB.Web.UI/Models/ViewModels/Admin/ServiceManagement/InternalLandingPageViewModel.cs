using UKMCAB.Web.UI.Models.ViewModels.Search;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.ServiceManagement
{
    public class InternalLandingPageViewModel : SearchViewModel
    {
        public new string? Title => "Manage the service";
        public int TotalDraftCABs { get; set; }
        public int TotalCABsPendingApproval { get; set; }
        public int TotalAccountRequests { get; set; }
       
    }
}
