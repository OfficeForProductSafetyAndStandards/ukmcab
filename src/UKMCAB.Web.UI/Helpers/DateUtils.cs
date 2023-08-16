using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UKMCAB.Web.UI.Helpers;

public class DateUtils
{
    public static DateTime? CheckDate(ModelStateDictionary modelState, string day, string month, string year, string modelKey, string errorMessagePart, DateTime? aptDate = null)
    {
        var date = $"{year}/{month}/{day}";
        var dayParsed = int.TryParse(day, out int parsedDay);
        var monthParsed = int.TryParse(month, out int parsedMonth);
        var yearParsed = int.TryParse(year, out int parsedYear);

        if (dayParsed && monthParsed && yearParsed)
        {

            if (RealDateErrorsAdded(modelState, modelKey, errorMessagePart, parsedDay, parsedMonth, parsedYear) > 0)
            {
                return null;
            }

            if (!DateService.IsAValidDayOfMonthAndYear(parsedDay, parsedMonth, parsedYear))
            {
                AddInvalidDayOfMonthAndYearError(modelState, modelKey, errorMessagePart);

                return null;
            }

            if (!DateService.IsPastDate(parsedDay, parsedMonth, parsedYear) && modelKey == "AppointmentDate")
            {
                AddDateMustBeInPastErrors(modelState, modelKey, errorMessagePart);
                return null;
            }

            if (!DateService.IsFutureDate(parsedDay, parsedMonth, parsedYear) && modelKey == "ReviewDate")
            {
                AddDateMustBeInFutureErrors(modelState, modelKey, errorMessagePart);
                return null;
            }

            if (!DateService.IsWithinFiveYearOfAppointmentDateAndInFuture(parsedDay, parsedMonth, parsedYear, aptDate) && modelKey == "ReviewDate")
            {
                AddDateMustBeWithin5YearsErrors(modelState, modelKey, errorMessagePart);
                return null;
            }

            if (DateTime.TryParse(date, out DateTime dateTime))
                return dateTime;

        }

        if (dayParsed || monthParsed || yearParsed)
        {
            AddMissingDateFieldsErrors(modelState, modelKey, errorMessagePart, day, month, year);
            return null;
        }

        if (!date.Equals("//"))
        {
            AddInvalidDateFormatErrors(modelState, modelKey, errorMessagePart, parsedDay, parsedMonth, parsedYear);
        }

        return null;
    }

    private static void AddInvalidDayOfMonthAndYearError(ModelStateDictionary modelState, string modelKey, string errorMessagePart)
    {
        modelState.AddModelError($"{modelKey}Day", string.Empty);
        modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must be a real date.");
    }

    private static int RealDateErrorsAdded(ModelStateDictionary modelState, string modelKey, string errorMessagePart, int day, int month, int year)
    {
        int errorsAdded = 0;

        if (!DateService.IsAValidDay(day))
        {
            modelState.AddModelError($"{modelKey}Day", string.Empty);
            errorsAdded++;
        }

        if (!DateService.IsAValidMonth(month))
        {
            modelState.AddModelError($"{modelKey}Month", string.Empty);
            errorsAdded++;
        }

        if (!DateService.IsAValidYear(year))
        {
            modelState.AddModelError($"{modelKey}Year", string.Empty);
            errorsAdded++;
        }

        if (errorsAdded > 0)
        {
            modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must be a real date.");
        }

        return errorsAdded;
    }

    private static void AddMissingDateFieldsErrors(ModelStateDictionary modelState, string modelKey, string errorMessagePart, string day, string month, string year)
    {
        var missingField = string.Empty;

        if (string.IsNullOrWhiteSpace(day))
        {
            missingField += "day and ";
            modelState.AddModelError($"{modelKey}Day", string.Empty);
        }

        if (string.IsNullOrWhiteSpace(month))
        {
            missingField += "month and ";
            modelState.AddModelError($"{modelKey}Month", string.Empty);
        }

        if (string.IsNullOrWhiteSpace(year))
        {
            missingField += "year";
            modelState.AddModelError($"{modelKey}Year", string.Empty);
        }

        missingField = missingField.TrimEnd(' ', 'a', 'n', 'd', ' ');

        modelState.AddModelError(modelKey, $"{errorMessagePart.ToSentenceCase()} date must include a {missingField}.");
    }

    private static void AddInvalidDateFormatErrors(ModelStateDictionary modelState, string modelKey, string errorMessagePart, int parsedDay, int parsedMonth, int parsedYear)
    {

        if (parsedDay == 0)
            modelState.AddModelError($"{modelKey}Day", string.Empty);

        if (parsedMonth == 0)
            modelState.AddModelError($"{modelKey}Month", string.Empty);

        if (parsedYear == 0)
            modelState.AddModelError($"{modelKey}Year", string.Empty);


        modelState.AddModelError(modelKey, $"The {errorMessagePart} date is not in a valid date format");
    }
    private static void AddAllFieldErrors(ModelStateDictionary modelState, string modelKey)
    {
        modelState.AddModelError($"{modelKey}Day", string.Empty);
        modelState.AddModelError($"{modelKey}Month", string.Empty);
        modelState.AddModelError($"{modelKey}Year", string.Empty);
    }

    private static void AddDateMustBeInFutureErrors(ModelStateDictionary modelState, string modelKey, string errorMessagePart)
    {
        AddAllFieldErrors(modelState, modelKey);
        modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be in the future.");
    }

    private static void AddDateMustBeInPastErrors(ModelStateDictionary modelState, string modelKey, string errorMessagePart)
    {
        AddAllFieldErrors(modelState, modelKey);
        modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be in the past.");
    }

    private static void AddDateMustBeWithin5YearsErrors(ModelStateDictionary modelState, string modelKey, string errorMessagePart)
    {
        AddAllFieldErrors(modelState, modelKey);
        modelState.AddModelError(modelKey, $"The {errorMessagePart} date must be within 5 years of the appointment date.");
    }
}
