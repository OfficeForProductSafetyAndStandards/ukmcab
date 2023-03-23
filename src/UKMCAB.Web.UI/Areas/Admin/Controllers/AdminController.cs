using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{

    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin}")]
    public class AdminController : Controller
    {
        [Route("/admin")]
        [Route("/admin/work-queue")]
        public IActionResult Index(WorkQueueViewModel model)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = "name";
            }
            model.WorkQueueItems = new List<WorkQueueItemViewModel>
            {
                new WorkQueueItemViewModel
                {
                    Id = "1",
                    Name = "Compatible Electronics, Inc",
                    CABNumber = "1234",
                    Status = "Draft"
                },
                new WorkQueueItemViewModel
                {
                    Id = "2",
                    Name = "Element Materials Technology Portland - Evergreen Inc",
                    CABNumber = "5678",
                    Status = "Archived"
                },
                new WorkQueueItemViewModel
                {
                    Id = "3",
                    Name = "Eurofins Hursley Limited",
                    CABNumber = "4321",
                    Status = "Draft"
                },
                new WorkQueueItemViewModel
                {
                    Id = "4",
                    Name = "Vista Laboratories, Inc",
                    CABNumber = "8765",
                    Status = "Archived"
                },
                new WorkQueueItemViewModel
                {
                    Id = "5",
                    Name = "3c Test Ltd",
                    CABNumber = "",
                    Status = "Archived"
                }
            };
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
                default:
                    model.WorkQueueItems = model.WorkQueueItems.OrderBy(wqi => wqi.Name).ToList();
                    break;
            }
        }
    }
}
