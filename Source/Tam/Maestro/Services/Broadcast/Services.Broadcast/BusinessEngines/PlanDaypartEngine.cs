using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanDaypartEngine : IApplicationService
    {
        PlanDaypartDto FindPlanDaypartWithMostIntersectingTime(
            List<PlanDaypartDto> planDayparts,
            string genre, 
            TimeRange timeRange);
    }

    public class PlanDaypartEngine : IPlanDaypartEngine
    {
        private const string NEWS_GENRE = "News";

        public PlanDaypartDto FindPlanDaypartWithMostIntersectingTime(
            List<PlanDaypartDto> planDayparts, 
            string genre, 
            TimeRange timeRange)
        {
            var planDaypartsForGenre = genre.Equals(NEWS_GENRE, StringComparison.InvariantCultureIgnoreCase) ?
                planDayparts.Where(x => x.DaypartTypeId == DaypartTypeEnum.News) :
                planDayparts.Where(x => x.DaypartTypeId != DaypartTypeEnum.News);

            var result = planDaypartsForGenre
                .OrderByDescending(x =>
                {
                    var daypartTimeRange = new TimeRange
                    {
                        StartTime = x.StartTimeSeconds,
                        EndTime = x.EndTimeSeconds
                    };

                    return DaypartTimeHelper.GetIntersectingTotalTime(timeRange, daypartTimeRange);
                })
                .FirstOrDefault();

            return result;
        }
    }
}
