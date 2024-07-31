using System.ComponentModel.DataAnnotations;
using UKMCAB.Core.Extensions;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABPublishMajorOrMinorViewModel : CreateEditCABViewModel
    {
        public CABPublishMajorOrMinorViewModel() { }

        public CABPublishMajorOrMinorViewModel(Document document, string? returnUrl)
        {
            LastAuditLogHistoryDate = document.LastAuditLogHistoryDate();
            CABId = document.CABId;
            ReturnUrl = returnUrl;
        }
        public DateTime? LastAuditLogHistoryDate { get; private set; }
        public string? CABId { get; private set; }

        [Required(ErrorMessage = "Select a product category")]
        public string? SelectedPublishOption { get; set; }

        public string? MinorPublish { get; private set; }
        public string? MajorPublish { get; private set; }

    }
}
