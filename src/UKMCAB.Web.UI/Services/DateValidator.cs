using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UKMCAB.Web.UI.Services
{
    public class DateValidator
    {
        public static DateTime? CheckDate(ModelStateDictionary modelState, string day, string month, string year, string modelKey, string errorMessagePart, DateTime? aptDate = null)
        {
            var date = $"{day}/{month}/{year}";

            if (int.TryParse(day, out int dayNum) && int.TryParse(month, out int monthNum) && int.TryParse(year, out int yearNum))
            {
               
                if (!DateService.IsAValidDay(dayNum, monthNum, yearNum) || !DateService.IsAValidMonth(monthNum))
                {
                    int maxDay = DateService.GetMaxDaysInMonth(monthNum, yearNum);
                    modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must be a real date.");

                    return null;
                }

                if (!DateService.IsTodayOrInPast(dayNum, monthNum, yearNum) && modelKey == "AppointmentDate")
                {
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be in the past.");
                    return null;
                }

                if (!DateService.IsTodayOrFuture(dayNum, monthNum, yearNum) && modelKey == "ReviewDate")
                {
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be in the future.");
                    return null;
                }

                if (!DateService.IsWithinFiveYearAndNotInPast(dayNum, monthNum, yearNum, aptDate) && modelKey == "ReviewDate")
                {
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be within 5 years of the appointment date.");
                    return null;
                }

                if (DateTime.TryParse(date, out DateTime dateTime))
                    return dateTime;

            }

            if (int.TryParse(day, out int dayNum1) || int.TryParse(month, out int monthNum1) || int.TryParse(year, out int yearNum1))
            {
                var missingField = string.Empty;
                string modelKeyLocal = string.Empty;
              

                if (string.IsNullOrWhiteSpace(day))
                {
                    missingField += "day and ";
                    modelKeyLocal = $"{modelKey}Day";
                    modelState.AddModelError(modelKeyLocal, string.Empty);
                }
                    

                if (string.IsNullOrWhiteSpace(month))
                {
                    missingField += "month and ";
                    modelKeyLocal = $"{modelKey}Month";
                    modelState.AddModelError(modelKeyLocal, string.Empty);
                }
                    

                if (string.IsNullOrWhiteSpace(year))
                {
                    missingField += "year";
                    modelKeyLocal = $"{modelKey}Year";
                    modelState.AddModelError(modelKeyLocal, string.Empty);
                }
                    

                missingField =  missingField.TrimEnd(' ', 'a', 'n', 'd', ' ');
                if (!string.IsNullOrEmpty(missingField))
                {
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
    }
}
