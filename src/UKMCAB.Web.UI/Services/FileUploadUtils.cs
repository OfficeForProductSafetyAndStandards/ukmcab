using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UKMCAB.Web.UI.Services
{
    public class FileUploadUtils: IFileUploadUtils
    {
        public string GetContentType(IFormFile? file, Dictionary<string, string> acceptedFileExtensionsContentTypes)
        {
            if (file == null)
            {
                return string.Empty;
            }

            return acceptedFileExtensionsContentTypes.SingleOrDefault(ct =>
                    ct.Key.Equals(Path.GetExtension(file.FileName), StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        public List<FileUpload> GetSelectedFilesFromLatestDocumentOrReturnEmptyList(IEnumerable<FileViewModel> filesSelectedByUser, List<FileUpload> uploadedFilesInLatestDocument)
        {
            List<FileUpload> fileUploadsInLatestDocumentMatchingSelectedFiles = new();

            if (filesSelectedByUser.Any())
            {
                foreach (var selectedFile in filesSelectedByUser)
                {
                    if (!selectedFile.IsDuplicated)
                    {
                        fileUploadsInLatestDocumentMatchingSelectedFiles.Add(uploadedFilesInLatestDocument[selectedFile.FileIndex]);
                    }
                }
            }

            return fileUploadsInLatestDocumentMatchingSelectedFiles;
        }

        public Document RemoveSelectedUploadedFilesFromDocumentAsync(List<FileUpload> selectedFileUploads, Document latestDocument, string docType)
        {
            if (latestDocument.Schedules != null && docType.Equals(nameof(latestDocument.Schedules)))
            {
                foreach (var fileToRemove in selectedFileUploads)
                {
                    latestDocument.Schedules.Remove(fileToRemove);
                }
            }
            else if (latestDocument.Documents != null && docType.Equals(nameof(latestDocument.Documents)))
            {
                foreach (var fileToRemove in selectedFileUploads)
                {
                    latestDocument.Documents.Remove(fileToRemove);
                }
            }
            return latestDocument;
        }

        public bool ValidateUploadFileAndAddAnyModelStateError(ModelStateDictionary modelState, IFormFile? file, string contentType, string acceptedFileTypes)
        {
            var isValidFile = true;

            if (file == null)
            {
                modelState.AddModelError("File", $"Select a {acceptedFileTypes} file 10 megabytes or less.");
                return false;
            }
            else
            {
                if (file.Length > 10485760)
                {
                    modelState.AddModelError("File", $"{file.FileName} can't be uploaded. Select a {acceptedFileTypes} file 10 megabytes or less.");
                    isValidFile = false;
                }

                if (string.IsNullOrWhiteSpace(contentType))
                {
                    modelState.AddModelError("File", $"{file.FileName} can't be uploaded. Files must be in {acceptedFileTypes} format to be uploaded.");
                    isValidFile = false;
                }
            }
            return isValidFile;
        }

        public List<FileUpload> GetSelectedFilesFromLatestDocumentByIds(List<Guid> FileIds, List<FileUpload>? uploadedFiles)
        {
            if (FileIds != null && FileIds.Any())
            {
                return uploadedFiles.Where(n => FileIds.Contains(n.Id)).ToList();
            }

            return new();
        }
    }
}
