using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IStandartDaypartEngine : IApplicationService
    {
        DaypartDefaultFullDto GetDaypartCodeByGenreAndTimeRange(string genre, TimeRange timeRange);
    }

    public class StandartDaypartEngine : IStandartDaypartEngine
    {
        private const string NEWS_GENRE = "News";

        private List<DaypartDefaultFullDto> _DaypartDefaults;

        public StandartDaypartEngine(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DaypartDefaults = dataRepositoryFactory
                .GetDataRepository<IDaypartDefaultRepository>()
                .GetAllDaypartDefaultsWithAllData();
        }

        public DaypartDefaultFullDto GetDaypartCodeByGenreAndTimeRange(string genre, TimeRange timeRange)
        {
            var daypartCodes = genre.Equals(NEWS_GENRE, StringComparison.InvariantCultureIgnoreCase) ?
                _DaypartDefaults.Where(x => x.DaypartType == DaypartTypeEnum.News) :
                _DaypartDefaults.Where(x => x.DaypartType != DaypartTypeEnum.News);

            var daypartCodeWithMostIntersectingTime = daypartCodes
                .OrderByDescending(x =>
                {
                    var daypartDefaultTimeRange = new TimeRange
                    {
                        StartTime = x.DefaultStartTimeSeconds,
                        EndTime = x.DefaultEndTimeSeconds
                    };

                    return DaypartTimeHelper.GetIntersectingTotalTime(timeRange, daypartDefaultTimeRange);
                })
                .First();

            return daypartCodeWithMostIntersectingTime;
        }
    }
}
