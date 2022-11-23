using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UKMCAB.Web.UI.Models
{
    public class ErrorModelList
    {
        public List<ErrorViewModel> ErrorViewModels { get; set; }

        public ErrorModelList(ModelStateDictionary modelState)
        {
            ErrorViewModels = modelState
                .Where(m => m.Value.ValidationState == ModelValidationState.Invalid)
                .SelectMany(v => v.Value.Errors.Select(e => new ErrorViewModel { Key = v.Key, Error = e.ErrorMessage }))
                .ToList();
        }
    }

    public class ErrorViewModel
    {
        public string Key { get; set; }
        public string Error { get; set; }
    }
}
