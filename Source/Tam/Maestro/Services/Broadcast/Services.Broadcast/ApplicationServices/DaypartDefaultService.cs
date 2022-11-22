﻿using Common.Services.ApplicationServices;
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


        public StandardDaypartService(IDataRepositoryFactory broadcastDataRepositoryFactory, IFeatureToggleHelper featureToggleHelper)
          
        {
            _StandardDaypartRepository = broadcastDataRepositoryFactory.GetDataRepository<IStandardDaypartRepository>();
            _FeatureToggleHelper = featureToggleHelper;
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
