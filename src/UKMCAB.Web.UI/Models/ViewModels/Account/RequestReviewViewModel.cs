using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class RequestReviewViewModel: ILayoutModel
    {
        public string? Title => "Registration request review";
        public UKMCABUser UserForReview { get; set; }

        public string RejectionReason { get; set; }
    }
}
