using UKMCAB.Data.Models;
using UKMCAB.Core.Services;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Data.Storage;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABController : Controller
    {
        private readonly ICachedPublishedCabService _cabAdminService;
        private readonly IFileStorage _fileStorage;

        public CABController(ICachedPublishedCabService cabAdminService, IFileStorage fileStorage)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
        }

        [HttpGet("search/cab-profile/{id}")]
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
