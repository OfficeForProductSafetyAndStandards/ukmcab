namespace UKMCAB.Core.Domain
{
    public record FileUpload(Guid Id, string Label, string? LegislativeArea, string? Category, string FileName, string BlobName, DateTime UploadDateTime, bool? Archived = false);
}
