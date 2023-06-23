namespace UKMCAB.Web.UI.Services
{
    public class DateValidator
    {
        public static bool ValidateDate(int day, int month, int year)
        {
            if (month < 1 || month > 12)
                return false;

            int maxDay = GetMaxDaysInMonth(month, year);

            return day >= 1 && day <= maxDay && year > 0;
        }

        public static bool DateIsInThePast(int day, int month, int year)
        {
            var currentDate = DateTime.Today;
            var inputDate = new DateTime(year, month, day);

            return inputDate < currentDate;

        }

        // Compare appointment date to renewal date. Renewal should always be ahead.

        // Check if input were numbers
        private static int GetMaxDaysInMonth(int month, int year)
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
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }
    }
}
