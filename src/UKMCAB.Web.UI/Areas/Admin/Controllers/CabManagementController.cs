using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Route("admin"), Authorize(Policy = Policies.CabManagement)]
    public class CabManagementController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IUserService _userService;

        public static class Routes
        {
            public const string CABManagement = "admin.cab-management";
        }

        public CabManagementController(ICABAdminService cabAdminService, IUserService userService)
        {
            _cabAdminService = cabAdminService;
            _userService = userService;
        }

        [HttpGet, Route("cab-management", Name = Routes.CABManagement)]
        public async Task<IActionResult> CABManagement(CABManagementViewModel model)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = "lastupd-desc";
            }

            var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value) ?? throw new InvalidOperationException("User account not found");
            var cabManagementItems = await _cabAdminService.FindAllCABManagementQueueDocumentsForUserRole(userAccount.Role);
            model.CABManagementItems = cabManagementItems.Any()
                ? cabManagementItems.Select(cmi => new CABManagementItemViewModel
                {
                    Id = cmi.CABId,
                    Name = cmi.Name,
                    URLSlug = cmi.URLSlug,
                    CABNumber = cmi.CABNumber,
                    CabNumberVisibility = cmi.CabNumberVisibility,
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
