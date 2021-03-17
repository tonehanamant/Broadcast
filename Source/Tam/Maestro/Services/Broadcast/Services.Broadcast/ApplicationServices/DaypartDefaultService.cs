using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IStandardDaypartService : IApplicationService
    {
        List<StandardDaypartDto> GetAllStandardDayparts();

        /// <summary>
        /// Gets all standard dayparts with all data.
        /// </summary>
        /// <returns></returns>
        List<StandardDaypartFullDto> GetAllStandardDaypartsWithAllData();
    }

    public class StandardDaypartService : IStandardDaypartService
    {        
        internal const string STANDARD_DAYPART_CODE_WEEKEND = "WKD";

        private readonly IStandardDaypartRepository _StandardDaypartRepository;
      

        public StandardDaypartService(IDataRepositoryFactory broadcastDataRepositoryFactory)
          
        {
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            
        }

        public List<StandardDaypartDto> GetAllStandardDayparts()
        {
            var dayparts = _StandardDaypartRepository.GetAllStandardDayparts();
          
            return dayparts;
        }

        ///<inheritdoc/>
        public List<StandardDaypartFullDto> GetAllStandardDaypartsWithAllData()
        {
            var standardDaypartDtos = _StandardDaypartRepository.GetAllStandardDaypartsWithAllData();
            DaypartTimeHelper.AddOneSecondToEndTime(standardDaypartDtos);
          
            return standardDaypartDtos;
        }

       
    }
}
