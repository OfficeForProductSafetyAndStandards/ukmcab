using System.Diagnostics.CodeAnalysis;

namespace UKMCAB.Data.Models
{
    public class FileUpload : IEquatable<FileUpload>
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public string? LegislativeArea { get; set; }
        public string? Category { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public DateTime UploadDateTime { get; set; }
        public bool? Archived { get; set; }
        
        public override bool Equals(object? obj) => Equals(obj as FileUpload);

        public bool Equals(FileUpload? other)
        {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.GetType() != GetType()) return false;

            return other is not null &&
                Label == other.Label &&
                LegislativeArea == other.LegislativeArea &&
                Category == other.Category &&
                FileName == other.FileName &&
                BlobName == other.BlobName &&
                UploadDateTime == other.UploadDateTime;
        }
        public override int GetHashCode() => 
            (Label, LegislativeArea, Category, FileName, BlobName, UploadDateTime).GetHashCode();

        public static bool operator ==(FileUpload upload, FileUpload other)
        {
            if (upload is null)
            {
                if (other is null)
                {
                    return true;
                }
                return false;
            }
            return upload.Equals(other);
        }

        public static bool operator !=(FileUpload upload, FileUpload other) => !(upload == other);
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
                       && x.UploadDateTime.Date.Equals(y.UploadDateTime.Date));
        }

        public int GetHashCode([DisallowNull] FileUpload obj)
        {
            return obj.BlobName.GetHashCode();
        }
    }
}
