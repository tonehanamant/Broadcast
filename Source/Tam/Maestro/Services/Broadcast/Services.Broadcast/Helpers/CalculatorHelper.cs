using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Helpers
{
    public static class CalculatorHelper
    {
        /// <summary>Gets the active dates between dates.</summary>
        /// <param name="weekStartDate">Start date of the week.</param>
        /// <param name="weekEndDate">End date of the week.</param>
        /// <param name="flightDays">The flightdays.</param>
        /// <param name="hiatusDays">The hiatusDays.</param>
        /// <param name="planDaypartDayIds">The plan daypart day ids.</param>       
        ///  <param name="activeDaysString">Output parameter activeDaysString.</param>
        /// <returns></returns>
        public static int CalculateActiveDays(DateTime weekStartDate, DateTime weekEndDate,
           List<int> flightDays, List<DateTime> hiatusDays, List<int> planDaypartDayIds,
           out string activeDaysString)
        {
            var daysOfWeek = new List<string> { "M", "Tu", "W", "Th", "F", "Sa", "Su" };
            activeDaysString = string.Empty;
            var hiatusDaysInWeek = hiatusDays.Where(x => weekStartDate <= x && weekEndDate >= x).ToList();

            var days = new List<int> { 1, 2, 3, 4, 5, 6, 7 };          
            var nonPlanDaypartDays = days.Except(planDaypartDayIds);

            var nonFlightDays = days.Except(flightDays);

            var toRemove = nonFlightDays.Union(nonPlanDaypartDays).Distinct();
            foreach (var day in toRemove)
            {
                daysOfWeek[day - 1] = null;
            }

            //if all the week is hiatus, return 0 active days
            if (hiatusDaysInWeek.Count == 7)
            {
                return 0;
            }

            //construct the active days string
            //null the hiatus days in the week
            for (int i = 0; i < daysOfWeek.Count; i++)
            {
                if (hiatusDaysInWeek.Contains(weekStartDate.AddDays(i)))
                {
                    daysOfWeek[i] = null;
                }
            }

            //group the active days that are not null
            var groupOfActiveDays = daysOfWeek.GroupConnectedItems((a, b) => !string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b));
            var activeDaysList = new List<string>();
            foreach (var group in groupOfActiveDays)
            {
                //if the group contains 1 or 2 elements, join them by comma
                if (group.Count() == 1 || group.Count() == 2)
                {
                    activeDaysList.Add(string.Join(",", group));
                }
                else  //if the group contains more then 3 elements, join the first and the last one with "-"
                {
                    activeDaysList.Add($"{group.First()}-{group.Last()}");
                }
            }

            activeDaysString = string.Join(",", activeDaysList);
            var numberOfActiveDays = daysOfWeek.Where(x => x != null).Count();
            //number of active days this week is 7 minus number of hiatus days
            return numberOfActiveDays;
        }       
    }
}
