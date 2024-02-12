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
        // There must be at least one legislative area.
        // If the LA has a data model (scope of appointment can be selected), then there must be at least one ScopeOfAppointment object with at least one procedure.
        // If any ScopeOfAppointment doesn't have a procedure, or the procedure is null or empty, validation will fail.

        RuleFor(x => x.LegislativeAreas)
            .Must((model, legislativeAreas) =>
            {
                bool complete = legislativeAreas.Any() && legislativeAreas.All(x => !x.CanChooseScopeOfAppointment || 
                        (x.ScopeOfAppointments.Any() && x.ScopeOfAppointments.All(y => y.Procedures != null && y.Procedures.Any() && y.Procedures.All(z => !string.IsNullOrEmpty(z)))));
                return complete;
            })
            .WithMessage("Legislative areas are incomplete");
    }
}
