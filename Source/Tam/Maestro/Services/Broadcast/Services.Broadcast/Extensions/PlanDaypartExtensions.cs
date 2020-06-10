using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Extensions
{
    public static class PlanDaypartExtensions
    {
        public static List<PlanDaypartDto> OrderDayparts(this List<PlanDaypart> planDaypartList, List<DaypartDefaultDto> daypartDefaults)
        {
            const int MONDAY = 1;
            const int TUESDAY = 2;
            const int WEDNESDAY = 3;
            const int THURSDAY = 4;
            const int FRIDAY = 5;
            const int SATURDAY = 6;

            // join the plans dayparts with daypart defaults, so that later we can order them
            var mappedPlanDaypartList = planDaypartList.Join(
                daypartDefaults,
                planDaypart => planDaypart.DaypartCodeId,
                daypartDefault => daypartDefault.Id,
                (planDaypart, daypartDefault) => new
                {
                    planDaypart.DaypartCodeId,
                    daypartDefault.Code,
                    daypartDefault.FullName,
                    planDaypart.DaypartTypeId,
                    planDaypart.StartTimeSeconds,
                    planDaypart.EndTimeSeconds,
                    planDaypart.IsEndTimeModified,
                    planDaypart.IsStartTimeModified,
                    planDaypart.Restrictions,
                    planDaypart.WeightingGoalPercent,
                    planDaypart.FlightDays,
                    planDaypart.VpvhForAudiences
                }).ToList();

            // return the ordered list
            return mappedPlanDaypartList
                .OrderBy(planDaypart => planDaypart.Code)
                .ThenByDescending(planDaypart => planDaypart.FlightDays.Contains(MONDAY))
                .ThenByDescending(planDaypart => planDaypart.FlightDays.Contains(TUESDAY))
                .ThenByDescending(planDaypart => planDaypart.FlightDays.Contains(WEDNESDAY))
                .ThenByDescending(planDaypart => planDaypart.FlightDays.Contains(THURSDAY))
                .ThenByDescending(planDaypart => planDaypart.FlightDays.Contains(FRIDAY))
                .ThenByDescending(planDaypart => planDaypart.FlightDays.Contains(SATURDAY))
                .ThenBy(planDaypart => planDaypart.StartTimeSeconds)
                .ThenBy(planDaypart => planDaypart.EndTimeSeconds)
                .Select(item => new PlanDaypartDto
                {
                    DaypartCodeId = item.DaypartCodeId,
                    DaypartTypeId = item.DaypartTypeId,
                    StartTimeSeconds = item.StartTimeSeconds,
                    EndTimeSeconds = item.EndTimeSeconds,
                    IsEndTimeModified = item.IsEndTimeModified,
                    IsStartTimeModified = item.IsStartTimeModified,
                    Restrictions = item.Restrictions,
                    WeightingGoalPercent = item.WeightingGoalPercent,
                    VpvhForAudiences = item.VpvhForAudiences
                }).ToList();
        }

        public static List<DaypartData> OrderDayparts(this List<DaypartData> daypartList)
        {
            const string MONDAY = "M";
            const string TUESDAY = "TU";
            const string WEDNESDAY = "W";
            const string THURSDAY = "TH";
            const string FRIDAY = "F";
            const string SATURDAY = "SA";
            return daypartList
                .OrderBy(daypart => daypart.DaypartCode)
                .ThenByDescending(daypart => daypart.FlightDays.StartsWith(MONDAY))
                .ThenByDescending(daypart => daypart.FlightDays.StartsWith(TUESDAY))
                .ThenByDescending(daypart => daypart.FlightDays.StartsWith(WEDNESDAY))
                .ThenByDescending(daypart => daypart.FlightDays.StartsWith(THURSDAY))
                .ThenByDescending(daypart => daypart.FlightDays.StartsWith(FRIDAY))
                .ThenByDescending(daypart => daypart.FlightDays.StartsWith(SATURDAY))
                .ThenBy(daypart => DaypartTimeHelper.ConvertFormattedTimeToSeconds(daypart.StartTime))
                .ThenBy(daypart => DaypartTimeHelper.ConvertFormattedTimeToSeconds(daypart.EndTime))
                .ToList();
        }
    }
}
