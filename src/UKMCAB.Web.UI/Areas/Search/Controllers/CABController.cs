using UKMCAB.Data.Models;
using UKMCAB.Core.Services;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Data.Storage;
using UKMCAB.Subscriptions.Core.Integration.CabService;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABController : Controller
    {
        private readonly ICachedPublishedCabService _cabAdminService;
        private readonly IFileStorage _fileStorage;

        public static class Routes
        {
            public const string CabDetails = "cab.detail";
        }

        public CABController(ICachedPublishedCabService cabAdminService, IFileStorage fileStorage)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
        }

        [HttpGet("search/cab-profile/{id}", Name = Routes.CabDetails)]
        public async Task<IActionResult> Index(string id)
        {
            var cabDocument = await _cabAdminService.FindPublishedDocumentByCABIdAsync(id);

            var cab = new CABProfileViewModel
            {
                CABId = cabDocument.CABId,
                PublishedDate = cabDocument.PublishedDate,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                UKASReferenceNumber =  string.Empty,
                Address = cabDocument.Address,
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
            var cabDocument = await _cabAdminService.FindPublishedDocumentByCABIdAsync(id);

            if (cabDocument != null)
            {
                var cab = new SubscriptionsCoreCabModel
                {
                    CABId = cabDocument.CABId,
                    PublishedDate = cabDocument.PublishedDate,
                    LastModifiedDate = cabDocument.LastUpdatedDate,
                    Name = cabDocument.Name,
                    UKASReferenceNumber = string.Empty,
                    Address = cabDocument.Address,
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
