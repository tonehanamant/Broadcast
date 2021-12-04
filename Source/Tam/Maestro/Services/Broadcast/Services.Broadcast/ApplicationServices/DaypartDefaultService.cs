using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
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
        internal const string CUSTOM_DAYPART_SPORTS_CODE = "CSP";

        private readonly IStandardDaypartRepository _StandardDaypartRepository;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsCustomDaypartEnabled;


        public StandardDaypartService(IDataRepositoryFactory broadcastDataRepositoryFactory, IFeatureToggleHelper featureToggleHelper)
          
        {
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _FeatureToggleHelper = featureToggleHelper;
            _IsCustomDaypartEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_CUSTOM_DAYPART));
        }

        public List<StandardDaypartDto> GetAllStandardDayparts()
        {
            var dayparts = _StandardDaypartRepository.GetAllStandardDayparts();

            if (!_IsCustomDaypartEnabled.Value)
            {
                var filtered = dayparts.Where(s => !s.Code.Equals(CUSTOM_DAYPART_SPORTS_CODE)).ToList();
                dayparts = filtered;
            }
          
            return dayparts;
        }

        ///<inheritdoc/>
        public List<StandardDaypartFullDto> GetAllStandardDaypartsWithAllData()
        {
            var standardDaypartDtos = _StandardDaypartRepository.GetAllStandardDaypartsWithAllData();

            if (!_IsCustomDaypartEnabled.Value)
            {
                var filtered = standardDaypartDtos.Where(s => s.DaypartType != DaypartTypeEnum.Sports).ToList();
                standardDaypartDtos = filtered;
            }

            DaypartTimeHelper.AddOneSecondToEndTime(standardDaypartDtos);

            return standardDaypartDtos;
        }
    }
}
