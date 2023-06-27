using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UKMCAB.Web.UI.Services
{
    public class DateValidator
    {
        public static DateTime? CheckDate(ModelStateDictionary modelState, string day, string month, string year, string modelKey, string errorMessagePart)
        {
            var date = $"{day}/{month}/{year}";

            if (int.TryParse(day, out int dayNum) && int.TryParse(month, out int monthNum) && int.TryParse(year, out int yearNum))
            {
                if (!DateService.IsAValidMonth(monthNum))
                {
                    modelState.AddModelError(modelKey, $"Please enter a valid month between 1 and 12.");

                    return null;
                }

                if (!DateService.IsAValidDay(dayNum, monthNum, yearNum))
                {
                    int maxDay = DateService.GetMaxDaysInMonth(monthNum, yearNum);
                    modelState.AddModelError(modelKey, $"Please enter a valid day between 1 and {maxDay}.");

                    return null;
                }

                if (!DateService.IsTodayOrInPast(dayNum, monthNum, yearNum) && modelKey == "AppointmentDate")
                {
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date cannot be in the future. Please enter a valid {errorMessagePart} date.");
                    return null;
                }

                if (!DateService.IsTodayOrFuture(dayNum, monthNum, yearNum) && modelKey == "RenewalDate")
                {
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date cannot be in the past. Please enter a valid {errorMessagePart} date.");
                    return null;
                }

                if (!DateService.IsWithinFiveYearAndNotInPast(dayNum, monthNum, yearNum) && modelKey == "RenewalDate")
                {
                    modelState.AddModelError(modelKey, $"The {errorMessagePart} date cannot exceed 5 years in the future from the CAB creation date. Please select a valid {errorMessagePart} date within the specified range.");
                    return null;
                }

                if (DateTime.TryParse(date, out DateTime dateTime))
                    return dateTime;

            }

            if (int.TryParse(day, out int dayNum1) || int.TryParse(month, out int monthNum1) || int.TryParse(year, out int yearNum1))
            {
                var missingField = "";

                if (string.IsNullOrWhiteSpace(day))
                    missingField += "day and ";

                if (string.IsNullOrWhiteSpace(month))
                    missingField += "month and ";

                if (string.IsNullOrWhiteSpace(year))
                    missingField += "year";

                missingField =  missingField.TrimEnd(' ', 'a', 'n', 'd', ' ');
                if (!string.IsNullOrEmpty(missingField))
                {
                    modelState.AddModelError(modelKey, $"The date must include a {missingField}.");
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
