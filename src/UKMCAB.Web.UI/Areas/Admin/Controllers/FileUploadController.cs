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
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }
            var cab = cabs.First();
            if (cab.CABData.Schedules != null && cab.CABData.Schedules.Count >= 5)
            {
                return RedirectToAction("SchedulesList", new { id });
            }

            var model = new FileUploadViewModel()
            {
                Title = SchedulesOptions.UploadTitle,
                UploadedFiles = cab.CABData.Schedules?.Select(s => s.FileName).ToList() ?? new List<string>(),
                Id = id
            };
            return View(model);
        }

        private ActionResult? ValidCABDocument(string id, List<Document> cabs)
        {
            if (cabs == null || !cabs.Any())
            {
                return NotFound();
            }

            if (cabs.Count > 1 || cabs.First().State != State.Created)
            {
                return BadRequest();
            }

            return null;
        }

        [HttpPost]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, FileUploadViewModel model)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }
            var cab = cabs.First();
            cab.CABData.Schedules ??= new List<FileUpload>();
            if (cab.CABData.Schedules.Count >= 5)
            {
                return RedirectToAction("SchedulesList", new { id });
            }

            ValidateUploadFile(model, SchedulesOptions.AcceptedFileExtensions, SchedulesOptions.AcceptedFileTypes, cab);

            if (ModelState.IsValid)
            {
                var result = await _fileStorage.UploadCABFile(cab.CABData.CABId, model.File.FileName, "schedules",
                    model.File.OpenReadStream());
                cab.CABData.Schedules.Add(result);

                var user = await _userManager.GetUserAsync(User);
                if (await _cabAdminService.UpdateCABAsync(user.Email, cab))
                {
                    return RedirectToAction("SchedulesList", new { id = cab.CABData.CABId });
                }

                ModelState.AddModelError("File", "There was an error updating the CAB record.");
            }

            model.Title = SchedulesOptions.UploadTitle;
            model.UploadedFiles = cab.CABData.Schedules.Select(s => s.FileName).ToList();
            model.Id = id;
            return View(model);
        }

        private void ValidateUploadFile(FileUploadViewModel model, string[] acceptedFileExtension, string acceptedFileTypes, Document cab)
        {
            if (model.File == null)
            {
                ModelState.AddModelError("File", "Please select a file for upload.");
            }
            else
            {
                if (model.File.Length > 10485760)
                {
                    ModelState.AddModelError("File", "Files must be no more that 10Mb in size.");
                }

                if (acceptedFileExtension.All(ext => ext != Path.GetExtension(model.File.FileName).ToLowerInvariant()))
                {
                    ModelState.AddModelError("File", $"Files must be in {acceptedFileTypes} format to be uploaded.");
                }

                cab.CABData.Schedules ??= new List<FileUpload>();

                if (cab.CABData.Schedules.Any(s => s.FileName.Equals(model.File.FileName)))
                {
                    ModelState.AddModelError("File", "Uploaded files must have different names to those already uploaded.");
                }
            }
        }


        [HttpGet]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }

            var cab = cabs.First();
            if (cab.CABData.Schedules == null || !cab.CABData.Schedules.Any())
            {
                return RedirectToAction("SchedulesUpload", new[] { id = cab.CABData.CABId });
            }

            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = cab.CABData.Schedules.Select(s => s.FileName).ToList(),
                Id = id
            });
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id, string fileName)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }

            var cab = cabs.First();
            cab.CABData.Schedules ??= new List<FileUpload>();

            var fileToRemove = cab.CABData.Schedules.SingleOrDefault(s =>
                s.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (fileToRemove != null)
            {
                var result = await _fileStorage.DeleteCABSchedule(fileToRemove.BlobName);
                // Even if this returns false because the file wasn't found we still want to remove it from the document
                cab.CABData.Schedules.Remove(fileToRemove);
                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateCABAsync(user.Email, cab);
            }

            if (!cab.CABData.Schedules.Any())
            {
                return RedirectToAction("SchedulesUpload", new { id = cab.CABData.CABId });
            }

            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = cab.CABData.Schedules.Select(s => s.FileName).ToList(),
                Id = id
            });
        }

        [HttpGet]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }
            var cab = cabs.First();
            cab.CABData.Documents ??= new List<FileUpload>();
            if (cab.CABData.Documents.Count >= 10)
            {
                return RedirectToAction("DocumentsList", new { id });
            }

            var model = new FileUploadViewModel()
            {
                Title = DocumentsOptions.UploadTitle,
                UploadedFiles = cab.CABData.Documents.Select(s => s.FileName).ToList(),
                Id = id
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id, FileUploadViewModel model)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }
            var cab = cabs.First();
            cab.CABData.Documents ??= new List<FileUpload>();
            if (cab.CABData.Documents.Count >= 10)
            {
                return RedirectToAction("DocumentsList", new { id });
            }

            ValidateUploadFile(model, DocumentsOptions.AcceptedFileExtensions, DocumentsOptions.AcceptedFileTypes, cab);

            if (ModelState.IsValid)
            {
                var result = await _fileStorage.UploadCABFile(cab.CABData.CABId, model.File.FileName, "documents",
                    model.File.OpenReadStream());
                cab.CABData.Documents.Add(result);

                var user = await _userManager.GetUserAsync(User);
                if (await _cabAdminService.UpdateCABAsync(user.Email, cab))
                {
                    return RedirectToAction("DocumentsList", new { id = cab.CABData.CABId });
                }

                ModelState.AddModelError("File", "There was an error updating the CAB record.");
            }

            model.Title = DocumentsOptions.UploadTitle;
            model.UploadedFiles = cab.CABData.Documents.Select(s => s.FileName).ToList();
            model.Id = id;
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }

            var cab = cabs.First();
            cab.CABData.Documents ??= new List<FileUpload>();

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = cab.CABData.Documents.Select(s => s.FileName).ToList(),
                Id = id
            });
        }

        [HttpPost]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, string fileName)
        {
            var cabs = await _cabAdminService.FindCABDocumentsByIdAsync(id);
            var redirect = ValidCABDocument(id, cabs);
            if (redirect != null)
            {
                return redirect;
            }

            var cab = cabs.First();
            cab.CABData.Documents ??= new List<FileUpload>();

            var fileToRemove = cab.CABData.Documents.SingleOrDefault(s =>
                s.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (fileToRemove != null)
            {
                var result = await _fileStorage.DeleteCABSchedule(fileToRemove.BlobName);
                // Even if this returns false because the file wasn't found we still want to remove it from the document
                cab.CABData.Documents.Remove(fileToRemove);
                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateCABAsync(user.Email, cab);
            }

            if (!cab.CABData.Documents.Any())
            {
                return RedirectToAction("DocumentsUpload", new { id = cab.CABData.CABId });
            }

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = cab.CABData.Documents.Select(s => s.FileName).ToList(),
                Id = id
            });
        }

    }
}
