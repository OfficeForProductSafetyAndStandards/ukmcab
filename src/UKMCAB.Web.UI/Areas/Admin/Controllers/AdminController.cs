using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{

    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin}")]
    public class AdminController : Controller
    {
        private readonly ICABAdminService _cabAdminService;

        public AdminController(ICABAdminService cabAdminService)
        {
            _cabAdminService = cabAdminService;
        }

        [Route("/admin")]
        [Route("/admin/work-queue")]
        public async Task<IActionResult> Index(WorkQueueViewModel model)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = "default";
            }

            var workQueueItems = await _cabAdminService.FindAllWorkQueueDocuments();
            model.WorkQueueItems = workQueueItems.Any()
                ? workQueueItems.Select(wqi => new WorkQueueItemViewModel
                {
                    Id = wqi.CABId,
                    Name = wqi.Name,
                    CABNumber = wqi.CABNumber,
                    Status = "Draft", // TODO: we don't have archived yet
                    LastUpdated = wqi.LastUpdatedDate
                }).ToList()
                : new List<WorkQueueItemViewModel>();
            FilterAndSortItems(model);

            return View(model);
        }

        private void FilterAndSortItems(WorkQueueViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Filter))
            {
                model.WorkQueueItems = model.WorkQueueItems.Where(wqi =>
                    model.Filter.Equals(wqi.Status, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            switch (model.Sort.ToLower())
            {
                case "status-desc":
                    model.WorkQueueItems = model.WorkQueueItems.OrderByDescending(wqi => wqi.Status).ToList();
                    break;
                case "status":
                    model.WorkQueueItems = model.WorkQueueItems.OrderBy(wqi => wqi.Status).ToList();
                    break;
                case "number-desc":
                    model.WorkQueueItems = model.WorkQueueItems.OrderByDescending(wqi => wqi.CABNumber).ToList();
                    break;
                case "number":
                    model.WorkQueueItems = model.WorkQueueItems.OrderBy(wqi => wqi.CABNumber).ToList();
                    break;
                case "name-desc":
                    model.WorkQueueItems = model.WorkQueueItems.OrderByDescending(wqi => wqi.Name).ToList();
                    break;
                case "name":
                    model.WorkQueueItems = model.WorkQueueItems.OrderBy(wqi => wqi.Name).ToList();
                    break;
                default:
                    model.WorkQueueItems = model.WorkQueueItems.OrderByDescending(wqi => wqi.LastUpdated).ToList();
                    break;
            }
        }
    }
}
