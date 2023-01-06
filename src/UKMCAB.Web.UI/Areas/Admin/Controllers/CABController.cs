using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin},{Constants.Roles.UKASUser}")]
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
            UpdateModel(model);

            return View(model);
        }

        private void UpdateModel(CreateCABViewModel model)
        {
            model.CountryList = Constants.Lists.Countries;
            model.BodyTypeList = Constants.Lists.BodyTypes;
            model.RegulationList = Constants.Lists.LegislativeAreas;
            model.IsUKASUser = User.IsInRole(Constants.Roles.UKASUser);
        }

        [HttpPost]
        [Route("admin/cab/create")]
        public async Task<IActionResult> Create(State SaveButton, CreateCABViewModel model)
        {
            UpdateModel(model);

            if (model.Regulations == null || !model.Regulations.Any())
            {
                ModelState.AddModelError("Regulations", "Please select at least one regulation from the list.");
            }
            
            if (string.IsNullOrWhiteSpace(model.Website) &&
                string.IsNullOrWhiteSpace(model.Email) &&
                string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("Website", "At least one of Email, Phone, or Website needs to be completed to create a new CAB.");
            }

            if (model.IsUKASUser && string.IsNullOrWhiteSpace(model.UKASReference))
            {
                ModelState.AddModelError("UKASReference", "A valid UKAS reference is required.");
            }
            else if (model.IsUKASUser)
            {
                var existingUKASReferenceDocuments = await _cabAdminService.FindCABDocumentsByUKASReferenceAsync(model.UKASReference);
                if (existingUKASReferenceDocuments.Any())
                {
                    ModelState.AddModelError("UKASReference", "A CAB with this UKAS reference already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                var existingCABDocuments = await _cabAdminService.FindCABDocumentsByNameAsync(model.Name);
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
                            UKASReference = model.UKASReference,
                            Address = model.Address,
                            Website = model.Website ?? string.Empty,
                            Email = model.Email ?? string.Empty,
                            Phone = model.Phone ?? string.Empty,
                            Country = model.RegisteredOfficeLocation ?? string.Empty,
                            BodyType = model.BodyTypes != null && model.BodyTypes.Any() ? string.Join(",", model.BodyTypes) : string.Empty,
                            Regulation = model.Regulations
                        },
                        SaveButton);
                    if (document != null)
                    {
                        return Redirect("/");
                    }
                    ModelState.AddModelError("Name", "Failed to save the CAB to the database. Please try again.");
                }
            }

            return View(model);
        }
    }
}
