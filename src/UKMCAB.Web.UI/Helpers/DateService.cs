namespace UKMCAB.Web.UI.Helpers;

public class DateService
{
    public static bool IsAValidDayOfMonthAndYear(int day, int month, int year)
    {
        if (IsAValidMonth(month))
        {
            int maxDay = GetMaxDaysInMonth(month, year);

            return day >= 1 && day <= maxDay;
        }

        return IsAValidDay(day);
    }

    public static bool IsAValidDay(int day)
    {
        return day >= 1 && day <= 31;
    }
    public static bool IsAValidMonth(int month)
    {
        return month >= 1 && month <= 12;

    }

    public static bool IsAValidYear(int year)
    {
        return year >= 1;

    }

    public static bool IsFutureDate(int day, int month, int year)
    {
        var today = DateTime.Today;

        if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
            return inputDate > today;

        return false;
    }

    public static bool IsPastDate(int day, int month, int year)
    {
        var today = DateTime.Today;

        if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
            return inputDate < today;

        return false;
    }

    public static bool IsWithinFiveYearOfAppointmentDateAndInFuture(int day, int month, int year, DateTime? aptDate)
    {

        var appointmentDateOrYesterday = aptDate != null ? (DateTime)aptDate : DateTime.Today.AddDays(-1);
        var tomorrow = DateTime.Today.AddDays(1);

        if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
            return inputDate <= appointmentDateOrYesterday.AddYears(5) && inputDate >= tomorrow;

        return false;
    }
    public static bool IsWithinFiveYearAndInThePast(int day, int month, int year)
    {
        var tomorrow = DateTime.Today.AddDays(1);
        var today = DateTime.Today;

        if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
            return inputDate >= tomorrow.AddYears(-5) && inputDate < today;

        return false;
    }

    public static int GetMaxDaysInMonth(int month, int year)
    {
        if (month == 2)
        {
            return IsLeapYear(year) ? 29 : 28;
        }
        else if (month == 4 || month == 6 || month == 9 || month == 11)
        {
            return 30;
        }

        return 31;
    }

    private static bool IsLeapYear(int year)
    {
        return year % 4 == 0 && year % 100 != 0 || year % 400 == 0;
    }
}
