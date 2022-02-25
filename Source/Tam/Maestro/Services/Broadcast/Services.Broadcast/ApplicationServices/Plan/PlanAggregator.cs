using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAggregator"/> class.
        /// </summary>
        /// <param name="broadcastDataRepositoryFactory">The broadcast data repository factory.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine.</param>
        /// <param name="aabEngine">The aab engine.</param>
        public PlanAggregator(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IQuarterCalculationEngine quarterCalculationEngine
            , IAabEngine aabEngine)
        {
            _QuarterCalculationEngine = quarterCalculationEngine;

            _CampaignRepository = broadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();

            _AabEngine = aabEngine;
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

            var totalHiatusDays = plan.FlightHiatusDays.Count(h => _IsFlightDay(h, plan.FlightDays));

            var totalActiveDays = _GetActiveFlightDays(plan.FlightStartDate.Value, plan.FlightEndDate.Value, 
                plan.FlightHiatusDays, plan.FlightDays);

            summary.TotalActiveDays = totalActiveDays;
            summary.TotalHiatusDays = totalHiatusDays;
        }

        private static int _GetActiveFlightDays(DateTime start, DateTime end, List<DateTime> hiatusDays, List<int> flightDays)
        {
            var dateIndex = start.AddDays(-1);
            var activeDayCount = 0;

            do
            {
                dateIndex = dateIndex.AddDays(1);
                var isAFlightDay = _IsFlightDay(dateIndex, flightDays);
                if (isAFlightDay)
                {
                    var isAHiatusDay = hiatusDays.Any(h => h.Date == dateIndex.Date);
                    if (!isAHiatusDay)
                    {
                        activeDayCount++;
                    }
                }
            } while (dateIndex.Date < end.Date);
            return activeDayCount;
        }

        private static bool _IsFlightDay(DateTime dt, List<int> flightDays)
        {
            var result = flightDays.Contains((int)dt.GetBroadcastDayOfWeek());
            return result;
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
                .Select(q => new PlanSummaryQuarterDto { Quarter = q.Quarter, Year = q.Year })
                .ToList();

            summary.PlanSummaryQuarters = coveredQuarters;
        }

        internal void AggregateProduct(PlanDto plan, PlanSummaryDto summary)
        {
            ProductDto product = null;

            if (!plan.ProductMasterId.HasValue)
            {
                return;
            }
            var campaign = _CampaignRepository.GetCampaign(plan.CampaignId);
            product = _AabEngine.GetAdvertiserProduct(campaign.AdvertiserMasterId.Value, plan.ProductMasterId.Value);


            summary.ProductName = product.Name;
        }
    }
}