using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UKMCAB.Web.UI.Services
{
    public class DateValidator
    {
        public static DateTime? CheckDate(ModelStateDictionary modelState, string day, string month, string year, string modelKey, string errorMessagePart, DateTime? aptDate = null)
        {
            var date = $"{day}/{month}/{year}";

            if (int.TryParse(day, out int dayNum) && int.TryParse(month, out int monthNum) && int.TryParse(year, out int yearNum))
            {

                //if (!DateService.IsAValidDay(dayNum, monthNum, yearNum) || !DateService.IsAValidMonth(monthNum))
                //{
                //    int maxDay = DateService.GetMaxDaysInMonth(monthNum, yearNum);
                //    modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must be a real date.");

                //    return null;
                //}

                var usedModelKey = string.Empty;

                if (!DateService.IsAValidMonth(monthNum))
                {
                    usedModelKey = $"{modelKey}Month";

                    modelState.AddModelError($"{modelKey}Month", string.Empty);
                    //modelState.AddModelError(usedModelKey, $"{errorMessagePart.ToSentenceCase()} date must be a real date.");

                    //return null;

                }

                if (!DateService.IsAValidDay(dayNum, monthNum, yearNum))
                {
                    modelState.AddModelError($"{modelKey}Day", string.Empty);

                    //if (string.IsNullOrWhiteSpace(usedModelKey))
                    //{
                    //    modelState.AddModelError($"{modelKey}Day", $"{errorMessagePart.ToSentenceCase()} date must be a real date.");
                    //}
                    //else
                    //{
                    //    modelState.AddModelError($"{modelKey}Day", string.Empty);
                    //}

                    //return null;
                }

                if (!DateService.IsAValidDay(dayNum, monthNum, yearNum) || !DateService.IsAValidMonth(monthNum))
                {
                    modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must be a real date.");
                    return null;
                }

                if (!DateService.IsTodayOrInPast(dayNum, monthNum, yearNum) && modelKey == "AppointmentDate")
                {
                    AddAllFieldErrors(modelState, modelKey);
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be in the past.");
                    return null;
                }

                if (!DateService.IsTodayOrFuture(dayNum, monthNum, yearNum) && modelKey == "ReviewDate")
                {
                    AddAllFieldErrors(modelState, modelKey);
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be in the future.");
                    return null;
                }

                if (!DateService.IsWithinFiveYearAndNotInPast(dayNum, monthNum, yearNum, aptDate) && modelKey == "ReviewDate")
                {
                    AddAllFieldErrors(modelState, modelKey);
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be within 5 years of the appointment date.");
                    return null;
                }

                if (DateTime.TryParse(date, out DateTime dateTime))
                    return dateTime;

            }

            if (int.TryParse(day, out int dayNum1) || int.TryParse(month, out int monthNum1) || int.TryParse(year, out int yearNum1))
            {
                var missingField = string.Empty;
                var missingFields = new List<string>();
                string modelKeyLocal = string.Empty;

              

                if (string.IsNullOrWhiteSpace(day))
                {
                    missingField += "day and ";
                    modelKeyLocal = $"{modelKey}Day";
                    //modelState.AddModelError(modelKeyLocal, string.Empty);
                    missingFields.Add(modelKeyLocal);
                }
                    

                if (string.IsNullOrWhiteSpace(month))
                {
                    missingField += "month and ";
                    modelKeyLocal = $"{modelKey}Month";
                    //modelState.AddModelError(modelKeyLocal, string.Empty);
                    missingFields.Add(modelKeyLocal);
                }
                    

                if (string.IsNullOrWhiteSpace(year))
                {
                    missingField += "year";
                    modelKeyLocal = $"{modelKey}Year";
                    //modelState.AddModelError(modelKeyLocal, string.Empty);
                    missingFields.Add(modelKeyLocal);
                }
                    

                missingField =  missingField.TrimEnd(' ', 'a', 'n', 'd', ' ');
                //if (!string.IsNullOrEmpty(missingField))
                //{
                //    modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must include a {missingField}.");
                //    return null;
                //}
                if(missingFields.Count > 0)
                {
                    //int c = 1;

                    foreach (var field in missingFields)
                    {
                        modelState.AddModelError(field, string.Empty);
                    }

                    modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must include a {missingField}.");

                    return null;
                }
                
            }

            if (!date.Equals("//"))
            {
                modelState.AddModelError(modelKey, $"The {errorMessagePart} date is not in a valid date format");
            }

            return null;
        }

        private static void AddAllFieldErrors(ModelStateDictionary modelState, string modelKey)
        {
            modelState.AddModelError($"{modelKey}Day", string.Empty);
            modelState.AddModelError($"{modelKey}Month", string.Empty);
            modelState.AddModelError($"{modelKey}Year", string.Empty);
        }
    }
}
