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
    }
}
