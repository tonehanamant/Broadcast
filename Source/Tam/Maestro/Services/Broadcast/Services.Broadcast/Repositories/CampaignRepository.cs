using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    /// <summary>
    /// Data operations for the Campaign Repository.
    /// </summary>
    /// <seealso cref="Common.Services.Repositories.IDataRepository" />
    public interface ICampaignRepository : IDataRepository
    {
        /// <summary>
        /// Gets the campaigns filtered by parameters.
        /// </summary>
        /// <param name="startDate">The start date to filter campaigns by</param>
        /// <param name="endDate">The end  date to filter the campaigns by</param>
        /// <param name="planStatus">The plan status to filter the campaigns by</param>
        /// <returns></returns>
        List<CampaignWithSummary> GetCampaignsWithSummary(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus);

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>CampaignDto object</returns>
        CampaignDto GetCampaign(int id);

        /// <summary>
        /// Creates the campaign.
        /// </summary>
        /// <param name="campaignDto">The campaign dto.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new campaign</returns>
        int CreateCampaign(SaveCampaignDto campaignDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Updates the campaign.
        /// </summary>
        /// <param name="campaignDto">The campaign dto.</param>
        /// <returns>Id of the campaign updated</returns>
        int UpdateCampaign(SaveCampaignDto campaignDto);

        /// <summary>
        /// Gets the list of all date ranges for campaign's plans
        /// </summary>
        /// <param name="planStatus">The status of the plans to filter the dates by.</param>
        /// <returns>A list of date ranges.</returns>
        List<DateRange> GetCampaignsDateRanges(PlanStatusEnum? planStatus);

        /// <summary>
        /// Gets the list of all statuses of the campaigns for the campaigns listing
        /// </summary>
        /// <param name="startDate">The start date to filter statuses by</param>
        /// <param name="endDate">The end  date to filter the statuses by</param>
        /// <returns></returns>
        List<PlanStatusEnum> GetCampaignsStatuses(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Updates the campaign last modified date.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="modifiedDate">The modified date.</param>
        /// <param name="modifiedBy">The modified by.</param>
        void UpdateCampaignLastModified(int campaignId, DateTime modifiedDate, string modifiedBy);
        /// <summary>
        /// Updates the unified campaign last sentat date.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="unifiedCampaignLastSentAt">unified campaign last sentat date.</param>
        void UpdateUnifiedCampaignLastSentAt(int campaignId, DateTime unifiedCampaignLastSentAt);

        /// <summary>
        /// Gets the campaign copy.
        /// </summary>
        /// <param name="campaignId">The identifier.</param>
        /// <returns>CampaignCopyDto object</returns>
        CampaignCopyDto GetCampaignCopy(int campaignId);

        /// <summary>
        /// Check if the campaign already exist in DB
        /// </summary>
        /// <param name="saveCampaignDto">The campaign dto.</param>
        /// <returns>CampaignDto object</returns>
        CampaignDto CheckCampaignExist(SaveCampaignDto saveCampaignDto);
        /// <summary>
        /// Gets the campaign with plans
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>returns CampaignExportDto object</returns>
        CampaignExportDto GetCampaignPlanForExport(int campaignId);
        /// <summary>
        /// Gets the campaign with Defaults
        /// </summary>
        /// <param name="campaignId">campaignId</param>
        /// <returns>returns CampaignWithDefaultsDto object</returns>
        CampaignWithDefaultsDto GetCampaignWithDefaults(int campaignId);
    }

    /// <summary>
    /// Data operations for the Campaign Repository.
    /// </summary>
    /// <seealso cref="BroadcastRepositoryBase" />
    /// <seealso cref="ICampaignRepository" />
    public class CampaignRepository : BroadcastRepositoryBase, ICampaignRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="configurationSettingsHelper">The p configuration web API client.</param>
        public CampaignRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        /// <inheritdoc />
        public int CreateCampaign(SaveCampaignDto campaignDto, string createdBy, DateTime createdDate)
        {
            var campaign = new campaign
            {
                name = campaignDto.Name,
                advertiser_id = campaignDto.AdvertiserId,
                advertiser_master_id = campaignDto.AdvertiserMasterId,
                agency_id = campaignDto.AgencyId,
                agency_master_id = campaignDto.AgencyMasterId,
                notes = campaignDto.Notes,
                created_by = createdBy,
                created_date = createdDate,
                modified_date = campaignDto.ModifiedDate,
                modified_by = campaignDto.ModifiedBy,
                unified_id = campaignDto.UnifiedId,
                max_fluidity_percent = campaignDto.MaxFluidityPercent,
                unified_campaign_last_sent_at = campaignDto.UnifiedCampaignLastSentAt,
                unified_campaign_last_received_at = campaignDto.UnifiedCampaignLastReceivedAt,
                account_executive = campaignDto.AccountExecutive,
                client_contact = campaignDto.ClientContact
            };

            return _InReadUncommitedTransaction(
               context =>
               {
                   context.campaigns.Add(campaign);
                   context.SaveChanges();
                   return campaign.id;
               });
        }

        /// <inheritdoc />
        public int UpdateCampaign(SaveCampaignDto campaignDto)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var existingCampaign = context.campaigns.Single(x => x.id == campaignDto.Id, "Invalid campaign id");

                   existingCampaign.name = campaignDto.Name;
                   existingCampaign.advertiser_id = campaignDto.AdvertiserId;
                   existingCampaign.advertiser_master_id = campaignDto.AdvertiserMasterId;
                   existingCampaign.agency_id = campaignDto.AgencyId;
                   existingCampaign.agency_master_id = campaignDto.AgencyMasterId;
                   existingCampaign.notes = campaignDto.Notes;
                   existingCampaign.modified_by = campaignDto.ModifiedBy;
                   existingCampaign.modified_date = campaignDto.ModifiedDate;
                   existingCampaign.unified_id = campaignDto.UnifiedId;
                   existingCampaign.max_fluidity_percent = campaignDto.MaxFluidityPercent;
                   existingCampaign.unified_campaign_last_sent_at = campaignDto.UnifiedCampaignLastSentAt;
                   existingCampaign.unified_campaign_last_received_at = campaignDto.UnifiedCampaignLastReceivedAt;
                   existingCampaign.account_executive = campaignDto.AccountExecutive;
                   existingCampaign.client_contact = campaignDto.ClientContact;

                   context.SaveChanges();

                   return existingCampaign.id;
               });
        }

        /// <inheritdoc />
        public void UpdateCampaignLastModified(int campaignId, DateTime modifiedDate, string modifiedBy)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   var existingCampaign = context.campaigns.Single(x => x.id == campaignId, "Invalid campaign id");

                   existingCampaign.modified_by = modifiedBy;
                   existingCampaign.modified_date = modifiedDate;

                   context.SaveChanges();
               });
        }

        public void UpdateUnifiedCampaignLastSentAt(int campaignId, DateTime unifiedCampaignLastSentAt)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   var existingCampaign = context.campaigns.Single(x => x.id == campaignId, "Invalid campaign id");

                   existingCampaign.unified_campaign_last_sent_at = unifiedCampaignLastSentAt;                 

                   context.SaveChanges();
               });
        }

        /// <inheritdoc />
        public List<CampaignWithSummary> GetCampaignsWithSummary(DateTime? startDate, DateTime? endDate, PlanStatusEnum? campaignStatus)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaigns = context.campaigns
                        .Include(campaign => campaign.plans)
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries)))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries.Select(z => z.plan_version_summary_quarters))))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts)))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts.Select(z => z.standard_dayparts))))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths)))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths.Select(z => z.spot_lengths))))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_weekly_breakdown)))
                        .Include(campaign => campaign.campaign_summaries)
                         .ToList();

                    var campaignsWithSummary = campaigns
                         .Select(item => new { campaign = item, summary = item.campaign_summaries.FirstOrDefault() ?? new campaign_summaries() })
                         .ToList();

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        campaignsWithSummary = campaignsWithSummary.Where(item =>
                            (item.summary == null &&
                             item.campaign.created_date >= startDate.Value &&
                             item.campaign.created_date <= endDate.Value) ||

                             (item.summary != null &&
                             item.summary.flight_start_Date == null &&
                             item.summary.flight_end_Date == null &&
                             item.campaign.created_date >= startDate.Value &&
                             item.campaign.created_date <= endDate.Value) ||

                            (item.summary != null &&
                            item.summary.flight_start_Date != null &&
                             item.summary.flight_end_Date == null &&
                             item.summary.flight_start_Date >= startDate.Value &&
                             item.summary.flight_start_Date <= endDate.Value) ||

                            (item.summary != null &&
                            item.summary.flight_start_Date != null &&
                             item.summary.flight_end_Date != null &&
                             item.summary.flight_start_Date <= endDate.Value &&
                             item.summary.flight_end_Date >= startDate.Value)).ToList();
                    }

                    if (campaignStatus.HasValue)
                    {
                        if (campaignStatus.Value == PlanStatusEnum.Working)
                            campaignsWithSummary = campaignsWithSummary.Where(c => c.summary.campaign_status == (byte)campaignStatus || c.summary == null || !c.summary.campaign_status.HasValue).ToList();
                        else
                            campaignsWithSummary = campaignsWithSummary.Where(p => p.summary.campaign_status == (byte)campaignStatus).ToList();

                    }

                    var result = campaignsWithSummary
                        .Select(x => _MapToCampaignAndCampaignSummary(x.campaign, x.summary))
                        .OrderByDescending(item => item.Campaign.ModifiedDate)
                        .ToList();
                    return result;
                });
        }

        private CampaignWithSummary _MapToCampaignAndCampaignSummary(campaign campaign, campaign_summaries summary)
        {
            var item = new CampaignWithSummary
            {
                Campaign = _MapToDto(campaign,true)
            };
            if (summary != null)
            {
                item.CampaignSummary = new CampaignSummaryDto
                {
                    ProcessingStatus = (CampaignAggregationProcessingStatusEnum)summary.processing_status,
                    ProcessingErrorMessage = summary.processing_status_error_msg,
                    CampaignId = summary.campaign_id,
                    QueuedAt = summary.queued_at,
                    QueuedBy = summary.queued_by,
                    FlightStartDate = summary.flight_start_Date,
                    FlightEndDate = summary.flight_end_Date,
                    FlightHiatusDays = summary.flight_hiatus_days,
                    FlightActiveDays = summary.flight_active_days,
                    Budget = (decimal?)summary.budget,
                    HHCPM = (decimal?)summary.hh_cpm,
                    HHImpressions = summary.hh_impressions,
                    HHRatingPoints = summary.hh_rating_points,
                    CampaignStatus = (PlanStatusEnum?)summary.campaign_status,
                    PlanStatusCountWorking = summary.plan_status_count_working,
                    PlanStatusCountReserved = summary.plan_status_count_reserved,
                    PlanStatusCountClientApproval = summary.plan_status_count_client_approval,
                    PlanStatusCountContracted = summary.plan_status_count_contracted,
                    PlanStatusCountLive = summary.plan_status_count_live,
                    PlanStatusCountComplete = summary.plan_status_count_complete,
                    PlanStatusCountScenario = summary.plan_status_count_scenario,
                    PlanStatusCountCanceled = summary.plan_status_count_canceled,
                    PlanStatusCountRejected = summary.plan_status_count_rejected,
                    ComponentsModified = summary.components_modified,
                    LastAggregated = summary.last_aggregated
                };
            }

            return item;
        }

        /// <inheritdoc />
        public CampaignDto GetCampaign(int campaignId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaign = context.campaigns
                        .Include(x => x.plans)
                        .Include(z => z.plans.Select(x => x.plan_versions))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths)))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths.Select(w => w.spot_lengths))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries)))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts)))                        
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts.Select(t => t.standard_dayparts))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts.Select(t => t.standard_dayparts.daypart))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries.Select(t => t.plan_version_summary_quarters))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_weekly_breakdown)))
                        .Single(c => c.id.Equals(campaignId), $"Could not find existing campaign with id '{campaignId}'");

                    return _MapToDto(campaign);
                });
        }

        private CampaignDto _MapToDto(campaign campaign, bool IsCampaignAndCampaignSummary = false)
        {
            var planVersions = campaign.plans.Where(plan => !(plan.deleted_at.HasValue)).SelectMany(x => x.plan_versions);

            var campaignDto = new CampaignDto
            {
                Id = campaign.id,
                Name = campaign.name,
                Notes = campaign.notes,
                ModifiedDate = campaign.modified_date,
                ModifiedBy = campaign.modified_by,
                AdvertiserId = campaign.advertiser_id,
                AdvertiserMasterId = campaign.advertiser_master_id,
                AgencyId = campaign.agency_id,
                AgencyMasterId = campaign.agency_master_id,
                MaxFluidityPercent = campaign.max_fluidity_percent,
                UnifiedId = campaign.unified_id,
                UnifiedCampaignLastSentAt = campaign.unified_campaign_last_sent_at,
                UnifiedCampaignLastReceivedAt = campaign.unified_campaign_last_received_at,
                AccountExecutive = campaign.account_executive,
                ClientContact = campaign.client_contact,
                Plans = planVersions.Where(y => y.plan_version_summaries.Any(s => s.processing_status == (int)PlanAggregationProcessingStatusEnum.Idle) && y.id == (IsCampaignAndCampaignSummary == false ? y.plan.latest_version_id : y.id))
                    .Select(version =>
                    {
                        var summary = version.plan_version_summaries.Single();

                        var draft = planVersions
                             .Where(x => x.plan_id == version.plan_id && x.is_draft == true).OrderBy(x => x.id).FirstOrDefault();

                        var totalAduImpressions = version.plan_version_weekly_breakdown.Sum(w => w.adu_impressions);

                        return new PlanSummaryDto
                        {
                            ProcessingStatus = (PlanAggregationProcessingStatusEnum)summary.processing_status,
                            ProductName = summary.product_name,
                            AudienceId = version.target_audience_id,
                            PostingType = (PostingTypeEnum)version.posting_type,
                            Status = (PlanStatusEnum)version.status,
                            Name = version.plan.name,
                            IsAduPlan = version.is_adu_plan ?? false,
                            SpotLengthValues = version.plan_version_creative_lengths.Select(x => x.spot_lengths.length).ToList(),
                            FlightStartDate = version.flight_start_date,
                            FlightEndDate = version.flight_end_date,
                            TargetCPM = version.target_cpm,
                            Budget = version.budget,
                            Equivalized = version.equivalized,
                            AvailableMarketCount = summary.available_market_count,
                            AvailableMarketsWithSovCount = summary.available_market_with_sov_count,
                            AvailableMarketTotalUsCoveragePercent = summary.available_market_total_us_coverage_percent,
                            PlanId = version.plan.id,
                            VersionId = version.id,
                            ModifiedDate = version.modified_date ?? version.created_date,
                            ModifiedBy = version.modified_by ?? version.created_by,
                            Dayparts = version.plan_version_dayparts.Select(d => d.standard_dayparts.code).ToList(),
                            TargetImpressions = version.target_impression,
                            TRP = version.target_rating_points,
                            AduImpressions = totalAduImpressions,
                            TotalActiveDays = summary.active_day_count,
                            TotalHiatusDays = summary.hiatus_days_count,
                            HasHiatus = summary.hiatus_days_count.HasValue && summary.hiatus_days_count.Value > 0,
                            HHImpressions = version.hh_impressions,
                            HHCPM = version.hh_cpm,
                            DraftModifiedBy = draft?.modified_by ?? draft?.created_by,
                            DraftModifiedDate = draft?.modified_date ?? draft?.created_date,
                            DraftId = draft?.id,
                            IsDraft = version.is_draft,
                            VersionNumber = version.version_number,
                            PlanSummaryQuarters = summary.plan_version_summary_quarters.Select(q => new PlanSummaryQuarterDto
                            {
                                Quarter = q.quarter,
                                Year = q.year
                            }).OrderBy(x => x.Year).ThenBy(x => x.Quarter).ToList(),
                            FluidityPercentage = summary.fluidity_percentage
                        };
                    }).ToList(),
                ViewDetailsUrl = campaign.view_details_url
            };

            campaignDto.HasPlans = campaignDto.Plans.Any();
            return campaignDto;
        }

        /// <inheritdoc />
        public List<DateRange> GetCampaignsDateRanges(PlanStatusEnum? planStatus)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaigns = _GetFilteredCampaignsWithoutValidPlans(null, null, planStatus, context);
                    var plans = _GetFilteredPlansWithDates(null, null, planStatus, context);

                    var plansDateRanges = plans.SelectMany(x => x.plan_versions).Where(y => y.id == y.plan.latest_version_id).Select(version => new
                    {
                        version.flight_start_date,
                        version.flight_end_date
                    }).ToList();

                    return campaigns
                            .Select(c => new DateRange(c.created_date, null))
                            .Concat(plansDateRanges.Select(c => new DateRange(c.flight_start_date, c.flight_end_date))).ToList();
                });
        }

        public List<PlanStatusEnum> GetCampaignsStatuses(DateTime? startDate, DateTime? endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    IQueryable<campaign_summaries> campaignSummaries = context.campaign_summaries;

                    var campaignsWithSummary = context.campaigns
                        .GroupJoin(
                            context.campaign_summaries,
                            campaigns => campaigns.id,
                            campaign_summaries => campaign_summaries.campaign_id,
                            (campaign, summary) => new { campaign, summaries = summary.DefaultIfEmpty() })
                         .Select(item => new { item.campaign, summary = item.summaries.FirstOrDefault() });

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        campaignsWithSummary = campaignsWithSummary.Where(item =>
                            (item.summary == null &&
                             item.campaign.created_date >= startDate.Value &&
                             item.campaign.created_date <= endDate.Value) ||

                             (item.summary != null &&
                             item.summary.flight_start_Date == null &&
                             item.summary.flight_end_Date == null &&
                             item.campaign.created_date >= startDate.Value &&
                             item.campaign.created_date <= endDate.Value) ||

                            (item.summary.flight_start_Date != null &&
                             item.summary.flight_end_Date == null &&
                             item.summary.flight_start_Date >= startDate.Value &&
                             item.summary.flight_start_Date <= endDate.Value) ||

                            (item.summary.flight_start_Date != null &&
                             item.summary.flight_end_Date != null &&
                             item.summary.flight_start_Date <= endDate.Value &&
                             item.summary.flight_end_Date >= startDate.Value));
                    }
                    var campaignStatuses = campaignsWithSummary
                        .Select(s => s.summary.campaign_status.HasValue ? (PlanStatusEnum)s.summary.campaign_status.Value : PlanStatusEnum.Working)
                        .Distinct().ToList();

                    return campaignStatuses;
                });
        }

        private IEnumerable<campaign> _GetFilteredCampaignsWithoutValidPlans(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus, QueryHintBroadcastContext context)
        {
            // Campaigns without plans are considered to be in Working status.
            if (planStatus.HasValue && planStatus != PlanStatusEnum.Working)
            {
                return new List<campaign>();
            }

            var plansWithoutStartDate = _GetFilteredPlansWithoutDates(planStatus, context);

            var campaignsIdsPlansNoStartDate = plansWithoutStartDate
                                                .Select(p => p.campaign_id)
                                                .ToList();

            var campaigns = (from c in context.campaigns
                             from p in c.plans.DefaultIfEmpty()
                             where p == null ||
                                campaignsIdsPlansNoStartDate.Contains(c.id)
                             select c);

            // Campaigns without plans are filtered by date of creation.
            if (startDate.HasValue && endDate.HasValue)
            {
                campaigns = campaigns.Where(c => c.created_date >= startDate &&
                                                 c.created_date <= endDate);
            }

            return campaigns;
        }

        private IEnumerable<plan> _GetFilteredPlansWithoutDates(PlanStatusEnum? planStatus, QueryHintBroadcastContext context)
        {
            var plansWithoutStartDate = (from plan in context.plans
                                         join version in context.plan_versions on plan.id equals version.plan_id
                                         where !(plan.deleted_at.HasValue) && version.flight_start_date == null && version.id == plan.latest_version_id
                                         select plan);
            if (planStatus.HasValue)
            {
                plansWithoutStartDate = plansWithoutStartDate
                                            .Where(p => p.plan_versions.Any(x => (PlanStatusEnum)x.status == planStatus));
            }
            return plansWithoutStartDate;
        }

        private IEnumerable<plan> _GetFilteredPlansWithDates(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus, QueryHintBroadcastContext context)
        {
            var plansWithStartDate = (from p in context.plans
                                      join v in context.plan_versions on p.id equals v.plan_id
                                      where !(p.deleted_at.HasValue) && v.id == p.latest_version_id && v.flight_start_date != null
                                      select p);

            if (startDate.HasValue && endDate.HasValue)
            {
                plansWithStartDate = plansWithStartDate.SelectMany(x => x.plan_versions)
                                    .Where(v =>
                                            (v.flight_start_date != null &&
                                             v.flight_end_date == null &&
                                             v.flight_start_date >= startDate &&
                                             v.flight_start_date <= endDate) ||

                                            (v.flight_start_date != null &&
                                             v.flight_end_date != null &&
                                             v.flight_start_date <= endDate &&
                                             v.flight_end_date >= startDate))
                                     .Select(v => v.plan);
            }

            if (planStatus.HasValue)
            {
                plansWithStartDate = plansWithStartDate.SelectMany(x => x.plan_versions).Where(v => v.status == (byte)planStatus).Select(v => v.plan);
            }

            return plansWithStartDate.ToList();
        }

        public CampaignCopyDto GetCampaignCopy(int campaignId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaign = context.campaigns
                        .Include(x => x.plans)
                        .Include(z => z.plans.Select(x => x.plan_versions))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries)))
                        .Single(c => c.id.Equals(campaignId), $"Could not find existing campaign with id '{campaignId}'");

                    return _MapToCampaignCopyDto(campaign);
                });
        }

        private CampaignCopyDto _MapToCampaignCopyDto(campaign campaign)
        {
            var campaignCopyDto = new CampaignCopyDto
            {
                Id = campaign.id,
                Name = campaign.name,
                AdvertiserMasterId = Convert.ToString(campaign.advertiser_master_id),
                AgencyMasterId = Convert.ToString(campaign.agency_master_id),
                Plans = campaign.plans.Where(plan => !(plan.deleted_at.HasValue)).SelectMany(x => x.plan_versions
                            .Where(y => y.plan_version_summaries.Any(s => s.processing_status == (int)PlanAggregationProcessingStatusEnum.Idle)))
                    .Select(version =>
                    {
                        return new PlansCopyDto
                        {
                            ProductMasterId = Convert.ToString(version.plan.product_master_id),
                            Name = version.plan.name,
                            StartDate = Convert.ToString(version.flight_start_date),
                            EndDate = Convert.ToString(version.flight_end_date),
                            SourcePlanId = version.plan.id,
                            Impressions = version.target_impression,
                            CPM = version.target_cpm,
                            Budget = version.budget,
                            SpotLengths = version.plan_version_creative_lengths.Select(x => x.spot_lengths.length).ToList(),
                            Status = (PlanStatusEnum)version.status,
                            VersionId = version.id,
                            IsDraft = version.is_draft,
                            LatestVersionId =version.plan.latest_version_id
                        };
                    }).ToList()
            };
            return campaignCopyDto;
        }

        /// <inheritdoc />
        public CampaignDto CheckCampaignExist(SaveCampaignDto saveCampaignDto)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaign = context.campaigns.Where(x => x.name == saveCampaignDto.Name &&
                     x.agency_master_id == saveCampaignDto.AgencyMasterId && x.advertiser_master_id == saveCampaignDto.AdvertiserMasterId).FirstOrDefault();
                    return _MapExistingCampaignToDto(campaign);
                });

        }
        private CampaignDto _MapExistingCampaignToDto(campaign campaign)
        {
            CampaignDto campaignDto = new CampaignDto();
            if (campaign != null)
            {
                campaignDto.Id = campaign.id;
                campaignDto.Name = campaign.name;
                campaignDto.AdvertiserMasterId = campaign.advertiser_master_id;
                campaignDto.AgencyMasterId = campaign.agency_master_id;
            }
            return campaignDto;
        }

        public CampaignExportDto GetCampaignPlanForExport(int campaignId)
        {
            
            return _InReadUncommitedTransaction(
                context =>
                {
                    CampaignExportDto obj = new CampaignExportDto(); 
                    var campaign = context.campaigns
                        .Include(x => x.plans)
                        .Include(z => z.plans.Select(x => x.plan_versions))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths)))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths.Select(w => w.spot_lengths))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries)))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts)))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts.Select(t => t.standard_dayparts))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts.Select(t => t.standard_dayparts.daypart))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries.Select(t => t.plan_version_summary_quarters))))
                        .Single(c => c.id.Equals(campaignId), $"Could not find existing campaign with id '{campaignId}'");

                     return _MapCampaignPlanForExportToDto(campaign);

                });
        }

        private CampaignExportDto _MapCampaignPlanForExportToDto(campaign campaign)
        {
            var planVersions = campaign.plans.Where(plan => !(plan.deleted_at.HasValue)).SelectMany(x => x.plan_versions);

            var campaignDto = new CampaignExportDto
            {
                Id = campaign.id,                
                Plans = planVersions.Where(y => y.plan_version_summaries.Any(s => s.processing_status == (int)PlanAggregationProcessingStatusEnum.Idle) )
                    .Select(version =>
                    {
                        var summary = version.plan_version_summaries.Single();
                        var draft = planVersions
                            .Where(x => x.plan_id == version.plan_id && x.is_draft == true).SingleOrDefault();
                        return new PlanExportSummaryDto
                        {
                            VersionId = version.id,
                            PlanId = version.plan.id,
                            Name = version.plan.name,
                            Status = (PlanStatusEnum)version.status,
                            DraftId = draft?.id,
                            IsDraft = version.is_draft,
                            VersionNumber = version.version_number,
                            PlanSummaryQuarters = summary.plan_version_summary_quarters.Select(q => new PlanSummaryQuarterDto
                            {
                                Quarter = q.quarter,
                                Year = q.year
                            }).OrderBy(x => x.Year).ThenBy(x => x.Quarter).ToList()
                        };
                    }).ToList()
            };
            campaignDto.HasPlans = campaignDto.Plans.Any();
            return campaignDto;
        }

        /// <inheritdoc />
        public CampaignWithDefaultsDto GetCampaignWithDefaults(int campaignId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var secondaryAudiences = context.campaigns
                        .Include(z => z.campaign_plan_secondary_audiences)
                        .Include(x=>x.campaign_plan_secondary_audiences.Select(z=>z.audience))
                        .Single(c => c.id.Equals(campaignId), $"Could not find existing campaign with id '{campaignId}'");

                    return _MapToCampaignDefaultDto(secondaryAudiences.campaign_plan_secondary_audiences.ToList(),secondaryAudiences);
                });
        }

        private CampaignWithDefaultsDto _MapToCampaignDefaultDto(List<campaign_plan_secondary_audiences> campaignPlanSecondaryAudiences, campaign c)
        {
            CampaignWithDefaultsDto campaignWithDefaults = new CampaignWithDefaultsDto();
            campaignWithDefaults.MaxFluidityPercent = c.max_fluidity_percent;
            campaignWithDefaults.SecondaryAudiences = _MapToSecondaryAudiencesDto(campaignPlanSecondaryAudiences);
            return campaignWithDefaults;
        }

        private List<CampaignPlanSecondaryAudiencesDto> _MapToSecondaryAudiencesDto(List<campaign_plan_secondary_audiences> campaignPlanSecondaryAudiences)
        {
            List<CampaignPlanSecondaryAudiencesDto> secondaryAudiences = new List<CampaignPlanSecondaryAudiencesDto>();

            secondaryAudiences = campaignPlanSecondaryAudiences.Select(secondaryAudience => new CampaignPlanSecondaryAudiencesDto
            {
                Type = AudienceTypeEnum.Nielsen,
                AudienceId = secondaryAudience.audience_id
            }).ToList();

            return secondaryAudiences;
        }
    }
}
