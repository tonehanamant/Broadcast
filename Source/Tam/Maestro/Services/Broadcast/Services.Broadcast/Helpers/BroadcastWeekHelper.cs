using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Helpers
{
    public class BroadcastWeek
    {
        public DateTime WeekStartDate { get; private set; }

        public DateTime WeekEndDate { get; private set; }

        public BroadcastWeek(DateTime startDate, DateTime endDate)
        {
            WeekStartDate = startDate;
            WeekEndDate = endDate;
        }
    }

    public static class BroadcastWeeksHelper
    {
        public static BroadcastWeek GetContainingWeek(DateTime candidate)
        {
            var daysPastStart = GetDaysPastWeekStart(candidate);
            var daysUntilEnd = GetDaysUntilWeekEnd(candidate);

            var result = new BroadcastWeek(
                startDate: candidate.AddDays(daysPastStart * -1),
                endDate: candidate.AddDays(daysUntilEnd));

            return result;
        }

        public static List<BroadcastWeek> GetContainingWeeks(DateTime startDate, DateTime endDate)
        {
            var weeksResult = new List<BroadcastWeek>();

            var startWeek = GetContainingWeek(startDate);
            var endWeek = GetContainingWeek(endDate);

            var indexWeek = new BroadcastWeek(startWeek.WeekStartDate, startWeek.WeekEndDate);
            while (indexWeek.WeekStartDate <= endWeek.WeekStartDate)
            {
                weeksResult.Add(indexWeek);
                indexWeek = new BroadcastWeek(indexWeek.WeekStartDate.AddDays(7), indexWeek.WeekEndDate.AddDays(7));
            }

            return weeksResult;
        }

        public static int GetDaysPastWeekStart(DateTime candidate)
        {
            switch (candidate.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
                default:
                    return 0;
            }
        }

        public static int GetDaysUntilWeekEnd(DateTime candidate)
        {
            switch (candidate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 6;
                case DayOfWeek.Tuesday:
                    return 5;
                case DayOfWeek.Wednesday:
                    return 4;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 2;
                case DayOfWeek.Saturday:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
