using FluentValidation;
using Microsoft.IdentityModel.Tokens;

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
        // 1. There must be at least one legislative area.
        // - Every LA must have IsComplete field equal true, which means
        // - The IsProvisional property set.
        // - If the LA requires scope of appointment to be chosen (HasDataModel="1" in the LA container data), then there
        //      must be at least one ScopeOfAppointment object with at least one procedure.
        //      If any ScopeOfAppointment doesn't have a procedure, or the procedure is null or empty, validation will fail.
        // 1.1 (addendum) All legislative areas must be archived, with NO active legislative areas

        RuleFor(x => x)
            .Must((_, vm) =>
            {
                var complete = 
                    (vm.ActiveLegislativeAreas.Any() && vm.ActiveLegislativeAreas.All(x => x.IsComplete)) ||
                    (!vm.ActiveLegislativeAreas.Any() && vm.ArchivedLegislativeAreas.Any());
                return complete;
            })
            .WithMessage("Legislative areas are incomplete");
    }
}
