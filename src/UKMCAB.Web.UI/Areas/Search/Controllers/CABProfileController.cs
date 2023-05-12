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

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABProfileController : Controller
    {
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly UserManager<UKMCABUser> _userManager;

        public static class Routes
        {
            public const string CabDetails = "cab.detail";
        }

        public CABProfileController(ICachedPublishedCABService cachedPublishedCabService, ICABAdminService cabAdminService, IFileStorage fileStorage, UserManager<UKMCABUser> userManager)
        {
            _cachedPublishedCabService = cachedPublishedCabService;
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _userManager = userManager;
        }

        [HttpGet("search/cab-profile/{id}", Name = Routes.CabDetails)]
        public async Task<IActionResult> Index(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);
            if (cabDocument == null && User.Identity.IsAuthenticated)
            {
                var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
                cabDocument = documents.SingleOrDefault(d => d.StatusValue == Status.Archived);
            }

            if (cabDocument == null)
            {
                throw new NotFoundException($"The CAB with the following CABId cound not be found: {id}");
            }

            var user = await _userManager.GetUserAsync(User);
            var opssUser = user != null && await _userManager.IsInRoleAsync(user, Constants.Roles.OPSSAdmin);
            var isArchived = cabDocument.StatusValue == Status.Archived;
            var cab = new CABProfileViewModel
            {
                IsLoggedIn = opssUser,
                IsArchived = isArchived,
                ArchivedBy = isArchived ? cabDocument.Archived.UserName : string.Empty,
                ArchivedDate = isArchived ? cabDocument.Archived.DateTime.ToString("dd MMM yyyy") : string.Empty,
                ArchiveReason = cabDocument.ArchivedReason,
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/search" : WebUtility.UrlDecode(returnUrl),
                CABId = cabDocument.CABId,
                PublishedDate = cabDocument.Published.DateTime,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                UKASReferenceNumber =  string.Empty,
                Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2, cabDocument.TownCity, cabDocument.Postcode, cabDocument.Country),
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                BodyNumber = cabDocument.CABNumber,
                BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                ProductSchedules = cabDocument.Schedules?.Select(pdf => new FileUpload
                {
                    BlobName = pdf.BlobName,
                    FileName = pdf.FileName
                }).ToList() ?? new List<FileUpload>() 
            };
            return View(cab);
        }

        [HttpPost]
        [Route("search/cab-profile/{id}")]
        public async Task<IActionResult> Index(string id, string? returnUrl, string? ArchiveReason)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB Id {id}");
            var user = await _userManager.GetUserAsync(User);
            var opssUser = user != null && await _userManager.IsInRoleAsync(user, Constants.Roles.OPSSAdmin);
            if (!string.IsNullOrWhiteSpace(ArchiveReason))
            {
                cabDocument = await _cabAdminService.ArchiveDocumentAsync(user, cabDocument, ArchiveReason);
            }
            else
            {
                ModelState.AddModelError("ArchiveReason", "State the reason for archiving this CAB record");
            }

            var isArchived = cabDocument.StatusValue == Status.Archived;
            var cab = new CABProfileViewModel
            {
                IsLoggedIn = opssUser,
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
                Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2, cabDocument.TownCity, cabDocument.Postcode, cabDocument.Country),
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                BodyNumber = cabDocument.CABNumber,
                BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                ProductSchedules = cabDocument.Schedules?.Select(pdf => new FileUpload
                {
                    BlobName = pdf.BlobName,
                    FileName = pdf.FileName
                }).ToList() ?? new List<FileUpload>()
            };
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
        public async Task<IActionResult> Download(string id, string file)
        {
            var fileStream = await _fileStorage.DownloadBlobStream($"{id}");
            if (fileStream != null)
            {
                return File(fileStream.FileStream, fileStream.ContentType, file);
            }

            return Ok("File does not exist");
        }
    }
}
