namespace UKMCAB.Web.UI.Services
{
    public class DateValidator
    {
        private DateTime? CheckDate(string date, string modelKey, string errorMessagePart)
        {
            if (DateTime.TryParse(date, out DateTime dateTime))
            {
                // Validate date - Appointment Date: No future date
                // Validate date - Renewal Date: No past date
                // Validate date - Appointment Date: Not more than 5yrs in the future
                return dateTime;
            }
            if (!date.Equals("//"))
            {
                // Check if day or month or year is invalid or missing and throw relevant error


                //ModelState.AddModelError(modelKey, $"The {errorMessagePart} date is not in a valid date format");
            }
            return null;
        }
        public static bool IsValidDay(int day, int month, int year)
        {
            //if (month < 1 || month > 12)
            //    return false;

            int maxDay = GetMaxDaysInMonth(month, year);

            return day >= 1 && day <= maxDay;
        }        
        
        public static bool IsValidMonth(int month)
        {
            return month >= 1 || month <= 12;          

        }
        
        public static bool DateIsWithinFiveYearInFuture(int day, int month, int year)
        {
            var currentDate = DateTime.Today;
            //var inputDate = new DateTime(year, month, day);
            DateTime inputDate = DateTime.MinValue;
           if(DateTime.TryParse($"{year}/{month}/{day}", out DateTime outDate))
                inputDate = outDate;

            return inputDate <= currentDate.AddYears(5);
        }        
        
        public static bool DateIsWithinFiveYearInThePast(int day, int month, int year)
        {
            var currentDate = DateTime.Today;
            //var inputDate = new DateTime(year, month, day);
            //DateTime.TryParse($"{year}/{month}/{day}", out inputDate);

            //var inputDate = Convert.ToDateTime($"{year}/{month}/{day}");
            DateTime inputDate = DateTime.MinValue;
            if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime outDate))
                inputDate = outDate;

            return inputDate >= currentDate.AddYears(-5) && inputDate <= currentDate;
        }

        public static bool CompareDates(DateTime date1, DateTime date2)
        {
            return date1 < date2;
        }
        
        //public static bool ValidateDate(int day, int month, int year)
        //{
        //    if (month < 1 || month > 12)
        //        return false;

        //    int maxDay = GetMaxDaysInMonth(month, year);

        //    return day >= 1 && day <= maxDay && year > 0;
        //}

        //public static bool DateIsInThePast(int day, int month, int year)
        //{
        //    var currentDate = DateTime.Today;
        //    var inputDate = new DateTime(year, month, day);

        //    return inputDate < currentDate;

        //}

        // Compare appointment date to renewal date. Renewal should always be ahead.

        // Check if input were numbers
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
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }
    }
}
