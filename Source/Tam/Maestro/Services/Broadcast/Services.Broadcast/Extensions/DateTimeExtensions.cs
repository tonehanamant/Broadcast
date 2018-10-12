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
    }
}
