using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IDaypartCodeService : IApplicationService
    {
        List<DaypartCodeDto> GetAllDaypartCodes();

        /// <summary>
        /// Gets the daypart code defaults.
        /// </summary>
        /// <returns>List of <see cref="DaypartCodeDefaultDto"/></returns>
        List<DaypartCodeDefaultDto> GetDaypartCodeDefaults();
    }

    public class DaypartCodeService : IDaypartCodeService
    {
        private readonly IDaypartCodeRepository _DaypartCodeRepository;

        public DaypartCodeService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _DaypartCodeRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartCodeRepository>();
        }

        public List<DaypartCodeDto> GetAllDaypartCodes()
        {
            return _DaypartCodeRepository.GetAllActiveDaypartCodes();
        }

        ///<inheritdoc/>
        public List<DaypartCodeDefaultDto> GetDaypartCodeDefaults()
        {
            List<DaypartCodeDefaultDto> defaultDaypartDtos = _DaypartCodeRepository.GetDaypartCodeDefaults();
            return defaultDaypartDtos;
        }
    }
}
