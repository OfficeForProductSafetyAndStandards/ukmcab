using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin}")]
    public class CABController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly UserManager<UKMCABUser> _userManager;

        public CABController(ICABAdminService cabAdminService, UserManager<UKMCABUser> userManager
            )
        {
            _cabAdminService = cabAdminService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("admin/cab/details/{id?}")]
        public async Task<IActionResult> Details(string id)
        {
            return View(new CABDetailsViewModel());
        }

        [HttpPost]
        [Route("admin/cab/details/{id?}")]
        public async Task<IActionResult> Details(string id, CABDetailsViewModel model, string submitType)
        {
            var appointmentDate = CheckDate(model.AppointmentDate, nameof(model.AppointmentDateDay), "appointment");
            var renewalDate = CheckDate(model.RenewalDate, nameof(model.RenewalDateDay), "renewal");
            if (ModelState.IsValid)
            {
                var document = new Document
                {
                    Name = model.Name,
                    CABNumber = model.CABNumber,
                    AppointmentDate = appointmentDate,
                    RenewalDate = renewalDate,
                    UKASReference = model.UKASReference
                };
                if (!await _cabAdminService.DocumentWithKeyIdentifiersExists(document))
                {
                    var user = await _userManager.GetUserAsync(User);
                    var createdDocument = await _cabAdminService.CreateDocumentAsync(user.Email, document);
                    if (createdDocument == null)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the document, please try again.");
                    }
                    else if (submitType == "continue")
                    {
                        return RedirectToAction("Contact", "CAB", new { Area = "admin", id= createdDocument.CABId });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin", new { Area = "admin" });
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Name), "A document already exists for this CAB name or number");
                }
            }

            return View(new CABDetailsViewModel());
        }

        private DateTime? CheckDate(string date, string modelKey, string errorMessagePart)
        {
            if (DateTime.TryParse(date, out DateTime dateTime))
            {
                return dateTime;
            }
            if(!date.Equals("//"))
            {
                ModelState.AddModelError(modelKey, $"The {errorMessagePart} date in not valid");
            }
            return null;
        }

        [HttpGet]
        [Route("admin/cab/contact/{id?}")]
        public async Task<IActionResult> Contact(string id)
        {
            return View(new CABContactViewModel());
        }




        [HttpGet]
        [Route("admin/cab/create")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateCABViewModel();
            await UpdateModel(model);

            return View(model);
        }

        private async Task UpdateModel(CreateCABViewModel model)
        {
            model.CountryList = Constants.Lists.Countries;
            model.BodyTypeList = Constants.Lists.BodyTypes;
            model.RegulationList = Constants.Lists.LegislativeAreas;
            var user = await _userManager.GetUserAsync(User);
            model.IsUKASUser = await _userManager.IsInRoleAsync(user, Constants.Roles.UKASUser);
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
                ModelState.AddModelError("Website",
                    "At least one of Email, Phone, or Website needs to be completed to create a new CAB.");
            }

            if (model.IsUKASUser && string.IsNullOrWhiteSpace(model.UKASReference))
            {
                ModelState.AddModelError("UKASReference", "A valid UKAS reference is required.");
            }
            else if (model.IsUKASUser)
            {
                var existingUKASReferenceDocuments =
                    await _cabAdminService.FindCABDocumentsByUKASReferenceAsync(model.UKASReference);
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

                    var document = await _cabAdminService.CreateDocumentAsync(user.Email, new Document()
                    {
                        Name = model.Name,
                        UKASReference = model.UKASReference,
                        Address = model.Address,
                        Website = model.Website ?? string.Empty,
                        Email = model.Email ?? string.Empty,
                        Phone = model.Phone ?? string.Empty,
                        Country = model.RegisteredOfficeLocation ?? string.Empty,
                        BodyTypes = model.BodyTypes != null && model.BodyTypes.Any()
                            ? model.BodyTypes
                            : new List<string>(),
                        LegislativeAreas = model.Regulations
                    });
                    if (document != null)
                    {
                        return RedirectToAction("SchedulesUpload", "FileUpload", new { id = document.CABData.CABId });
                    }

                    ModelState.AddModelError("Name", "Failed to save the CAB to the database. Please try again.");
                }
            }

            return View(model);
        }

    }
}
