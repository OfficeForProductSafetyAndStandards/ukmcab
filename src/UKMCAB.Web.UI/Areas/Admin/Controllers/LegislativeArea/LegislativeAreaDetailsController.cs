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
using UKMCAB.Web.UI.Services;
using UKMCAB.Core.Security;
using System.Net;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IDistCache _distCache;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly ILegislativeAreaDetailService _legislativeAreaDetailService;    
    private const string CacheKey = "soa_create_{0}";

    public static class Routes
    {
        public const string AddLegislativeArea = "legislative.area.add-legislativearea";
        public const string AddPurposeOfAppointment = "legislative.area.add-purpose-of-appointment";
        public const string AddCategory = "legislative.area.add-category";
        public const string AddSubCategory = "legislative.area.add-sub-category";
        public const string AddProduct = "legislative.area.add-product";
        public const string AddProcedure = "legislative.area.add-procedure";
        public const string RemoveOrArchiveLegislativeArea = "legislative.area.remove-archive-legislativearea";
        public const string RemoveOrArchiveLegislativeAreaOption = "legislative.area.remove-archive-legislativearea-option";
        public const string RemoveLegislativeAreaRequest = "legislative.area.remove-legislativearea-request";
    }

    public LegislativeAreaDetailsController(
        ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        ILegislativeAreaDetailService legislativeAreaDetailService,
        IUserService userService,
        IDistCache distCache) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _distCache = distCache;
        _legislativeAreaDetailService = legislativeAreaDetailService;
    }

    #region AddLegislativeArea

    [HttpGet("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, string? returnUrl, bool fromSummary)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                             throw new InvalidOperationException();
        var cabLegislativeAreaIds = latestDocument.DocumentLegislativeAreas.Select(n => n.LegislativeAreaId).ToList();

        var vm = new LegislativeAreaViewModel
        {
            CABId = id,
            LegislativeAreas = await GetLegislativeSelectListItemsAsync(cabLegislativeAreaIds),
            ReturnUrl = returnUrl,
            IsFromSummary = fromSummary
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    [HttpPost("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, LegislativeAreaViewModel vm, string submitType)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                             throw new InvalidOperationException();
        if (submitType == Constants.SubmitType.Save)
        {
            TempData[Constants.TempDraftKey] =
                $"Draft record saved for {latestDocument.Name} <br>CAB number {latestDocument.CABNumber}";
        }

        var legislativeArea = new LegislativeAreaModel();
        var cabLegislativeAreaIds = new List<Guid>();
        if (vm.SelectedLegislativeAreaId != Guid.Empty)
        {
            cabLegislativeAreaIds =
                latestDocument.DocumentLegislativeAreas.Select(n => n.LegislativeAreaId).ToList();
            legislativeArea =
                await _legislativeAreaService.GetLegislativeAreaByIdAsync(vm.SelectedLegislativeAreaId);

            if (cabLegislativeAreaIds.Contains(vm.SelectedLegislativeAreaId))
            {
                ModelState.AddModelError(nameof(vm.SelectedLegislativeAreaId),
                    $"Legislative area '{legislativeArea.Name}' already exists in Cab.");
            }
        }
        else
        {
            if (submitType == Constants.SubmitType.Save)
                return RedirectToRoute(CabManagementController.Routes.CABManagement,
                    new { unlockCab = latestDocument.CABId });

            ModelState.AddModelError(nameof(vm.SelectedLegislativeAreaId), "Select a legislative area");
        }

        if (ModelState.IsValid)
        {
            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

            // add document new legislative area;
            await _cabAdminService.AddLegislativeAreaAsync(userAccount, id, vm.SelectedLegislativeAreaId, legislativeArea.Name, legislativeArea.RoleId);

            if (!legislativeArea.HasDataModel)
            {
                return RedirectToRoute(LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation,
                    new { id, laId = vm.SelectedLegislativeAreaId, fromSummary = vm.IsFromSummary });
            }

            // add new document scope of appointment to cache;
            var scopeOfAppointmentId = Guid.NewGuid();
            await CreateScopeOfAppointmentInCacheAsync(scopeOfAppointmentId, vm.SelectedLegislativeAreaId);

            switch (submitType)
            {
                case Constants.SubmitType.Continue:
                    return RedirectToRoute(Routes.AddPurposeOfAppointment, new { id, scopeId = scopeOfAppointmentId, fromSummary = vm.IsFromSummary });
                // save additional info
                case Constants.SubmitType.AdditionalInfo:
                    return RedirectToRoute(
                        LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation,
                        new { id, laId = vm.SelectedLegislativeAreaId, fromSummary = vm.IsFromSummary });
                case Constants.SubmitType.Save:
                    return RedirectToRoute(CabManagementController.Routes.CABManagement,
                        new { unlockCab = latestDocument.CABId });
            }
        }

        vm.LegislativeAreas = await GetLegislativeSelectListItemsAsync(cabLegislativeAreaIds);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    #region PrivateMethods

    private async Task CreateScopeOfAppointmentInCacheAsync(Guid scopeId, Guid legislativeAreaId)
    {
        var scopeOfAppointment = new DocumentScopeOfAppointment
        {
            Id = scopeId,
            LegislativeAreaId = legislativeAreaId
        };
        await _distCache.SetAsync(string.Format(CacheKey, scopeId.ToString()), scopeOfAppointment,
            TimeSpan.FromHours(1));
    }

    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync(
        List<Guid> excludeLegislativeAreaIds)
    {
        var legislativeAreas = await _legislativeAreaService.GetLegislativeAreasAsync(excludeLegislativeAreaIds);
        return legislativeAreas.OrderBy(la => la.Name).Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
    }

    #endregion

    #endregion

    #region AddPurposeOfAppointment

    [HttpGet("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, Guid scopeId, Guid? compareScopeId,
        Guid? legislativeAreaId, bool fromSummary)
    {
        if (scopeId == Guid.Empty && legislativeAreaId != null)
        {
            scopeId = Guid.NewGuid();
        }

        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        if (existingScopeOfAppointment != null || legislativeAreaId != null)
        {
            //Create a new scope of id in session to replace existing
            await CreateScopeOfAppointmentInCacheAsync(scopeId,
                legislativeAreaId ?? existingScopeOfAppointment.LegislativeAreaId);
        }

        var documentScopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (documentScopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary });
        var options =
            await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(
                documentScopeOfAppointment.LegislativeAreaId);
        var legislativeArea =
            await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentScopeOfAppointment.LegislativeAreaId);
        if (legislativeArea == null)
        {
            throw new InvalidOperationException(
                $"Legislative area not found for {documentScopeOfAppointment.LegislativeAreaId}");
        }

        if (!options.PurposeOfAppointments.Any())
        {
            return RedirectToRoute(Routes.AddCategory, new { id, scopeId, compareScopeId, fromSummary });
        }

        var selectListItems = options.PurposeOfAppointments.OrderBy(poa => poa.Name)
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
            ScopeId = scopeId,
            IsFromSummary = fromSummary,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }

    [HttpPost("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, PurposeOfAppointmentViewModel vm,
        [FromForm] Guid scopeId,
        Guid? compareScopeId)
    {
        var documentScopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (documentScopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary = vm.IsFromSummary });
        if (ModelState.IsValid)
        {
            documentScopeOfAppointment.PurposeOfAppointmentId = vm.SelectedPurposeOfAppointmentId;
            await _distCache.SetAsync(string.Format(CacheKey, scopeId.ToString()), documentScopeOfAppointment,
                TimeSpan.FromHours(1));

            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                                 throw new InvalidOperationException();
            var documentLegislativeArea = latestDocument.DocumentLegislativeAreas.FirstOrDefault(la => la.LegislativeAreaId == documentScopeOfAppointment.LegislativeAreaId);
            documentLegislativeArea?.MarkAsDraft(latestDocument);
            
            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

            return RedirectToRoute(Routes.AddCategory, new { id, scopeId, compareScopeId, fromSummary = vm.IsFromSummary });
        }

        var options =
            await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(
                documentScopeOfAppointment.LegislativeAreaId);
        vm.PurposeOfAppointments = options.PurposeOfAppointments
            .Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }

    #endregion

    #region AddCategory

    [HttpGet("add-category/{scopeId}", Name = Routes.AddCategory)]
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId, Guid? compareScopeId, bool fromSummary)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary });
        var categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId,
            scopeOfAppointment.LegislativeAreaId);

        var selectListItems = categories.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProduct, new { id, scopeId, compareScopeId, fromSummary });
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

        var categoryIds = selectListItems.Select(i => Guid.Parse(i.Value)).ToList();
        var hasProducts = await HasProductsAsync(legislativeArea.Id, purposeOfAppointment?.Id, categoryIds);

        var vm = new CategoryViewModel
        {
            CABId = id,
            Categories = selectListItems,
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointment = purposeOfAppointment?.Name,
            IsFromSummary = fromSummary,
            HasProducts = hasProducts
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
    }

    [HttpPost("add-category/{scopeId}", Name = Routes.AddCategory)]
    public async Task<IActionResult> AddCategory(Guid id, Guid scopeId, CategoryViewModel vm, Guid? compareScopeId)
    {
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary = vm.IsFromSummary });

        if (vm.HasProducts)
        {
            ModelState.Remove("SelectedCategoryIds");
        } 
        else
        {
            ModelState.Remove("SelectedCategoryId");
        }

        if (ModelState.IsValid)
        {
            scopeOfAppointment.CategoryId = vm.SelectedCategoryId;
            scopeOfAppointment.CategoryIds = vm.SelectedCategoryIds! ?? new List<Guid>();
            await _distCache.SetAsync(string.Format(CacheKey, scopeId.ToString()), scopeOfAppointment,
                TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddSubCategory, new { id, scopeId, compareScopeId, fromSummary = vm.IsFromSummary });
        }

        vm.Categories = await GetCategoriesSelectListItemsAsync(scopeOfAppointment.PurposeOfAppointmentId,
            scopeOfAppointment.LegislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddCategory.cshtml", vm);
    }

    #region PrivateMethods

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
                return scopeOfAppointmentOptionsModel.Categories.OrderBy(c => c.Name).Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        scopeOfAppointmentOptionsModel =
            await _legislativeAreaService
                .GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId);

        return scopeOfAppointmentOptionsModel.Categories.Any()
            ? scopeOfAppointmentOptionsModel.Categories.OrderBy(c => c.Name).Select(x => new SelectListItem
            { Text = x.Name, Value = x.Id.ToString() })
            : new List<SelectListItem>();
    }

    #endregion

    #endregion

    #region AddSubCategory

    [HttpGet("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId, Guid? compareScopeId, bool fromSummary)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary });
        var subcategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);

        var selectListItems = subcategories.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProduct, new { id, scopeId, compareScopeId, fromSummary });

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
            Category = category?.Name,
            IsFromSummary = fromSummary
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
    }

    [HttpPost("add-sub-category/{scopeId}", Name = Routes.AddSubCategory)]
    public async Task<IActionResult> AddSubCategory(Guid id, Guid scopeId, SubCategoryViewModel vm,
        Guid? compareScopeId)
    {
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary = vm.IsFromSummary });
        if (ModelState.IsValid)
        {
            scopeOfAppointment.SubCategoryId = vm.SelectedSubCategoryId;
            await _distCache.SetAsync(string.Format(CacheKey, scopeId.ToString()), scopeOfAppointment,
                TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddProduct, new { id, scopeId, compareScopeId, fromSummary = vm.IsFromSummary });
        }

        vm.SubCategories = await GetSubCategoriesSelectListItemsAsync(scopeOfAppointment.CategoryId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddSubCategory.cshtml", vm);
    }

    #region PrivateMethods

    private async Task<bool> HasProductsAsync(Guid legislativeAreaId, Guid? purposeOfAppointmentId, List<Guid> categoryIds)
    {
        List<SelectListItem> products;
        foreach (var categoryId in categoryIds)
        {
            var subCategories = await GetSubCategoriesSelectListItemsAsync(categoryId);
            if (subCategories.Any())
            {
                foreach (var subCategory in subCategories)
                {
                    products = (await GetProductSelectListItemsAsync(Guid.Parse(subCategory.Value), categoryId, purposeOfAppointmentId, legislativeAreaId)).ToList();
                    if (products.Any())
                    {
                        return true;
                    }
                }
            } else
            {
                products = (await GetProductSelectListItemsAsync(null, categoryId, purposeOfAppointmentId, legislativeAreaId)).ToList();
                if (products.Any())
                {
                    return true;
                }
            }
        }
        return false;
    }

    private async Task<IEnumerable<SelectListItem>> GetSubCategoriesSelectListItemsAsync(Guid? categoryId)
    {
        if (categoryId != null)
        {
            var scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync((Guid)categoryId);
            if (scopeOfAppointmentOptionsModel.Subcategories.Any())
            {
                return scopeOfAppointmentOptionsModel.Subcategories.OrderBy(s => s.Name).Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }

    #endregion

    #endregion

    #region AddProduct

    [HttpGet("add-product/{scopeId}", Name = Routes.AddProduct)]
    public async Task<IActionResult> AddProduct(Guid id, Guid scopeId, Guid? compareScopeId, bool fromSummary)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary });
        var products = await GetProductSelectListItemsAsync(scopeOfAppointment.SubCategoryId,
            scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);

        var selectListItems = products.ToList();
        if (!selectListItems.Any()) return RedirectToRoute(Routes.AddProcedure, new { id, scopeId, compareScopeId, fromSummary });
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
            SubCategory = subCategory?.Name,
            IsFromSummary = fromSummary
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProduct.cshtml", vm);
    }

    [HttpPost("add-product/{scopeId}", Name = Routes.AddProduct)]
    public async Task<IActionResult> AddProduct(Guid id, Guid scopeId, ProductViewModel vm, Guid? compareScopeId)
    {
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary = vm.IsFromSummary });
        if (ModelState.IsValid)
        {
            scopeOfAppointment.ProductIds = vm.SelectedProductIds!;
            await _distCache.SetAsync(string.Format(CacheKey, scopeId.ToString()), scopeOfAppointment,
                TimeSpan.FromHours(1));
            return RedirectToRoute(Routes.AddProcedure, new { id, scopeId, compareScopeId, fromSummary = vm.IsFromSummary });
        }

        vm.Products = await GetProductSelectListItemsAsync(scopeOfAppointment.SubCategoryId,
            scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId, scopeOfAppointment.LegislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProduct.cshtml", vm);
    }

    #region PrivateMethods

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
                return scopeOfAppointmentOptionsModel.Products.OrderBy(p => p.Name).Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        if (categoryId != null)
        {
            scopeOfAppointmentOptionsModel =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(categoryId.Value);
            if (scopeOfAppointmentOptionsModel.Products.Any())
            {
                return scopeOfAppointmentOptionsModel.Products.OrderBy(p => p.Name).Select(x => new SelectListItem
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
                return scopeOfAppointmentOptionsModel.Products.OrderBy(p => p.Name).Select(x => new SelectListItem
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
                return scopeOfAppointmentOptionsModel.Products.OrderBy(p => p.Name).Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }

    #endregion

    #endregion

    #region AddProcedure

    [HttpGet("add-procedure/{scopeId}", Name = Routes.AddProcedure)]
    public async Task<IActionResult> AddProcedure(Guid id, Guid scopeId, Guid? compareScopeId, bool fromSummary, int indexOfProduct = 0, int indexOfCategory = 0)
    {
        var existingScopeOfAppointment = await GetCompareScopeOfAppointment(id, compareScopeId);
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        Guid? productId = null;
        Guid? categoryId = null;

        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary });

        if (scopeOfAppointment.ProductIds.Any() && indexOfProduct < scopeOfAppointment.ProductIds.Count)
        {
            productId = scopeOfAppointment.ProductIds[indexOfProduct];
        }

        if (scopeOfAppointment.CategoryIds.Any() && indexOfCategory < scopeOfAppointment.CategoryIds.Count)
        {
            categoryId = scopeOfAppointment.CategoryIds[indexOfCategory];
        }
        else if (!scopeOfAppointment.CategoryIds.Any())
        {
            categoryId = scopeOfAppointment.CategoryId;
        }

        var procedures = await GetProcedureSelectListItemsAsync(productId, categoryId,
            scopeOfAppointment.PurposeOfAppointmentId);
        var selectListItems = procedures.ToList();

        var legislativeArea =
            await _legislativeAreaService.GetLegislativeAreaByIdAsync(scopeOfAppointment.LegislativeAreaId);
        var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId != null
            ? await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync((Guid)scopeOfAppointment
                .PurposeOfAppointmentId)
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

        string? categoryName = null;
        if (categoryId != null)
        {
            var category = await _legislativeAreaService.GetCategoryByIdAsync((Guid)categoryId);
            categoryName = category!.Name;
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
            Category = categoryName,
            CurrentCategoryId = categoryId,
            SubCategory = subCategory?.Name,
            IsLastAction = indexOfProduct >= scopeOfAppointment.ProductIds.Count - 1
                || indexOfCategory >= scopeOfAppointment.CategoryIds.Count - 1,
            IsFromSummary = fromSummary
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProcedure.cshtml", vm);
    }

    [HttpPost("add-procedure/{scopeId}", Name = Routes.AddProcedure)]
    public async Task<IActionResult> AddProcedure(Guid id, Guid scopeId, int indexOfProduct, int indexOfCategory, ProcedureViewModel vm,
        Guid? compareScopeId, string submitType)
    {
        var scopeOfAppointment =
            await _distCache.GetAsync<DocumentScopeOfAppointment>(string.Format(CacheKey, scopeId.ToString()));
        Guid? productId = null;

        if (scopeOfAppointment == null)
            return RedirectToRoute(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary = vm.IsFromSummary });

        if (ModelState.IsValid)
        {
            if (vm.CurrentProductId.HasValue)
            {
                var productAndProcedures = new ProductAndProcedures
                {
                    ProductId = vm.CurrentProductId,
                    ProcedureIds = (List<Guid>)vm.SelectedProcedureIds!
                };
                scopeOfAppointment.ProductIdAndProcedureIds.Add(productAndProcedures);
            }

            if (!vm.CurrentProductId.HasValue && vm.CurrentCategoryId.HasValue)
            {
                var categoryAndProcedures = new CategoryAndProcedures
                {
                    CategoryId = vm.CurrentCategoryId,
                    CategoryIds = (List<Guid>)vm.SelectedProcedureIds!
                };
                scopeOfAppointment.CategoryIdAndProcedureIds.Add(categoryAndProcedures);
            }

            await _distCache.SetAsync(string.Format(CacheKey, scopeId.ToString()), scopeOfAppointment,
                TimeSpan.FromHours(1));
            if (indexOfProduct + 1 < scopeOfAppointment.ProductIds.Count)
            {
                return RedirectToRoute(Routes.AddProcedure,
                    new { id, scopeId, indexOfProduct = indexOfProduct + 1, indexOfCategory = 0, compareScopeId, fromSummary = vm.IsFromSummary });
            }
            if (indexOfCategory + 1 < scopeOfAppointment.CategoryIds.Count)
            {
                return RedirectToRoute(Routes.AddProcedure,
                    new { id, scopeId, indexOfProduct = 0, indexOfCategory = indexOfCategory + 1, compareScopeId, fromSummary = vm.IsFromSummary });
            }

            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                                 throw new InvalidOperationException();
            var documentLegislativeArea = latestDocument.DocumentLegislativeAreas.FirstOrDefault(la => la.LegislativeAreaId == scopeOfAppointment.LegislativeAreaId);
            documentLegislativeArea?.MarkAsDraft(latestDocument);

            latestDocument.ScopeOfAppointments.Add(scopeOfAppointment);
            latestDocument.HiddenScopeOfAppointments =
                await SetHiddenScopeOfAppointmentsAsync(latestDocument.ScopeOfAppointments);
            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            var updatedDocument = await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
            var existingScopeOfAppointment =
                updatedDocument.ScopeOfAppointments.FirstOrDefault(s => s.Id == compareScopeId);

            if (existingScopeOfAppointment != null)
            {
                updatedDocument.ScopeOfAppointments.Remove(existingScopeOfAppointment);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, updatedDocument);
            }

            if (submitType == Constants.SubmitType.Add)
            {
                return RedirectToRoute(Routes.AddPurposeOfAppointment,
                    new { id, scopeId = Guid.Empty, legislativeAreaId = vm.LegislativeAreaId, fromSummary = vm.IsFromSummary });
            }

            return RedirectToRoute(
                LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation,
                new { id, laId = scopeOfAppointment.LegislativeAreaId, fromSummary = vm.IsFromSummary });
        }

        if (scopeOfAppointment.ProductIds.Any() && indexOfProduct < scopeOfAppointment.ProductIds.Count)
        {
            productId = scopeOfAppointment.ProductIds[indexOfProduct];
        }

        if (scopeOfAppointment.CategoryIds.Any() && indexOfProduct < scopeOfAppointment.CategoryIds.Count)
        {
            productId = scopeOfAppointment.CategoryIds[indexOfProduct];
        }

        vm.Procedures = await GetProcedureSelectListItemsAsync(productId, scopeOfAppointment.CategoryId,
            scopeOfAppointment.PurposeOfAppointmentId);

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddProcedure.cshtml", vm);
    }

    #region PrivateMethods

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
                return scopeOfAppointmentOptionsModel.Procedures.OrderBy(p => p.Name).Select(x => new SelectListItem
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
                return scopeOfAppointmentOptionsModel.Procedures.OrderBy(p => p.Name).Select(x => new SelectListItem
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
                return scopeOfAppointmentOptionsModel.Procedures.OrderBy(p => p.Name).Select(x => new SelectListItem
                { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        return new List<SelectListItem>();
    }

    private async Task<List<string>> SetHiddenScopeOfAppointmentsAsync(
        IEnumerable<DocumentScopeOfAppointment> scopeOfAppointments)
    {
        var hiddenScopeOfAppointments = new List<string>();
        var documentScopeOfAppointments = scopeOfAppointments.ToList();
        var legislativeAreaIds = documentScopeOfAppointments.Select(a => a.LegislativeAreaId)
            .Distinct()
            .ToList();

        var allLegislativeAreas = await _legislativeAreaService.GetAllLegislativeAreasAsync();
        var regulation = allLegislativeAreas
            .Where(n => legislativeAreaIds.Contains(n.Id) && !string.IsNullOrWhiteSpace(n.Regulation))
            .Select(a => a.Regulation)
            .Distinct()
            .ToList();

        var purposeOfAppointmentIds = documentScopeOfAppointments.Where(a => a.PurposeOfAppointmentId != null)
            .Select(a => a.PurposeOfAppointmentId)
            .Distinct()
            .ToList();
        var returnHiddenScopeOfAppointments = hiddenScopeOfAppointments.Concat(regulation).ToList();

        if (purposeOfAppointmentIds.Any())
        {
            foreach (Guid purposeOfAppointmentId in purposeOfAppointmentIds)
            {
                var purposeOfAppointment =
                    await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(purposeOfAppointmentId);
                if (purposeOfAppointment?.Name != null)
                    returnHiddenScopeOfAppointments.Add(purposeOfAppointment.Name);
            }
        }

        var categoryIds = documentScopeOfAppointments.Where(a => a.CategoryId != null)
            .Select(a => a.CategoryId)
            .Distinct()
            .ToList();

        if (categoryIds.Any())
        {
            foreach (Guid categoryId in categoryIds)
            {
                var category = await _legislativeAreaService.GetCategoryByIdAsync(categoryId);
                if (category?.Name != null) returnHiddenScopeOfAppointments.Add(category.Name);
            }
        }

        var subcategoryIds = documentScopeOfAppointments.Where(a => a.SubCategoryId != null)
            .Select(a => a.SubCategoryId)
            .Distinct()
            .ToList();

        if (subcategoryIds.Any())
        {
            foreach (Guid subcategoryId in subcategoryIds)
            {
                var subcategory = await _legislativeAreaService.GetSubCategoryByIdAsync(subcategoryId);
                if (subcategory?.Name != null) returnHiddenScopeOfAppointments.Add(subcategory.Name);
            }
        }

        var productAndProcedures = documentScopeOfAppointments
            .Select(a => a.ProductIdAndProcedureIds)
            .ToList();

        if (!productAndProcedures.Any()) return returnHiddenScopeOfAppointments;
        
        foreach (var pp in productAndProcedures.SelectMany(pp => pp).Distinct())
        {
            if (!pp.ProductId.HasValue) continue;
            var product = await _legislativeAreaService.GetProductByIdAsync(pp.ProductId.Value);
            if (product?.Name != null) 
                returnHiddenScopeOfAppointments.Add(product.Name);
        }

        return returnHiddenScopeOfAppointments;
    }

    #endregion

    #endregion

    #region RemoveLegislativeArea

    [HttpGet("remove/{legislativeAreaId}/{actionType}", Name = Routes.RemoveOrArchiveLegislativeArea)]
    public async Task<IActionResult> RemoveOrArchiveLegislativeArea(Guid id, Guid legislativeAreaId, RemoveActionEnum actionType, string? returnUrl, bool fromSummary)
    {
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

        // if legislative area has any schedules then give an option to remove/archive product schedules
        var schedules = latestDocument!.Schedules;
        if (schedules != null && schedules.Any(n => n.LegislativeArea != null && string.Equals(n.LegislativeArea, legislativeArea.Name, StringComparison.CurrentCultureIgnoreCase)))
        {
            return RedirectToRoute(Routes.RemoveOrArchiveLegislativeAreaOption, new { id, legislativeAreaId, actionType, fromSummary });
        }
        else
        {
            var actionText = actionType == RemoveActionEnum.Remove ? "Remove" : "Archive";

            var vm = new LegislativeAreaRemoveViewModel
            {
                Title = $"{actionText} legislative area",
                LegislativeAreaRemoveAction = actionType,
                CabId = id,
                LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId),
                ReturnUrl = returnUrl,
                FromSummary = fromSummary
            };
            return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeArea.cshtml", vm);
        }
    }

    [HttpPost("remove/{legislativeAreaId}/{actionType}", Name = Routes.RemoveOrArchiveLegislativeArea)]
    public async Task<IActionResult> RemoveOrArchiveLegislativeArea(Guid id, Guid legislativeAreaId, LegislativeAreaRemoveViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            LegislativeAreaActionMessageEnum? laActionMessageActionType = null;

            var singleDraftDoc = await _cabAdminService.IsSingleDraftDocAsync(id);

            if (vm.LegislativeAreaRemoveAction == RemoveActionEnum.Remove)
            {
                if (UserRoleId == Roles.UKAS.Id && !singleDraftDoc)
                {
                    return RedirectToRoute(Routes.RemoveLegislativeAreaRequest, new
                    {
                        id,
                        legislativeAreaId,
                        returnUrl = WebUtility.UrlEncode(HttpContext.Request.GetRequestUri().PathAndQuery)
                    });
                }
                else
                {
                    await _cabAdminService.RemoveLegislativeAreaAsync(userAccount, id, legislativeAreaId, vm.Title);
                    laActionMessageActionType = LegislativeAreaActionMessageEnum.LegislativeAreaRemoved;
                    return RedirectToAction("ReviewLegislativeAreas", "LegislativeAreaReview",
                        new { Area = "admin", id, actionType = laActionMessageActionType, vm.FromSummary });
                }
            }
            else
            {
                // When OPSS - Admin approves the Archive request 
                if (UserRoleId == Roles.OPSS.Id)
                {
                    await _cabAdminService.ArchiveLegislativeAreaAsync(userAccount, id, legislativeAreaId);
                    laActionMessageActionType = LegislativeAreaActionMessageEnum.LegislativeAreaArchived;
                    return RedirectToAction("ReviewLegislativeAreas", "LegislativeAreaReview",
                        new { Area = "admin", id, actionType = laActionMessageActionType, vm.FromSummary });
                }
                else
                {
                    return RedirectToRoute(ArchiveLegislativeAreaRequestController.Routes.ArchiveLegislativeArea,
                    new { Area = "admin", id, legislativeAreaId, removeActionEnum = vm.LegislativeAreaRemoveAction.Value,
                        returnUrl = WebUtility.UrlEncode(HttpContext.Request.GetRequestUri().PathAndQuery)
                    });
                }
            }
        }

        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        vm.LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeArea.cshtml", vm);
    }

    [HttpGet("remove-with-option/{legislativeAreaId}/{actionType}", Name = Routes.RemoveOrArchiveLegislativeAreaOption)]
    public async Task<IActionResult> RemoveOrArchiveLegislativeAreaWithOption(Guid id, Guid legislativeAreaId, RemoveActionEnum actionType, bool fromSummary, string? returnUrl)
    {
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);
        LegislativeAreaActionMessageEnum? laActionMessageActionType = null;

        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var actionText = actionType == RemoveActionEnum.Remove ? "Remove" : "Archive";

        var vm = new LegislativeAreaRemoveWithOptionViewModel
        {
            Title = $"{actionText} legislative area",
            LegislativeAreaRemoveAction = actionType,
            CabId = id,
            LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId),
            ProductSchedules = latestDocument.Schedules?.Where(n => n.LegislativeArea == legislativeArea.Name).ToList(),
            ReturnUrl = returnUrl,
            FromSummary = fromSummary
        };
        return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeAreaWithOption.cshtml", vm);
    }

    [HttpPost("remove-with-option/{legislativeAreaId}/{actionType}", Name = Routes.RemoveOrArchiveLegislativeAreaOption)]
    public async Task<IActionResult> RemoveOrArchiveLegislativeAreaWithOption(Guid id, Guid legislativeAreaId, LegislativeAreaRemoveWithOptionViewModel vm,
        string? returnUrl)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);

        if (ModelState.IsValid)
        {
            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            LegislativeAreaActionMessageEnum? laActionMessageActionType = null;

            var singleDraftDoc = await _cabAdminService.IsSingleDraftDocAsync(id);

            // legislative arease selected to remove
            if (vm.LegislativeAreaRemoveAction == RemoveActionEnum.Remove)
            {
                if (UserRoleId == Roles.UKAS.Id && !singleDraftDoc)
                {
                    return RedirectToRoute(Routes.RemoveLegislativeAreaRequest, 
                        new 
                        { 
                            id, 
                            legislativeAreaId,
                            returnUrl = WebUtility.UrlEncode(HttpContext.Request.GetRequestUri().PathAndQuery)
                        });
                }
                else
                {
                    await _cabAdminService.RemoveLegislativeAreaAsync(userAccount, id, legislativeAreaId, legislativeArea.Name);
                    laActionMessageActionType = LegislativeAreaActionMessageEnum.LegislativeAreaRemovedProductScheduleRemoved;

                    return RedirectToAction("ReviewLegislativeAreas", "LegislativeAreaReview", new { Area = "admin", id, actionType = laActionMessageActionType, vm.FromSummary });
                }
            }
            // legislative area selected to archive
            else
            {                
                // When OPSS - Admin approves the Archive request 
                if (UserRoleId == Roles.OPSS.Id)
                {
                    await _cabAdminService.ArchiveLegislativeAreaAsync(userAccount, id, legislativeAreaId);              

                    List<Guid> scheduleIds = latestDocument?.Schedules?.Where(n => n.LegislativeArea != null && n.LegislativeArea == legislativeArea.Name).Select(n => n.Id).ToList();

                    // product schedule selected to remove
                    if (vm.ProductScheduleAction == RemoveActionEnum.Remove)
                    {
                        if (scheduleIds != null && scheduleIds.Any())
                        {
                            await _cabAdminService.RemoveSchedulesAsync(userAccount, id, scheduleIds);
                        }

                        laActionMessageActionType = LegislativeAreaActionMessageEnum.LegislativeAreaArchivedProductScheduleRemoved;

                    }
                    // product schedule selected to archive
                    else
                    {
                        if (scheduleIds != null && scheduleIds.Any())
                        {
                            await _cabAdminService.ArchiveSchedulesAsync(userAccount, id, scheduleIds);
                        }
                        laActionMessageActionType = LegislativeAreaActionMessageEnum.LegislativeAreaArchivedProductScheduleArchived;
                    }

                    return RedirectToAction("ReviewLegislativeAreas", "LegislativeAreaReview", new { Area = "admin", id, actionType = laActionMessageActionType, vm.FromSummary });
                }
                else
                {
                    return RedirectToRoute(ArchiveLegislativeAreaRequestController.Routes.ArchiveLegislativeArea,
                    new { 
                        Area = "admin", 
                        id, legislativeAreaId, 
                        removeActionEnum = vm.ProductScheduleAction.Value 
                    });
                }
            }           
        }
        else
        {
            vm.LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId);
            vm.ProductSchedules = latestDocument.Schedules?.Where(n => n.LegislativeArea == legislativeArea.Name).ToList();
            return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeAreaWithOption.cshtml", vm);
        }
    }

    [HttpGet("remove-request/{legislativeAreaId}", Name = Routes.RemoveLegislativeAreaRequest)]
    public async Task<IActionResult> RemoveLegislativeAreaRequest(Guid id, Guid legislativeAreaId, string? returnUrl)
    {   
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);                
        
        var vm = new LegislativeAreaRemoveRequestViewModel
        {
            CabId = id,
            Title = legislativeArea.Name,
            ReturnUrl = returnUrl
        };
        return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeAreaRequest.cshtml", vm);
    }

    [HttpPost("remove-request/{legislativeAreaId}", Name = Routes.RemoveLegislativeAreaRequest)]
    public async Task<IActionResult> RemoveLegislativeAreaRequest(Guid id, Guid legislativeAreaId, LegislativeAreaRemoveRequestViewModel vm)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

        if (ModelState.IsValid)
        {
            // set document legislative area status to pending approval to remove
            var documentLegislativeArea =
                latestDocument.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == legislativeAreaId);
            documentLegislativeArea.Status = LAStatus.PendingSubmissionToRemove;
            documentLegislativeArea.RequestReason = vm.UserNotes;

            await _cabAdminService.UpdateOrCreateDraftDocumentAsync((await _userService.GetAsync(User.GetUserId()!))!, latestDocument);
            return RedirectToAction("summary", "cab", new { Area = "admin", id, subSectionEditAllowed = true });
        }
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/RemoveLegislativeAreaRequest.cshtml", vm);
    }
    
    #endregion

    #region PrivateMethods

    private async Task<DocumentScopeOfAppointment?> GetCompareScopeOfAppointment(Guid id, Guid? compareScopeId)
    {
        var cab = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ?? throw new InvalidOperationException();
        var existingScopeOfAppointment = cab.ScopeOfAppointments.FirstOrDefault(s => s.Id == compareScopeId);
        return existingScopeOfAppointment;
    }

    #endregion
   
}