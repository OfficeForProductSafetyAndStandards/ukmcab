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
    }
}
