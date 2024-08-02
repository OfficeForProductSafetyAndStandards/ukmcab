using FluentValidation;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public class CABLegislativeAreasViewModel : CreateEditCABViewModel
{
    public List<CABLegislativeAreasItemViewModel> ActiveLegislativeAreas { get; } = new();
    public List<CABLegislativeAreasItemViewModel> ArchivedLegislativeAreas { get; } = new();
}

public class CABLegislativeAreasViewModelValidator : AbstractValidator<CABLegislativeAreasViewModel>
{
    public CABLegislativeAreasViewModelValidator()
    {
        // 1. There must be at least one active legislative area;
        //    OR no active legislative areas and at least one archived legislative area
        // - Every ACTIVE LA must have IsComplete field equal true, which means
        // - The IsProvisional property set.
        // - If the LA requires scope of appointment to be chosen (HasDataModel="1" in the LA container data), then there
        //      must be at least one ScopeOfAppointment object with at least one procedure.
        //      If any ScopeOfAppointment doesn't have a procedure, or the procedure is null or empty, validation will fail.

        RuleFor(vm => vm.ActiveLegislativeAreas)
            .Must((activeLAs) =>
            {
                return activeLAs.All(x => x.IsComplete);
            })
            .WithMessage("Legislative areas are incomplete");

        RuleFor(vm => vm)
            .Must((vm) =>
            {
                return vm.ActiveLegislativeAreas.Any() || vm.ArchivedLegislativeAreas.Any();
            })
            .WithMessage("Legislative areas are incomplete: either have active or archived legislative areas");


    }
}
