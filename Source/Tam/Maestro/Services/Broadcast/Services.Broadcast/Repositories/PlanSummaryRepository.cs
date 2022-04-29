using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System.Data.Entity;
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
        void SaveSummary(PlanSummaryDto summary);

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
        /// <param name="configurationSettingsHelper">The p configuration key.</param>
        public PlanSummaryRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        /// <inheritdoc />
        public void SetProcessingStatusForPlanSummary(int planVersionId, PlanAggregationProcessingStatusEnum processingStatus)
        {
            _InReadUncommitedTransaction(context =>
            {
                var entity = (from summary in context.plan_version_summaries
                              join version in context.plan_versions on summary.plan_version_id equals version.id
                              where version.id == planVersionId
                              select summary
                                ).SingleOrDefault($"More than one summary found for plan version id {planVersionId}.")
                             ?? new plan_version_summaries { plan_version_id = planVersionId };

                if (entity.id <= 0)
                {
                    context.plan_version_summaries.Add(entity);
                }

                entity.processing_status = (int)processingStatus;
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void SaveSummary(PlanSummaryDto summary)
        {
            _InReadUncommitedTransaction(context =>
            {
                var entity = context.plan_version_summaries
                                 .Where(s => s.plan_version_id == summary.VersionId)
                                 .Include(s => s.plan_version_summary_quarters)
                                 .SingleOrDefault($"More than one summary found for plan id {summary.PlanId}.")
                             ?? new plan_version_summaries { plan_version_id = summary.VersionId };

                _HydrateFromDto(entity, summary, context);

                if (entity.id <= 0)
                {
                    context.plan_version_summaries.Add(entity);
                }

                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public PlanSummaryDto GetSummaryForPlan(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = (from summary in context.plan_version_summaries
                              join version in context.plan_versions on summary.plan_version_id equals version.id
                              where version.id == version.plan.latest_version_id && version.plan_id == planId
                              select summary)
                                .Include(s => s.plan_version_summary_quarters)
                                .Include(s => s.plan_versions)
                                .Single($"No summary found for {planId}.");
                var dto = _MapToDto(entity);
                return dto;
            });
        }

        private void _HydrateFromDto(plan_version_summaries entity, PlanSummaryDto dto, QueryHintBroadcastContext context)
        {
            entity.hiatus_days_count = dto.TotalHiatusDays;
            entity.active_day_count = dto.TotalActiveDays;
            entity.available_market_count = dto.AvailableMarketCount;
            entity.available_market_with_sov_count = dto.AvailableMarketsWithSovCount;
            entity.available_market_total_us_coverage_percent = dto.AvailableMarketTotalUsCoveragePercent;
            entity.blackout_market_count = dto.BlackoutMarketCount;
            entity.blackout_market_total_us_coverage_percent = dto.BlackoutMarketTotalUsCoveragePercent;
            entity.product_name = dto.ProductName;
            entity.processing_status = (int)dto.ProcessingStatus;
            entity.fluidity_percentage = dto.FluidityPercentage;
            _HydrateQuartersFromDto(entity, dto, context);
        }

        private void _HydrateQuartersFromDto(plan_version_summaries entity, PlanSummaryDto dto, QueryHintBroadcastContext context)
        {
            context.plan_version_summary_quarters.RemoveRange(entity.plan_version_summary_quarters);
            dto.PlanSummaryQuarters.ForEach(q => entity.plan_version_summary_quarters.Add(new plan_version_summary_quarters { quarter = q.Quarter, year = q.Year }));
        }

        private PlanSummaryDto _MapToDto(plan_version_summaries entity)
        {
            var dto = new PlanSummaryDto
            {
                PlanId = entity.plan_versions.plan_id,
                ProcessingStatus = (PlanAggregationProcessingStatusEnum)entity.processing_status,
                TotalHiatusDays = entity.hiatus_days_count,
                TotalActiveDays = entity.active_day_count,
                AvailableMarketCount = entity.available_market_count,
                AvailableMarketsWithSovCount = entity.available_market_with_sov_count,
                AvailableMarketTotalUsCoveragePercent = entity.available_market_total_us_coverage_percent,
                BlackoutMarketCount = entity.blackout_market_count,
                BlackoutMarketTotalUsCoveragePercent = entity.blackout_market_total_us_coverage_percent,
                ProductName = entity.product_name,
                PlanSummaryQuarters = entity.plan_version_summary_quarters.Select(q => new PlanSummaryQuarterDto { Quarter = q.quarter, Year = q.year }).ToList()
            };
            return dto;
        }
    }
}