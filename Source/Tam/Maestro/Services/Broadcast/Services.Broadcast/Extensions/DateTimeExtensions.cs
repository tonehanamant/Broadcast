using System;

namespace Services.Broadcast.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetNextMonday(this DateTime date)
        {
            var monday = date;

            while(true)
            {
                if (monday.DayOfWeek == DayOfWeek.Monday)
                {
                    return monday;
                }

                monday = monday.AddDays(1);
            }
        }

        public static DateTime GetWeekMonday(this DateTime date)
        {
            var differenceToMonday = (date.DayOfWeek - DayOfWeek.Monday);

            if (differenceToMonday < 0)
                differenceToMonday += 7;

            return date.AddDays(-differenceToMonday);
        }

        public static string ToFileDateFormat(this DateTime date, string format = null)
        {
            return date.ToString(format ?? "MMddyyyyy");
        }
    }
}
