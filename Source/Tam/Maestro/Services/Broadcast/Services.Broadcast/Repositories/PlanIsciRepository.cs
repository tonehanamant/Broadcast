using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IPlanIsciRepository : IDataRepository
    {
        /// <summary>
        /// list of Iscis based on search key.
        /// </summary>
        /// <param name="month">Isci search month input</param> 
        /// <param name="year">Isci search year input</param> 
        List<IsciAdvertiserDto> GetAvailableIscis(int month, int year);
    }
    /// <summary>
    /// Data operations for the PlanIsci Repository.
    /// </summary>
    /// <seealso cref="BroadcastRepositoryBase" />
    /// <seealso cref="IPlanIsciRepository" />
    public class PlanIsciRepository : BroadcastRepositoryBase, IPlanIsciRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        /// <param name="featureToggleHelper">The p configuration web API client.</param>
        /// <param name="configurationSettingsHelper">The p configuration web API client.</param>
        public PlanIsciRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }
        public List<IsciAdvertiserDto> GetAvailableIscis(int month, int year)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = (from isci in context.reel_iscis
                              join isci_adr in context.reel_isci_advertiser_name_references on isci.id equals isci_adr.reel_isci_id
                              join pro in context.reel_isci_products on isci.isci equals pro.isci
                              join sl in context.spot_lengths on isci.spot_length_id equals sl.id
                              where isci.active_start_date.Month <= month && isci.active_start_date.Year <= year &&
                                    isci.active_end_date.Month >= month && isci.active_end_date.Year >= year
                              select new IsciAdvertiserDto
                              {
                                  AdvertiserName = isci_adr.advertiser_name_reference,
                                  Id = isci.id,
                                  SpotLengthDuration = sl.length,
                                  ProductName = pro.product_name,
                                  Isci = isci.isci
                              }).ToList();
                return result;
            });
        }
    }
}
