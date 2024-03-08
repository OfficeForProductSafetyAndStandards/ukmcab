using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Helpers
{
    public class AlertMessagesUtils
    {
        public readonly static Dictionary<ProductScheduleActionMessageEnum, string> ProductScheduleActionMessages = new Dictionary<ProductScheduleActionMessageEnum, string>
        {
            {ProductScheduleActionMessageEnum.ProductScheduleRemoved, "The product schedule has been removed."},
            {ProductScheduleActionMessageEnum.ProductScheduleRemovedLegislativeAreaArchived, "The product schedule has been removed and the legislative area has been archived."},
            {ProductScheduleActionMessageEnum.ProductScheduleRemovedLegislativeAreaRemoved, "The product schedule and legislative area have been removed."},
            {ProductScheduleActionMessageEnum.ProductScheduleRemovedLegislativeAreaProvisional, "The product schedule has been removed and the legislative area will be shown as provisional."},
            {ProductScheduleActionMessageEnum.ProductScheduleArchived, "The product schedule has been archived."},
            {ProductScheduleActionMessageEnum.ProductScheduleArchivedLegislativeAreaArchived, "The product schedule and legislative area have been archived."},
            {ProductScheduleActionMessageEnum.ProductScheduleArchivedLegislativeAreaRemoved, "The product schedule has been archived and the legislative area has been removed."},
            {ProductScheduleActionMessageEnum.ProductScheduleArchivedLegislativeAreaProvisional, "The product schedule has been archived and the legislative area will be shown as provisional."},
            {ProductScheduleActionMessageEnum.ProductScheduleFileReplaced, "The replacement file has been uploaded."},
            {ProductScheduleActionMessageEnum.ProductScheduleFileUsedAgain, "The file has been used again."},
        };


        public readonly static Dictionary<LegislativeAreaActionMessageEnum, string> LegislativeAreaActionMessages = new Dictionary<LegislativeAreaActionMessageEnum, string>
        {
            {LegislativeAreaActionMessageEnum.LegislativeAreaRemoved, "The legislative area has been removed."},
            {LegislativeAreaActionMessageEnum.LegislativeAreaRemovedProductScheduleRemoved, "The  legislative area and associated product schedules have been removed."},
            {LegislativeAreaActionMessageEnum.LegislativeAreaArchived, "The legislative area has been archived."},
            {LegislativeAreaActionMessageEnum.LegislativeAreaArchivedProductScheduleArchived, "The legislative area and associated product schedules have been archived."},
            {LegislativeAreaActionMessageEnum.LegislativeAreaArchivedProductScheduleRemoved, "The legislative area has been archived and the associated product schedules removed."},
            {LegislativeAreaActionMessageEnum.AssessmentProcedureRemoved, "The applicable conformity assessment procedure has been removed."},
            
        };
    }
}
