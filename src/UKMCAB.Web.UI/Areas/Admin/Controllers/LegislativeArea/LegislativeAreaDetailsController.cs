using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Data.Models.LegislativeAreas;
using System.Security.Claims;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IDistCache _distCache;
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
    }

    public LegislativeAreaDetailsController(
        ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService,
        IDistCache distCache)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _userService = userService;
        _distCache = distCache;
    }

    [HttpGet("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, string? returnUrl)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                             throw new InvalidOperationException();
        var cabLegislativeAreaIds = latestDocument.DocumentLegislativeAreas.Select(n => n.LegislativeAreaId).ToList();

        var vm = new LegislativeAreaViewModel
        {
            CABId = id,
            LegislativeAreas = await GetLegislativeSelectListItemsAsync(cabLegislativeAreaIds),
            ReturnUrl = returnUrl,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    [HttpPost("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, LegislativeAreaViewModel vm, string submitType)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                             throw new InvalidOperationException();
        var cabLegislativeAreaIds = latestDocument.DocumentLegislativeAreas.Select(n => n.LegislativeAreaId).ToList();
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(vm.SelectedLegislativeAreaId);

        if (cabLegislativeAreaIds.Contains(vm.SelectedLegislativeAreaId))
        {
            ModelState.AddModelError(nameof(vm.SelectedLegislativeAreaId),
                $"Legislative area '{legislativeArea.Name}' already exists in Cab.");
        }

        if (ModelState.IsValid)
        {
            // add document new legislative area;
            var documentLegislativeAreaId =
                await _cabAdminService.AddLegislativeAreaAsync(id, vm.SelectedLegislativeAreaId, legislativeArea.Name);

            // add new document scope of appointment to cache;
            var scopeOfAppointmentId = Guid.NewGuid();
            var scopeOfAppointment = new DocumentScopeOfAppointment
            {
                Id = scopeOfAppointmentId,
                LegislativeAreaId = vm.SelectedLegislativeAreaId
            };
            await _distCache.SetAsync(scopeOfAppointmentId.ToString(), scopeOfAppointment, TimeSpan.FromHours(1));

            return submitType switch
            {
                Constants.SubmitType.Continue => RedirectToRoute(Routes.AddPurposeOfAppointment,
                    new { id, scopeId = scopeOfAppointmentId }),
                // save additional info
                Constants.SubmitType.AdditionalInfo => RedirectToRoute(
                    LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation,
                    new { id, laId = vm.SelectedLegislativeAreaId }),
                _ => RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true })
            };
        }

        vm.LegislativeAreas = await GetLegislativeSelectListItemsAsync(cabLegislativeAreaIds);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    [HttpGet("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, Guid scopeId)
    {
        //todo handle existing cabs which need scope of appointment
        var documentScopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (documentScopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
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
        var documentScopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (documentScopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        if (ModelState.IsValid)
        {
            documentScopeOfAppointment.PurposeOfAppointmentId = vm.SelectedPurposeOfAppointmentId;
            await _distCache.SetAsync(documentScopeOfAppointment.Id.ToString(), documentScopeOfAppointment,
                TimeSpan.FromHours(1));
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
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        var categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId,
            scopeOfAppointment.LegislativeAreaId);

        var selectListItems = categories.ToList();
        if (selectListItems.Any())
        {
            var legislativeArea =
                await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
            var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null
                ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(
                    (Guid)scopeOfAppointment.PurposeOfAppointmentId)
                : null;

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
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId, CategoryViewModel vm)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        if (ModelState.IsValid)
        {
            scopeOfAppointment.CategoryId = vm.SelectedCategoryId;
            await _distCache.SetAsync(scopeOfAppointment.Id.ToString(), scopeOfAppointment, TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddSubCategory, new { id, scopeId });
        }

        vm.Categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId,
            scopeOfAppointment.LegislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
    }

    [HttpGet("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        var subcategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);

        var selectListItems = subcategories.ToList();
        if (selectListItems.Any())
        {
            var legislativeArea =
                await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
            var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null
                ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(
                    (Guid)scopeOfAppointment.PurposeOfAppointmentId)
                : null;
            var category = scopeOfAppointment.CategoryId != null
                ? await _legislativeAreaService.GetCategoryByIdAsync((Guid)scopeOfAppointment.CategoryId)
                : null;

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
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId, SubCategoryViewModel vm)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        if (ModelState.IsValid)
        {
            scopeOfAppointment.SubCategoryId = vm.SelectedSubCategoryId;
            await _distCache.SetAsync(scopeOfAppointment.Id.ToString(), scopeOfAppointment, TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddProduct, new { id, scopeId });
        }

        vm.SubCategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
    }

    [HttpGet("add-product/{scopeId}", Name = Routes.AddProduct)]
    public async Task<IActionResult> AddProduct(Guid id, Guid scopeId)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        var products = await GetProductSelectListItemsAsync(scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);

        var selectListItems = products.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProcedure, new { id, scopeId });
        var legislativeArea =
            await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
        var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null
            ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment
                .PurposeOfAppointmentId)
            : null;
        var category = scopeOfAppointment.CategoryId != null
            ? await _legislativeAreaService.GetCategoryByIdAsync((Guid)scopeOfAppointment.CategoryId)
            : null;
        var subCategory = scopeOfAppointment.SubCategoryId != null
            ? await _legislativeAreaService.GetSubCategoryByIdAsync((Guid)scopeOfAppointment.SubCategoryId)
            : null;

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

    [HttpPost("add-product/{scopeId}", Name = Routes.AddProduct)]
    public async Task<IActionResult> AddProduct(Guid id, Guid scopeId, ProductViewModel vm)
    {

        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        if (ModelState.IsValid)
        {
            scopeOfAppointment.ProductIds = vm.SelectedProductIds!;
            await _distCache.SetAsync(scopeOfAppointment.Id.ToString(), scopeOfAppointment, TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddProcedure, new { id, scopeId });
        }

        vm.Products = await GetProductSelectListItemsAsync(scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProduct.cshtml", vm);

    }

    [HttpGet("add-procedure/{scopeId}", Name = Routes.AddProcedure)]
    public async Task<IActionResult> AddProcedure(Guid id, Guid scopeId, int indexOfProduct = 0)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        Guid? productId = null;

        //var selectListItems = new List<SelectListItem>();

        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);

        if (scopeOfAppointment.ProductIds.Any() && indexOfProduct < scopeOfAppointment.ProductIds.Count)
        {
            productId = scopeOfAppointment.ProductIds[indexOfProduct];
        }

        var procedures = await GetProcedureSelectListItemsAsync(productId, scopeOfAppointment.CategoryId, scopeOfAppointment.PurposeOfAppointmentId);
        var selectListItems = procedures.ToList();

        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
        var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment.PurposeOfAppointmentId) : null;
        var category = scopeOfAppointment.CategoryId != null ? await _legislativeAreaService.GetCategoryByIdAsync((Guid)scopeOfAppointment.CategoryId) : null;
        var subCategory = scopeOfAppointment.SubCategoryId != null ? await _legislativeAreaService.GetSubCategoryByIdAsync((Guid)scopeOfAppointment.SubCategoryId) : null;
        string? productName = null;
        if (productId != null)
        {
            var product = await _legislativeAreaService.GetProductByIdAsync((Guid)productId);
            productName = product.Name;
        }

        var vm = new ProcedureViewModel
        {
            CABId = id,
            Product = productName,
            CurrentProductId = productId,
            Procedures = selectListItems,
            LegislativeArea = legislativeArea?.Name,
            PurposeOfAppointment = purposeOfAppointment?.Name,
            Category = category?.Name,
            SubCategory = subCategory?.Name,
            ShowContinueToNextStep = indexOfProduct >= scopeOfAppointment.ProductIds.Count - 1
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProcedure.cshtml", vm);
    }

    [HttpPost("add-procedure/{scopeId}", Name = Routes.AddProcedure)]
    public async Task<IActionResult> AddProcedure(Guid id, Guid scopeId, int indexOfProduct, ProcedureViewModel vm)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(scopeId.ToString());
        Guid? productId = null;

        if (scopeOfAppointment == null)
            return RedirectToRoute(Routes.AddLegislativeArea);
        

        if (ModelState.IsValid)
        {
            var productAndProcedures = new ProductAndProcedures
            {
                ProductId = vm.CurrentProductId,
                ProcedureIds = (List<Guid>)vm.SelectedProcedureIds!
            };

            scopeOfAppointment.ProductIdAndProcedureIds.Add(productAndProcedures);
            await _distCache.SetAsync(scopeOfAppointment.Id.ToString(), scopeOfAppointment, TimeSpan.FromHours(1));
            if (indexOfProduct + 1 < scopeOfAppointment.ProductIds.Count)
            {
                return RedirectToRoute(Routes.AddProcedure, new { id, scopeId, indexOfProduct = indexOfProduct + 1 });
            }
            else
            {
                var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
                latestDocument.ScopeOfAppointments.Add(scopeOfAppointment);

                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
                // AddLegislativeAreaAsync will throw error if the legislative area exists. Need to added directly.
                var la = latestDocument.DocumentLegislativeAreas.First(dLa => dLa.LegislativeAreaId == scopeOfAppointment.LegislativeAreaId);
                if (la == null)
                {
                    await _cabAdminService.AddLegislativeAreaAsync(id, scopeOfAppointment.LegislativeAreaId, legislativeArea.Name);
                }                
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

                return RedirectToRoute(LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation, new { id, laId = scopeOfAppointment.LegislativeAreaId });
            }
            
        }

        if (scopeOfAppointment.ProductIds.Any() && indexOfProduct < scopeOfAppointment.ProductIds.Count)
        {
            productId = scopeOfAppointment.ProductIds[indexOfProduct];
        }

        vm.Procedures = await GetProcedureSelectListItemsAsync(productId, scopeOfAppointment.CategoryId, scopeOfAppointment.PurposeOfAppointmentId);

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProcedure.cshtml", vm);
    }

    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync(
        List<Guid> excludeLegislativeAreaIds)
    {
        var legislativeAreas = await _legislativeAreaService.GetLegislativeAreasAsync(excludeLegislativeAreaIds);
        return legislativeAreas.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
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

            if (scopeOfAppointmentOptionsModel.Categories.Any())
            {
                return scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        scopeOfAppointmentOptionsModel =
            await _legislativeAreaService
                .GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId);

        return scopeOfAppointmentOptionsModel.Categories.Any()
            ? scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem
            { Text = x.Name, Value = x.Id.ToString() })
            : new List<SelectListItem>();
    }

    private async Task<IEnumerable<SelectListItem>> GetSubCategoriesSelectListItemsAsync(Guid? categoryId)
    {
        if (categoryId != null)
        {
            var scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync((Guid)categoryId);
            if (scopeOfAppointmentOptionsModel.Subcategories.Any())
            {
                return scopeOfAppointmentOptionsModel.Subcategories.Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }

    private async Task<IEnumerable<SelectListItem>> GetProductSelectListItemsAsync(Guid? categoryId,
        Guid? purposeOfAppointmentId, Guid? legislativeAreaId)
    {
        ScopeOfAppointmentOptionsModel? scopeOfAppointmentOptionsModel;
        if (categoryId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(categoryId.Value);
            if (scopeOfAppointmentOptionsModel.Products.Any())
            {
                return scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (purposeOfAppointmentId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(
                    purposeOfAppointmentId.Value);
            if (scopeOfAppointmentOptionsModel.Products.Any())
            {
                return scopeOfAppointmentOptionsModel.Categories.Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (legislativeAreaId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService
                    .GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId.Value);
            if (scopeOfAppointmentOptionsModel.Products.Any())
            {
                return scopeOfAppointmentOptionsModel.Products.Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }

    private async Task<IEnumerable<SelectListItem>> GetProcedureSelectListItemsAsync(Guid? productId, Guid? categoryId, Guid? purposeOfAppointmentId)
    {
        ScopeOfAppointmentOptionsModel? scopeOfAppointmentOptionsModel;

        if (productId != null)
        {
            scopeOfAppointmentOptionsModel =
            await _legislativeAreaService
                    .GetNextScopeOfAppointmentOptionsForProductAsync(productId.Value);
            if (scopeOfAppointmentOptionsModel.Procedures.Any())
            {
                return scopeOfAppointmentOptionsModel.Procedures.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (categoryId != null)
        {
            scopeOfAppointmentOptionsModel =
            await _legislativeAreaService
                    .GetNextScopeOfAppointmentOptionsForCategoryAsync(categoryId.Value);
            if (scopeOfAppointmentOptionsModel.Procedures.Any())
            {
                return scopeOfAppointmentOptionsModel.Procedures.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (purposeOfAppointmentId != null)
        {
            scopeOfAppointmentOptionsModel =
            await _legislativeAreaService
                    .GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(purposeOfAppointmentId.Value);
            if (scopeOfAppointmentOptionsModel.Procedures.Any())
            {
                return scopeOfAppointmentOptionsModel.Procedures.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }

}