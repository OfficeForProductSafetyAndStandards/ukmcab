﻿namespace UKMCAB.Core.Security;
public static class Policies
{
    public const string CabManagement = "cab-mgt";
    public const string UserManagement = "user-mgt";
    public const string SuperAdmin = "sa";
    public const string GovernmentUserNotes = "gov-user-notes";
    public const string ApproveRequests = "approve-requests";
    public const string LegislativeAreaManage = "la.manage";
    public const string LegislativeAreaApprove = "la.approve";
    public const string EditCabPendingApproval = "edit-cab-pending-approval";
    public const string CanRequest = "request-cab-la-changes";
    public const string DeleteCab = "delete-cab";
}
