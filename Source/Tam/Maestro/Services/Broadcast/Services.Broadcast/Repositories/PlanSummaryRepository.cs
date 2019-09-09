using System.Data.Entity;
using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    /// <summary>
    /// Data access operations for plan summary date.
    /// </summary>
    public interface IPlanSummaryRepository : IDataRepository
    {
        /// <summary>
        /// Saves the summary.
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <returns>The id of the summary saved.</returns>
        int SaveSummary(PlanSummaryDto summary);

        /// <summary>
        /// Gets the summary for plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>The summary for the identified plan.</returns>
        PlanSummaryDto GetSummaryForPlan(int planId);

        /// <summary>
        /// Sets the aggregation processing status for plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="processingStatus">The processing status.</param>
        void SetProcessingStatusForPlanSummary(int planId, PlanAggregationProcessingStatusEnum processingStatus);
    }

    /// <summary>
    /// Data access operations for plan summary date.
    /// </summary>
    public class PlanSummaryRepository : BroadcastRepositoryBase, IPlanSummaryRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanSummaryRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        public PlanSummaryRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
        {
        }

        /// <inheritdoc />
        public void SetProcessingStatusForPlanSummary(int planId, PlanAggregationProcessingStatusEnum processingStatus)
        {
            _InReadUncommitedTransaction(context =>
            {
                var entity = context.plan_summary
                                 .Where(s => s.plan_id == planId)
                                 .SingleOrDefault($"More than one summary found for plan id {planId}.")
                             ?? new plan_summary { plan_id = planId, processing_status = (int)processingStatus};

                if (entity.id <= 0)
                {
                    context.plan_summary.Add(entity);
                }

                entity.processing_status = (int) processingStatus;
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public int SaveSummary(PlanSummaryDto summary)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = context.plan_summary
                                 .Where(s => s.plan_id == summary.PlanId)
                                 .Include(s => s.plan_summary_quarters)
                                 .SingleOrDefault($"More than one summary found for plan id {summary.PlanId}.")
                             ?? new plan_summary { plan_id = summary.PlanId };

                    _HydrateFromDto(entity, summary, context);

                    if (entity.id <= 0)
                    {
                        context.plan_summary.Add(entity);
                    }

                    context.SaveChanges();
                    return entity.id;
                });
        }

        /// <inheritdoc />
        public PlanSummaryDto GetSummaryForPlan(int planId)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var entity = context.plan_summary
                        .Where(s => s.plan_id == planId)
                        .Include(s => s.plan_summary_quarters)
                        .Single($"No summary found for {planId}.");
                    var dto = _MapToDto(entity);
                    return dto;
                });
        }

        private void _HydrateFromDto(plan_summary entity, PlanSummaryDto dto, QueryHintBroadcastContext context)
        {
            entity.plan_id = dto.PlanId;
            entity.hiatus_days_count = dto.TotalHiatusDays;
            entity.active_day_count = dto.TotalActiveDays;
            entity.available_market_count = dto.AvailableMarketCount;
            entity.available_market_total_us_coverage_percent = dto.AvailableMarketTotalUsCoveragePercent;
            entity.blackout_market_count = dto.BlackoutMarketCount;
            entity.blackout_market_total_us_coverage_percent = dto.BlackoutMarketTotalUsCoveragePercent;
            entity.product_name = dto.ProductName;
            entity.processing_status = (int)dto.ProcessingStatus;
            entity.audience_name = dto.AudienceName;
            _HydrateQuartersFromDto(entity, dto, context);
        }

        private void _HydrateQuartersFromDto(plan_summary entity, PlanSummaryDto dto, QueryHintBroadcastContext context)
        {
            context.plan_summary_quarters.RemoveRange(entity.plan_summary_quarters);
            dto.PlanSummaryQuarters.ForEach(q => entity.plan_summary_quarters.Add(new plan_summary_quarters { quarter = q.Quarter, year = q.Year }));
        }

        private PlanSummaryDto _MapToDto(plan_summary entity)
        {
            var dto = new PlanSummaryDto
            {
                PlanId = entity.plan_id,
                ProcessingStatus = (PlanAggregationProcessingStatusEnum)entity.processing_status,
                TotalHiatusDays = entity.hiatus_days_count,
                TotalActiveDays = entity.active_day_count,
                AvailableMarketCount = entity.available_market_count,
                AvailableMarketTotalUsCoveragePercent = entity.available_market_total_us_coverage_percent,
                BlackoutMarketCount = entity.blackout_market_count,
                BlackoutMarketTotalUsCoveragePercent = entity.blackout_market_total_us_coverage_percent,
                ProductName = entity.product_name,
                AudienceName = entity.audience_name,
                PlanSummaryQuarters = entity.plan_summary_quarters.Select(q => new PlanSummaryQuarterDto { Quarter = q.quarter, Year = q.year}).ToList()
            };
            return dto;
        }
    }
}