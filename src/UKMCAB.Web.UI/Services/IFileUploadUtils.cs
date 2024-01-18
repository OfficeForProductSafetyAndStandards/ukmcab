namespace UKMCAB.Web.UI.Services
{
    public interface IFileUploadUtils
    {
        string GetContentType(IFormFile? file, Dictionary<string, string> acceptedFileExtensionsContentTypes);
    }
}
