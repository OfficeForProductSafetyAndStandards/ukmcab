﻿using System.ComponentModel;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums
{
    public enum RemoveActionEnum
    {   
        Remove = 0,     
        Archive = 1       
    }

    public enum ProductScheduleActionMessageEnum
    {
        ProductScheduleRemoved = 0,
        ProductScheduleRemovedLegislativeAreaArchived = 1,
        ProductScheduleRemovedLegislativeAreaRemoved = 2,
        ProductScheduleRemovedLegislativeAreaProvisional = 3,
        ProductScheduleArchived = 4,
        ProductScheduleArchivedLegislativeAreaArchived = 5,
        ProductScheduleArchivedLegislativeAreaRemoved = 6,
        ProductScheduleArchivedLegislativeAreaProvisional = 7,
        ProductScheduleFileReplaced = 8,
        ProductScheduleFileUsedAgain = 9,
    }

    public enum LegislativeAreaActionEnum
    {
        Remove = 0,
        Archive = 1,
        MarkAsProvisional = 2
    }

    public enum LegislativeAreaApproveActionEnum
    {
        Approve = 0,
        Decline = 1,
    }

    public enum LegislativeAreaActionMessageEnum
    {
        LegislativeAreaRemoved = 0,        
        LegislativeAreaRemovedProductScheduleRemoved = 1,
        LegislativeAreaArchived = 2,
        LegislativeAreaArchivedProductScheduleArchived = 3,
        LegislativeAreaArchivedProductScheduleRemoved = 4,
        ScopeOfAppointmentRemoved = 5,
        LegislativeAreaUnArchived = 6,
    }

    public enum LegislativeAreaReviewActionEnum
    {
        [Description("approve")]
        Add = 0,
        [Description("remove")]
        Remove  = 1,
        [Description("archive")]
        ArchiveAndArchiveSchedule = 2,
        [Description("archive")]
        ArchiveAndRemoveSchedule = 3,
        [Description("unarchive")]
        Unarchive = 4
    }
}
