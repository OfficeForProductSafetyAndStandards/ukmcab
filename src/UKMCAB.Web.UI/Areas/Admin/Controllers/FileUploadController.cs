using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin},{Constants.Roles.UKASUser}")]
    public class FileUploadController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly IFileStorage _fileStorage;

        public FileUploadController(ICABAdminService cabAdminService, UserManager<UKMCABUser> userManager,
        IFileStorage fileStorage)
        {
            _cabAdminService = cabAdminService;
            _userManager = userManager;
            _fileStorage = fileStorage;
        }

        [HttpGet]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id)
        {

            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            if (cabs == null || !cabs.Any())
            {
                return NotFound();
            }

            if (cabs.Count > 1 || cabs.First().State != State.Created)
            {
                return BadRequest();
            }

            var cab = cabs.First();
            if (cab.CABData.Schedules != null && cab.CABData.Schedules.Count >= 5)
            {
                return RedirectToAction("SchedulesList", new { id = id });
            }

            var model = new SchedulesUploadViewModel
            {
                UploadedFiles = cab.CABData.Schedules?.Select(s => s.FileName).ToList() ?? new List<string>(),
                Id = id
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, SchedulesUploadViewModel model)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            if (cabs == null || !cabs.Any())
            {
                return NotFound();
            }

            if (cabs.Count > 1 || cabs.First().State != State.Created)
            {
                return BadRequest();
            }

            var cab = cabs.First();
            if (cab.CABData.Schedules == null)
            {
                cab.CABData.Schedules = new List<FileUpload>();
            }
            else if (cab.CABData.Schedules.Count >= 5)
            {
                return RedirectToAction("SchedulesList", new { id = id });
            }

            if (model.Files == null || !model.Files.Any())
            {
                ModelState.AddModelError("Files", "Please select a file for upload.");
            }
            else
            {
                if (model.Files.Count > 5)
                {
                    ModelState.AddModelError("Files", "A maximum of 5 files can be selected for upload at a time.");
                }

                if (model.Files.Any(f => f.Length > 10485760))
                {
                    ModelState.AddModelError("Files", "Files must be no more that 10Mb in size.");
                }

                if (model.Files.Any(f => Path.GetExtension(f.FileName).ToLowerInvariant() != ".pdf"))
                {
                    ModelState.AddModelError("Files", "Files must be in PDF format to be uploaded.");
                }

                if (model.Files.Any(f => cab.CABData.Schedules.Any(s => s.FileName.Equals(f.FileName))))
                {
                    ModelState.AddModelError("Files", "Uploaded files must have different names to those already uploaded.");
                }
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                foreach (var formFile in model.Files)
                {
                    if (cab.CABData.Schedules.Any(s =>
                            s.FileName.Equals(formFile.FileName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    var result = await _fileStorage.UploadCABSchedule(cab.CABData.CABId, formFile.FileName,
                        formFile.OpenReadStream());
                    cab.CABData.Schedules.Add(result);
                }

                if (await _cabAdminService.UpdateCABAsync(user.Email, cab))
                {
                    return RedirectToAction("SchedulesList", new { id = cab.CABData.CABId });
                }

                ModelState.AddModelError("Files", "There was an error updating the CAB record.");
            }

            model.UploadedFiles = cab.CABData.Schedules.Select(s => s.FileName).ToList();
            model.Id = id;
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            if (cabs == null || !cabs.Any())
            {
                return NotFound();
            }

            if (cabs.Count > 1 || cabs.First().State != State.Created)
            {
                return BadRequest();
            }

            var cab = cabs.First();

            if (cab.CABData.Schedules == null || !cab.CABData.Schedules.Any())
            {
                return RedirectToAction("SchedulesUpload", new[] { id = cab.CABData.CABId });
            }

            return View(new SchedulesListViewModel
            {
                UploadedFiles = cab.CABData.Schedules.Select(s => s.FileName).ToList(),
                Id = id
            });
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id, string fileName)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            if (cabs == null || !cabs.Any())
            {
                return NotFound();
            }

            if (cabs.Count > 1 || cabs.First().State != State.Created)
            {
                return BadRequest();
            }

            var cab = cabs.First();

            if (cab.CABData.Schedules == null || !cab.CABData.Schedules.Any())
            {
                return RedirectToAction("SchedulesUpload", new[] { id = cab.CABData.CABId });
            }

            var fileToRemove = cab.CABData.Schedules.SingleOrDefault(s =>
                s.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (fileToRemove != null)
            {
                var result = await _fileStorage.DeleteCABSchedule(fileToRemove.BlobName);
                // Even if this returns false because the file wasn't found we still want to remove it from the document
                //if (result)
                //{
                    cab.CABData.Schedules.Remove(fileToRemove);
                    var user = await _userManager.GetUserAsync(User);
                    await _cabAdminService.UpdateCABAsync(user.Email, cab);
                //}
            }

            return View(new SchedulesListViewModel
            {
                UploadedFiles = cab.CABData.Schedules.Select(s => s.FileName).ToList(),
                Id = id
            });
        }
    }
}
