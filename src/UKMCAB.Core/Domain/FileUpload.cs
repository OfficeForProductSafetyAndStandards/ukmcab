namespace UKMCAB.Core.Domain
{
    public record FileUpload(string Label, string? LegislativeArea, string? Category, string FileName, string BlobName, DateTime UploadDateTime);
}
