using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using System.Xml;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABProfileController : Controller
    {
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly TelemetryClient _telemetryClient;
        private readonly IFeedService _feedService;
        private readonly IUserService _userService;

        public static class Routes
        {
            public const string CabDetails = "cab.detail";
            public const string CabDraftProfile = "cab.profile.draft";
            public const string TrackInboundLinkCabDetails = "cab.details.inbound-email-link";
            public const string CabFeed = "cab.feed";
        }

        public CABProfileController(ICachedPublishedCABService cachedPublishedCabService,
            ICABAdminService cabAdminService, IFileStorage fileStorage, TelemetryClient telemetryClient,
            IFeedService feedService, IUserService userService)
        {
            _cachedPublishedCabService = cachedPublishedCabService;
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _telemetryClient = telemetryClient;
            _feedService = feedService;
            _userService = userService;
        }

        [HttpGet("~/__subscriptions/__inbound/cab/{id}", Name = Routes.TrackInboundLinkCabDetails)]
        public IActionResult TrackInboundLinkCabDetails(string id)
        {
            _telemetryClient.TrackEvent(AiTracking.Events.CabViewedViaSubscriptionsEmail,
                HttpContext.ToTrackingMetadata());
            return RedirectToRoute(Routes.CabDetails, new { id });
        }

        [HttpGet("search/cab-profile/{id}", Name = Routes.CabDetails)]
        public async Task<IActionResult> Index(string id, string? returnUrl, int pagenumber = 1)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLAsync(id);
            if (cabDocument != null && !id.Equals(cabDocument.URLSlug))
            {
                return RedirectToActionPermanent("Index", new { id = cabDocument.URLSlug, returnUrl });
            }

            if (cabDocument == null || (cabDocument.StatusValue == Status.Archived && !User.Identity.IsAuthenticated))
            {
                return NotFound();
            }

            var cab = await GetCabProfileViewModel(cabDocument, returnUrl, pagenumber);

            _telemetryClient.TrackEvent(AiTracking.Events.CabViewed, HttpContext.ToTrackingMetadata(new()
            {
                [AiTracking.Metadata.CabId] = id,
                [AiTracking.Metadata.CabName] = cab.Name
            }));

            return View(cab);
        }

        [HttpGet("profile/draft/{id:guid}", Name = Routes.CabDraftProfile), Authorize]
        public async Task<IActionResult> DraftAsync(string id)
        {
            var cabDocument = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(id);

            if (cabDocument != null)
            {
                var cab = await GetCabProfileViewModel(cabDocument, null, 0);
                return View("Index", cab);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("search/cab-profile-feed/{id}", Name = Routes.CabFeed)]
        public async Task<IActionResult> Feed(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);
            if (cabDocument == null)
            {
                throw new NotFoundException($"The CAB with the following CAB url cound not be found: {id}");
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

        private async Task<CABProfileViewModel> GetCabProfileViewModel(Document cabDocument, string returnUrl,
            int pagenumber = 1)
        {
            var isArchived = cabDocument.StatusValue == Status.Archived;
            var auditLogOrdered = cabDocument.AuditLog.OrderBy(a => a.DateTime).ToList();
            var isUnarchivedRequest = auditLogOrdered.Any(al => al.Action == AuditCABActions.UnarchiveRequest);
            var isPublished = cabDocument.StatusValue == Status.Published;
            var archiveAudit = isArchived ? auditLogOrdered.Last(al => al.Action == AuditCABActions.Archived) : null;
            var publishedAudit = auditLogOrdered.LastOrDefault(al => al.Action == AuditCABActions.Published);

            var fullHistory = await _cachedPublishedCabService.FindAllDocumentsByCABIdAsync(cabDocument.CABId);
            var hasDraft = fullHistory.Any(d => d.StatusValue == Status.Draft);

            var userAccount = User != null && User.Identity.IsAuthenticated
                ? await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value)
                : null;
            var history = new AuditLogHistoryViewModel(fullHistory, userAccount, pagenumber);

            var cab = new CABProfileViewModel
            {
                IsArchived = isArchived,
                IsUnarchivedRequest = isUnarchivedRequest,
                IsPublished = isPublished,
                HasDraft = hasDraft,
                ArchivedBy = isArchived && archiveAudit != null ? archiveAudit.UserName : string.Empty,
                ArchivedDate = isArchived && archiveAudit != null
                    ? archiveAudit.DateTime.ToString("dd MMM yyyy")
                    : string.Empty,
                ArchiveReason = isArchived && archiveAudit != null ? archiveAudit.Comment : string.Empty,
                AuditLogHistory = history,
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : WebUtility.UrlDecode(returnUrl),
                CABId = cabDocument.CABId,
                PublishedDate = publishedAudit?.DateTime ?? null,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                AppointmentDate = cabDocument.AppointmentDate,
                ReviewDate = cabDocument.RenewalDate,
                UKASReferenceNumber = cabDocument.UKASReference,
                Address = cabDocument.GetAddress(),
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                PointOfContactName = cabDocument.PointOfContactName ?? string.Empty,
                PointOfContactEmail = cabDocument.PointOfContactEmail ?? string.Empty,
                PointOfContactPhone = cabDocument.PointOfContactPhone ?? string.Empty,
                IsPointOfContactPublicDisplay = cabDocument.IsPointOfContactPublicDisplay,
                BodyNumber = cabDocument.CABNumber,
                CabNumberVisibility = cabDocument.CabNumberVisibility,
                BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                ProductSchedules = new CABDocumentsViewModel
                {
                    Id = "product-schedules",
                    Title = "Product schedules",
                    CABId = cabDocument.CABId,
                    Documents = cabDocument.Schedules?.Select(s => new FileUpload
                    {
                        Label = s.Label ?? s.FileName,
                        FileName = s.FileName,
                        BlobName = s.BlobName,
                        LegislativeArea = s.LegislativeArea
                    }).ToList() ?? new List<FileUpload>(),
                    DocumentType = DataConstants.Storage.Schedules
                },
                SupportingDocuments = new CABDocumentsViewModel
                {
                    Id = "supporting-documents",
                    Title = "Supporting documents",
                    CABId = cabDocument.CABId,
                    Documents = cabDocument.Documents?.Select(s => new FileUpload
                    {
                        Label = s.Label ?? s.FileName,
                        FileName = s.FileName,
                        BlobName = s.BlobName,
                        Category = s.Category
                    }).ToList() ?? new List<FileUpload>(),
                    DocumentType = DataConstants.Storage.Documents
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

            return cab;
        }

        [HttpGet]
        [Route("search/archive-cab/{id}")]
        public async Task<IActionResult> ArchiveCAB(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLAsync(id);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {id}");
            if (cabDocument.StatusValue != Status.Published)
            {
                return RedirectToAction("Index", new { url = id, returnUrl });
            }

            var draft = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(id);
            return View(new ArchiveCABViewModel
            {
                CABId = id,
                ReturnURL = returnUrl,
                HasDraft = draft != null
            });
        }

        [HttpPost]
        [Route("search/archive-cab/{id}")]
        public async Task<IActionResult> ArchiveCAB(string id, ArchiveCABViewModel model)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLAsync(id);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {id}");
            if (ModelState.IsValid)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.ArchiveDocumentAsync(userAccount, cabDocument.CABId, model.ArchiveReason);
                _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                {
                    [AiTracking.Metadata.CabId] = id,
                    [AiTracking.Metadata.CabName] = cabDocument.Name
                }));
                return RedirectToAction("Index", new { id = cabDocument.URLSlug, returnUrl = model.ReturnURL });
            }

            return View(model);
        }

        [HttpPost]
        [Route("search/cab-profile/archive/submit-js")]
        public async Task<IActionResult> ArchiveJs(string CABId, string ArchiveReason)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(CABId);
            if (cabDocument == null)
            {
                return Json(new JsonSubmitResult
                {
                    Submitted = false,
                    ErrorMessage = "No published CAB document was found."
                });
            }

            if (cabDocument.StatusValue == Status.Published && !string.IsNullOrWhiteSpace(ArchiveReason))
            {
                try
                {
                    var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value);
                    await _cabAdminService.ArchiveDocumentAsync(userAccount, cabDocument.CABId, ArchiveReason);
                    _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                    {
                        [AiTracking.Metadata.CabId] = CABId,
                        [AiTracking.Metadata.CabName] = cabDocument.Name
                    }));
                    return Json(new JsonSubmitResult
                    {
                        Submitted = true,
                        ErrorMessage = string.Empty
                    });
                }
                catch
                {
                    return Json(new JsonSubmitResult
                    {
                        Submitted = false,
                        ErrorMessage = "There was a problem submitting your archive request, please try again later."
                    });
                }
            }

            return Json(new JsonSubmitResult
            {
                Submitted = false,
                ErrorMessage = "There was a problem submitting your archive request, please try again later."
            });
        }

        [HttpGet]
        [Route("search/unarchive-cab/{id}")]
        public async Task<IActionResult> UnarchiveCAB(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLAsync(id);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {id}");
            if (cabDocument.StatusValue != Status.Archived)
            {
                return RedirectToAction("Index", new { url = id, returnUrl });
            }

            return View(new UnarchiveCABViewModel
            {
                CABId = id,
                ReturnURL = returnUrl
            });
        }

        [HttpPost]
        [Route("search/unarchive-cab/{id}")]
        public async Task<IActionResult> UnarchiveCAB(string id, UnarchiveCABViewModel model)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLAsync(id);
            Guard.IsTrue(cabDocument != null, $"No archived document found for CAB URL: {id}");
            if (ModelState.IsValid)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UnarchiveDocumentAsync(userAccount, cabDocument.CABId, model.UnarchiveReason);
                _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                {
                    [AiTracking.Metadata.CabId] = id,
                    [AiTracking.Metadata.CabName] = cabDocument.Name
                }));
                return RedirectToAction("Summary", "CAB", new { area = "Admin", id = cabDocument.CABId });
            }

            return View(model);
        }

        [HttpPost]
        [Route("search/cab-profile/unarchive/submit-js")]
        public async Task<IActionResult> UnarchiveJs(string CABId, string UnarchiveReason)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(CABId);
            if (cabDocument == null)
            {
                return Json(new JsonSubmitResult
                {
                    Submitted = false,
                    ErrorMessage = "No published CAB document was found."
                });
            }

            if (cabDocument.StatusValue == Status.Archived && !string.IsNullOrWhiteSpace(UnarchiveReason))
            {
                try
                {
                    var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value);
                    await _cabAdminService.UnarchiveDocumentAsync(userAccount, CABId, UnarchiveReason);
                    _telemetryClient.TrackEvent(AiTracking.Events.CabUnarchived, HttpContext.ToTrackingMetadata(new()
                    {
                        [AiTracking.Metadata.CabId] = CABId
                    }));
                    return Json(new JsonSubmitResult
                    {
                        Submitted = true,
                        ErrorMessage = string.Empty
                    });
                }
                catch
                {
                    return Json(new JsonSubmitResult
                    {
                        Submitted = false,
                        ErrorMessage = "There was a problem submitting your unarchive request, please try again later."
                    });
                }
            }

            return Json(new JsonSubmitResult
            {
                Submitted = false,
                ErrorMessage = "There was a problem submitting your unarchive request, please try again later."
            });
        }


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
                    BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                    RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                    RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                    LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
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
    }
}