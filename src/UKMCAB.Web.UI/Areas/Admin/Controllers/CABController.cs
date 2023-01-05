using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = Constants.Roles.OPSSAdmin)]
    public class CABController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly UserManager<UKMCABUser> _userManager;

        public CABController(ICABAdminService cabAdminService, UserManager<UKMCABUser> userManager)
        {
            _cabAdminService = cabAdminService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("admin/cab/create")]
        public IActionResult Create()
        {
            var model = new CreateCABViewModel();
            LoadLists(model);

            return View(model);
        }

        private void LoadLists(CreateCABViewModel model)
        {
            model.CountryList = Constants.Lists.Countries;
            model.BodyTypeList = Constants.Lists.BodyTypes;
            model.RegulationList = Constants.Lists.LegislativeAreas;
        }

        [HttpPost]
        [Route("admin/cab/create")]
        public async Task<IActionResult> Create(CreateCABViewModel model)
        {
            if (model.Regulations == null || !model.Regulations.Any())
            {
                ModelState.AddModelError("Regulations", "Please select at least one regulation from the list.");
            }
            if (string.IsNullOrWhiteSpace(model.Website) &&
                string.IsNullOrWhiteSpace(model.Email) &&
                string.IsNullOrWhiteSpace(model.Phone)
               )
            {
                ModelState.AddModelError("Website", "At least one of Email, Phone, or Website needs to be completed to create a new CAB.");
            }
            if (ModelState.IsValid)
            {
                var existingCABDocuments = await _cabAdminService.FindCABDocumentsAsync(model.Name);
                if (existingCABDocuments.Any())
                {
                    ModelState.AddModelError("Name", "A CAB with this name already exists.");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);

                    var document = await _cabAdminService.CreateCABDocumentAsync(user.Email, new CABData
                    {
                        Name = model.Name,
                        Address = model.Address,
                        Website = model.Website ?? string.Empty,
                        Email = model.Email ?? string.Empty,
                        Phone = model.Phone ?? string.Empty,
                        Country = model.RegisteredOfficeLocation ?? string.Empty,
                        BodyType = model.BodyTypes != null && model.BodyTypes.Any() ? string.Join(",", model.BodyTypes) : string.Empty,
                        Regulation = model.Regulations
                    });
                    if (document != null)
                    {
                        return Redirect("/");
                    }
                    ModelState.AddModelError("Name", "Failed to save the CAB to the database. Please try again.");
                }
            }
            LoadLists(model);

            return View(model);
        }
    }
}
