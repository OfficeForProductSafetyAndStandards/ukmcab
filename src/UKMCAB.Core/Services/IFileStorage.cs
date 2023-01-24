using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public interface IFileStorage
    {
        Task<FileUpload> UploadCABSchedule(string cabId, string FileName, Stream stream);

        Task<bool> DeleteCABSchedule(string blobName);
    }
}
