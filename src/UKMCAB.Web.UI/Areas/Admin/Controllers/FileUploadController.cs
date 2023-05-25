using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Data.Models;
using UKMCAB.Core.Services;
using UKMCAB.Data;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using UKMCAB.Data.Storage;
using Document = UKMCAB.Data.Models.Document;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin},{Constants.Roles.UKASUser}")]
    public class FileUploadController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly IFileStorage _fileStorage;

        public FileUploadController(ICABAdminService cabAdminService, UserManager<UKMCABUser> userManager, IFileStorage fileStorage)
        {
            _cabAdminService = cabAdminService;
            _userManager = userManager;
            _fileStorage = fileStorage;
        }

        [HttpGet]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            if (latestVersion.Schedules != null && latestVersion.Schedules.Count >= 5)
            {
                return RedirectToAction("SchedulesList", fromSummary ? new {id, fromSummary = "true"} : new { id });
            }

            var model = new FileUploadViewModel
            {
                Title = SchedulesOptions.UploadTitle,
                UploadedFiles = latestVersion.Schedules?.Select(s => s.FileName).ToList() ?? new List<string>(),
                CABId = id
            };
            model.IsFromSummary = fromSummary;
            model.DocumentStatus = latestVersion.StatusValue;
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, FileUploadViewModel model, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            latestVersion.Schedules ??= new List<FileUpload>();
            if (latestVersion.Schedules.Count >= 5)
            {
                return RedirectToAction("SchedulesList", model.IsFromSummary ? new { id, fromSummary = "true" }: new { id });
            }

            var contentType = ValidateUploadFileAndGetContentType(model, SchedulesOptions.AcceptedFileExtensionsContentTypes, SchedulesOptions.AcceptedFileTypes, latestVersion);

            if (ModelState.IsValid)
            {
                var result = await _fileStorage.UploadCABFile(latestVersion.CABId, model.File.FileName, model.File.FileName, DataConstants.Storage.Schedules,
                    model.File.OpenReadStream(), contentType);
                latestVersion.Schedules.Add(result);

                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion);
                return RedirectToAction("SchedulesList", model.IsFromSummary ? new { id = latestVersion.CABId, fromSummary = "true" } : new { id = latestVersion.CABId });
            }

            model.Title = SchedulesOptions.UploadTitle;
            model.UploadedFiles = latestVersion.Schedules.Select(s => s.FileName).ToList();
            model.CABId = id;
            model.IsFromSummary = fromSummary;
            model.DocumentStatus = latestVersion.StatusValue;
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/save-as-draft/{id}")]
        public async Task<IActionResult> SaveAsDraft(string id)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            if (latestVersion.StatusValue == Status.Created)
            {
                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion, true);
            }
            TempData[Constants.TempDraftKey] = $"Draft record saved for {latestVersion.Name} <br>CAB number {latestVersion.CABNumber}";
            return RedirectToAction("Index", "Admin", new { Area = "admin" });
        }

        private string ValidateUploadFileAndGetContentType(FileUploadViewModel model, Dictionary<string, string> acceptedFileExtensionsContentTypes, string acceptedFileTypes, Document document)
        {
            var contentType = string.Empty;
            if (model.File == null)
            {
                ModelState.AddModelError("File", $"Select a {acceptedFileTypes} file 10 megabytes or less.");
            }
            else
            {
                if (model.File.Length > 10485760)
                {
                    ModelState.AddModelError("File", "Files must be no more that 10Mb in size.");
                }

                contentType = acceptedFileExtensionsContentTypes.SingleOrDefault(ct =>
                    ct.Key.Equals(Path.GetExtension(model.File.FileName), StringComparison.InvariantCultureIgnoreCase)).Value;
                if (string.IsNullOrWhiteSpace(contentType))
                {
                    ModelState.AddModelError("File", $"Files must be in {acceptedFileTypes} format to be uploaded.");
                }

                document.Schedules ??= new List<FileUpload>();

                if (document.Schedules.Any(s => s.FileName.Equals(model.File.FileName)))
                {
                    ModelState.AddModelError("File", "Uploaded files must have different names to those already uploaded.");
                }
            }

            return contentType;
        }

        [HttpGet]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit

            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = latestVersion.Schedules?.Select(s => s.FileName).ToList() ?? new List<string>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id, string fileName, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestVersion.Schedules ??= new List<FileUpload>();

            var fileToRemove = latestVersion.Schedules.SingleOrDefault(s =>
                s.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (fileToRemove != null)
            {
                var result = await _fileStorage.DeleteCABSchedule(fileToRemove.BlobName);
                // Even if this returns false because the file wasn't found we still want to remove it from the document
                latestVersion.Schedules.Remove(fileToRemove);
                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion);
            }

            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = latestVersion.Schedules.Select(s => s.FileName).ToList(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpGet]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            latestVersion.Documents ??= new List<FileUpload>();
            if (latestVersion.Documents.Count >= 10)
            {
                return RedirectToAction("DocumentsList", fromSummary ? new { id, fromSummary = "true" } : new { id });
            }

            var model = new FileUploadViewModel()
            {
                Title = DocumentsOptions.UploadTitle,
                UploadedFiles = latestVersion.Documents.Select(s => s.FileName).ToList(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id, FileUploadViewModel model, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();
            if (latestVersion.Documents.Count >= 10)
            {
                return RedirectToAction("DocumentsList", model.IsFromSummary ? new { id, fromSummary = "true" } : new { id });
            }

            var contentType = ValidateUploadFileAndGetContentType(model, DocumentsOptions.AcceptedFileExtensionsContentTypes, DocumentsOptions.AcceptedFileTypes, latestVersion);

            if (ModelState.IsValid)
            {
                var result = await _fileStorage.UploadCABFile(latestVersion.CABId, model.File.FileName, model.File.FileName, DataConstants.Storage.Documents,
                    model.File.OpenReadStream(), contentType);
                latestVersion.Documents.Add(result);

                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion);
                return RedirectToAction("DocumentsList", model.IsFromSummary ? new { id = latestVersion.CABId, fromSummary = "true" } : new { id = latestVersion.CABId });
            }

            model.Title = DocumentsOptions.UploadTitle;
            model.UploadedFiles = latestVersion.Documents.Select(s => s.FileName).ToList();
            model.CABId = id;
            model.IsFromSummary = fromSummary;
            model.DocumentStatus = latestVersion.StatusValue;
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = latestVersion.Documents.Select(s => s.FileName).ToList(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, string fileName, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();

            var fileToRemove = latestVersion.Documents.SingleOrDefault(s =>
                s.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (fileToRemove != null)
            {
                var result = await _fileStorage.DeleteCABSchedule(fileToRemove.BlobName);
                // Even if this returns false because the file wasn't found we still want to remove it from the document
                latestVersion.Documents.Remove(fileToRemove);
                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion);
            }

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = latestVersion.Documents.Select(s => s.FileName).ToList(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

    }
}
