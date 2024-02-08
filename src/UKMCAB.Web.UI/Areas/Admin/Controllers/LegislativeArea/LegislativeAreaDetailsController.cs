using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IEditLockService _editLockService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;
    
    public static class Routes
    {
        public const string LegislativeAreaDetails = "legislative.area.details";
        public const string AddLegislativeArea = "legislative.area.add-legislativearea";
        public const string AddPurposeOfAppointment = "legislative.area.add-purpose-of-appointment";
        public const string AddCategory = "legislative.area.add-category";
        public const string AddSubCategory = "legislative.area.add-sub-category";        
        
        public const string AddProduct = "legislative.area.add-product";        
        public const string LegislativeAreaSelected = "legislative.area.selected";
    }

    public LegislativeAreaDetailsController(
        ICABAdminService cabAdminService,
        IEditLockService editLockService,
        ILegislativeAreaService legislativeAreaService, 
        IUserService userService)
    {
        _cabAdminService = cabAdminService;
        _editLockService = editLockService;
        _legislativeAreaService = legislativeAreaService;
        _userService = userService;
    }

    [HttpGet("details/{laId}", Name = Routes.LegislativeAreaDetails)]
    public async Task<IActionResult> Details(Guid id, Guid laId)
    {
        var vm = new LegislativeAreaDetailViewModel(Title: "Legislative area details");

        return View("~/Areas/Admin/views/CAB/LegislativeArea/Details.cshtml", vm);
    }

    [HttpGet("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, string? returnUrl)
    {
        var legislativeareas = await _legislativeAreaService.GetAllLegislativeAreasAsync();

        var vm = new LegislativeAreaViewModel
        {
            CABId = id,
            LegislativeAreas = await this.GetLegislativeSelectListItemsAsync(),
            ReturnUrl = returnUrl,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    [HttpPost("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, LegislativeAreaViewModel vm, string submitType)
    {
        if (ModelState.IsValid)
        {   
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

            // Implies no document or archived
            if (latestDocument == null)
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            // add  document new legislative area;
            var documentLegislativeAreaId = Guid.NewGuid();

            var documentLegislativeArea = new DocumentLegislativeArea
            {
                Id = documentLegislativeAreaId,
                LegislativeAreaId = vm.SelectedLegislativeAreaId
            };

            latestDocument.DocumentLegislativeAreas.Add(documentLegislativeArea);

            // add new document scope of appointment;
            var scopeOfAppointmentId = Guid.NewGuid();

            var scopeOfAppointment = new DocumentScopeOfAppointment
            {
                Id = scopeOfAppointmentId,
                LegislativeAreaId = vm.SelectedLegislativeAreaId
            };

            latestDocument.ScopeOfAppointments.Add(scopeOfAppointment);

            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);

            return submitType switch
            {
                Constants.SubmitType.Continue => RedirectToRoute(Routes.AddPurposeOfAppointment,
                    new { id, scopeId = scopeOfAppointmentId }),
                // save additional info
                Constants.SubmitType.AdditionalInfo => RedirectToRoute(Routes.LegislativeAreaDetails,
                    new { id, laId = documentLegislativeAreaId }),
                _ => RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true })
            };
        }

        vm.LegislativeAreas = await this.GetLegislativeSelectListItemsAsync();
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }


    [HttpGet("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, Guid scopeId)
    {
        //todo get scope of appointment
        var laId = Guid.Parse("e840c3d0-6153-4baa-82ab-0374b81d46fe");
        var options = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(laId);
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(laId);
        if (legislativeArea == null)
        {
            throw new InvalidOperationException($"Legislative Area not found for {laId}");
        }

        if (!options.PurposeOfAppointments.Any())
        {
            return RedirectToRoute(Routes.AddCategory, new { id, scopeId });
        }

        var selectListItems = options.PurposeOfAppointments
            .Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();
        var vm = new PurposeOfAppointmentViewModel
        {
            Title = "Select purpose of appointment",
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointments = selectListItems,
            CabId = id
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }

    [HttpPost("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, PurposeOfAppointmentViewModel vm, Guid scopeId)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        // Implies no document or archived
        if (latestDocument == null)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }
        var documentScopeOfAppointment = latestDocument.ScopeOfAppointments.First(s => s.Id == scopeId);

        if (ModelState.IsValid)
        {
            documentScopeOfAppointment.PurposeOfAppointmentId = vm.SelectedPurposeOfAppointmentId;
            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

            return RedirectToRoute(Routes.AddCategory, new { id, scopeId });
        }

        var options =
            await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(
                documentScopeOfAppointment.LegislativeAreaId);
        vm.PurposeOfAppointments = options.PurposeOfAppointments
            .Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }

    [HttpGet("add-category/{scopeId}", Name = Routes.AddCategory)]
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId)
    {
        var scopeOfAppointment = await _cabAdminService.GetDocumentScopeOfAppointmentAsync(id, scopeId);        
        var categories = await this.GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
        var purposeOfAppointment = await this._legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment.PurposeOfAppointmentId);

        if (categories != null && categories.Any())
        {
            var vm = new CategoryViewModel
            {
                CABId = id,
                Categories = categories,
                LegislativeArea = legislativeArea.Name,
                PurposeOfAppointment = purposeOfAppointment.Name
            };

            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
        }
        else
        {
            return RedirectToRoute(Routes.AddProduct, new { id, scopeId });
        }
            
    }

    [HttpPost("add-category/{scopeId}", Name = Routes.AddCategory)]
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId, CategoryViewModel vm, string submitType)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var scopeOfAppointment = latestDocument.ScopeOfAppointments.Where(n => n.Id == scopeId).First(s => s.Id == scopeId) ?? throw new InvalidOperationException();

        if (ModelState.IsValid)
        {
            // Implies no document or archived
            if (latestDocument == null)
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            else
            {
                scopeOfAppointment.CategoryId = vm.SelectedCategoryId;

                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);

                return submitType switch
                {
                    Constants.SubmitType.Continue => RedirectToRoute(Routes.AddSubCategory, new { id, scopeId }),
                    _ => RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true })

                };
            }
        }
        else
        {   
            vm.Categories = await this.GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
        }
    }

    [HttpGet("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId)
    {
        var scopeOfAppointment = await _cabAdminService.GetDocumentScopeOfAppointmentAsync(id, scopeId);
       
        var subcategories = await this.GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);

        if (subcategories != null && subcategories.Any())
        {
            var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
            var purposeOfAppointment = await this._legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment.PurposeOfAppointmentId);
            var category = await this._legislativeAreaService.GetCategoryByIdAsync((Guid)scopeOfAppointment.CategoryId);

            var vm = new SubCategoryViewModel
            {
                CABId = id,
                SubCategories = subcategories,
                LegislativeArea = legislativeArea.Name,
                PurposeOfAppointment = purposeOfAppointment.Name,
                Category = category.Name
            };

            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
        }
        else
        {
            return RedirectToRoute(Routes.AddProduct, new { id, scopeId });
        }       
    }

    [HttpPost("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId, SubCategoryViewModel vm, string submitType)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var scopeOfAppointment = latestDocument.ScopeOfAppointments.Where(n => n.Id == scopeId).First(s => s.Id == scopeId) ?? throw new InvalidOperationException();

        if (ModelState.IsValid)
        {
            // Implies no document or archived
            if (latestDocument == null)
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            else
            {  
                scopeOfAppointment.SubCategoryId = vm.SelectedSubCategoryId;

                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);


                return submitType switch
                {
                    Constants.SubmitType.Continue => RedirectToRoute(Routes.AddProduct, new { id, scopeId }),
                    _ => RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true })

                };
            }
        }
        else
        {  
            vm.SubCategories = await this.GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId, scopeOfAppointment.LegislativeAreaId);
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
        }
    }

    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync()
    {
        var legislativeAreas = await _legislativeAreaService.GetAllLegislativeAreasAsync();
        return legislativeAreas.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListItemsAsync(Guid? purposeOfAppointmentId, Guid legislativeAreaId)
    {
        IEnumerable<SelectListItem>? list = null;
        ScopeOfAppointmentOptionsModel? scopeOfAppointmentOptionsModel = null;

        if (purposeOfAppointmentId != null)
        {
            scopeOfAppointmentOptionsModel = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync((Guid)purposeOfAppointmentId);
        }

        if (scopeOfAppointmentOptionsModel != null && scopeOfAppointmentOptionsModel.Categories.Any())
        {
            list =  scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
        }
        else
        {
            scopeOfAppointmentOptionsModel = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId);

            if(scopeOfAppointmentOptionsModel != null && scopeOfAppointmentOptionsModel.Categories.Any())
            {
                list = scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return list;
    }

    private async Task<IEnumerable<SelectListItem>> GetSubCategoriesSelectListItemsAsync(Guid? categoryId, Guid legislativeAreaId)
    {
        IEnumerable<SelectListItem>? list = null;
       
        if (categoryId != null)
        {
            var scopeOfAppointmentOptionsModel = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync((Guid)categoryId);
            if (scopeOfAppointmentOptionsModel != null && scopeOfAppointmentOptionsModel.Subcategories.Any())
            {
                list = scopeOfAppointmentOptionsModel.Subcategories.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }        

        return list;
    }
    
    [HttpGet("selected-legislative-area", Name = Routes.LegislativeAreaSelected)]
    public async Task<IActionResult> SelectedLegislativeArea()
    {
        var vm = new SelectedLegislativeAreasViewModel() 
        { 
            ReturnUrl = "/",
            SelectedLegislativeAreas = new[]
            {
                new SelectedLegislativeAreaViewModel
                {
                    LegislativeAreaName = "Non-automatic weighting instruments",
                    LegislativeAreaDetails = new List<LegislativeAreaListItemViewModel> 
                    { new LegislativeAreaListItemViewModel 
                        { 
                            PurposeOfAppointment = "",
                            Category = "MI-005 Measuring systems for the continuous and dynamic measurement of quantities of liquid other than water",
                            SubCategory = "",
                            Product = "Measuring systems on a pipelines (Accuracy Class 0.3)",
                            Procedure = "Module G Conformity based on unit verification"
                        },
                        new LegislativeAreaListItemViewModel
                        {
                            PurposeOfAppointment = "",
                            Category = "MI-006 Automatic weighing machines",
                            SubCategory = "Automatic catch weigher",
                            Product = "Automatic catch weigher",
                            Procedure = "Module D1 Quality assurance of the production process"
                        }
                    }
                },
                new SelectedLegislativeAreaViewModel
                {
                    LegislativeAreaName = "Pressure equipment",
                    LegislativeAreaDetails = new List<LegislativeAreaListItemViewModel>
                    { new LegislativeAreaListItemViewModel
                        {
                            PurposeOfAppointment = "Conformity assessment of Pressure Equipment falling within Regulation 6 and classified in accordance with Schedule 3 as either Category I, II, III, or IV equipment",
                            Category = "Category II",
                            SubCategory = "",
                            Product = "Lorem ipsum dolor siture",
                            Procedure = "Part 2 � Module A2 Internal production control plus supervised pressure equipment checks at random"
                        },
                        new LegislativeAreaListItemViewModel
                        {
                            PurposeOfAppointment = "Not applicable",
                            Category = "Lorem ipsum dolor siture",
                            SubCategory = "",
                            Product = "Not applicable",
                            Procedure = "Part 2 � Module A2 Internal production control plus supervised pressure equipment checks at random"
                        }
                    }
                }

            }
        };
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/SelectedLegislativeArea.cshtml", vm);
    }
}