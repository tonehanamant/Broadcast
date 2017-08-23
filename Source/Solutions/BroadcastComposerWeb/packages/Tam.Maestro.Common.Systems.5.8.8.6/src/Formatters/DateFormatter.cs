using System;

namespace Tam.Maestro.Common.Formatters
{
    public static class DateFormatter
    {
        public static string GetMonth(int monthId)
        {
            switch (monthId)
            {
                case (1):
                    return ("January");
                case (2):
                    return ("February");
                case (3):
                    return ("March");
                case (4):
                    return ("April");
                case (5):
                    return ("May");
                case (6):
                    return ("June");
                case (7):
                    return ("July");
                case (8):
                    return ("August");
                case (9):
                    return ("September");
                case (10):
                    return ("October");
                case (11):
                    return ("November");
                case (12):
                    return ("December");
                default:
                    return ("");
            }
        }

        public static string GetMonthAbbr(int pMonthId)
        {
            switch (pMonthId)
            {
                case (1):
                    return ("Jan");
                case (2):
                    return ("Feb");
                case (3):
                    return ("Mar");
                case (4):
                    return ("Apr");
                case (5):
                    return ("May");
                case (6):
                    return ("Jun");
                case (7):
                    return ("Jul");
                case (8):
                    return ("Aug");
                case (9):
                    return ("Sep");
                case (10):
                    return ("Oct");
                case (11):
                    return ("Nov");
                case (12):
                    return ("Dec");
                default:
                    return ("");

            }
        }

        public static string GetYear(int year)
        {
            if (year < 0)
                throw new ApplicationException("Year cannot be less than 0");
            if (year > 2200)
                throw new ApplicationException("Year cannot be greater than 2200");
            if (year >= 100 && year < 1000)
                throw new ApplicationException("Year cannot be 3 digits.");
            if (year >= 0 && year <= 69)
            {
                return string.Format("20{0}", year.ToString("D2"));
            }
            if (year >= 70 && year <= 99)
            {
                return string.Format("19{0}", year.ToString("D2"));
            }
            return year.ToString();
        }

        public static DateTime AdjustStartDate(DateTime pIn)
        {
            if (pIn.DayOfWeek == DayOfWeek.Monday)
                return pIn;

            int lDays = 0;
            if (pIn.DayOfWeek == DayOfWeek.Sunday)
                lDays = 6;
            else if (pIn.DayOfWeek == DayOfWeek.Saturday)
                lDays = 5;
            else if (pIn.DayOfWeek == DayOfWeek.Friday)
                lDays = 4;
            else if (pIn.DayOfWeek == DayOfWeek.Thursday)
                lDays = 3;
            else if (pIn.DayOfWeek == DayOfWeek.Wednesday)
                lDays = 2;
            else if (pIn.DayOfWeek == DayOfWeek.Tuesday)
                lDays = 1;

            pIn = pIn.Subtract(new TimeSpan(lDays, 0, 0, 0));
            return pIn;
        }

        public static DateTime AdjustEndDate(DateTime pIn)
        {
            int lDays = 0;
            if (pIn.DayOfWeek == DayOfWeek.Sunday)
                return pIn;

            if (pIn.DayOfWeek == DayOfWeek.Monday)
                lDays = 6;
            else if (pIn.DayOfWeek == DayOfWeek.Tuesday)
                lDays = 5;
            else if (pIn.DayOfWeek == DayOfWeek.Wednesday)
                lDays = 4;
            else if (pIn.DayOfWeek == DayOfWeek.Thursday)
                lDays = 3;
            else if (pIn.DayOfWeek == DayOfWeek.Friday)
                lDays = 2;
            else if (pIn.DayOfWeek == DayOfWeek.Saturday)
                lDays = 1;

            pIn = pIn.Add(new TimeSpan(lDays, 0, 0, 0));
            return pIn;
        }
    }
}
