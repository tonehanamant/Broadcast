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
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public StandardDaypartService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IFeatureToggleHelper featureToggleHelper)
        {
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _FeatureToggleHelper = featureToggleHelper;
        }

        public List<StandardDaypartDto> GetAllStandardDayparts()
        {
            var dayparts = _StandardDaypartRepository.GetAllStandardDayparts();
            AssertToggle_EnableDaypartWKD(dayparts);
            return dayparts;
        }

        ///<inheritdoc/>
        public List<StandardDaypartFullDto> GetAllStandardDaypartsWithAllData()
        {
            var standardDaypartDtos = _StandardDaypartRepository.GetAllStandardDaypartsWithAllData();
            DaypartTimeHelper.AddOneSecondToEndTime(standardDaypartDtos);
            AssertToggle_EnableDaypartWKD(standardDaypartDtos);
            return standardDaypartDtos;
        }

        private void AssertToggle_EnableDaypartWKD<T>(List<T> dayparts) where T : StandardDaypartDto
        {
            if (!_FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_DAYPART_WKD))
            {
                var toRemove = dayparts.SingleOrDefault(d => d.Code.Equals(STANDARD_DAYPART_CODE_WEEKEND, StringComparison.OrdinalIgnoreCase));
                if (toRemove != null)
                {
                    dayparts.Remove(toRemove);
                }
            }
        }
    }
}
