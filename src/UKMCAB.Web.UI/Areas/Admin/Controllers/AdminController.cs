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
        public async Task<IActionResult> Index(CABManagementViewModel model)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = "lastupd-desc";
            }

            var cabManagementItems = await _cabAdminService.FindAllCABManagementQueueDocuments();
            model.CABManagementItems = cabManagementItems.Any()
                ? cabManagementItems.Select(cmi => new CABManagementItemViewModel
                {
                    Id = cmi.CABId,
                    Name = cmi.Name,
                    URLSlug = cmi.URLSlug,
                    CABNumber = cmi.CABNumber,
                    Status = cmi.Status,
                    LastUpdated = cmi.LastUpdatedDate
                }).ToList()
                : new List<CABManagementItemViewModel>();

            FilterSortAndPaginateItems(model);

            return View(model);
        }

        private void FilterSortAndPaginateItems(CABManagementViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Filter))
            {
                model.CABManagementItems = model.CABManagementItems.Where(wqi =>
                    model.Filter.Equals(wqi.Status, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            switch (model.Sort.ToLower())
            {
                case "status-desc":
                    model.CABManagementItems = model.CABManagementItems.OrderByDescending(cmi => cmi.Status).ThenByDescending(cmi => cmi.LastUpdated).ToList();
                    break;
                case "status":
                    model.CABManagementItems = model.CABManagementItems.OrderBy(cmi => cmi.Status).ThenByDescending(cmi => cmi.LastUpdated).ToList();
                    break;
                case "number-desc":
                    model.CABManagementItems = model.CABManagementItems.OrderByDescending(cmi => cmi.CABNumber).ToList();
                    break;
                case "number":
                    model.CABManagementItems = model.CABManagementItems.OrderBy(cmi => cmi.CABNumber).ToList();
                    break;
                case "name-desc":
                    model.CABManagementItems = model.CABManagementItems.OrderByDescending(cmi => cmi.Name).ToList();
                    break;
                case "name":
                    model.CABManagementItems = model.CABManagementItems.OrderBy(cmi => cmi.Name).ToList();
                    break;
                case "lastupd":
                    model.CABManagementItems = model.CABManagementItems.OrderBy(cmi => cmi.LastUpdated).ToList();
                    break;
                case "lastupd-desc":
                default:
                    model.CABManagementItems = model.CABManagementItems.OrderByDescending(cmi => cmi.LastUpdated).ToList();
                    break;
            }
            model.Pagination = new PaginationViewModel
            {
                Total = model.CABManagementItems.Count,
                PageNumber = model.PageNumber,
                ResultsPerPage = DataConstants.Search.CABManagementQueueResultsPerPage,
                ResultType = "items"
            };

            if (model.Pagination.Total > 10)
            {
                var skip = (model.PageNumber - 1) * 10; 
                model.CABManagementItems = model.CABManagementItems.Skip(skip).Take(10).ToList();
            }
        }
    }
}
