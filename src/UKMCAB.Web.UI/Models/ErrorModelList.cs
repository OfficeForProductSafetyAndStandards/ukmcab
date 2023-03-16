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

        public ErrorModelList(ModelStateDictionary modelState, string[] fieldOrder)
        {
            ErrorViewModels = modelState
                .Where(m => m.Value.ValidationState == ModelValidationState.Invalid)
                .SelectMany(v => v.Value.Errors.Select(e => new ErrorViewModel { Key = v.Key, Error = e.ErrorMessage }))
                .ToList();
            if (fieldOrder != null)
            {
                var sortedList = ErrorViewModels.Where(e => string.IsNullOrEmpty(e.Key) || fieldOrder.All(f => !f.Equals(e.Key))).ToList();
                foreach (var field in fieldOrder)
                {
                    sortedList.AddRange(ErrorViewModels.Where(e => field.Equals(e.Key)));
                }

                ErrorViewModels = sortedList;
            }
        }
    }

    public class ErrorViewModel
    {
        public string Key { get; set; }
        public string Error { get; set; }
    }
}
