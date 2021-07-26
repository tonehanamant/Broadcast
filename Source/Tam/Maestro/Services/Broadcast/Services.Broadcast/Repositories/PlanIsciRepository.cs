using System;
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
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Repositories
{
    public interface IPlanIsciRepository : IDataRepository
    {
        /// <summary>
        /// list of Iscis based on search key.
        /// </summary>
        /// <param name="startDate">Isci search start date input</param> 
        /// <param name="endDate">Isci search end date input</param> 
        List<IsciAdvertiserDto> GetAvailableIscis(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the available plans for Isci mapping
        /// </summary>
        /// <param name="mediaMonthStartDate">The media month start date</param>
        /// <param name="mediaMonthEndDate">The media month end date</param>
        /// <returns>List of IsciPlanDetailDto object</returns>
        List<IsciPlanDetailDto> GetAvailableIsciPlans(DateTime mediaMonthStartDate, DateTime mediaMonthEndDate);
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
        public List<IsciAdvertiserDto> GetAvailableIscis(DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
              var result = (from isci in context.reel_iscis
                            join isci_adr in context.reel_isci_advertiser_name_references on isci.id equals isci_adr.reel_isci_id
                            join pro in context.reel_isci_products on isci.isci equals pro.isci into ps
                            from pro in ps.DefaultIfEmpty()
                            join sl in context.spot_lengths on isci.spot_length_id equals sl.id
                            where ( isci.active_start_date <=startDate &&  isci.active_end_date>=endDate)
                            ||(isci.active_start_date>=startDate && isci.active_start_date <= endDate)
                            || (isci.active_end_date >= startDate && isci.active_end_date <= endDate)
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

        /// <inheritdoc />
        public List<IsciPlanDetailDto> GetAvailableIsciPlans(DateTime mediaMonthStartDate, DateTime mediaMonthEndDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planEntities = (from plans in context.plans
                                    join plan_versions in context.plan_versions on plans.latest_version_id equals plan_versions.id
                                    where ((plan_versions.flight_start_date <= mediaMonthStartDate && plan_versions.flight_end_date >= mediaMonthEndDate)
                                              || (plan_versions.flight_start_date >= mediaMonthStartDate && plan_versions.flight_start_date <= mediaMonthEndDate)
                                              || (plan_versions.flight_end_date >= mediaMonthStartDate && plan_versions.flight_end_date <= mediaMonthEndDate))
                                          && (plan_versions.status == (int)PlanStatusEnum.Contracted
                                              || plan_versions.status == (int)PlanStatusEnum.Live
                                              || plan_versions.status == (int)PlanStatusEnum.Complete)
                                    select plans).ToList();

                var result = planEntities.Select(plan =>
                {
                    var planVersion = plan.plan_versions.Where(x => x.id == plan.latest_version_id).Single();
                    var planVersionSummary = planVersion.plan_version_summaries.Single();
                    var isciPlanDetail = new IsciPlanDetailDto()
                    {
                        Id = plan.id,
                        Title = plan.name,
                        AdvertiserMasterId = plan.campaign.advertiser_master_id,
                        SpotLengthValues = planVersion.plan_version_creative_lengths.Select(x => x.spot_lengths.length).ToList(),
                        AudienceCode = planVersion.audience.code,
                        Dayparts = planVersion.plan_version_dayparts.Select(d => d.standard_dayparts.code).ToList(),
                        FlightStartDate = planVersion.flight_start_date,
                        FlightEndDate = planVersion.flight_end_date,
                        ProductName = planVersionSummary.product_name,
                        IsciAdvertisers = plan.plan_iscis.Where(x => x.deleted_at == null).Select(x => _MapToIsciAdvertiserDto(x, context)).ToList()
                    };
                    return isciPlanDetail;
                }).ToList();
                return result;
            });
        }

        private IsciAdvertiserDto _MapToIsciAdvertiserDto(plan_iscis planIscis, QueryHintBroadcastContext context)
        {
            var isciEntities = (from reel_iscis in context.reel_iscis
                               join spot_lengths in context.spot_lengths on reel_iscis.spot_length_id equals spot_lengths.id
                               join rip in context.reel_isci_products on reel_iscis.isci equals rip.isci into reel_isci_products_group
                               from reel_isci_products in reel_isci_products_group.DefaultIfEmpty()
                               where reel_iscis.isci == planIscis.isci
                               select new
                               {
                                   SpotLengthDuration = spot_lengths.length,
                                   ProductName = reel_isci_products.product_name
                               }).SingleOrDefault();

            var isciAdvertiser = new IsciAdvertiserDto()
            {
                Id = planIscis.id,
                Isci = planIscis.isci,
                SpotLengthDuration = isciEntities?.SpotLengthDuration ?? 0,
                ProductName = isciEntities?.ProductName
            };
            return isciAdvertiser;
        }
    }
}
