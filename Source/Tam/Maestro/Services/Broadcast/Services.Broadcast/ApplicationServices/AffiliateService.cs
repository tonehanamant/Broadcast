using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAffiliateService : IApplicationService
    {
        List<LookupDto> GetAllAffiliates();
    }

    public class AffiliateService : IAffiliateService
    {
        internal const string STATION_SECONDARY_AFFILIATIONS = "MyNet";
        private readonly IAffiliateRepository _AffiliateRepository;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private Lazy<bool> _IsStationSecondaryAffiliationsEnabled;

        public AffiliateService(IDataRepositoryFactory broadcastDataRepositoryFactory, IFeatureToggleHelper featureToggleHelper)
        {
            _AffiliateRepository = broadcastDataRepositoryFactory.GetDataRepository<IAffiliateRepository>();
            _FeatureToggleHelper = featureToggleHelper;
            _IsStationSecondaryAffiliationsEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_STATION_SECONDARY_AFFILIATIONS));
        }

        public List<LookupDto> GetAllAffiliates()
        {
            var affiliates = _AffiliateRepository.GetAllAffiliates();
            var affiliatesWithOrWithoutStationSecondaryAffiliations = new List<LookupDto>();
            if (!_IsStationSecondaryAffiliationsEnabled.Value)
            {
                foreach (var affiliate in affiliates)
                {
                    if (affiliate.Display != STATION_SECONDARY_AFFILIATIONS)
                    {
                        affiliatesWithOrWithoutStationSecondaryAffiliations.Add(affiliate);
                    }
                }
            }
            else
            {
                affiliatesWithOrWithoutStationSecondaryAffiliations = affiliates;
            }
            return affiliatesWithOrWithoutStationSecondaryAffiliations;
        }
    }
}
