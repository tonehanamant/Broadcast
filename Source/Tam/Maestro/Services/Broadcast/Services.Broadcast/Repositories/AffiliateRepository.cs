using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IAffiliateRepository : IDataRepository
    {
        /// <summary>
        /// Gets all affiliates.
        /// </summary>
        /// <returns></returns>
        List<LookupDto> GetAllAffiliates();
    }

    public class AffiliateRepository : BroadcastRepositoryBase, IAffiliateRepository
    {
        public AffiliateRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) 
            : base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient, featureToggleHelper, configurationSettingsHelper)
        {
        }

        ///<inheritdoc/>
        public List<LookupDto> GetAllAffiliates()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.affiliates
                    .ToList()
                    .OrderBy(x => x.name)
                    .Select(_MapToLookupDto)
                    .ToList();
            });
        }

        private LookupDto _MapToLookupDto(affiliate affiliate)
        {
            return new LookupDto
            {
                Id = affiliate.id,
                Display = affiliate.name
            };
        }
    }
}
