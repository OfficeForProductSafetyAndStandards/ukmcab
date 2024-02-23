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
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IDistCache _distCache;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;
    private const string CacheKey = "soa_create_{0}";
    
    public static class Routes
    {
        public const string AddLegislativeArea = "legislative.area.add-legislativearea";
        public const string AddPurposeOfAppointment = "legislative.area.add-purpose-of-appointment";
        public const string AddCategory = "legislative.area.add-category";
        public const string AddSubCategory = "legislative.area.add-sub-category";
        public const string AddProduct = "legislative.area.add-product";
        public const string AddProcedure = "legislative.area.add-procedure";
        public const string RemoveLegislativeArea = "legislative.area.remove-legislativearea";
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
            await _cabAdminService.AddLegislativeAreaAsync(id, vm.SelectedLegislativeAreaId, legislativeArea.Name);

            // add new document scope of appointment to cache;
            var scopeOfAppointmentId = Guid.NewGuid();
            await CreateScopeOfAppointmentInCacheAsync(scopeOfAppointmentId, vm.SelectedLegislativeAreaId);

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

    private async Task CreateScopeOfAppointmentInCacheAsync(Guid scopeId, Guid legislativeAreaId)
    {
        var scopeOfAppointment = new DocumentScopeOfAppointment
        {
            Id = scopeId,
            LegislativeAreaId = legislativeAreaId
        };
        await _distCache.SetAsync(string.Format(CacheKey,scopeId.ToString()), scopeOfAppointment, TimeSpan.FromHours(1));
    }

    [HttpGet("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, Guid scopeId, Guid? compareScopeId, Guid? legislativeAreaId)
    {
        if (scopeId == Guid.Empty && legislativeAreaId != null)
        {
            scopeId = Guid.NewGuid();
            await CreateScopeOfAppointmentInCacheAsync(scopeId, (Guid)legislativeAreaId);
            return RedirectToRoute(Routes.AddPurposeOfAppointment, new { id, scopeId, legislativeAreaId });
        }
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        if (existingScopeOfAppointment != null)
        {
            //Create a new scope of id in session to replace existing
            await CreateScopeOfAppointmentInCacheAsync(scopeId, existingScopeOfAppointment.LegislativeAreaId);
        }

        var documentScopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (documentScopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
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
            return RedirectToRoute(Routes.AddCategory, new { id, scopeId, compareScopeId });
        }

        var selectListItems = options.PurposeOfAppointments
            .Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();


        if (existingScopeOfAppointment is { PurposeOfAppointmentId: not null })
        {
            var itemFound = selectListItems.FirstOrDefault(i =>
                i.Value == existingScopeOfAppointment.PurposeOfAppointmentId.Value.ToString());
            if (itemFound != null) itemFound.Selected = true;
        }

        var vm = new PurposeOfAppointmentViewModel
        {
            Title = "Select purpose of appointment",
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointments = selectListItems,
            CabId = id,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }

    private async Task<DocumentScopeOfAppointment?> GetCompareScopeOfAppointment(Guid id, Guid? compareScopeId)
    {
        var cab = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ?? throw new InvalidOperationException();
        var existingScopeOfAppointment = cab.ScopeOfAppointments.FirstOrDefault(s => s.Id == compareScopeId);
        return existingScopeOfAppointment;
    }

    [HttpPost("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, PurposeOfAppointmentViewModel vm, Guid scopeId,
        Guid? compareScopeId)
    {
        var documentScopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (documentScopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
        if (ModelState.IsValid)
        {
            documentScopeOfAppointment.PurposeOfAppointmentId = vm.SelectedPurposeOfAppointmentId;
            await _distCache.SetAsync(string.Format(CacheKey,scopeId.ToString()), documentScopeOfAppointment,
                TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddCategory, new { id, scopeId, compareScopeId });
        }

        var options =
            await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(
                documentScopeOfAppointment.LegislativeAreaId);
        vm.PurposeOfAppointments = options.PurposeOfAppointments
            .Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }

    [HttpGet("add-category/{scopeId}", Name = Routes.AddCategory)]
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId, Guid? compareScopeId)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
        var categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId,
            scopeOfAppointment.LegislativeAreaId);

        var selectListItems = categories.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProduct, new { id, scopeId, compareScopeId });
        var legislativeArea =
            await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
        var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null
            ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(
                (Guid)scopeOfAppointment.PurposeOfAppointmentId)
            : null;

        if (existingScopeOfAppointment is { CategoryId: not null })
        {
            var itemFound = selectListItems.FirstOrDefault(i =>
                i.Value == existingScopeOfAppointment.CategoryId.Value.ToString());
            if (itemFound != null) itemFound.Selected = true;
        }

        var vm = new CategoryViewModel
        {
            CABId = id,
            Categories = selectListItems,
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointment = purposeOfAppointment?.Name
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
    }

    [HttpPost("add-category/{scopeId}", Name = Routes.AddCategory)]
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId, CategoryViewModel vm, Guid? compareScopeId)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
        if (ModelState.IsValid)
        {
            scopeOfAppointment.CategoryId = vm.SelectedCategoryId;
            await _distCache.SetAsync(string.Format(CacheKey,scopeId.ToString()), scopeOfAppointment, TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddSubCategory, new { id, scopeId, compareScopeId });
        }

        vm.Categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId,
            scopeOfAppointment.LegislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
    }

    [HttpGet("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId, Guid? compareScopeId)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
        var subcategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);

        var selectListItems = subcategories.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProduct, new { id, scopeId, compareScopeId });

        var legislativeArea =
            await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
        var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null
            ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(
                (Guid)scopeOfAppointment.PurposeOfAppointmentId)
            : null;
        var category = scopeOfAppointment.CategoryId != null
            ? await _legislativeAreaService.GetCategoryByIdAsync((Guid)scopeOfAppointment.CategoryId)
            : null;
        if (existingScopeOfAppointment is { SubCategoryId: not null })
        {
            var itemFound = selectListItems.FirstOrDefault(i =>
                i.Value == existingScopeOfAppointment.SubCategoryId.Value.ToString());
            if (itemFound != null) itemFound.Selected = true;
        }

        var vm = new SubCategoryViewModel
        {
            CABId = id,
            SubCategories = selectListItems,
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointment = purposeOfAppointment?.Name,
            Category = category?.Name
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
    }

    [HttpPost("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId, SubCategoryViewModel vm,
        Guid? compareScopeId)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
        if (ModelState.IsValid)
        {
            scopeOfAppointment.SubCategoryId = vm.SelectedSubCategoryId;
            await _distCache.SetAsync(string.Format(CacheKey,scopeId.ToString()), scopeOfAppointment, TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddProduct, new { id, scopeId, compareScopeId });
        }

        vm.SubCategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
    }

    [HttpGet("add-product/{scopeId}", Name = Routes.AddProduct)]
    public async Task<IActionResult> AddProduct(Guid id, Guid scopeId, Guid? compareScopeId)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
        var products = await GetProductSelectListItemsAsync(scopeOfAppointment.SubCategoryId,
            scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);

        var selectListItems = products.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProcedure, new { id, scopeId, compareScopeId });
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
        if (existingScopeOfAppointment != null && existingScopeOfAppointment.ProductIds.Any())
        {
            foreach (var item in selectListItems)
            {
                item.Selected = existingScopeOfAppointment.ProductIds.Contains(Guid.Parse(item.Value));
            }
        }

        var vm = new ProductViewModel
        {
            CABId = id,
            Products = selectListItems,
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointment = purposeOfAppointment?.Name,
            Category = category?.Name,
            SubCategory = subCategory?.Name
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProduct.cshtml", vm);
    }

    [HttpPost("add-product/{scopeId}", Name = Routes.AddProduct)]
    public async Task<IActionResult> AddProduct(Guid id, Guid scopeId, ProductViewModel vm, Guid? compareScopeId)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });
        if (ModelState.IsValid)
        {
            scopeOfAppointment.ProductIds = vm.SelectedProductIds!;
            await _distCache.SetAsync(string.Format(CacheKey,scopeId.ToString()), scopeOfAppointment, TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddProcedure, new { id, scopeId, compareScopeId });
        }

        vm.Products = await GetProductSelectListItemsAsync(scopeOfAppointment.SubCategoryId,
            scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProduct.cshtml", vm);
    }

    [HttpGet("add-procedure/{scopeId}", Name = Routes.AddProcedure)]
    public async Task<IActionResult> AddProcedure(Guid id, Guid scopeId, Guid? compareScopeId, int indexOfProduct = 0)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        Guid? productId = null;

        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });

        if (scopeOfAppointment.ProductIds.Any() && indexOfProduct < scopeOfAppointment.ProductIds.Count)
        {
            productId = scopeOfAppointment.ProductIds[indexOfProduct];
        }

        var procedures = await GetProcedureSelectListItemsAsync(productId, scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId);
        var selectListItems = procedures.ToList();

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
        string? productName = null;
        if (productId != null)
        {
            var product = await _legislativeAreaService.GetProductByIdAsync((Guid)productId);
            productName = product!.Name;
        }

        var existingProcedures = existingScopeOfAppointment?.ProductIdAndProcedureIds.SelectMany(p => p.ProcedureIds)
            .ToList();
        if (existingProcedures != null && existingProcedures.Any())
        {
            foreach (var item in selectListItems)
            {
                item.Selected = existingProcedures.Contains(Guid.Parse(item.Value));
            }
        }

        var vm = new ProcedureViewModel
        {
            CABId = id,
            Product = productName,
            CurrentProductId = productId,
            Procedures = selectListItems,
            LegislativeAreaId = legislativeArea?.Id,
            LegislativeArea = legislativeArea?.Name,
            PurposeOfAppointment = purposeOfAppointment?.Name,
            Category = category?.Name,
            SubCategory = subCategory?.Name,
            ShowContinueToNextStep = indexOfProduct >= scopeOfAppointment.ProductIds.Count - 1
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProcedure.cshtml", vm);
    }

    [HttpPost("add-procedure/{scopeId}", Name = Routes.AddProcedure)]
    public async Task<IActionResult> AddProcedure(Guid id, Guid scopeId, int indexOfProduct, ProcedureViewModel vm,
        Guid? compareScopeId, string submitType)
    {
        var scopeOfAppointment = await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey,scopeId.ToString()));
        Guid? productId = null;

        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id });

        if (ModelState.IsValid)
        {
            var productAndProcedures = new ProductAndProcedures
            {
                ProductId = vm.CurrentProductId,
                ProcedureIds = (List<Guid>)vm.SelectedProcedureIds!
            };

            scopeOfAppointment.ProductIdAndProcedureIds.Add(productAndProcedures);
            await _distCache.SetAsync(string.Format(CacheKey,scopeId.ToString()), scopeOfAppointment, TimeSpan.FromHours(1));
            if (indexOfProduct + 1 < scopeOfAppointment.ProductIds.Count)
            {
                return RedirectToRoute(Routes.AddProcedure, new { id, scopeId, indexOfProduct = indexOfProduct + 1, compareScopeId});
            }

            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                                 throw new InvalidOperationException();
            latestDocument.ScopeOfAppointments.Add(scopeOfAppointment);

            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            var updatedDocument = await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
            var existingScopeOfAppointment = updatedDocument.ScopeOfAppointments.FirstOrDefault(s => s.Id == compareScopeId);
            if (existingScopeOfAppointment != null)
            {
                updatedDocument.ScopeOfAppointments.Remove(existingScopeOfAppointment);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, updatedDocument);
            }

            if (submitType == Constants.SubmitType.Add)
            {
                return RedirectToRoute(Routes.AddPurposeOfAppointment, new { id, scopeId = Guid.Empty, legislativeAreaId = vm.LegislativeAreaId });
            }

            return RedirectToRoute(
                LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation,
                new { id, laId = scopeOfAppointment.LegislativeAreaId });
        }

        if (scopeOfAppointment.ProductIds.Any() && indexOfProduct < scopeOfAppointment.ProductIds.Count)
        {
            productId = scopeOfAppointment.ProductIds[indexOfProduct];
        }

        vm.Procedures = await GetProcedureSelectListItemsAsync(productId, scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId);

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProcedure.cshtml", vm);
    }

    [HttpGet("remove/{legislativeAreaId}", Name = Routes.RemoveLegislativeArea)]
    public async Task<IActionResult> RemoveLegislativeArea(Guid id, Guid legislativeAreaId, string? returnUrl)
    {
        var cabDocuments = await _cabAdminService.FindAllDocumentsByCABIdAsync(id.ToString());
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);

        // only one document and draft mode the remove else give user an option to remove or archive
        if (cabDocuments.Count == 1 && cabDocuments.First().StatusValue == Status.Draft)
        {
            await _cabAdminService.RemoveLegislativeAreaAsync(id, legislativeAreaId, legislativeArea.Name);
            return RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true });
        }
        else
        {
            var latestDocument = GetLatestDocumentAsync(cabDocuments);
            var legislativeAreaArchived = latestDocument?.DocumentLegislativeAreas.Where(n => n.LegislativeAreaId == legislativeAreaId).First().Archived;

            var vm = new LegislativeAreaRemoveViewModel
            {
                Title = legislativeArea.Name,
                CabId = id,
                LegislativeArea = await PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId),
                ProductSchedules = latestDocument?.Schedules ?? new List<FileUpload>(),
                ShowArchiveOption = !legislativeAreaArchived.GetValueOrDefault(),
                ReturnUrl = returnUrl
            };
            return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeArea.cshtml", vm);
        }
    }

    [HttpPost("remove/{legislativeAreaId}", Name = Routes.RemoveLegislativeArea)]
    public async Task<IActionResult> RemoveLegislativeArea(Guid id, Guid legislativeAreaId, LegislativeAreaRemoveViewModel vm,
        string? returnUrl)
    {
        if (ModelState.IsValid)
        {
            if (vm.Action == RemoveActionEnum.Remove)
            {
                await _cabAdminService.RemoveLegislativeAreaAsync(id, legislativeAreaId, vm.Title);
            }
            else
            {
                await _cabAdminService.ArchiveLegislativeAreaAsync(id, legislativeAreaId);
            }

            return RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true });
        }
        else
        {
            var cabDocuments = await _cabAdminService.FindAllDocumentsByCABIdAsync(id.ToString());
            var latestDocument = GetLatestDocumentAsync(cabDocuments);

            vm.LegislativeArea = await PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId);
            vm.ProductSchedules = latestDocument?.Schedules ?? new List<FileUpload>();
           
            return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeArea.cshtml", vm);
        }
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
        ScopeOfAppointmentOptionsModel? scopeOfAppointmentOptionsModel;

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

    private async Task<IEnumerable<SelectListItem>> GetProductSelectListItemsAsync(Guid? subCategoryId,
        Guid? categoryId,
        Guid? purposeOfAppointmentId, Guid? legislativeAreaId)
    {
        ScopeOfAppointmentOptionsModel scopeOfAppointmentOptionsModel;
        if (subCategoryId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForSubCategoryAsync(subCategoryId.Value);
            if (scopeOfAppointmentOptionsModel.Products.Any())
            {
                return scopeOfAppointmentOptionsModel.Products.Select(x => new SelectListItem
                    { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (categoryId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(categoryId.Value);
            if (scopeOfAppointmentOptionsModel.Products.Any())
            {
                return scopeOfAppointmentOptionsModel.Products.Select(x => new SelectListItem
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
                return scopeOfAppointmentOptionsModel.Products.Select(x => new SelectListItem
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

    private async Task<IEnumerable<SelectListItem>> GetProcedureSelectListItemsAsync(Guid? productId, Guid? categoryId,
        Guid? purposeOfAppointmentId)
    {
        ScopeOfAppointmentOptionsModel scopeOfAppointmentOptionsModel;

        if (productId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService
                    .GetNextScopeOfAppointmentOptionsForProductAsync(productId.Value);
            if (scopeOfAppointmentOptionsModel.Procedures.Any())
            {
                return scopeOfAppointmentOptionsModel.Procedures.Select(x => new SelectListItem
                    { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (categoryId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService
                    .GetNextScopeOfAppointmentOptionsForCategoryAsync(categoryId.Value);
            if (scopeOfAppointmentOptionsModel.Procedures.Any())
            {
                return scopeOfAppointmentOptionsModel.Procedures.Select(x => new SelectListItem
                    { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (purposeOfAppointmentId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService
                    .GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(purposeOfAppointmentId.Value);
            if (scopeOfAppointmentOptionsModel.Procedures.Any())
            {
                return scopeOfAppointmentOptionsModel.Procedures.Select(x => new SelectListItem
                    { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }

    public Document? GetLatestDocumentAsync(List<Document> documents)
    {  
        // if a newly create cab or a draft version exists this will be the latest version, there should be no more than one
        if (documents.Any(d => d is { StatusValue: Status.Draft }))
        {
            return documents.Single(d => d is { StatusValue: Status.Draft });
        }

        // if no draft or created version exists then see if a published version exists, again should only ever be one
        if (documents.Any(d => d is { StatusValue: Status.Published }))
        {
            return documents.Single(d => d is { StatusValue: Status.Published });
        }

        return null;
    }

    private async Task<CABLegislativeAreasItemViewModel> PopulateCABLegislativeAreasItemViewModelAsync(Document? cab, Guid LegislativeAreaId)
    {      
        var documentLegislativeArea = cab.DocumentLegislativeAreas.Where(n => n.LegislativeAreaId == LegislativeAreaId).First() ?? throw new InvalidOperationException("document legislative area not found");

        var legislativeArea =
                await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentLegislativeArea
                    .LegislativeAreaId);

        var legislativeAreaViewModel = new CABLegislativeAreasItemViewModel()
        {
            Name = legislativeArea.Name,
            IsProvisional = documentLegislativeArea.IsProvisional,
            AppointmentDate = documentLegislativeArea.AppointmentDate,
            ReviewDate = documentLegislativeArea.ReviewDate,
            Reason = documentLegislativeArea.Reason,
            CanChooseScopeOfAppointment = legislativeArea.HasDataModel,
        };

        var scopeOfAppointments = cab.ScopeOfAppointments.Where(x => x.LegislativeAreaId == legislativeArea.Id);
        foreach (var scopeOfAppointment in scopeOfAppointments)
        {
            var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId.HasValue
                ? (await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(scopeOfAppointment
                    .PurposeOfAppointmentId.Value))?.Name
                : null;

            var category = scopeOfAppointment.CategoryId.HasValue
                ? (await _legislativeAreaService.GetCategoryByIdAsync(scopeOfAppointment.CategoryId.Value))
                ?.Name
                : null;

            var subCategory = scopeOfAppointment.SubCategoryId.HasValue
                ? (await _legislativeAreaService.GetSubCategoryByIdAsync(scopeOfAppointment.SubCategoryId
                    .Value))?.Name
                : null;

            foreach (var productProcedure in scopeOfAppointment.ProductIdAndProcedureIds)
            {
                var soaViewModel = new LegislativeAreaListItemViewModel()
                {
                    LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                    PurposeOfAppointment = purposeOfAppointment,
                    Category = category,
                    SubCategory = subCategory,
                };

                if (productProcedure.ProductId.HasValue)
                {
                    var product =
                        await _legislativeAreaService.GetProductByIdAsync(productProcedure.ProductId.Value);
                    soaViewModel.Product = product!.Name;
                }

                foreach (var procedureId in productProcedure.ProcedureIds)
                {
                    var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                    soaViewModel.Procedures?.Add(procedure!.Name);
                }

                legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
            }
        }

        return legislativeAreaViewModel;
    }

}