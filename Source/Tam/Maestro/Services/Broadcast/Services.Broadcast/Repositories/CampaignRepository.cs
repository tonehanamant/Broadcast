using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
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
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        public CampaignRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        /// <inheritdoc />
        public int CreateCampaign(SaveCampaignDto campaignDto, string createdBy, DateTime createdDate)
        {
            var campaign = new campaign
            {
                name = campaignDto.Name,
                advertiser_id = campaignDto.AdvertiserId,
                agency_id = campaignDto.AgencyId,
                notes = campaignDto.Notes,
                created_by = createdBy,
                created_date = createdDate,
                modified_date = campaignDto.ModifiedDate,
                modified_by = campaignDto.ModifiedBy
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
                   existingCampaign.agency_id = campaignDto.AgencyId;
                   existingCampaign.notes = campaignDto.Notes;
                   existingCampaign.modified_by = campaignDto.ModifiedBy;
                   existingCampaign.modified_date = campaignDto.ModifiedDate;

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

        /// <inheritdoc />
        public List<CampaignWithSummary> GetCampaignsWithSummary(DateTime? startDate, DateTime? endDate, PlanStatusEnum? campaignStatus)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaignsWithSummary = context.campaigns
                        .Include(campaign => campaign.plans)
                        .Include(campaign => campaign.plans.Select(x=>x.plan_versions))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y=>y.plan_version_summaries)))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts)))
                        .Include(campaign => campaign.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths.Select(z=>z.spot_lengths))))
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

                    if (campaignStatus.HasValue)
                    {
                        if (campaignStatus.Value == PlanStatusEnum.Working)
                            campaignsWithSummary = campaignsWithSummary.Where(c => c.summary.campaign_status == (byte)campaignStatus || c.summary == null || !c.summary.campaign_status.HasValue);
                        else
                            campaignsWithSummary = campaignsWithSummary.Where(p => p.summary.campaign_status == (byte)campaignStatus);

                    }

                    return campaignsWithSummary.ToList()
                        .Select(x => _MapToCampaignAndCampaignSummary(x.campaign, x.summary))
                        .OrderByDescending(item => item.Campaign.ModifiedDate)
                        .ToList();
                });
        }

        private CampaignWithSummary _MapToCampaignAndCampaignSummary(campaign campaign, campaign_summaries summary)
        {
            var item = new CampaignWithSummary
            {
                Campaign = _MapToDto(campaign)
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
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_creative_lengths.Select(w=>w.spot_lengths))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries)))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts)))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts.Select(t=>t.daypart_defaults))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_dayparts.Select(t => t.daypart_defaults.daypart))))
                        .Include(z => z.plans.Select(x => x.plan_versions.Select(y => y.plan_version_summaries.Select(t=>t.plan_version_summary_quarters))))
                        .Single(c => c.id.Equals(campaignId), $"Could not find existing campaign with id '{campaignId}'");
       
                    return _MapToDto(campaign); 
                });
        }
        
        private CampaignDto _MapToDto(campaign campaign)
        {
            var campaignDto = new CampaignDto
            {
                Id = campaign.id,
                Name = campaign.name,
                Notes = campaign.notes,
                ModifiedDate = campaign.modified_date,
                ModifiedBy = campaign.modified_by,
                AdvertiserId = campaign.advertiser_id,
                AgencyId = campaign.agency_id,
                Plans = campaign.plans.SelectMany(x => x.plan_versions.Where(y => y.plan_version_summaries.Any(s => s.processing_status == (int)PlanAggregationProcessingStatusEnum.Idle)))
                                    .Where(x=>x.id == x.plan.latest_version_id)
                    .Select(version =>
                    {
                        var summary = version.plan_version_summaries.Single();
                        var draft = campaign.plans.SelectMany(x=> x.plan_versions)
                            .Where(x => x.plan_id == version.plan_id && x.is_draft == true).SingleOrDefault();

                        return new PlanSummaryDto
                        {
                            ProcessingStatus = (PlanAggregationProcessingStatusEnum)summary.processing_status,
                            ProductName = summary.product_name,
                            AudienceId = version.target_audience_id,
                            PostingType = (PostingTypeEnum)version.posting_type,
                            Status = (PlanStatusEnum)version.status,
                            Name = version.plan.name,
                            SpotLengthValues = version.plan_version_creative_lengths.Select(x=> x.spot_lengths.length).ToList(),                            
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
                            Dayparts = version.plan_version_dayparts.Select(d => d.daypart_defaults.code).ToList(),
                            TargetImpressions = version.target_impression,
                            TRP = version.target_rating_points,
                            TotalActiveDays = summary.active_day_count,
                            TotalHiatusDays = summary.hiatus_days_count,
                            HasHiatus = summary.hiatus_days_count.HasValue && summary.hiatus_days_count.Value > 0,
                            HHImpressions = version.hh_impressions,
                            HHCPM = version.hh_cpm,
                            DraftModifiedBy = draft?.modified_by ?? draft?.created_by,
                            DraftModifiedDate = draft?.modified_date ?? draft?.created_date,
                            DraftId = draft?.id,
                            PlanSummaryQuarters = summary.plan_version_summary_quarters.Select(q => new PlanSummaryQuarterDto
                            {
                                Quarter = q.quarter,
                                Year = q.year
                            }).OrderBy(x=>x.Year).ThenBy(x=>x.Quarter).ToList()
                        };
                    }).ToList()
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

                    var plansDateRanges = plans.SelectMany(x=>x.plan_versions).Select(version => new { version.flight_start_date, version.flight_end_date }).ToList();

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
                                         where version.flight_start_date == null && version.id == plan.latest_version_id
                                         select plan);
            if (planStatus.HasValue)
            {
                plansWithoutStartDate = plansWithoutStartDate
                                            .Where(p => p.plan_versions.Any(x=> (PlanStatusEnum)x.status == planStatus));
            }
            return plansWithoutStartDate;
        }

        private IEnumerable<plan> _GetFilteredPlansWithDates(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus, QueryHintBroadcastContext context)
        {
            var plansWithStartDate = (from p in context.plans
                                      join v in context.plan_versions on p.id equals v.plan_id
                                      where v.id == p.latest_version_id && v.flight_start_date != null
                                      select p);

            if (startDate.HasValue && endDate.HasValue)
            {
                plansWithStartDate = plansWithStartDate.SelectMany(x=>x.plan_versions)
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
                plansWithStartDate = plansWithStartDate.SelectMany(x => x.plan_versions).Where(v => v.status == (byte)planStatus).Select(v=>v.plan);
            }

            return plansWithStartDate.ToList();
        }

    }
}
