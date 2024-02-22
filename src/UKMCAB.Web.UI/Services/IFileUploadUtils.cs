using Microsoft.AspNetCore.Mvc.ModelBinding;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Services
{
    public interface IFileUploadUtils
    {
        string GetContentType(IFormFile? file, Dictionary<string, string> acceptedFileExtensionsContentTypes);
        List<FileUpload> GetSelectedFilesFromLatestDocumentOrReturnEmptyList(IEnumerable<FileViewModel> selectedViewModels, List<FileUpload> uploadedFiles);
        Document RemoveSelectedUploadedFilesFromDocumentAsync(List<FileUpload> selectedFileUploads, Document latestDocument, string docType);
        bool ValidateUploadFileAndAddAnyModelStateError(ModelStateDictionary modelState, IFormFile? file, string contentType, string acceptedFileTypes);

        List<FileUpload> GetSelectedFilesFromLatestDocumentByIds(List<Guid> FileIds, List<FileUpload>? uploadedFiles);
    }
}
