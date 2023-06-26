namespace UKMCAB.Web.UI.Services
{
    public class DateService
    {
        public static bool IsAValidDay(int day, int month, int year)
        {
            if (IsAValidMonth(month)) 
            {
                int maxDay = GetMaxDaysInMonth(month, year);

                return day >= 1 && day <= maxDay;
            }
            
            return false;
        }        
        
        public static bool IsAValidMonth(int month)
        {
            return month >= 1 && month <= 12;          

        }       
       
        public static bool IsTodayOrFuture(int day, int month, int year)
        {
            var currentDate = DateTime.Today;

            if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
                return inputDate >= currentDate;

            return false;
        }

        public static bool IsTodayOrInPast(int day, int month, int year)
        {
            var currentDate = DateTime.Today;

            if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
                return inputDate <= currentDate;

            return false;
        }

        public static bool IsWithinFiveYearAndNotInPast(int day, int month, int year)
        {
            var currentDate = DateTime.Today;

            if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
                return inputDate <= currentDate.AddYears(5) && inputDate >= currentDate;

            return false;
        }
        public static bool IsWithinFiveYearAndNotFuture(int day, int month, int year)
        {
            var currentDate = DateTime.Today;

            if (DateTime.TryParse($"{year}/{month}/{day}", out DateTime inputDate))
                return inputDate >= currentDate.AddYears(-5) && inputDate <= currentDate;

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
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }
    }
}
