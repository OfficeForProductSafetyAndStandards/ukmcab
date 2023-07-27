using System.Diagnostics.CodeAnalysis;

namespace UKMCAB.Data.Models
{
    public class FileUpload 
    {
        public string Label { get; set; }
        public string? LegislativeArea { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public DateTime UploadDateTime { get; set; }
    }

    public class FileUploadComparer : IEqualityComparer<FileUpload>
    {
        public bool Equals(FileUpload? x, FileUpload? y)
        {
            return (x == null && y == null)
                   || (x != null
                       && y != null
                       && x.BlobName.Equals(y.BlobName)
                       && x.Label.Equals(y.Label)
                       && x.FileName.Equals(y.FileName)
                       && ((x.LegislativeArea == null && y.LegislativeArea == null)
                           || (x.LegislativeArea?.Equals(y.LegislativeArea) ?? false))
                       && x.UploadDateTime.Date.Equals(y.UploadDateTime.Date));
        }

        public int GetHashCode([DisallowNull] FileUpload obj)
        {
            return obj.BlobName.GetHashCode();
        }
    }
}
