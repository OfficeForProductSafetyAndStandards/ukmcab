using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using System.Linq;

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
        public const string AddLegislativeArea = "legislative.area.add-legislativearea";
        public const string AddPurposeOfAppointment = "legislative.area.add-purpose-of-appointment";
        public const string AddCategory = "legislative.area.add-category";
        public const string AddSubCategory = "legislative.area.add-sub-category";     
        public const string AddProduct = "legislative.area.add-product";
        public const string AddProcedure = "legislative.area.add-procedure";
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

    [HttpGet("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, string? returnUrl)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ?? throw new InvalidOperationException();
        var cabLegislativeAreaIds = latestDocument.DocumentLegislativeAreas.Where(n => n.LegislativeAreaId != null).Select(n => n.LegislativeAreaId).ToList();

        var vm = new LegislativeAreaViewModel
        {
            CABId = id,
            LegislativeAreas = await this.GetLegislativeSelectListItemsAsync(cabLegislativeAreaIds),
            ReturnUrl = returnUrl,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    [HttpPost("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, LegislativeAreaViewModel vm, string submitType)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ?? throw new InvalidOperationException();
        var cabLegislativeAreaIds = latestDocument.DocumentLegislativeAreas.Where(n => n.LegislativeAreaId != null).Select(n => n.LegislativeAreaId).ToList();

        if(cabLegislativeAreaIds.Contains(vm.SelectedLegislativeAreaId))
        {
            var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(vm.SelectedLegislativeAreaId);
            ModelState.AddModelError(nameof(vm.SelectedLegislativeAreaId), $"Legislative area '{legislativeArea?.Name}' already exists in Cab.");
        }

        if (ModelState.IsValid)
        {   
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

            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

            return submitType switch
            {
                Constants.SubmitType.Continue => RedirectToRoute(Routes.AddPurposeOfAppointment,
                    new { id, scopeId = scopeOfAppointmentId }),
                // save additional info
                Constants.SubmitType.AdditionalInfo => RedirectToRoute( LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation,
                    new { id, laId = documentLegislativeAreaId }),
                _ => RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true })
            };
        }

        vm.LegislativeAreas = await GetLegislativeSelectListItemsAsync(cabLegislativeAreaIds);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }


    [HttpGet("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, Guid scopeId)
    {
        var documentScopeOfAppointment = await _cabAdminService.GetDocumentScopeOfAppointmentAsync(id, scopeId);
        var options =
            await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(
                documentScopeOfAppointment.LegislativeAreaId);
        var legislativeArea =
            await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentScopeOfAppointment.LegislativeAreaId);
        if (legislativeArea == null)
        {
            throw new InvalidOperationException(
                $"Legislative Area not found for {documentScopeOfAppointment.LegislativeAreaId}");
        }

        if (options.PurposeOfAppointments.Any())
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
        var documentScopeOfAppointment = latestDocument?.ScopeOfAppointments.First(s => s.Id == scopeId) ??
                                         throw new InvalidOperationException();

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
        var categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);

        var selectListItems = categories.ToList();
        if (selectListItems.Any())
        {
            var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
            var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment.PurposeOfAppointmentId) : null;

            var vm = new CategoryViewModel
            {
                CABId = id,
                Categories = selectListItems,
                LegislativeArea = legislativeArea?.Name,
                PurposeOfAppointment = purposeOfAppointment?.Name
            };

            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
        }

        return RedirectToRoute(Routes.AddProduct, new { id, scopeId });
    }

    [HttpPost("add-category/{scopeId}", Name = Routes.AddCategory)]
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId, CategoryViewModel vm, string submitType)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ?? throw new InvalidOperationException();
        var scopeOfAppointment = latestDocument.ScopeOfAppointments.Where(n => n.Id == scopeId).First(s => s.Id == scopeId) ?? throw new InvalidOperationException();

        if (ModelState.IsValid)
        {            
            scopeOfAppointment.CategoryId = vm.SelectedCategoryId;

            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

            return submitType switch
            {
                Constants.SubmitType.Continue => RedirectToRoute(Routes.AddSubCategory, new { id, scopeId }),
                _ => RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true })

            };           
        }

        vm.Categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
    }

    [HttpGet("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId)
    {
        var scopeOfAppointment = await _cabAdminService.GetDocumentScopeOfAppointmentAsync(id, scopeId);
        var subcategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);

        var selectListItems = subcategories.ToList();
        if (selectListItems.Any())
        {
            var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
            var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment.PurposeOfAppointmentId) : null;
            var category = scopeOfAppointment.CategoryId != null ? await _legislativeAreaService.GetCategoryByIdAsync((Guid)scopeOfAppointment.CategoryId) : null;

            var vm = new SubCategoryViewModel
            {
                CABId = id,
                SubCategories = selectListItems,
                LegislativeArea = legislativeArea?.Name,
                PurposeOfAppointment = purposeOfAppointment?.Name,
                Category = category?.Name
            };

            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
        }

        return RedirectToRoute(Routes.AddProduct, new { id, scopeId });
    }

    [HttpPost("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId, SubCategoryViewModel vm, string submitType)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ?? throw new InvalidOperationException();
        var scopeOfAppointment = latestDocument.ScopeOfAppointments.Where(n => n.Id == scopeId).First(s => s.Id == scopeId) ?? throw new InvalidOperationException();

        if (ModelState.IsValid)
        {  
            scopeOfAppointment.SubCategoryId = vm.SelectedSubCategoryId;

            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

            return submitType switch
            {
                Constants.SubmitType.Continue => RedirectToRoute(Routes.AddProduct, new { id, scopeId }),
                _ => RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true })
            };           
        }

        vm.SubCategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
    }   

    [HttpGet("add-product/{scopeId}", Name = Routes.AddProduct)]
    public async Task<IActionResult> AddProduct(Guid id, Guid scopeId)
    {
        var scopeOfAppointment = await _cabAdminService.GetDocumentScopeOfAppointmentAsync(id, scopeId);
        var products = await GetProductSelectListItemsAsync(scopeOfAppointment.CategoryId);

        var selectListItems = products.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProcedure, new { id, scopeId });
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
        var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment.PurposeOfAppointmentId) : null;
        var category = scopeOfAppointment.CategoryId != null ? await _legislativeAreaService.GetCategoryByIdAsync((Guid)scopeOfAppointment.CategoryId) : null;
        var subCategory = scopeOfAppointment.SubCategoryId != null ? await _legislativeAreaService.GetSubCategoryByIdAsync((Guid)scopeOfAppointment.SubCategoryId) : null;

        var vm = new ProductViewModel
        {
            CABId = id,
            Products = selectListItems,
            LegislativeArea = legislativeArea?.Name,
            PurposeOfAppointment = purposeOfAppointment?.Name,
            Category = category?.Name,
            SubCategory = subCategory?.Name
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProduct.cshtml", vm);

    }
    
    
    [HttpGet("selected-legislative-area", Name = Routes.LegislativeAreaSelected)]
    public IActionResult SelectedLegislativeArea()
    {
        var vm = new SelectedLegislativeAreasViewModel
        {
            ReturnUrl = "/",
            SelectedLegislativeAreas = new[]
            {
            new SelectedLegislativeAreaViewModel
            {
                LegislativeAreaName = "Non-automatic weighting instruments",
                LegislativeAreaDetails = new List<LegislativeAreaListItemViewModel>
                {
                    new()
                    {
                        PurposeOfAppointment = "",
                        Category =
                            "MI-005 Measuring systems for the continuous and dynamic measurement of quantities of liquid other than water",
                        SubCategory = "",
                        Product = "Measuring systems on a pipelines (Accuracy Class 0.3)",
                        Procedure = "Module G Conformity based on unit verification"
                    },
                    new()
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
                {
                    new()
                    {
                        PurposeOfAppointment =
                            "Conformity assessment of Pressure Equipment falling within Regulation 6 and classified in accordance with Schedule 3 as either Category I, II, III, or IV equipment",
                        Category = "Category II",
                        SubCategory = "",
                        Product = "Lorem ipsum dolor siture",
                        Procedure =
                            "Part 2 Module A2 Internal production control plus supervised pressure equipment checks at random"
                    },
                    new()
                    {
                        PurposeOfAppointment = "Not applicable",
                        Category = "Lorem ipsum dolor siture",
                        SubCategory = "",
                        Product = "Not applicable",
                        Procedure =
                            "Part 2 Module A2 Internal production control plus supervised pressure equipment checks at random"
                    }
                }
            }
        }
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/SelectedLegislativeArea.cshtml", vm);
    }
   

    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync(List<Guid?> selectedLegislativeAreaIds)
    {
        var legislativeAreas = await _legislativeAreaService.GetAvailableCabLegislativeAreasAsync(selectedLegislativeAreaIds);
        return legislativeAreas.Select(x => new SelectListItem(){ Text = x.Name, Value = x.Id.ToString() });
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListItemsAsync(Guid? purposeOfAppointmentId,
        Guid legislativeAreaId)
    {
        ScopeOfAppointmentOptionsModel? scopeOfAppointmentOptionsModel = null;

        if (purposeOfAppointmentId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(
                    (Guid)purposeOfAppointmentId);
        }

        if (scopeOfAppointmentOptionsModel != null && scopeOfAppointmentOptionsModel.Categories.Any())
        {
            return scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
        }

        scopeOfAppointmentOptionsModel =
            await _legislativeAreaService
                .GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId);

        if (scopeOfAppointmentOptionsModel.Categories.Any())
        {
            return scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
        }

        return new List<SelectListItem>();
    }

    private async Task<IEnumerable<SelectListItem>> GetSubCategoriesSelectListItemsAsync(Guid? categoryId)
    {
        if (categoryId != null)
        {
            var scopeOfAppointmentOptionsModel = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync((Guid)categoryId);
            if (scopeOfAppointmentOptionsModel.Subcategories.Any())
            {
               return scopeOfAppointmentOptionsModel.Subcategories.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }  
    
    private async Task<IEnumerable<SelectListItem>> GetProductSelectListItemsAsync(Guid? categoryId)
    {
        if (categoryId != null)
        {
            var scopeOfAppointmentOptionsModel = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync((Guid)categoryId);
            if (scopeOfAppointmentOptionsModel.Products.Any())
            {
                return scopeOfAppointmentOptionsModel.Products.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }  
 }