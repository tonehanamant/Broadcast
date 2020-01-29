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
        public static List<PlanDaypartDto> OrderDayparts(this List<PlanDaypartDto> planDaypartList, List<DaypartDefaultDto> daypartDefaults)
        {
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
                    planDaypart.WeightingGoalPercent
                }).ToList();

            // return the ordered list
            return mappedPlanDaypartList
                .OrderBy(planDaypart => planDaypart.Code)
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
                    WeightingGoalPercent = item.WeightingGoalPercent
                }).ToList();
        }

        public static List<DaypartData> OrderDayparts(this List<DaypartData> daypartList)
        {
            return daypartList
                .OrderBy(daypart => daypart.DaypartCode)
                .ThenBy(daypart => DaypartTimeHelper.ConvertFormattedTimeToSeconds(daypart.StartTime))
                .ThenBy(daypart => DaypartTimeHelper.ConvertFormattedTimeToSeconds(daypart.EndTime))
                .ToList();
        }
    }
}
