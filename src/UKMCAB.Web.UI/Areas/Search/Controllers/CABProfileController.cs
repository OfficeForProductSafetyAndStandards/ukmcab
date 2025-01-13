using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using System.Xml;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Storage;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Web.UI.Areas.Home.Controllers;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Services;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using LegislativeAreaViewModel = UKMCAB.Web.UI.Models.ViewModels.Search.LegislativeAreaViewModel;
using UKMCAB.Core.Extensions;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    //TODO: Split this controller
    [Area("search")]
    public class CABProfileController : Controller
    {
        private readonly CoreEmailTemplateOptions _templateOptions;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly ICABAdminService _cabAdminService;
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly IEditLockService _editLockService;
        private readonly IFeedService _feedService;
        private readonly IFileStorage _fileStorage;
        private readonly ILegislativeAreaService _legislativeAreaService;
        private readonly IUserService _userService;
        private readonly IWorkflowTaskService _workflowTaskService;
        private readonly IDistCache _distCache;
        private readonly TelemetryClient _telemetryClient;
        private const string CacheKey = "audit_history_{0}";

        public static class Routes
        {
            public const string CabDetails = "cab.detail";
            public const string CabDetailsLegislativeArea = "cab.detail.legislative-area";

            public const string CabDraftProfile = "cab.profile.draft";
            public const string CabProfileHistoryDetails = "cab.profile.history-details";
            public const string TrackInboundLinkCabDetails = "cab.details.inbound-email-link";
            public const string CabFeed = "cab.feed";
        }

        public CABProfileController(ICachedPublishedCABService cachedPublishedCabService,
            ICABAdminService cabAdminService, IFileStorage fileStorage, TelemetryClient telemetryClient,
            IFeedService feedService, IUserService userService, IOptions<CoreEmailTemplateOptions> templateOptions,
            IAsyncNotificationClient notificationClient, IWorkflowTaskService workflowTaskService,
            IEditLockService editLockService, ILegislativeAreaService legislativeAreaService, IDistCache distCache)
        {
            _cabAdminService = cabAdminService;
            _cachedPublishedCabService = cachedPublishedCabService;
            _editLockService = editLockService;
            _feedService = feedService;
            _fileStorage = fileStorage;
            _legislativeAreaService = legislativeAreaService;
            _notificationClient = notificationClient;
            _telemetryClient = telemetryClient;
            _templateOptions = templateOptions.Value;
            _userService = userService;
            _workflowTaskService = workflowTaskService;
            _distCache = distCache;
        }

        [HttpGet("~/__subscriptions/__inbound/cab/{id}", Name = Routes.TrackInboundLinkCabDetails)]
        public IActionResult TrackInboundLinkCabDetails(string id)
        {
            _telemetryClient.TrackEvent(AiTracking.Events.CabViewedViaSubscriptionsEmail,
                HttpContext.ToTrackingMetadata());
            return RedirectToRoute(Routes.CabDetails, new { id });
        }

        [HttpGet("search/cab-profile/{id}", Name = Routes.CabDetails)]
        public async Task<IActionResult> Index(string id, string? returnUrl, string? unlockCab, int pagenumber = 1)
        {
            
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(id);
            returnUrl = (returnUrl == "confirmation" || Url.IsLocalUrl(returnUrl)) ? returnUrl : default;

            if (cabDocument != null && !id.Equals(cabDocument.URLSlug))
            {
                return RedirectToActionPermanent("Index", new { id = cabDocument.URLSlug, returnUrl });
            }

            if (cabDocument == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(unlockCab))
            {
                await _editLockService.RemoveEditLockForCabAsync(unlockCab);
            }

            var cab = await GetCabProfileForIndex(cabDocument, returnUrl, pagenumber);
            cab.CabLegislativeAreas.ShowLabels = false;
            _telemetryClient.TrackEvent(AiTracking.Events.CabViewed, HttpContext.ToTrackingMetadata(new()
            {
                [AiTracking.Metadata.CabId] = id,
                [AiTracking.Metadata.CabName] = cab.Name
            }));
            return View(cab);
        }

        [HttpGet("search/cab-profile/{id}/{legislativeAreaId}", Name = Routes.CabDetailsLegislativeArea)]
        public async Task<IActionResult> Index(
            string id, 
            string? returnUrl, 
            string? unlockCab, 
            Guid legislativeAreaId,
            Guid? purposeOfAppointmentId, 
            Guid? categoryId, 
            Guid? subCategoryId, 
            Guid? productId, 
            Guid? scopeOfAppointmentId, 
            Guid? ppeProductTypeId,
            Guid? protectionAgainstRiskId, 
            Guid? areaOfCompetencyId, 
            int pagenumber = 1)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(id) ??
                              throw new InvalidOperationException();
            returnUrl = (returnUrl == "confirmation" || Url.IsLocalUrl(returnUrl)) ? returnUrl : default;
            var vm = await GetCabProfileForIndex(cabDocument, returnUrl, pagenumber);
            var la = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);
            vm.CabLegislativeAreas.LegislativeAreaId = legislativeAreaId;
            vm.CabLegislativeAreas.LegislativeAreaName = la.Name;
            vm.CabLegislativeAreas.ShowArchivedTag = cabDocument.DocumentLegislativeAreas
                .Where(l => l.Archived == true).Select(l => l.LegislativeAreaId).Contains(legislativeAreaId);
            vm.CabLegislativeAreas.ShowProvisionalTag = cabDocument.DocumentLegislativeAreas
                .Where(l => l.IsProvisional == true).Select(l => l.LegislativeAreaId).Contains(legislativeAreaId);
            vm.CabLegislativeAreas.Regulation = la.Regulation;

            var designatedStandards = await _legislativeAreaService.GetDesignatedStandardsForDocumentAsync(cabDocument);
            designatedStandards = designatedStandards.Where(i => i.LegislativeAreaId == legislativeAreaId).ToList();
            vm.CabLegislativeAreas.DesignatedStandards = designatedStandards.Select(d => new DesignatedStandardReadOnlyViewModel(d.Id, d.Name, d.ReferenceNumber, d.NoticeOfPublicationReference)).ToList();

            if (productId.HasValue)
            {
                await GetProceduresAsync(null, null, null, null, productId, null, null, null, null, cabDocument, vm);
            }

            if (subCategoryId.HasValue)
            {
                await GetProductsAsync(null, null, null, subCategoryId, cabDocument, vm);
            }
            else if (categoryId.HasValue)
            {
                //Show Sub Category / Products or procedures 
                var subCatIds = cabDocument.ScopeOfAppointments
                    .Where(s => s.CategoryId != null &&
                                s.CategoryId == categoryId && s.SubCategoryId != null)
                    .Select(s => s.SubCategoryId);
                foreach (var subCatId in subCatIds)
                {
                    var subCat = await _legislativeAreaService
                        .GetSubCategoryByIdAsync(subCatId ?? Guid.Empty);

                    if (subCat != null)
                    {
                        vm.CabLegislativeAreas.SubCategories.Add(new ValueTuple<Guid, string>
                        {
                            Item1 = subCat.Id,
                            Item2 = subCat.Name
                        });
                    }
                }

                if (!vm.CabLegislativeAreas.SubCategories.Any())
                {
                    await GetProductsAsync(null, purposeOfAppointmentId, categoryId, subCategoryId, cabDocument, vm);
                    if (!vm.CabLegislativeAreas.Products.Any())
                    {
                        await GetProceduresAsync(null, purposeOfAppointmentId, categoryId, subCategoryId, productId, null, null, null, null,
                            cabDocument, vm);
                    }
                }
            }
            else if (purposeOfAppointmentId.HasValue)
            {
                //Show Categories / Products or procedures 
                await GetCategoriesAsync(null, purposeOfAppointmentId, cabDocument, vm);

                if (!vm.CabLegislativeAreas.Categories.Any())
                {
                    await GetProductsAsync(null, purposeOfAppointmentId, categoryId, subCategoryId, cabDocument, vm);
                    if (!vm.CabLegislativeAreas.Products.Any())
                    {
                        await GetProceduresAsync(null, purposeOfAppointmentId, categoryId, subCategoryId, productId, null, null, null, null,
                            cabDocument,
                            vm);
                    }
                }
            }
       
            if (scopeOfAppointmentId.HasValue && ppeProductTypeId.HasValue)
            {
                await GetProceduresAsync(null, null, null, null, null, null, scopeOfAppointmentId, ppeProductTypeId, null, cabDocument, vm);

                vm.CabLegislativeAreas.PpeProductType = new ValueTuple<Guid?, string?>
                {
                    Item1 = ppeProductTypeId,
                    Item2 = (await _legislativeAreaService
                        .GetPpeProductTypeByIdAsync(ppeProductTypeId.Value))!.Name
                };
            }
            if (scopeOfAppointmentId.HasValue && protectionAgainstRiskId.HasValue)
            {
                await GetProceduresAsync(null, null, null, null, null, null, scopeOfAppointmentId, null, protectionAgainstRiskId, cabDocument, vm);

                vm.CabLegislativeAreas.ProtectionAgainstRisk = new ValueTuple<Guid?, string?>
                {
                    Item1 = protectionAgainstRiskId,
                    Item2 = (await _legislativeAreaService
                        .GetProtectionAgainstRiskByIdAsync(protectionAgainstRiskId.Value))!.Name
                };
            }
            if (scopeOfAppointmentId.HasValue && areaOfCompetencyId.HasValue)
            {
                await GetProceduresAsync(null, null, null, null, null, areaOfCompetencyId, scopeOfAppointmentId, null, null, cabDocument, vm);

                vm.CabLegislativeAreas.AreaOfCompetency = new ValueTuple<Guid?, string?>
                {
                    Item1 = areaOfCompetencyId,
                    Item2 = (await _legislativeAreaService
                        .GetAreaOfCompetencyByIdAsync(areaOfCompetencyId.Value))!.Name
                };
            }

            var purposeOfAppointmentIds = cabDocument.ScopeOfAppointments
                .Where(a => a.LegislativeAreaId == legislativeAreaId)
                .Where(a => a.PurposeOfAppointmentId.HasValue)
                .Select(p => p.PurposeOfAppointmentId!.Value).ToList();
            foreach (var pId in purposeOfAppointmentIds)
            {
                vm.CabLegislativeAreas.PurposeOfAppointments.Add(new ValueTuple<Guid, string>
                {
                    Item1 = pId,
                    Item2 = (await _legislativeAreaService
                        .GetPurposeOfAppointmentByIdAsync(pId))!.Name
                });
            }

            foreach (var soa in cabDocument.ScopeOfAppointments)
            {
                if (soa.LegislativeAreaId == legislativeAreaId)
                {
                    await AddPpeProductTypesToVm(vm, soa);
                    await AddProtectionAgainstRisksToVm(vm, soa);
                    await AddAreaOfCompetenciesToVm(vm, soa);                   
                    vm.CabLegislativeAreas.ScopeOfAppointmentId = soa.Id;
                }
            }

            if (!purposeOfAppointmentIds.Any())
            {
                //Get Next Level Categories
                await GetCategoriesAsync(legislativeAreaId, null, cabDocument, vm);

                if (!vm.CabLegislativeAreas.Categories.Any())
                {
                    await GetProductsAsync(legislativeAreaId, null, null, null, cabDocument, vm);
                    if (!vm.CabLegislativeAreas.Products.Any())
                    {
                        await GetProceduresAsync(legislativeAreaId, null, null, null, null, null, null, null, null, cabDocument, vm);
                    }
                }
            }

            vm.CabLegislativeAreas.PurposeOfAppointment = new ValueTuple<Guid?, string?>
            {
                Item1 = purposeOfAppointmentId ?? null,
                Item2 = purposeOfAppointmentId.HasValue
                    ? (await _legislativeAreaService
                        .GetPurposeOfAppointmentByIdAsync(purposeOfAppointmentId.Value))?.Name
                    : null
            };
            vm.CabLegislativeAreas.Category = new ValueTuple<Guid?, string?>
            {
                Item1 = categoryId ?? null,
                Item2 = categoryId.HasValue
                    ? (await _legislativeAreaService
                        .GetCategoryByIdAsync(categoryId.Value))?.Name
                    : null
            };
            vm.CabLegislativeAreas.SubCategory = new ValueTuple<Guid?, string?>
            {
                Item1 = subCategoryId ?? null,
                Item2 = subCategoryId.HasValue
                    ? (await _legislativeAreaService
                        .GetSubCategoryByIdAsync(subCategoryId.Value))?.Name
                    : null
            };
            vm.CabLegislativeAreas.Product = new ValueTuple<Guid?, string?>
            {
                Item1 = productId ?? null,
                Item2 = productId.HasValue
                    ? (await _legislativeAreaService
                        .GetProductByIdAsync(productId.Value))?.Name
                    : null
            };

            return View(vm);
        }

        private async Task AddPpeProductTypesToVm(CABProfileViewModel vm, DocumentScopeOfAppointment soa)
        {
            if (soa.PpeProductTypeIds.Count == 0)
                return;

            foreach (var ppeProdTypeId in soa.PpeProductTypeIds)
            {
                vm.CabLegislativeAreas.PpeProductTypes.Add(new ValueTuple<Guid, string>
                {
                    Item1 = ppeProdTypeId,
                    Item2 = (await _legislativeAreaService
                .GetPpeProductTypeByIdAsync(ppeProdTypeId))!.Name
                });
            }
        }

        private async Task AddProtectionAgainstRisksToVm(CABProfileViewModel vm, DocumentScopeOfAppointment soa)
        {
            if (soa.ProtectionAgainstRiskIds.Count == 0)
                return;

            foreach (var proctectionAgainstRiskId in soa.ProtectionAgainstRiskIds)
            {
                vm.CabLegislativeAreas.ProtectionAgainstRisks.Add(new ValueTuple<Guid, string>
                {
                    Item1 = proctectionAgainstRiskId,
                    Item2 = (await _legislativeAreaService
                .GetProtectionAgainstRiskByIdAsync(proctectionAgainstRiskId))!.Name
                });
            }
        }
        
        private async Task AddAreaOfCompetenciesToVm(CABProfileViewModel vm, DocumentScopeOfAppointment soa)
        {
            if (soa.AreaOfCompetencyIds.Count == 0)
                return;

            foreach (var areaOfCompetenciesId in soa.AreaOfCompetencyIds)
            {
                vm.CabLegislativeAreas.AreaOfCompetencies.Add(new ValueTuple<Guid, string>
                {
                    Item1 = areaOfCompetenciesId,
                    Item2 = (await _legislativeAreaService
                .GetAreaOfCompetencyByIdAsync(areaOfCompetenciesId))!.Name
                });
            }
        }

        private async Task GetProtectionAgainstRiskAsync(Guid? scopeOfAppointmentId, Document cabDocument, CABProfileViewModel vm)
        {
            Guid? protectionAgainstRiskId = cabDocument.ScopeOfAppointments
                .Where(soa => soa.Id == scopeOfAppointmentId)
                .First().ProtectionAgainstRiskId;

            vm.CabLegislativeAreas.ProtectionAgainstRisks.Add( new ValueTuple<Guid?, string>
            {
                Item1 = protectionAgainstRiskId,
                Item2 = (await _legislativeAreaService
                        .GetProtectionAgainstRiskByIdAsync(protectionAgainstRiskId.Value))!.Name
            });
        }

        private async Task GetAreaOfCompetenciesAsync(Guid? scopeOfAppointmentId, Document cabDocument, CABProfileViewModel vm)
        {
            var areaOfCompetencyIds = cabDocument.ScopeOfAppointments
                .Where(soa => soa.Id == scopeOfAppointmentId)
                .SelectMany(soa => soa.AreaOfCompetencyIds).ToList(); 

            foreach (var id in areaOfCompetencyIds)
            {
                vm.CabLegislativeAreas.AreaOfCompetencies.Add(new ValueTuple<Guid, string>
                {
                    Item1 = id,
                    Item2 = (await _legislativeAreaService
                        .GetAreaOfCompetencyByIdAsync(id))!.Name
                });
            }
        }
        private async Task GetCategoriesAsync(Guid? legislativeId, Guid? purposeOfAppointmentId, Document cabDocument,
            CABProfileViewModel vm)
        {
            IEnumerable<Guid> catIds;
            if (purposeOfAppointmentId.HasValue)
            {
                catIds = cabDocument.ScopeOfAppointments
                    .Where(s => s.PurposeOfAppointmentId != null &&                                
                    s.PurposeOfAppointmentId == purposeOfAppointmentId && (s.CategoryId != null || s.CategoryIds.Any()))
                    .SelectMany(s => s.CategoryId != null ? new List<Guid> { s.CategoryId.Value } : s.CategoryIds).ToList();
            }
            else
            {
                catIds = cabDocument.ScopeOfAppointments
                    .Where(s => s.LegislativeAreaId == legislativeId && (s.CategoryId != null || s.CategoryIds.Any()))
                    .SelectMany(s => s.CategoryId != null ? new List<Guid> { s.CategoryId.Value } : s.CategoryIds).ToList();
            }


            foreach (var id in catIds)
            {
                vm.CabLegislativeAreas.Categories.Add(new ValueTuple<Guid, string>
                {
                    Item1 = id,
                    Item2 = (await _legislativeAreaService
                        .GetCategoryByIdAsync(id))!.Name
                });
            }
        }

        private async Task GetProductsAsync(Guid? legislativeAreaId, Guid? purposeOfAppointmentId, Guid? categoryId,
            Guid? subCategoryId,
            Document cabDocument,
            CABProfileViewModel vm)
        {
            IEnumerable<Guid> prodIds;
            if (purposeOfAppointmentId.HasValue)
            {
                prodIds = cabDocument.ScopeOfAppointments
                    .Where(s => s.PurposeOfAppointmentId != null &&
                                s.PurposeOfAppointmentId == purposeOfAppointmentId && s.ProductIds.Any())
                    .Select(s => s.ProductIds).SelectMany(p => p).ToList();
            }
            else if (categoryId.HasValue)
            {
                prodIds = cabDocument.ScopeOfAppointments
                    .Where(s => s.CategoryId != null &&
                                s.CategoryId == categoryId && s.ProductIds.Any()).Select(s => s.ProductIds)
                    .SelectMany(p => p).ToList();
            }
            else if (subCategoryId.HasValue)
            {
                prodIds = cabDocument.ScopeOfAppointments
                    .Where(s => s.SubCategoryId != null &&
                                s.SubCategoryId == subCategoryId && s.ProductIds.Any()).Select(s => s.ProductIds)
                    .SelectMany(p => p).ToList();
            }
            else
            {
                prodIds = cabDocument.ScopeOfAppointments
                    .Where(s =>
                        s.LegislativeAreaId == legislativeAreaId && s.ProductIds.Any()).Select(s => s.ProductIds)
                    .SelectMany(p => p).ToList();
            }

            foreach (var prodId in prodIds)
            {
                vm.CabLegislativeAreas.Products.Add(new ValueTuple<Guid, string>
                {
                    Item1 = prodId,
                    Item2 = (await _legislativeAreaService
                        .GetProductByIdAsync(prodId))!.Name
                });
            }
        }

        private async Task GetProceduresAsync(Guid? legislativeAreaId, Guid? purposeOfAppointmentId, Guid? categoryId,
            Guid? subCategoryId,
            Guid? productId, Guid? areaOfCompetencyId, Guid? scopeOfAppointmentId, Guid? ppeProductTypeId, Guid? protectionAgainstRiskId, Document cabDocument,
            CABProfileViewModel vm)
        {
            IEnumerable<DocumentScopeOfAppointment> scopeOfAppointments;
            if (purposeOfAppointmentId.HasValue)
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.PurposeOfAppointmentId != null &&
                                s.PurposeOfAppointmentId == purposeOfAppointmentId && 
                                (s.ProductIdAndProcedureIds.Count != 0 || s.CategoryIdAndProcedureIds.Count != 0));
            }
            else if (categoryId.HasValue)
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.CategoryId != null || s.CategoryIds!= null &&
                                (s.CategoryId == categoryId || (s.CategoryIds?.Contains(categoryId.Value) ?? false)) && s.ProductIdAndProcedureIds.Any());
            }
            else if (subCategoryId.HasValue)
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.SubCategoryId != null &&
                                s.SubCategoryId == subCategoryId && s.ProductIdAndProcedureIds.Any());
            }
            else if (productId.HasValue)
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.ProductIds.Contains(productId!.Value) && s.ProductIdAndProcedureIds.Any());
            }
            else if (ppeProductTypeId.HasValue)
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.PpeProductTypeIds.Contains(ppeProductTypeId!.Value) && s.PpeProductTypeIdAndProcedureIds.Any());
            }
            else if (protectionAgainstRiskId.HasValue)
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.ProtectionAgainstRiskIds.Contains(protectionAgainstRiskId!.Value) && s.ProtectionAgainstRiskIdAndProcedureIds.Any());
            }
            else if (areaOfCompetencyId.HasValue)
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.AreaOfCompetencyIds.Contains(areaOfCompetencyId!.Value) && s.AreaOfCompetencyIdAndProcedureIds.Any());
            }
            else
            {
                scopeOfAppointments = cabDocument.ScopeOfAppointments
                    .Where(s => s.LegislativeAreaId == legislativeAreaId && s.ProductIdAndProcedureIds.Any());
            }

            IEnumerable<Guid> procIds;
            if (productId.HasValue || scopeOfAppointments.Any(soa => soa.ProductIdAndProcedureIds.Count != 0))
            {
                procIds = scopeOfAppointments.ToList()
                    .Select(s => s.ProductIdAndProcedureIds)
                    .SelectMany(pp => pp)
                    .Where(i => i.ProductId == productId)
                    .SelectMany(pr => pr.ProcedureIds);
            }
            else if (categoryId.HasValue || scopeOfAppointments.Any(soa => soa.CategoryIdAndProcedureIds.Count != 0))
            {
                procIds = scopeOfAppointments.ToList()
                    .Select(s => s.CategoryIdAndProcedureIds)
                    .SelectMany(cp => cp)
                    .SelectMany(pr => pr.ProcedureIds);
            }
            else if(ppeProductTypeId.HasValue || scopeOfAppointments.Any(soa =>  soa.PpeProductTypeIdAndProcedureIds.Count != 0))
            {
                procIds = scopeOfAppointments
                    .SelectMany(s => s.PpeProductTypeIdAndProcedureIds)
                    .Where(ppt => ppt.PpeProductTypeId == ppeProductTypeId)                 
                    .SelectMany(pr => pr.ProcedureIds);                
            }
            else if(protectionAgainstRiskId.HasValue || scopeOfAppointments.Any(soa =>  soa.ProtectionAgainstRiskIdAndProcedureIds.Count != 0))
            {
                procIds = scopeOfAppointments
                    .SelectMany(s => s.ProtectionAgainstRiskIdAndProcedureIds)
                    .Where(par => par.ProtectionAgainstRiskId == protectionAgainstRiskId)                              
                    .SelectMany(pr => pr.ProcedureIds);                
            }
            else
            {
                procIds = scopeOfAppointments.Where(soa => soa.Id == scopeOfAppointmentId).ToList()
                    .SelectMany(s => s.AreaOfCompetencyIdAndProcedureIds)
                    .Where(aoc => aoc.AreaOfCompetencyId == areaOfCompetencyId)                 
                    .SelectMany(pr => pr.ProcedureIds);                
            }
            
            foreach (var procId in procIds)
            {
                vm.CabLegislativeAreas.Procedures.Add(new ValueTuple<Guid, string>
                {
                    Item1 = procId,
                    Item2 = (await _legislativeAreaService
                        .GetProcedureByIdAsync(procId))!.Name
                });
            }
        }

        #region profileDraft

        [HttpGet("profile/draft/{id:guid}", Name = Routes.CabDraftProfile), Authorize]
        public async Task<IActionResult> DraftAsync(string id, int pagenumber = 1)
        {
            var cabDocument = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(id);

            if (cabDocument != null)
            {
                var cab = await GetCabProfileViewModel(cabDocument, null, true, false, pagenumber);
                return View("Index", cab);
            }
            else
            {
                return NotFound();
            }
        }

        #endregion

        #region profileFeed

        [HttpGet("search/cab-profile-feed/{id}", Name = Routes.CabFeed)]
        public async Task<IActionResult> Feed(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);
            if (cabDocument == null)
            {
                throw new NotFoundException($"The CAB with the following CAB url count not be found: {id}");
            }

            var feed = _feedService.GetSyndicationFeed(cabDocument.Name, Request, cabDocument, Url);

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                Indent = true,
                ConformanceLevel = ConformanceLevel.Document
            };

            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    feed.GetAtom10Formatter().WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }

                stream.Position = 0;
                var content = new StreamReader(stream).ReadToEnd();
                return Content(content, "application/atom+xml;charset=utf-8");
            }
        }

        #endregion

        private async Task<CABProfileViewModel> GetCabProfileForIndex(Document cabDocument, string? returnUrl,
            int pagenumber = 1)
        {
            var userAccount = User.Identity is { IsAuthenticated: true }
                ? await _userService.GetAsync(
                    User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value)
                : null;
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : default;

            var unarchiveRequests = await _workflowTaskService.GetByCabIdAsync(
                cabDocument.CABId.ToGuid()!.Value,
                new List<TaskType> { TaskType.RequestToUnarchiveForDraft, TaskType.RequestToUnarchiveForPublish });
            var unpublishRequests = await _workflowTaskService.GetByCabIdAsync(
                cabDocument.CABId.ToGuid()!.Value,
                new List<TaskType> { TaskType.RequestToArchive, TaskType.RequestToUnpublish });
            var requireApproval = userAccount != null && !string.Equals(userAccount.Role, Roles.OPSS.Id);

            var cab = await GetCabProfileViewModel(
                cabDocument,
                returnUrl,
                userAccount != null,
                requireApproval && (!unarchiveRequests.Any() || unarchiveRequests.All(t => t.Completed)),
                pagenumber,
                requireApproval && (!unpublishRequests.Any() || unpublishRequests.All(t => t.Completed))
            );

            cab = await GetRequestInformation(cab);
            return cab;
        }

        private async Task<CABProfileViewModel> GetCabProfileViewModel(Document cabDocument, string? returnUrl,
            bool loggedIn = false, bool showRequestToUnarchive = false, int pagenumber = 1,
            bool showRequestToUnpublish = false)
        {
            var isArchived = cabDocument.StatusValue == Status.Archived;
            var auditLogOrdered = cabDocument.AuditLog.OrderBy(a => a.DateTime).ToList();

            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : default;

            var isUnarchivedRequest =
                auditLogOrdered.Last().Action == AuditCABActions.UnarchivedToDraft;
            var isPublished = cabDocument.StatusValue == Status.Published;
            var archiveAudit =
                isArchived ? auditLogOrdered.Last(al => al.Action == AuditCABActions.Archived) : null;
            var publishedAudit = auditLogOrdered.FirstOrDefault(al => al.Action == AuditCABActions.Published);

            var fullHistory = await _cachedPublishedCabService.FindAllDocumentsByCABIdAsync(cabDocument.CABId);
            var hasDraft = fullHistory.Any(d => d.StatusValue == Status.Draft);

            var history = new AuditLogHistoryViewModel(fullHistory, pagenumber, loggedIn);

            var cab = new CABProfileViewModel
            {
                HasUserAccount = loggedIn,
                IsArchived = isArchived,
                IsUnarchivedRequest = isUnarchivedRequest,
                ShowRequestToUnarchive = showRequestToUnarchive,
                ShowRequestToUnpublish = showRequestToUnpublish,
                IsPublished = isPublished,
                HasDraft = hasDraft,
                ArchivedBy = isArchived && archiveAudit != null ? archiveAudit.UserName : string.Empty,
                ArchivedDate = isArchived && archiveAudit != null
                    ? archiveAudit.DateTime.ToStringBeisFormat()
                    : string.Empty,
                ArchiveReason = isArchived && archiveAudit != null ? archiveAudit.Comment : string.Empty,
                AuditLogHistory = history,
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : WebUtility.UrlDecode(returnUrl),
                IsOPSSUser = User.IsInRole(Roles.OPSS.Id),
                CABId = cabDocument.CABId,
                CABUrl = cabDocument.URLSlug,
                PublishedDate = publishedAudit?.DateTime ?? null,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                AppointmentDate = cabDocument.AppointmentDate,
                ReviewDate = cabDocument.RenewalDate,
                UKASReferenceNumber = cabDocument.UKASReference,
                Address = cabDocument.GetAddressArray().ToList(),
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                PointOfContactName = cabDocument.PointOfContactName ?? string.Empty,
                PointOfContactEmail = cabDocument.PointOfContactEmail ?? string.Empty,
                PointOfContactPhone = cabDocument.PointOfContactPhone ?? string.Empty,
                IsPointOfContactPublicDisplay = cabDocument.IsPointOfContactPublicDisplay,
                BodyNumber = cabDocument.CABNumber,
                PreviousBodyNumbers = cabDocument.PreviousCABNumbers,
                CabNumberVisibility = cabDocument.CabNumberVisibility,
                BodyTypes = cabDocument.BodyTypes,
                MRACountries = cabDocument.MRACountries,
                RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                RegisteredTestLocations = cabDocument.TestingLocations,
                Status = cabDocument.Status,
                SubStatus = cabDocument.SubStatus.GetEnumDescription(),
                StatusCssStyle = CssClassUtils.CabStatusStyle(cabDocument.StatusValue),
                LegislativeAreas = cabDocument.DocumentLegislativeAreas.Select(l => l.LegislativeAreaName).ToList(),
                ProductSchedules = new CABDocumentsViewModel
                {
                    Id = "product-schedules",
                    Title = "Product schedules",
                    CABId = cabDocument.CABId,
                    Documents = cabDocument.Schedules?.Select(s => new FileUpload
                    {
                        Label = s.Label ?? s.FileName,
                        Category = s.Category,
                        FileName = s.FileName,
                        BlobName = s.BlobName,
                        LegislativeArea = s.LegislativeArea,
                        Archived = s.Archived,
                        CreatedBy = s.CreatedBy,
                    }).ToList() ?? new List<FileUpload>(),
                    DocumentType = DataConstants.Storage.Schedules
                },
                SupportingDocuments = new CABDocumentsViewModel
                {
                    Id = "supporting-documents",
                    Title = "Supporting documents",
                    CABId = cabDocument.CABId,
                    Documents = loggedIn
                        ? cabDocument.Documents?.Select(s => new FileUpload
                        {
                            Label = s.Label ?? s.FileName,
                            FileName = s.FileName,
                            BlobName = s.BlobName,
                            Category = s.Category,
                            Publication = s.Publication
                        }).ToList() ?? new List<FileUpload>()
                        : cabDocument.PublicDocuments().Select(f => new FileUpload
                        {
                            Label = f.Label ?? f.FileName,
                            FileName = f.FileName,
                            BlobName = f.BlobName,
                            Category = f.Category,
                            Publication = f.Publication
                        }).ToList(),
                    DocumentType = DataConstants.Storage.Documents,
                },
                GovernmentUserNotes = new UserNoteListViewModel(new Guid(cabDocument.id),
                    cabDocument.GovernmentUserNotes, pagenumber)
                {
                    CabHasDraft = hasDraft
                },
                FeedLinksViewModel = new FeedLinksViewModel
                {
                    FeedUrl = Url.RouteUrl(Routes.CabFeed, new { id = cabDocument.CABId }),
                    EmailUrl = Url.RouteUrl(
                        Subscriptions.Controllers.SubscriptionsController.Routes.Step0RequestCabSubscription,
                        new { id = cabDocument.CABId }),
                    CABName = cabDocument.Name
                }
            };

            ShareUtils.AddDetails(HttpContext, cab.FeedLinksViewModel);

            var listCabLegislateArea =
                await GetCABLegislativeAreasAsync(cabDocument.DocumentLegislativeAreas.Where(la => la.Status == LAStatus.Published));
            cab.CabLegislativeAreas = new CABLegislativeAreasModel
            {
                CabUrl = cab.CABUrl,
                ActiveLegislativeAreas = listCabLegislateArea.Where(l => !l.IsArchived).ToList(),
                ArchivedLegislativeAreas = listCabLegislateArea.Where(l => l.IsArchived).ToList()
            };

            return cab;
        }

        private async Task<List<LegislativeAreaViewModel>> GetCABLegislativeAreasAsync(
            IEnumerable<DocumentLegislativeArea> documentLegislativeAreas)
        {
            var allLegislativeAreas = (await _legislativeAreaService.GetAllLegislativeAreasAsync()).ToList();

            var legislativeAreasList = documentLegislativeAreas.Select(x => new LegislativeAreaViewModel
            {
                LegislativeAreaId = x.LegislativeAreaId,
                Name = allLegislativeAreas.Single(y => y.Id == x.LegislativeAreaId).Name,
                Regulation = allLegislativeAreas.Single(y => y.Id == x.LegislativeAreaId).Regulation,
                HasDataModel = allLegislativeAreas.Single(y => y.Id == x.LegislativeAreaId).HasDataModel,
                IsProvisional = x.IsProvisional != null && x.IsProvisional.Value,
                IsArchived = x.Archived != null && x.Archived.Value,
                AppointmentDate = x.AppointmentDate,
                ReviewDate = x.ReviewDate,
                Reason = x.Reason,
                PointOfContactName = x.PointOfContactName,
                PointOfContactEmail = x.PointOfContactEmail,
                PointOfContactPhone = x.PointOfContactPhone,
                IsPointOfContactPublicDisplay = x.IsPointOfContactPublicDisplay,
                
            }).ToList();

            return legislativeAreasList;
        }

        private async Task<CABProfileViewModel> GetRequestInformation(CABProfileViewModel profileViewModel)
        {
            var tasks = await _workflowTaskService.GetByCabIdAsync(Guid.Parse(profileViewModel.CABId));
            var task = tasks.FirstOrDefault(t =>
                new List<TaskType>
                {
                    TaskType.RequestToUnarchiveForDraft,
                    TaskType.RequestToUnarchiveForPublish,
                    TaskType.RequestToArchive,
                    TaskType.RequestToUnpublish
                }.Contains(t.TaskType) &&
                !t.Completed);

            int? summaryBreak = task?.Body.Length > 60
                ? task.Body[..60].LastIndexOf(" ", StringComparison.Ordinal)
                : null;
            profileViewModel.RequestFirstAndLastName = task?.Submitter.FirstAndLastName;
            profileViewModel.RequestUserGroup = task?.Submitter.UserGroup;
            profileViewModel.RequestReasonSummary =
                summaryBreak.HasValue ? task?.Body.TruncateWithEllipsis(summaryBreak.Value) : null;
            profileViewModel.RequestReason = task?.Body;
            profileViewModel.RequestTaskType = task?.TaskType;
            return profileViewModel;
        }

        #region ArchiveCAB

        [HttpGet, Route("search/archive-cab/{cabUrl}"), Authorize(Policy = Policies.ApproveRequests)]
        public async Task<IActionResult> ArchiveCAB(string cabUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(cabUrl);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {cabUrl}");
            if (cabDocument.StatusValue != Status.Published)
            {
                return RedirectToAction("Index", new { url = cabUrl });
            }

            var draft = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(cabDocument.CABId);
            return View(new ArchiveCABViewModel
            {
                CABId = cabDocument.CABId,
                Name = cabDocument.Name,
                HasDraft = draft != null
            });
        }

        [HttpPost, Route("search/archive-cab/{cabUrl}"), Authorize(Policy = Policies.ApproveRequests)]
        public async Task<IActionResult> ArchiveCAB(string cabUrl, ArchiveCABViewModel model)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(cabUrl);

            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {cabUrl}");
            if (ModelState.IsValid)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                        .Value);
                //Get draft before archiving deletes it
                var draft = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(cabDocument.CABId);
                await _cabAdminService.ArchiveDocumentAsync(userAccount, cabDocument.CABId,
                    model.ArchiveInternalReason,
                    model.ArchivePublicReason);
                _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                {
                    [AiTracking.Metadata.CabId] = cabDocument.CABId,
                    [AiTracking.Metadata.CabName] = cabDocument.Name
                }));

                if (draft != null)
                {
                    await SendDraftCabDeletedNotification(userAccount, cabDocument,
                        draft.AuditLog.First(al => al.Action == AuditCABActions.Created).UserId);
                }

                return RedirectToAction("Index", new { id = cabDocument.URLSlug });
            }

            model.Name = cabDocument.Name ?? string.Empty;
            return View(model);
        }

        /// <summary>
        /// Sends a notification when archived and an associated draft is deleted.
        /// </summary>
        /// <param name="archiverAccount"></param>
        /// <param name="cabDocument"></param>
        /// <param name="draftUserId"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task SendDraftCabDeletedNotification(UserAccount archiverAccount, Document cabDocument,
            string draftUserId)
        {
            var draftCreator = await _userService.GetAsync(draftUserId);
            if (draftCreator == null)
            {
                return;
            }

            await SendEmailForDeleteDraftAsync(cabDocument.Name!, draftCreator.EmailAddress!);

            var archiverRoleId = Roles.RoleId(archiverAccount.Role) ?? throw new InvalidOperationException();

            var archiver = new User(archiverAccount.Id, archiverAccount.FirstName, archiverAccount.Surname,
                archiverRoleId, archiverAccount.EmailAddress ?? throw new InvalidOperationException());

            var cabCreatorRoleId = Roles.RoleId(draftCreator.Role) ?? throw new InvalidOperationException();

            var assignee = new User(draftCreator.Id, draftCreator.FirstName, draftCreator.Surname,
                cabCreatorRoleId, draftCreator.EmailAddress ?? throw new InvalidOperationException());

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    TaskType.DraftCabDeletedFromArchiving,
                    archiver,
                    assignee.RoleId,
                    assignee,
                    DateTime.Now,
                    $"The draft record for {cabDocument.Name} has been deleted because the CAB profile was archived. Contact UKMCAB support if you need the draft record to be added to the service again.",
                    archiver,
                    DateTime.Now,
                    false,
                    null,
                    true,
                    Guid.Parse(cabDocument.CABId)));
        }

        private async Task SendEmailForDeleteDraftAsync(string cabName, string receiverEmailAddress)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "CABName", cabName },
                {
                    "ContactSupportURL", UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(FooterController.Routes.ContactUs))
                },
                {
                    "NotificationsURL", UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(Admin.Controllers.NotificationController.Routes.Notifications))
                }
            };

            await _notificationClient.SendEmailAsync(receiverEmailAddress,
                _templateOptions.NotificationDraftCabDeletedFromArchiving, personalisation);
        }

        #endregion

        #region UnarchiveCAB

        [HttpGet, Route("search/unarchive-cab/{id}"), Authorize(Policy = Policies.ApproveRequests)]
        public async Task<IActionResult> UnarchiveCAB(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(id);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {id}");
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : default;
            if (cabDocument.StatusValue != Status.Archived)
            {
                return RedirectToAction("Index", new { url = id, returnUrl });
            }

            return View(new UnarchiveCABViewModel
            {
                CABId = id,
                CABName = cabDocument.Name,
                ReturnURL = returnUrl
            });
        }

        [HttpPost]
        [Route("search/unarchive-cab/{id}"), Authorize(Policy = Policies.ApproveRequests)]
        public async Task<IActionResult> UnarchiveCAB(string id, UnarchiveCABViewModel model)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(id);
            Guard.IsTrue(cabDocument != null, $"No archived document found for CAB URL: {id}");
            if (ModelState.IsValid)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                        .Value);
                await _cabAdminService.UnarchiveDocumentAsync(userAccount, cabDocument.CABId,
                    model.UnarchiveInternalReason, model.UnarchivePublicReason, false, true);
                _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                {
                    [AiTracking.Metadata.CabId] = id,
                    [AiTracking.Metadata.CabName] = cabDocument.Name
                }));
                return RedirectToAction("Summary", "CAB", new { area = "Admin", id = cabDocument.CABId });
            }

            model.CABName = cabDocument.Name;

            return View(model);
        }

        #endregion

        #region SubscriptionAndSchedule

        /// <summary>
        /// CAB data API used by the Email Subscriptions Core
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("~/__api/subscriptions/core/cab/{id}")]
        public async Task<IActionResult> GetCabAsync(string id)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);

            if (cabDocument != null)
            {
                var auditLogOrdered = cabDocument.AuditLog.OrderBy(a => a.DateTime).ToList();
                var publishedAudit = auditLogOrdered.Last(al => al.Action == AuditCABActions.Published);
                var cab = new SubscriptionsCoreCabModel
                {
                    CABId = cabDocument.CABId,
                    PublishedDate = publishedAudit.DateTime,
                    LastModifiedDate = cabDocument.LastUpdatedDate,
                    Name = cabDocument.Name,
                    UKASReferenceNumber = string.Empty,
                    Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2,
                        cabDocument.TownCity, cabDocument.Postcode, cabDocument.Country),
                    Website = cabDocument.Website,
                    Email = cabDocument.Email,
                    Phone = cabDocument.Phone,
                    BodyNumber = cabDocument.CABNumber,
                    BodyTypes = cabDocument.BodyTypes,
                    RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                    RegisteredTestLocations = cabDocument.TestingLocations,
                    LegislativeAreas = cabDocument.DocumentLegislativeAreas.Select(l => l.LegislativeAreaName).ToList(),
                    ProductSchedules = cabDocument.Schedules?.Select(pdf => new SubscriptionsCoreCabFileModel
                    {
                        BlobName = pdf.BlobName,
                        FileName = pdf.FileName
                    }).ToList() ?? new List<SubscriptionsCoreCabFileModel>(),
                };
                return Json(cab);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("search/cab-schedule-download/{id}")]
        public async Task<IActionResult> Download(string id, string file, string filetype)
        {
            var fileStream = await _fileStorage.DownloadBlobStream($"{id}/{filetype}/{file}");
            if (fileStream != null)
            {
                return File(fileStream.FileStream, fileStream.ContentType, file);
            }

            return Ok("File does not exist");
        }

        [HttpGet("search/cab-schedule-view/{id}")]
        public async Task<IActionResult> View(string id, string file, string filetype)
        {
            var fileStream = await _fileStorage.DownloadBlobStream($"{id}/{filetype}/{file}");
            if (fileStream != null)
            {
                return File(fileStream.FileStream, fileStream.ContentType);
            }

            return Ok("File does not exist");
        }

        [HttpGet("search/cab-schedule-view/replaced-file/{id}")]
        public async Task<IActionResult> ViewReplacedFile(string id, string file, string filetype)
        {
            var fileStream = await _fileStorage.DownloadBlobStream($"{id}/{filetype}/replaced_files/{file}");
            if (fileStream != null)
            {
                return File(fileStream.FileStream, fileStream.ContentType);
            }

            return Ok("File does not exist");
        }

        #endregion

        #region CabHistory

        [HttpGet("search/cab/history-details")]
        public async Task<IActionResult> ShowCABHistoryDetails(Guid tempId)
        {
            var auditHistoryItemViewModel = await _distCache.GetAsync<AuditHistoryItemViewModel>(string.Format(CacheKey, tempId.ToString()));

            if (auditHistoryItemViewModel == null) 
            {
                // If not found, e.g. user refreshes screen after an hour, redirect to root.
                return Redirect("/");
            }

            return View(auditHistoryItemViewModel);
        }

        [HttpPost("search/cab/history-details", Name = Routes.CabProfileHistoryDetails)]
        public async Task<IActionResult> CABHistoryDetails(AuditHistoryItemViewModel auditHistoryItemViewModel)
        {
            var tempId = Guid.NewGuid();
            await _distCache.SetAsync(string.Format(CacheKey, tempId.ToString()), auditHistoryItemViewModel, TimeSpan.FromHours(1));

            return RedirectToAction(nameof(CABHistoryDetails), new { tempId });
        }

        #endregion
    }
}