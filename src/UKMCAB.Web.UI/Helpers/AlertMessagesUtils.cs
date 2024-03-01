using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Helpers
{
    public class AlertMessagesUtils
    {
        public static Dictionary<ProductScheduleRemoveMessageEnum, string> ProductScheduleRemovalArchiveMessages = new Dictionary<ProductScheduleRemoveMessageEnum, string>
        {
            {ProductScheduleRemoveMessageEnum.ProductScheduleRemoved, "The product schedule has been removed."},
            {ProductScheduleRemoveMessageEnum.ProductScheduleRemovedLegislativeAreaArchived, "The product schedule has been removed and the legislative area has been archived."},
            {ProductScheduleRemoveMessageEnum.ProductScheduleRemovedLegislativeAreaRemoved, "The product schedule and legislative area have been removed."},
            {ProductScheduleRemoveMessageEnum.ProductScheduleRemovedLegislativeAreaProvisional, "The product schedule has been removed and the legislative area will be shown as provisional."},
            {ProductScheduleRemoveMessageEnum.ProductScheduleArchived, "The product schedule has been archived."},
            {ProductScheduleRemoveMessageEnum.ProductScheduleArchivedLegislativeAreaArchived, "The product schedule and legislative area have been archived."},
            {ProductScheduleRemoveMessageEnum.ProductScheduleArchivedLegislativeAreaRemoved, "The product schedule has been archived and the legislative area has been removed."},
            {ProductScheduleRemoveMessageEnum.ProductScheduleArchivedLegislativeAreaProvisional, "The product schedule has been archived and the legislative area will be shown as provisional."},            
        };
    }
}
