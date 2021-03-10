using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.ApplicationServices.Plan
{
    /// <summary>
    /// Operations for working with aggregated plan data.
    /// </summary>
    public interface IPlanAggregator
    {
        PlanSummaryDto Aggregate(PlanDto plan);
    }

    /// <summary>
    /// Operations for working with aggregated plan data.
    /// </summary>
    public class PlanAggregator : IPlanAggregator
    {
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly IAabEngine _AabEngine;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly Lazy<bool> _IsAabEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAggregator"/> class.
        /// </summary>
        /// <param name="broadcastDataRepositoryFactory">The broadcast data repository factory.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine.</param>
        /// <param name="trafficApiCache">The api to the traffic app</param>
        public PlanAggregator(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IQuarterCalculationEngine quarterCalculationEngine
            , IAabEngine aabEngine
            , IFeatureToggleHelper featureToggleHelper)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;

            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();

            _AabEngine = aabEngine;
            _FeatureToggleHelper = featureToggleHelper;
            _IsAabEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION));

        }

        /// <inheritdoc/>
        public PlanSummaryDto Aggregate(PlanDto plan)
        {
            var summary = new PlanSummaryDto { PlanId = plan.Id, VersionId = plan.VersionId };
            PerformAggregations(plan, summary);
            return summary;
        }

        /// <summary>
        /// Performs the aggregations.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="summary">The summary.</param>
        /// <param name="runParallelAggregations">if set to <c>true</c> [run parallel aggregations].</param>
        internal void PerformAggregations(PlanDto plan, PlanSummaryDto summary, bool runParallelAggregations = true)
        {
            var aggFunctions = GetAggregationFunctionList();

            if (runParallelAggregations)
            {
                Parallel.ForEach(aggFunctions, (aggF) => aggF(plan, summary));
                return;
            }

            foreach (var aggF in aggFunctions)
            {
                aggF(plan, summary);
            }
        }

        private delegate void AggregationFunction(PlanDto plan, PlanSummaryDto summary);

        /// <summary>
        /// Gets the aggregation function list.
        /// </summary>
        /// <remarks>
        /// Register new aggregations to run here.
        /// </remarks>
        /// <returns></returns>
        private List<AggregationFunction> GetAggregationFunctionList()
        {
            var aggFunctions = new List<AggregationFunction>
            {
                AggregateFlightDays,
                AggregateAvailableMarkets,
                AggregateBlackoutMarkets,
                AggregateQuarters,
                AggregateProduct
            };
            return aggFunctions;
        }

        internal void AggregateFlightDays(PlanDto plan, PlanSummaryDto summary)
        {
            if (plan.FlightEndDate.HasValue == false)
            {
                return;
            }
            
            var totalHiatusDays = plan.FlightHiatusDays.Count;
            var totalFlightDays = Convert.ToInt32(plan.FlightEndDate.Value.Subtract(plan.FlightStartDate.Value).TotalDays) + 1;
            var totalActiveDays = totalFlightDays - totalHiatusDays;

            summary.TotalActiveDays = totalActiveDays;
            summary.TotalHiatusDays = totalHiatusDays;
        }

        internal void AggregateAvailableMarkets(PlanDto plan, PlanSummaryDto summary)
        {
            if (plan.AvailableMarkets.Any() == false)
            {
                return;
            }

            var availableMarketCount = plan.AvailableMarkets.Count;
            var availableMarketCoverage = plan.AvailableMarkets.Sum(s => s.PercentageOfUS);

            summary.AvailableMarketCount = availableMarketCount;
            summary.AvailableMarketTotalUsCoveragePercent = availableMarketCoverage;
            summary.AvailableMarketsWithSovCount = plan.AvailableMarkets.Where(x => x.ShareOfVoicePercent.HasValue).Count();
        }

        internal void AggregateBlackoutMarkets(PlanDto plan, PlanSummaryDto summary)
        {
            if (plan.BlackoutMarkets.Any() == false)
            {
                return;
            }

            var blackoutMarketCount = plan.BlackoutMarkets.Count;
            var blackoutMarketCoverage = plan.BlackoutMarkets.Sum(s => s.PercentageOfUS);

            summary.BlackoutMarketCount = blackoutMarketCount;
            summary.BlackoutMarketTotalUsCoveragePercent = blackoutMarketCoverage;
        }

        internal void AggregateQuarters(PlanDto plan, PlanSummaryDto summary)
        {
            if (plan.FlightStartDate.HasValue == false ||
                plan.FlightEndDate.HasValue == false)
            {
                return;
            }

            var coveredQuarters = _QuarterCalculationEngine
                .GetAllQuartersBetweenDates(plan.FlightStartDate.Value, plan.FlightEndDate.Value)
                .OrderBy(s => s.Year).ThenBy(s => s.Quarter)
                .Select(q => new PlanSummaryQuarterDto { Quarter = q.Quarter, Year = q.Year})
                .ToList();

            summary.PlanSummaryQuarters = coveredQuarters;
        }

        internal void AggregateProduct(PlanDto plan, PlanSummaryDto summary)
        {
            ProductDto product = null;
            if (_IsAabEnabled.Value)
            {
                if (!plan.ProductMasterId.HasValue)
                {
                    return;
                }
                var campaign = _CampaignRepository.GetCampaign(plan.CampaignId);
                product = _AabEngine.GetAdvertiserProduct(campaign.AdvertiserMasterId.Value, plan.ProductMasterId.Value);
            }
            else
            {
                if (!plan.ProductId.HasValue)
                {
                    return;
                }
                product = _AabEngine.GetProduct(plan.ProductId.Value);
            }

            summary.ProductName = product.Name;
        }
    }
}