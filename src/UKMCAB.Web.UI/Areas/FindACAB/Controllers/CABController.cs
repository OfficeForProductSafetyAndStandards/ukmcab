using UKMCAB.Core.Models;
using UKMCAB.Web.UI.Models.ViewModels.FindACAB;

namespace UKMCAB.Web.UI.Areas.FindACAB.Controllers
{
    [Area("findacab")]
    public class CABController : Controller
    {
        [Route("find-a-cab/cab-profile/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var cab = new CABProfileViewModel
            {
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
                LegislativeBodies = new List<string>
                {
                    "Construction products"
                },
                ProductSchedules = new List<FileUpload>
                {
                    new()
                    {
                        BlobName = "2134/schedules/test1.pdf",
                        FileName = "test1.pdf"
                    },
                    new()
                    {
                        BlobName = "2134/schedules/test2.pdf",
                        FileName = "test2.pdf"
                    }
                }
            };
            return View(cab);
        }
    }
}
