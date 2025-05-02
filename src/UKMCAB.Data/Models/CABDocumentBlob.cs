namespace UKMCAB.Data.Models
{
    public class CABDocumentBlob
    {
        // Required for EntityFramework
        internal CABDocumentBlob() { }
        public CABDocumentBlob(Document document)
        {
            id = document.id;
            StatusValue = document.StatusValue;
            CABId = document.CABId;
            SubStatus = document.SubStatus;
            CreatedByUserGroup = document.CreatedByUserGroup;
            URLSlug = document.URLSlug;
            Name = document.Name;
            UKASReference = document.UKASReference;
            CABNumber = document.CABNumber;
            Version = document.Version;
            CabBlob = document;
        }
    
        public string id { get; set; } = string.Empty;
        public Status StatusValue { get; set; }
        public string CABId { get; set; } = string.Empty;
        public SubStatus SubStatus { get; set; }
        public string CreatedByUserGroup { get; set; } = string.Empty;
        public string URLSlug { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public string? UKASReference { get; set; } = string.Empty;
        public string? CABNumber { get; set; }
        public Document CabBlob { get; set; }
        public string Version { get; set; } = string.Empty;
    
        internal void Update(Document document)
        {
            StatusValue = document.StatusValue;
            CABId = document.CABId;
            SubStatus = document.SubStatus;
            CreatedByUserGroup = document.CreatedByUserGroup;
            URLSlug = document.URLSlug;
            Name = document.Name;
            UKASReference = document.UKASReference;
            CABNumber = document.CABNumber;
            Version = document.Version;
            CabBlob = document;
        }
    }
}