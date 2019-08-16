using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices.Plan
{
    /// <summary>
    /// Operations for working with aggregated plan data.
    /// </summary>
    public interface IPlanAggregator
    {
        PlanSummaryDto Aggregate(PlanDto plan);

        // TODO: remove this after PRI-11436 is implemented	
        void SetProducts(List<LookupDto> products);
        // END Block to Remove
    }

    /// <summary>
    /// Operations for working with aggregated plan data.
    /// </summary>
    public class PlanAggregator : IPlanAggregator
    {
        #region Fields

        private readonly IDaypartCodeRepository _DaypartCodeRepository;
        private readonly IAudienceRepository _AudienceRepository;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

        #endregion // #region Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAggregator"/> class.
        /// </summary>
        /// <param name="broadcastDataRepositoryFactory">The broadcast data repository factory.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine.</param>
        public PlanAggregator(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            _DaypartCodeRepository = broadcastDataRepositoryFactory.GetDataRepository<IDaypartCodeRepository>();
            _AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>();
            _QuarterCalculationEngine = quarterCalculationEngine;
        }

        #endregion // #region Constructor

        #region Operations

        // TODO: remove this after PRI-11436 is implemented
        private List<LookupDto> _Products;
        public void SetProducts(List<LookupDto> products)
        {
            _Products = products;
        }
        // END Block to Remove

        /// <inheritdoc/>
        public PlanSummaryDto Aggregate(PlanDto plan)
        {
            var summary = new PlanSummaryDto { PlanId = plan.Id };
            PerformAggregations(plan, summary);
            return summary;
        }

        #endregion // #region Operations

        #region Helpers

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

        #endregion Helpers

        #region AggregationFunctions

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
                AggregateAudience,
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
            var availableMarketCoverage = plan.AvailableMarkets.Sum(s => s.PercentageOfUs);

            summary.AvailableMarketCount = availableMarketCount;
            summary.AvailableMarketTotalUsCoveragePercent = availableMarketCoverage;
        }

        protected void AggregateBlackoutMarkets(PlanDto plan, PlanSummaryDto summary)
        {
            if (plan.BlackoutMarkets.Any() == false)
            {
                return;
            }

            var blackoutMarketCount = plan.BlackoutMarkets.Count;
            var blackoutMarketCoverage = plan.BlackoutMarkets.Sum(s => s.PercentageOfUs);

            summary.BlackoutMarketCount = blackoutMarketCount;
            summary.BlackoutMarketTotalUsCoveragePercent = blackoutMarketCoverage;
        }

        protected void AggregateAudience(PlanDto plan, PlanSummaryDto summary)
        {
            if (plan.AudienceId <= 0)
            {
                return;
            }

            var audience = _AudienceRepository.GetAudiencesByIds(new List<int> {plan.AudienceId})
                .Single("Invalid number of Primary Audiences.");
            var audienceName = audience.Display;

            summary.AudienceName = audienceName;
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

            var found = _Products.Single(p => p.Id == plan.ProductId, "Product not found.");
            summary.ProductName = found.Display;
        }

        #endregion // #region AggregationFunctions
    }
}