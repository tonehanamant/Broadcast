using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Services.Broadcast.Helpers;
using System.Linq;
using System;

namespace Services.Broadcast.ApplicationServices
{
    public interface IDaypartDefaultService : IApplicationService
    {
        List<DaypartDefaultDto> GetAllDaypartDefaults();

        /// <summary>
        /// Gets the daypart code defaults.
        /// </summary>
        /// <returns>List of <see cref="DaypartDefaultFullDto"/></returns>
        List<DaypartDefaultFullDto> GetAllDaypartDefaultsWithAllData();
    }

    public class DaypartDefaultService : IDaypartDefaultService
    {
        internal const string DEFAULT_DAYPART_CODE_WEEKEND = "WKD";

        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;

        public DaypartDefaultService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
        }

        public List<DaypartDefaultDto> GetAllDaypartDefaults()
        {
            var dayparts = _DaypartDefaultRepository.GetAllDaypartDefaults();
            AssertToggle_EnableDaypartWKD(dayparts);
            return dayparts;
        }

        ///<inheritdoc/>
        public List<DaypartDefaultFullDto> GetAllDaypartDefaultsWithAllData()
        {
            var defaultDaypartDtos = _DaypartDefaultRepository.GetAllDaypartDefaultsWithAllData();
            DaypartTimeHelper.AddOneSecondToEndTime(defaultDaypartDtos);
            AssertToggle_EnableDaypartWKD(defaultDaypartDtos);
            return defaultDaypartDtos;
        }

        private void AssertToggle_EnableDaypartWKD<T>(List<T> dayparts) where T : DaypartDefaultDto
        {
            // BP-1076 - Part 1 : Filtering always is temporary.
            // BP-1076 - Part 2 will implement a feature flag around this.
            // This is here to allow dev of BP-1076 - Part 2.
            var toRemove = dayparts.SingleOrDefault(d => d.Code.Equals(DEFAULT_DAYPART_CODE_WEEKEND, StringComparison.OrdinalIgnoreCase));
            if (toRemove != null)
            {
                dayparts.Remove(toRemove);
            }
        }
    }
}
