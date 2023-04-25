using Microsoft.AspNetCore.Identity;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Search;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABProfileController : Controller
    {
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly IFileStorage _fileStorage;
        private readonly UserManager<UKMCABUser> _userManager;

        public CABProfileController(ICachedPublishedCABService cachedPublishedCabService, IFileStorage fileStorage, UserManager<UKMCABUser> userManager)
        {
            _cachedPublishedCabService = cachedPublishedCabService;
            _fileStorage = fileStorage;
            _userManager = userManager;
        }

        [HttpGet("search/cab-profile/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);
            var user = await _userManager.GetUserAsync(User);
            var opssUser = user != null && await _userManager.IsInRoleAsync(user, Constants.Roles.OPSSAdmin);
            var cab = new CABProfileViewModel
            {
                IsLoggedIn = opssUser,
                CABId = cabDocument.CABId,
                PublishedDate = cabDocument.PublishedDate,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                UKASReferenceNumber =  string.Empty,
                Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2, cabDocument.TownCity, cabDocument.Postcode),
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

        [HttpGet("~/__api/cab/{id}")]
        public async Task<IActionResult> GetCabAsync(string id)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);

            if (cabDocument != null)
            {
                var cab = new CABProfileViewModel
                {
                    CABId = cabDocument.CABId,
                    PublishedDate = cabDocument.PublishedDate,
                    LastModifiedDate = cabDocument.LastUpdatedDate,
                    Name = cabDocument.Name,
                    UKASReferenceNumber = string.Empty,
                    Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2, cabDocument.TownCity, cabDocument.Postcode),
                    Website = cabDocument.Website,
                    Email = cabDocument.Email,
                    Phone = cabDocument.Phone,
                    BodyNumber = cabDocument.CABNumber,
                    BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                    RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                    RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                    LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                    ProductSchedules = cabDocument.Schedules.Select(pdf => new FileUpload
                    {
                        BlobName = pdf.BlobName,
                        FileName = pdf.FileName
                    }).ToList(),
                };
                return Json(cab);  // TODO: transform into models provided by the UKMCAB.Subscriptions.Core assembly
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
