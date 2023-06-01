using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services;
using UKMCAB.Data;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

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
        [Route("/admin/cab-management")]
        public async Task<IActionResult> Index(WorkQueueViewModel model)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = "lastupd-desc";
            }

            var workQueueItems = await _cabAdminService.FindAllWorkQueueDocuments();
            model.WorkQueueItems = workQueueItems.Any()
                ? workQueueItems.Select(wqi => new WorkQueueItemViewModel
                {
                    Id = wqi.CABId,
                    Name = wqi.Name,
                    CABNumber = wqi.CABNumber,
                    Status = wqi.Status,
                    LastUpdated = wqi.LastUpdatedDate
                }).ToList()
                : new List<WorkQueueItemViewModel>();

            FilterSortAndPaginateItems(model);

            return View(model);
        }

        private void FilterSortAndPaginateItems(WorkQueueViewModel model)
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
                case "lastupd":
                    model.WorkQueueItems = model.WorkQueueItems.OrderBy(wqi => wqi.LastUpdated).ToList();
                    break;
                case "lastupd-desc":
                default:
                    model.WorkQueueItems = model.WorkQueueItems.OrderByDescending(wqi => wqi.LastUpdated).ToList();
                    break;
            }
            model.Pagination = new PaginationViewModel
            {
                Total = model.WorkQueueItems.Count,
                PageNumber = model.PageNumber,
                ResultsPerPage = DataConstants.Search.WorkQueurResultsPerPage,
                ResultType = "items"
            };

            if (model.Pagination.Total > 10)
            {
                var skip = (model.PageNumber - 1) * 10; 
                model.WorkQueueItems = model.WorkQueueItems.Skip(skip).Take(10).ToList();
            }
        }
    }
}
