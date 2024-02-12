using FluentValidation;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public class CABLegislativeAreasViewModel
{
    public List<CABLegislativeAreasItemViewModel> LegislativeAreas { get; set; } = new();

    public bool IsCompleted { get; set; }
}

public class CABLegislativeAreasViewModelValidator : AbstractValidator<CABLegislativeAreasViewModel>
{
    public CABLegislativeAreasViewModelValidator()
    {
        // 1. There must be at least one legislative area.
        // - Every LA must have the IsProvisional property set.
        // - If the LA requires scope of appointment to be chosen (HasDataModel="1" in the LA container data), then there
        //   must be at least one ScopeOfAppointment object with at least one procedure.
        //   If any ScopeOfAppointment doesn't have a procedure, or the procedure is null or empty, validation will fail.

        RuleFor(x => x.LegislativeAreas)
            .Must((model, legislativeAreas) =>
            {
                bool complete = legislativeAreas.Any() && legislativeAreas.All(x => x.IsProvisional.HasValue && 
                    (!x.CanChooseScopeOfAppointment || (x.ScopeOfAppointments.Any() && x.ScopeOfAppointments.All(y => y.Procedures != null && y.Procedures.Any() && y.Procedures.All(z => !string.IsNullOrEmpty(z))))));
                return complete;
            })
            .WithMessage("Legislative areas are incomplete");
    }
}
