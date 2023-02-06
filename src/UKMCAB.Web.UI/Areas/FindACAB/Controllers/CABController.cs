using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Web.UI.Models.ViewModels.FindACAB;

namespace UKMCAB.Web.UI.Areas.FindACAB.Controllers
{
    [Area("findacab")]
    public class CABController : Controller
    {
        private readonly IFileStorage _fileStorage;

        public CABController(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage;
        }

        [Route("find-a-cab/cab-profile/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var cab = new CABProfileViewModel
            {
                CABId = id,
                PublishedDate = new DateTime(2020, 12, 3),
                LastModifiedDate = new DateTime(2019, 10, 31),
                Name = "CATG Ltd",
                UKASReferenceNumber = id == "0" ? string.Empty : "12345-6789",
                AddressLine1 = "29a Prince Crescent",
                AddressLine2 = string.Empty,
                TownCity = "Morecambe",
                Postcode = "LA4 6BY",
                Country = "United Kingdom",
                Website = id == "0" ? string.Empty : "catg.co.uk",
                Email = id == "0" ? string.Empty : "info@catg.co.uk",
                Phone = id == "0" ? string.Empty : "+44 (0) 1542 400632",
                RegisteredOfficeLocations = new List<string>
                {
                    "France",
                    "Italy",
                    "United Kingdom"
                },
                RegisteredTestLocations = new List<string>
                {
                    "France",
                    "Italy",
                    "United Kingdom"
                },
                BodyNumber = "1245",
                BodyTypes = new List<string>
                {
                    "Approved body",
                    "NI Notified body"
                },
                LegislativeAreas = new List<string>
                {
                    "Construction products"
                },
                ProductSchedules = new List<FileUpload>
                {
                    new()
                    {
                        BlobName = "9e521c41-e511-4099-9c8e-07891cc23f4d/schedules/20230125111150-css-cheat-sheet-v1.pdf",
                        FileName = "20230125111150-css-cheat-sheet-v1.pdf"
                    },
                    new()
                    {
                        BlobName = "9e521c41-e511-4099-9c8e-07891cc23f4d/schedules/20230125113639-regular-expressions-cheat-sheet-v2.pdf",
                        FileName = "20230125113639-regular-expressions-cheat-sheet-v2.pdf"
                    },
                    new()
                    {
                        BlobName = "9e521c41-e511-4099-9c8e-07891cc23f4d/schedules/20230126114543-html-cheat-sheet-v1.pdf",
                        FileName = "20230126114543-html-cheat-sheet-v1.pdf"
                    },
                    new()
                    {
                        BlobName = "9e521c41-e511-4099-9c8e-07891cc23f4d/schedules/20230127022229-html-character-entities-cheat-sheet.pdf",
                        FileName = "20230127022229-html-character-entities-cheat-sheet.pdf"
                    }
                }
            };
            return View(cab);
        }

        [HttpGet]
        [Route("find-a-cab/cab-schedule-download/{id}")]
        public async Task<IActionResult> Download(string id, string file, string type)
        {
            var fileStream = await _fileStorage.DownloadBlobStream($"{id}/{type}/{file}");
            if (fileStream != null)
            {
                return File(fileStream.FileStream, fileStream.ContentType, file);
            }

            return Ok("File does not exist");
        }
    }
}
