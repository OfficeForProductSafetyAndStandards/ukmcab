namespace UKMCAB.Core.Domain
{
    using System.Collections.Generic;
    using UKMCAB.Data.Models;

    public class CabManagementDetailsModel
    {
        public List<Document> AllCabs { get; set; } = new();
        public List<Document> DraftCabs { get; set; } = new();
        public List<Document> PendingDraftCabs { get; set; } = new();
        public List<Document> PendingPublishCabs { get; set; } = new();
        public List<Document> PendingArchiveCabs { get; set; } = new();
    }
}
