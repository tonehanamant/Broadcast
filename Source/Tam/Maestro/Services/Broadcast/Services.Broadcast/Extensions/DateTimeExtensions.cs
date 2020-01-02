using System;
using System.Collections.Generic;
using System.Linq;

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

        public static string ToFileDateFormat(this DateTime date)
        {
            return date.ToString("MMddyyyyy");
        }

        public static List<List<DateTime>> GroupConsecutiveDays(this List<DateTime> dates)
        {
            var groups = new List<List<DateTime>>();
            if (!dates.Any())
            {
                return groups;
            }

            dates.Sort();
            // the group for the first element
            var group1 = new List<DateTime>() { dates[0] };
            groups.Add(group1);

            DateTime lastDate = dates[0];
            for (int i = 1; i < dates.Count; i++)
            {
                DateTime currDate = dates[i];
                TimeSpan timeDiff = currDate - lastDate;

                //we create another group if the difference is more than 1 day
                bool isNewGroup = timeDiff.Days > 1;
                if (isNewGroup)
                {
                    groups.Add(new List<DateTime>());
                }
                groups.Last().Add(currDate);
                lastDate = currDate;
            }

            return groups;
        }
    }
}
