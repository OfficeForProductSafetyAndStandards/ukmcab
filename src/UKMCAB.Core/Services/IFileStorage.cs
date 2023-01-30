using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public interface IFileStorage
    {
        Task<FileUpload> UploadCABFile(string cabId, string FileName, string DirectoryName, Stream stream);

        Task<bool> DeleteCABSchedule(string blobName);
    }
}
