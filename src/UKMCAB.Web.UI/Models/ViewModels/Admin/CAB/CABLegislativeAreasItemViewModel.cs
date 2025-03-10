﻿using UKMCAB.Common.Extensions;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABLegislativeAreasItemViewModel
    {
        public Guid? LegislativeAreaId { get; set; }

        public string? Name { get; set; }

        public bool? IsProvisional { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? Reason { get; set; }
        public string? RequestReason { get; set; }

        public string? PointOfContactName { get; set; }
        public string? PointOfContactEmail { get; set; }
        public string? PointOfContactPhone { get; set; }
        public bool? IsPointOfContactPublicDisplay { get; set; }

        public List<LegislativeAreaListItemViewModel> ScopeOfAppointments { get; set; } = new();

        public bool CanChooseScopeOfAppointment { get; set; }
        public bool? IsArchived { get; init; }
        public Guid? SelectedScopeOfAppointmentId { get; set; }

        public bool ShowPurposeOfAppointmentColumn => ScopeOfAppointments != null &&
                                                      ScopeOfAppointments.Any(x =>
                                                          !string.IsNullOrEmpty(x.PurposeOfAppointment));

        public bool ShowCategoryColumn => ScopeOfAppointments != null &&
                                          ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.Category));

        public bool ShowProductColumn => ScopeOfAppointments != null &&
                                         ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.Product));

        public bool ShowProcedureColumn => ScopeOfAppointments != null &&
                                         ScopeOfAppointments.Any(x => x.Procedures != null && x.Procedures!.Any());

        public bool ShowDesignatedStandardColumns => ScopeOfAppointments != null &&
                                          ScopeOfAppointments.Any(x => x.DesignatedStandards != null && x.DesignatedStandards.Any());

        public bool ShowPpeProductTypeColumn => ScopeOfAppointments != null &&
                                         ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.PpeProductType));

        public bool ShowProtectionAgainstRiskColumn => ScopeOfAppointments != null &&
                                         ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.ProtectionAgainstRisk));

        public bool ShowAreaOfCompetencyColumn => ScopeOfAppointments != null &&
                                         ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.AreaOfCompetency));

        public LAStatus Status { get; set; }
        public string StatusName => GetStatusName();
        public string StatusCssStyle { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public bool ShowEditActions { get; set; }
        public bool? NewlyCreated { get; set; }
        public bool IsNewlyCreated => NewlyCreated ?? false;
        public bool? MRABypass { get; set; }

        public bool IsComplete => IsProvisional.HasValue &&
                                    (ReviewDate == null || (ReviewDate != null && ReviewDate >= DateTime.Today)) &&
                                    (!CanChooseScopeOfAppointment || (MRABypass ?? false) || (ScopeOfAppointments.Any() && ScopeOfAppointments.All(
                                        y => 
                                            (y.Procedures != null && y.Procedures.Any() &&
                                            y.Procedures.All(z => !string.IsNullOrEmpty(z))) ||
                                            (y.DesignatedStandards != null && y.DesignatedStandards.Any())
                                        )));
        private string GetStatusName()
        {
            return Status switch
            {
                LAStatus.Approved or
                    LAStatus.PendingApprovalToUnarchiveByOpssAdmin
                    =>
                    $"{Status.GetEnumDescription()} by {RoleName}",
                LAStatus.Declined or
                    LAStatus.DeclinedToRemoveByOGD or
                    LAStatus.DeclinedToUnarchiveByOGD or
                    LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD or
                    LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD
                    =>
                    $"{Status.GetEnumDescription()} by {RoleName}",
                LAStatus.PendingApproval
                    or LAStatus.PendingApprovalToRemove
                    or LAStatus.PendingApprovalToArchiveAndArchiveSchedule
                    or LAStatus.PendingApprovalToArchiveAndRemoveSchedule
                    or LAStatus.PendingApprovalToUnarchive
                    =>
                    $"{Status.GetEnumDescription()} from {RoleName}",
                _ => Status.GetEnumDescription()
            };
        }

        public bool CanOnlyBeActionedByUkas => 
            Status == LAStatus.DeclinedByOpssAdmin ||
            Status == LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS ||
            Status == LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS ||
            Status == LAStatus.DeclinedToRemoveByOPSS ||
            Status == LAStatus.DeclinedToUnarchiveByOPSS;
    }
}