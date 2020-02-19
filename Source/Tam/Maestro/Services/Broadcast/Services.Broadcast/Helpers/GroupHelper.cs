using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Helpers
{
    public static class GroupHelper
    {
        public static string GroupWeekDays(List<int> selectedDays)
        {
            var daysOfWeek = new List<string> { "M", "TU", "W", "TH", "F", "SA", "SU" };
            var result = string.Empty;

            //construct the daypart days list
            for (int i = 0; i < daysOfWeek.Count; i++)
            {
                if (!selectedDays.Contains(i + 1))
                {
                    daysOfWeek[i] = null;
                }
            }

            var groupOfDays = daysOfWeek.GroupConnectedItems((a, b) => !string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b));
            var daysList = new List<string>();
            foreach (var group in groupOfDays)
            {
                //if the group contains 1 element, add it
                if (group.Count() == 1)
                {
                    daysList.Add(group.First());
                }
                else  //if the group contains more than 1 element, join the first and the last one with "-"
                {
                    daysList.Add($"{group.First()}-{group.Last()}");
                }
            }

            result = string.Join(",", daysList);
            return result;
        }
    }
}
