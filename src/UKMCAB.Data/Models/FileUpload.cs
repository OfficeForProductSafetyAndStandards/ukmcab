using System.Diagnostics.CodeAnalysis;

namespace UKMCAB.Data.Models
{
    public class FileUpload 
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public string? LegislativeArea { get; set; }
        public string? Category { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public DateTime UploadDateTime { get; set; }
        public bool? Archived { get; set; }
        public string? Publication { get; set; }

        public bool IsPublic => !(Archived ?? false) && (Publication?.Equals(DataConstants.Publications.Public) ?? false);
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
                       && ((x.Category == null && y.Category == null)
                           || (x.Category?.Equals(y.Category) ?? false))
                       && x.UploadDateTime.Date.Equals(y.UploadDateTime.Date)
                       && ((x.Publication == null && y.Publication == null)
                           || (x.Publication?.Equals(y.Publication) ?? false)));
        }

        public int GetHashCode([DisallowNull] FileUpload obj)
        {
            return obj.BlobName.GetHashCode();
        }
    }
}
