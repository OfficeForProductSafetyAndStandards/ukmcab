namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    using UKMCAB.Common.Extensions;
    using UKMCAB.Data.Models;

    public class CABManagementItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string URLSlug { get; set; }= string.Empty;
        public string CABNumber { get; set; }= string.Empty;
        public string? CabNumberVisibility { get; set; }
        public string UKASReference { get; set; } = string.Empty;
        public string SubStatus { get; set; }= string.Empty;
        public bool IsPendingApprovalToUnarchive { get; set; }
        public DateTime LastUpdated { get; set; }
        public string UserGroup { get; set; }


        public CABManagementItemViewModel(Document doc)
        {
            Id = doc.CABId.ToString();
            Name = doc.Name;
            URLSlug = doc.URLSlug;
            CABNumber = doc.CABNumber;
            CabNumberVisibility = doc.CabNumberVisibility;
            UKASReference = doc.UKASReference;
            SubStatus = doc.SubStatus == Data.Models.SubStatus.None ? "Draft" : doc.SubStatus.GetEnumDescription();
            IsPendingApprovalToUnarchive = 
                this.SubStatus == Data.Models.SubStatus.PendingApprovalToUnarchivePublish.GetEnumDescription() || 
                this.SubStatus == Data.Models.SubStatus.PendingApprovalToUnarchive.GetEnumDescription();
            LastUpdated = doc.LastUpdatedDate;
            UserGroup = doc.CreatedByUserGroup;

        }
    }
}
