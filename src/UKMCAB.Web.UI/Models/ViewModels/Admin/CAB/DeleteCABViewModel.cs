using FluentValidation;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public record DeleteCABViewModel(string? Title, string CabId, string CabName, bool HasExistingVersion) : BasicPageModel(Title)
{
    public string? DeleteReason { get; set; }
}

public class DeleteCABViewModelValidator : AbstractValidator<DeleteCABViewModel>
{
    public DeleteCABViewModelValidator()
    {
        RuleFor(x => x.DeleteReason)
            .Must((model, deleteReason) =>
            {
                return !model.HasExistingVersion || deleteReason.IsNotNullOrEmpty();
            })
            .WithMessage("Enter notes");
    }
}
