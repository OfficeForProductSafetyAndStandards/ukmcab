using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Data;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABPublishTypeViewModel
    {
        public CABPublishTypeViewModel()
        {            
        }
        public CABPublishTypeViewModel(bool isOPSSUser)
        {
            IsOPSSUser = isOPSSUser;
        }
        public string? SelectedPublishType { get; set; }

        public bool IsOPSSUser { get; set; }

        public IEnumerable<SelectListItem> PublishTypes => new List<SelectListItem>()
        {
            new SelectListItem("Publish as a minor change (this will not update the \'Last published date\' for the CAB, the site feed, or any email subscriptions that the CAB appears in)", DataConstants.PublishType.MinorPublish),
            new SelectListItem("Publish as a major change", DataConstants.PublishType.MajorPublish)
        };
    }
}
