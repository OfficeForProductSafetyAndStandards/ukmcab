using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Route("admin"), Authorize(Policy = Policies.CabManagement)]
    public class CabManagementController : UI.Controllers.ControllerBase
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IEditLockService _editLockService;

        public static class Routes
        {
            public const string CABManagement = "admin.cab-management";
        }

        public static class TabNames
        {
            public const string All = "all";
            public const string Draft = "draft";
            public const string PendingDraft = "pending-draft";
            public const string PendingPublish = "pending-publish";
            public const string PendingArchive = "pending-archive";
        }

        public CabManagementController(ICABAdminService cabAdminService, IUserService userService, IEditLockService editLockService) :
            base(userService)
        {
            _cabAdminService = cabAdminService;
            _editLockService = editLockService;
        }

        [HttpGet, Route("cab-management", Name = Routes.CABManagement)]
        public async Task<IActionResult> CABManagement([FromQuery] string? unlockCab, string? tabName = TabNames.All, int pageNumber = 1, 
            [FromQuery(Name = "sf")] string? sortField = null, [FromQuery(Name = "sd")] string? sortDirection = null)
        {
            if (!string.IsNullOrWhiteSpace(unlockCab))
            {
                await _editLockService.RemoveEditLockForCabAsync(unlockCab);
            }

            var cabs = await _cabAdminService.FindAllCABManagementQueueDocumentsForUserRole(CurrentUser.Role);

            var model = new CABManagementViewModel
            {
                AllCount = cabs.AllCabs.Count(),
                DraftCount = cabs.DraftCabs.Count(),
                PendingDraftCount = cabs.PendingDraftCabs.Count(),
                PendingPublishCount = cabs.PendingPublishCabs.Count(),
                PendingArchiveCount = cabs.PendingArchiveCabs.Count(),
                Pagination = new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultType = string.Empty,
                    ResultsPerPage = Constants.RowsPerPage
                },
                TabName = tabName,
                SortField = sortField ?? nameof(CABManagementItemViewModel.LastUpdated),
                SortDirection = sortDirection ?? SortDirectionHelper.Descending,
                RoleId = UserRoleId,
            };

            switch(tabName)
            {
                case TabNames.All:
                    model.CABManagementItems = cabs.AllCabs.Select(d => new CABManagementItemViewModel(d)).ToList();
                    break;
                case TabNames.Draft:
                    model.CABManagementItems = cabs.DraftCabs.Select(d => new CABManagementItemViewModel(d)).ToList();
                    break;
                case TabNames.PendingDraft:
                    model.CABManagementItems = cabs.PendingDraftCabs.Select(d => new CABManagementItemViewModel(d)).ToList();
                    break;
                case TabNames.PendingPublish:
                    model.CABManagementItems = cabs.PendingPublishCabs.Select(d => new CABManagementItemViewModel(d)).ToList();
                    break;
                case TabNames.PendingArchive:
                    model.CABManagementItems = cabs.PendingArchiveCabs.Select(d => new CABManagementItemViewModel(d)).ToList();
                    break;
            }
            model.Pagination.Total = model.CABManagementItems.Count();

            SortAndPaginateItems(model);

            return View(model);
        }

        private void SortAndPaginateItems(CABManagementViewModel model)
        {
            switch (model.SortField)
            {
                case nameof(CABManagementItemViewModel.Status):
                    model.CABManagementItems = model.SortDirection == SortDirectionHelper.Ascending ? 
                        model.CABManagementItems.OrderBy(cmi => cmi.Status).ThenByDescending(cmi => cmi.LastUpdated).ToList() :
                        model.CABManagementItems.OrderByDescending(cmi => cmi.Status).ThenByDescending(cmi => cmi.LastUpdated).ToList();
                    break;
                
                case nameof(CABManagementItemViewModel.CABNumber):
                    model.CABManagementItems = model.SortDirection == SortDirectionHelper.Ascending ?
                        model.CABManagementItems.OrderBy(cmi => cmi.CABNumber).ThenByDescending(cmi => cmi.LastUpdated).ToList() :
                        model.CABManagementItems.OrderByDescending(cmi => cmi.CABNumber).ThenByDescending(cmi => cmi.LastUpdated).ToList();
                    break;

                case nameof(CABManagementItemViewModel.UKASReference):
                    model.CABManagementItems = model.SortDirection == SortDirectionHelper.Ascending ?
                        model.CABManagementItems.OrderBy(cmi => cmi.UKASReference).ThenByDescending(cmi => cmi.LastUpdated).ToList() :
                        model.CABManagementItems.OrderByDescending(cmi => cmi.UKASReference).ThenByDescending(cmi => cmi.LastUpdated).ToList();
                    break;

                case nameof(CABManagementItemViewModel.Name):
                    model.CABManagementItems = model.SortDirection == SortDirectionHelper.Ascending ?
                        model.CABManagementItems.OrderBy(cmi => cmi.Name).ThenByDescending(cmi => cmi.LastUpdated).ToList() :
                        model.CABManagementItems.OrderByDescending(cmi => cmi.Name).ThenByDescending(cmi => cmi.LastUpdated).ToList();
                    break;
                
                case nameof(CABManagementItemViewModel.LastUpdated):
                    model.CABManagementItems = model.SortDirection == SortDirectionHelper.Ascending ?
                        model.CABManagementItems.OrderBy(cmi => cmi.LastUpdated).ThenBy(cmi => cmi.CABNumber).ToList() :
                        model.CABManagementItems.OrderByDescending(cmi => cmi.LastUpdated).ThenBy(cmi => cmi.CABNumber).ToList();
                    break;

                case nameof(CABManagementItemViewModel.UserGroup):
                    model.CABManagementItems = model.SortDirection == SortDirectionHelper.Ascending ?
                        model.CABManagementItems.OrderBy(cmi => cmi.UserGroup).ThenByDescending(cmi => cmi.LastUpdated).ToList() :
                        model.CABManagementItems.OrderByDescending(cmi => cmi.UserGroup).ThenByDescending(cmi => cmi.LastUpdated).ToList();
                    break;

                default:
                    model.CABManagementItems = model.CABManagementItems.OrderByDescending(cmi => cmi.LastUpdated).ThenBy(cmi => cmi.CABNumber).ToList();
                    break;
            }

            if (model.Pagination.Total > 10)
            {
                var skip = (model.Pagination.PageNumber - 1) * 10; 
                model.CABManagementItems = model.CABManagementItems.Skip(skip).Take(10).ToList();
            }
        }
    }
}
