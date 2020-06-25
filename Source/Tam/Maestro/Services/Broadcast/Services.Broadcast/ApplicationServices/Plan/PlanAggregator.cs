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
        private readonly IAudienceRepository _AudienceRepository;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly ITrafficApiCache _TrafficApiCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAggregator"/> class.
        /// </summary>
        /// <param name="broadcastDataRepositoryFactory">The broadcast data repository factory.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine.</param>
        /// <param name="trafficApiCache">The api to the traffic app</param>
        public PlanAggregator(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IQuarterCalculationEngine quarterCalculationEngine
            , ITrafficApiCache trafficApiCache)
        {
            _AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>();
            _QuarterCalculationEngine = quarterCalculationEngine;
            _TrafficApiCache = trafficApiCache;
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
        protected void PerformAggregations(PlanDto plan, PlanSummaryDto summary, bool runParallelAggregations = true)
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

        protected void AggregateFlightDays(PlanDto plan, PlanSummaryDto summary)
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

        protected void AggregateAvailableMarkets(PlanDto plan, PlanSummaryDto summary)
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

        protected void AggregateBlackoutMarkets(PlanDto plan, PlanSummaryDto summary)
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

        protected void AggregateQuarters(PlanDto plan, PlanSummaryDto summary)
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

        protected void AggregateProduct(PlanDto plan, PlanSummaryDto summary)
        {
            if (plan.ProductId < 1)
            {
                return;
            }

            summary.ProductName = _TrafficApiCache.GetProduct(plan.ProductId).Name;
        }
    }
}