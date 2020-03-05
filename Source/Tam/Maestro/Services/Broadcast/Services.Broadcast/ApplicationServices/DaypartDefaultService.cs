using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Services.Broadcast.Helpers;

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
        private readonly IDaypartDefaultRepository _DaypartDefaultRepository;

        public DaypartDefaultService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _DaypartDefaultRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartDefaultRepository>();
        }

        public List<DaypartDefaultDto> GetAllDaypartDefaults()
        {
            return _DaypartDefaultRepository.GetAllDaypartDefaults();
        }

        ///<inheritdoc/>
        public List<DaypartDefaultFullDto> GetAllDaypartDefaultsWithAllData()
        {
            var defaultDaypartDtos = _DaypartDefaultRepository.GetAllDaypartDefaultsWithAllData();
            DaypartTimeHelper.AddOneSecondToEndTime(defaultDaypartDtos);
            return defaultDaypartDtos;
        }
    }
}
