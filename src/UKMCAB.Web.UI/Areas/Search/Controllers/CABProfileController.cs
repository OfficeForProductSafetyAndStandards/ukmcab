using System.Net;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Services;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using Microsoft.ApplicationInsights;
using UKMCAB.Data;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABProfileController : Controller
    {
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly TelemetryClient _telemetryClient;

        public static class Routes
        {
            public const string CabDetails = "cab.detail";
        }

        public CABProfileController(ICachedPublishedCABService cachedPublishedCabService, ICABAdminService cabAdminService, IFileStorage fileStorage, 
            UserManager<UKMCABUser> userManager, TelemetryClient telemetryClient)
        {
            _cachedPublishedCabService = cachedPublishedCabService;
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _userManager = userManager;
            _telemetryClient = telemetryClient;
        }

        [HttpGet("search/cab-profile/{id}", Name = Routes.CabDetails)]
        public async Task<IActionResult> Index(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLAsync(id);
            if (cabDocument != null && !id.Equals(cabDocument.URLSlug))
            {
                return RedirectToActionPermanent("Index", new { id = cabDocument.URLSlug, returnUrl});
            }

            if (cabDocument == null || (cabDocument.StatusValue == Status.Archived && !User.Identity.IsAuthenticated))
            {
                throw new NotFoundException($"The CAB with the following CAB url cound not be found: {id}");
            }

            var user = await _userManager.GetUserAsync(User);
            var isOPSSUer = user != null && await _userManager.IsInRoleAsync(user, Constants.Roles.OPSSAdmin);

            var cab = GetCabProfileViewModel(cabDocument, isOPSSUer, returnUrl);

            _telemetryClient.TrackEvent(AiTracking.Events.CabViewed, HttpContext.ToTrackingMetadata(new() 
            { 
                [AiTracking.Metadata.CabId] = id, 
                [AiTracking.Metadata.CabName] = cab.Name 
            }));

            return View(cab);
        }

        private CABProfileViewModel GetCabProfileViewModel(Document cabDocument, bool isOPSSUer, string returnUrl)
        {
            var isArchived = cabDocument.StatusValue == Status.Archived;
            var cab = new CABProfileViewModel
            {
                IsLoggedIn = isOPSSUer,
                IsArchived = isArchived,
                ArchivedBy = isArchived ? cabDocument.Archived.UserName : string.Empty,
                ArchivedDate = isArchived ? cabDocument.Archived.DateTime.ToString("dd MMM yyyy") : string.Empty,
                ArchiveReason = cabDocument.ArchivedReason,
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/search" : WebUtility.UrlDecode(returnUrl),
                CABId = cabDocument.CABId,
                PublishedDate = cabDocument.Published.DateTime,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                UKASReferenceNumber = string.Empty,
                Address = cabDocument.GetAddress(),
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                PointOfContactName = cabDocument.PointOfContactName ?? string.Empty,
                PointOfContactEmail = cabDocument.PointOfContactEmail ?? string.Empty,
                PointOfContactPhone = cabDocument.PointOfContactPhone ?? string.Empty,
                IsPointOfContactPublicDisplay = cabDocument.IsPointOfContactPublicDisplay,
                BodyNumber = cabDocument.CABNumber,
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
                        BlobName = s.BlobName
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
                        BlobName = s.BlobName
                    }).ToList() ?? new List<FileUpload>(),
                    DocumentType = DataConstants.Storage.Documents
                }
            };
            return cab;
        }

        [HttpPost]
        [Route("search/cab-profile/{id}")]
        public async Task<IActionResult> Index(string id, string? returnUrl, string? ArchiveReason)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLAsync(id);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {id}");
            var user = await _userManager.GetUserAsync(User);
            var isOPSSUer = user != null && await _userManager.IsInRoleAsync(user, Constants.Roles.OPSSAdmin);
            if (!string.IsNullOrWhiteSpace(ArchiveReason))
            {
                await _cabAdminService.ArchiveDocumentAsync(user, cabDocument, ArchiveReason);
                _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                {
                    [AiTracking.Metadata.CabId] = id,
                    [AiTracking.Metadata.CabName] = cabDocument.Name
                }));
                return RedirectToAction("Index", new { url = id, returnUrl });
            }
            ModelState.AddModelError("ArchiveReason", "State the reason for archiving this CAB record");

            var cab = GetCabProfileViewModel(cabDocument, isOPSSUer, returnUrl);

            return View(cab);
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
                var cab = new SubscriptionsCoreCabModel
                {
                    CABId = cabDocument.CABId,
                    PublishedDate = cabDocument.Published.DateTime,
                    LastModifiedDate = cabDocument.LastUpdatedDate,
                    Name = cabDocument.Name,
                    UKASReferenceNumber = string.Empty,
                    Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2, cabDocument.TownCity, cabDocument.Postcode, cabDocument.Country),
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
