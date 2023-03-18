using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    /// <summary>
    /// Data accessors for CampaignSummary data.
    /// </summary>
    /// <seealso cref="Common.Services.Repositories.IDataRepository" />
    public interface ICampaignSummaryRepository : IDataRepository
    {
        /// <summary>
        /// Saves the summary.
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <returns></returns>
        int SaveSummary(CampaignSummaryDto summary);

        /// <summary>
        /// Sets the summary processing status to in progress.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="queuedBy">The queued by.</param>
        /// <param name="queuedAt">The queued at.</param>
        void SetSummaryProcessingStatusToInProgress(int campaignId, string queuedBy, DateTime queuedAt);

        /// <summary>
        /// Sets the summary processing status to error.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        void SetSummaryProcessingStatusToError(int campaignId, string errorMessage);

        /// <summary>
        /// Gets the summary for the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        CampaignSummaryDto GetSummaryForCampaign(int campaignId);
    }

    /// <summary>
    /// Data accessors for CampaignSummary data.
    /// </summary>
    /// <seealso cref="Common.Services.Repositories.BroadcastRepositoryBase" />
    /// <seealso cref="ICampaignSummaryRepository" />
    public class CampaignSummaryRepository : BroadcastRepositoryBase, ICampaignSummaryRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignSummaryRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="configurationSettingsHelper">The p configuration web API client.</param>
        public CampaignSummaryRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        /// <inheritdoc />
        public CampaignSummaryDto GetSummaryForCampaign(int campaignId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                campaign_summaries entity = context.campaign_summaries
                    .Where(s => s.campaign_id == campaignId)
                    .SingleOrDefault($"Too many summaries found for campaign '{campaignId}'.");

                var summary = entity == null ? null : new CampaignSummaryDto
                {
                    ProcessingStatus = (CampaignAggregationProcessingStatusEnum) entity.processing_status,
                    ProcessingErrorMessage = entity.processing_status_error_msg,
                    CampaignId = entity.campaign_id,
                    QueuedAt = entity.queued_at,
                    QueuedBy = entity.queued_by,
                    FlightStartDate = entity.flight_start_Date,
                    FlightEndDate = entity.flight_end_Date,
                    FlightHiatusDays = entity.flight_hiatus_days,
                    FlightActiveDays = entity.flight_active_days,
                    Budget = _ToNullableDecimal(entity.budget),
                    HHCPM = _ToNullableDecimal(entity.hh_cpm),
                    HHImpressions = entity.hh_impressions,
                    HHRatingPoints = entity.hh_rating_points,
                    HHAduImpressions = entity.hh_adu_impressions,
                    CampaignStatus = (PlanStatusEnum?) entity.campaign_status,
                    PlanStatusCountWorking = entity.plan_status_count_working,
                    PlanStatusCountReserved = entity.plan_status_count_reserved,
                    PlanStatusCountClientApproval = entity.plan_status_count_client_approval,
                    PlanStatusCountContracted = entity.plan_status_count_contracted,
                    PlanStatusCountLive = entity.plan_status_count_live,
                    PlanStatusCountComplete = entity.plan_status_count_complete,
                    PlanStatusCountScenario = entity.plan_status_count_scenario,
                    PlanStatusCountCanceled = entity.plan_status_count_canceled,
                    PlanStatusCountRejected = entity.plan_status_count_rejected,
                    ComponentsModified = entity.components_modified,
                    LastAggregated = entity.last_aggregated
                };

                return summary;
            });
        }
        
        private decimal? _ToNullableDecimal(double? candidate)
        {
            return candidate.HasValue ? Convert.ToDecimal(candidate) : (decimal?)null;
        }

        /// <inheritdoc />
        public void SetSummaryProcessingStatusToInProgress(int campaignId, string queuedBy, DateTime queuedAt)
        {
            _InReadUncommitedTransaction(context =>
            {
                var entity = context.campaign_summaries
                                 .Where(e => e.campaign_id == campaignId)
                                 .SingleOrDefault($"More than one summary found for campaign id {campaignId}")
                             ?? new campaign_summaries { campaign_id = campaignId, queued_at = queuedAt, queued_by = queuedBy };

                entity.processing_status = (int)CampaignAggregationProcessingStatusEnum.InProgress;
                entity.processing_status_error_msg = null;
                entity.queued_at = queuedAt;
                entity.queued_by = queuedBy;

                if (entity.id < 1)
                {
                    context.campaign_summaries.Add(entity);
                }
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void SetSummaryProcessingStatusToError(int campaignId, string errorMessage)
        {
            _InReadUncommitedTransaction(context =>
            {
                var entity = context.campaign_summaries
                    .Where(e => e.campaign_id == campaignId)
                    .Single($"No summary found for campaign id {campaignId}");

                entity.processing_status = (int)CampaignAggregationProcessingStatusEnum.Error;
                entity.processing_status_error_msg = errorMessage;

                if (entity.id < 1)
                {
                    context.campaign_summaries.Add(entity);
                }

                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public int SaveSummary(CampaignSummaryDto summary)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = context.campaign_summaries
                                 .Where(e => e.campaign_id == summary.CampaignId)
                                 .SingleOrDefault($"More than one summary found for campaign id {summary.CampaignId}")
                             ?? new campaign_summaries { campaign_id = summary.CampaignId };

                if (!_IsSafeToSave(entity, summary))
                {
                    _LogUnsafeToSave(entity, summary);
                    return entity.id;
                }

                _HydrateFromDto(entity, summary);

                if (entity.id <= 0)
                {
                    context.campaign_summaries.Add(entity);
                }

                context.SaveChanges();
                return entity.id;
            });
        }

        private bool _IsSafeToSave(campaign_summaries entity, CampaignSummaryDto dto)
        {
            if (entity.components_modified >= dto.ComponentsModified)
            {
                return false;
            }
            return true;
        }

        private void _LogUnsafeToSave(campaign_summaries entity, CampaignSummaryDto summary)
        {
            var message = $@"A campaign summary save would stomp on data with a more recent component date.
                            CampaignID : {summary.CampaignId};
                            SummaryId : {entity.id};
                            Entity.ComponentsModified : {entity.components_modified};
                            Summary.ComponentsModified : {summary.ComponentsModified};
                            Summary.QueuedAt : {summary.QueuedAt};
                            Summary.QueuedBy : {summary.QueuedBy};";

            _LogWarning(message);
        }
        
        private void _HydrateFromDto(campaign_summaries entity, CampaignSummaryDto dto)
        {
            entity.processing_status = (int) dto.ProcessingStatus;
            entity.processing_status_error_msg = dto.ProcessingErrorMessage;
            entity.flight_start_Date = dto.FlightStartDate;
            entity.flight_end_Date = dto.FlightEndDate;
            entity.flight_hiatus_days = dto.FlightHiatusDays;
            entity.flight_active_days = dto.FlightActiveDays;
            entity.budget = _ToNullableDouble(dto.Budget);
            entity.hh_cpm = _ToNullableDouble(dto.HHCPM);
            entity.hh_impressions = dto.HHImpressions;
            entity.hh_rating_points = dto.HHRatingPoints;
            entity.hh_adu_impressions = dto.HHAduImpressions;
            entity.campaign_status = (int?) dto.CampaignStatus;
            entity.components_modified = dto.ComponentsModified;
            entity.last_aggregated = dto.LastAggregated;
            entity.plan_status_count_working = dto.PlanStatusCountWorking;
            entity.plan_status_count_reserved = dto.PlanStatusCountReserved;
            entity.plan_status_count_client_approval = dto.PlanStatusCountClientApproval;
            entity.plan_status_count_contracted = dto.PlanStatusCountContracted;
            entity.plan_status_count_live = dto.PlanStatusCountLive;
            entity.plan_status_count_complete = dto.PlanStatusCountComplete;
            entity.plan_status_count_scenario = dto.PlanStatusCountScenario;
            entity.plan_status_count_canceled = dto.PlanStatusCountCanceled;
            entity.plan_status_count_rejected = dto.PlanStatusCountRejected;

            // don't want to overwrite the existing value on these
            entity.queued_by = string.IsNullOrWhiteSpace(entity.queued_by) ? dto.QueuedBy : entity.queued_by;
            entity.queued_at = entity.queued_at == DateTime.MinValue ? dto.QueuedAt : entity.queued_at;
        }

        private double? _ToNullableDouble(decimal? candidate)
        {
            return candidate.HasValue ? Convert.ToDouble(candidate) : (double?)null;
        }
    }
}