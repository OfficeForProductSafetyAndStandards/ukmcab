using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Web.UI.Models.ViewModels.Search;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;

        public CABController(ICABAdminService cabAdminService, IFileStorage fileStorage)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
        }

        [Route("search/cab-profile/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var cabDocument = await _cabAdminService.FindCABDocumentByIdAsync(id);

            var cab = new CABProfileViewModel
            {
                CABId = cabDocument.Id,
                PublishedDate = cabDocument.PublishedDate.HasValue ? cabDocument.PublishedDate.Value.DateTime : null,
                LastModifiedDate = cabDocument.LastUpdatedDate.HasValue ? cabDocument.LastUpdatedDate.Value.DateTime : null,
                Name = cabDocument.Name,
                UKASReferenceNumber =  string.Empty,
                Address = cabDocument.Address,
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                BodyNumber = cabDocument.BodyNumber,
                BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                ProductSchedules = cabDocument.PDFs.Select(pdf => new FileUpload
                {
                    BlobName = pdf.BlobName,
                    FileName = pdf.ClientFileName
                }).ToList(), 
            };
            return View(cab);
        }

        [HttpGet]
        [Route("search/cab-schedule-download/{id}")]
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
