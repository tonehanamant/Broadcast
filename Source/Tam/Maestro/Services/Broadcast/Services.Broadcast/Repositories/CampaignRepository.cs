using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
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
        List<CampaignListItemDto> GetCampaigns(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus);

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        CampaignDto GetCampaign(int id);

        /// <summary>
        /// Creates the campaign.
        /// </summary>
        /// <param name="campaignDto">The campaign dto.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new campaign</returns>
        int CreateCampaign(CampaignDto campaignDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Updates the campaign.
        /// </summary>
        /// <param name="campaignDto">The campaign dto.</param>
        /// <returns>Id of the campain updated</returns>
        int UpdateCampaign(CampaignDto campaignDto);

        /// <summary>
        /// Gets the list of all date ranges for campaign's plans
        /// </summary>
        /// <param name="planStatus">The status of the plans to filter the dates by.</param>
        /// <returns>A list of date ranges.</returns>
        List<DateRange> GetCampaignsDateRanges(PlanStatusEnum? planStatus);

        /// <summary>
        /// Gets the list of all statuses of the plans for the campaigns listing
        /// </summary>
        /// <param name="startDate">The start date to filter statuses by</param>
        /// <param name="endDate">The end  date to filter the statuses by</param>
        /// <returns></returns>
        List<PlanStatusEnum> GetCampaignsPlanStatuses(DateTime? startDate, DateTime? endDate);
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
        public int CreateCampaign(CampaignDto campaignDto, string createdBy, DateTime createdDate)
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
        public int UpdateCampaign(CampaignDto campaignDto)
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
        public List<CampaignListItemDto> GetCampaigns(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var filteredPlans = _GetFilteredPlansWithDates(startDate, endDate, planStatus, context);
                    var plansCampaignIds = filteredPlans.Select(p => p.campaign_id).ToList();
                    var campaignIdsWithNoPlans = _GetFilteredCampaignIds(startDate, endDate, planStatus, context);

                    var campaignsIds = plansCampaignIds
                        .Concat(campaignIdsWithNoPlans)
                        .Distinct();

                    return (from c in context.campaigns
                            where campaignsIds.Contains(c.id)
                            orderby c.modified_date descending
                            select c).Select(_MapToCampaignListItemDto).ToList();
                });
        }

        /// <inheritdoc />
        public CampaignDto GetCampaign(int campaignId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaign = context.campaigns
                        .Include(x => x.plans)
                        .Include(x => x.plans.Select(p => p.spot_lengths))
                        .Include(x => x.plans.Select(p => p.plan_dayparts))
                        .Include(x => x.plans.Select(p => p.plan_dayparts.Select(d => d.daypart_codes)))
                        .Include(x => x.plans.Select(p => p.plan_summary))
                        .Include(x => x.plans.Select(p => p.plan_summary.Select(s => s.plan_summary_quarters)))
                        .Single(c => c.id.Equals(campaignId), $"Could not find existing campaign with id '{campaignId}'");

                    var campaignDto = _MapToDto(campaign);

                    return campaignDto;
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
                Plans = campaign.plans
                    .Where(x => x.plan_summary.Any(s => s.processing_status == (int)PlanAggregationProcessingStatusEnum.Idle))
                    .Select(plan =>
                    {
                        var summary = plan.plan_summary.Single();

                        return new PlanSummaryDto
                        {
                            ProductName = summary.product_name,
                            AudienceName = summary.audience_name,
                            PostingType = (PostingTypeEnum)plan.posting_type,
                            Status = (PlanStatusEnum)plan.status,
                            Name = plan.name,
                            SpotLength = plan.spot_lengths.length,
                            FlightStartDate = plan.flight_start_date,
                            FlightEndDate = plan.flight_end_date,
                            TargetCPM = plan.cpm,
                            Budget = plan.budget,
                            Equivalized = plan.equivalized,
                            AvailableMarketCount = summary.available_market_count,
                            AvailableMarketTotalUsCoveragePercent = summary.available_market_total_us_coverage_percent,
                            PlanId = plan.id,
                            ModifiedDate = plan.modified_date,
                            ModifiedBy = plan.modified_by,
                            Dayparts = plan.plan_dayparts.Select(d => d.daypart_codes.code).ToList(),
                            TargetImpressions = plan.delivery_impressions,
                            TRP = plan.delivery_rating_points,
                            TotalActiveDays = summary.active_day_count,
                            TotalHiatusDays = summary.hiatus_days_count,
                            HasHiatus = summary.hiatus_days_count.HasValue && summary.hiatus_days_count.Value > 0,
                            PlanSummaryQuarters = summary.plan_summary_quarters.Select(q => new PlanSummaryQuarterDto
                            {
                                Quarter = q.quarter,
                                Year = q.year
                            }).ToList()
                        };
                    }).ToList(),
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
                    var campaignsWithValidPlans = (from p in context.plans
                                                   where p.flight_start_date != null
                                                   select p.campaign_id).Distinct().ToList();

                    var campaignsDates = (from c in context.campaigns
                                          where !campaignsWithValidPlans.Contains(c.id)
                                          select c.created_date).ToList();

                    var plans = (from c in context.campaigns
                                 from p in c.plans
                                 where campaignsWithValidPlans.Contains(c.id)
                                 select p);

                    if (planStatus.HasValue)
                    {
                        plans.Where(p => p.status == (byte)planStatus);
                    }

                    var plansDateRanges = plans.Select(p => new { p.flight_start_date, p.flight_end_date }).ToList();

                    return campaignsDates
                            .Select(c => new DateRange(c, null))
                            .Concat(plansDateRanges.Select(c => new DateRange(c.flight_start_date, c.flight_end_date))).ToList();
                 });
        }

        public List<PlanStatusEnum> GetCampaignsPlanStatuses(DateTime? startDate, DateTime? endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var filteredPlans = _GetFilteredPlansWithDates(startDate, endDate, null, context);
                    var plansIds = filteredPlans.Select(p => p.id).ToList();
                    var campaignsIds = _GetFilteredCampaignIds(startDate, endDate, null, context);

                    var plans = (from p in context.plans
                                 where plansIds.Contains(p.id)
                                 select p);

                    var planStatuses = plans.Select(p => (PlanStatusEnum)p.status).Distinct().ToList();

                    if (campaignsIds.Any() && !planStatuses.Contains(PlanStatusEnum.Working))
                    {
                        planStatuses.Add(PlanStatusEnum.Working);
                    }

                    return planStatuses;
                });
        }

        private List<int> _GetFilteredCampaignIds(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus, QueryHintBroadcastContext context)
        {
            // Campaigns without plans are considered to be in Working status.
            if (planStatus.HasValue && planStatus != PlanStatusEnum.Working)
            {
                return new List<int>();
            }

            var campaignsIdsPlansNoStartDate = _GetCampaignIdsForFilteredPlansWithoutDates(planStatus, context);

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

            return campaigns.Select(c => c.id).ToList();
        }

        private List<int> _GetCampaignIdsForFilteredPlansWithoutDates(PlanStatusEnum? planStatus, QueryHintBroadcastContext context)
        {
            var plansWithoutStartDate = (from p in context.plans
                                         where p.flight_start_date == null
                                         select p);

            if (planStatus.HasValue)
            {
                plansWithoutStartDate = plansWithoutStartDate
                                            .Where(p => p.status == (byte)planStatus);
            }

            var campaignsIdsPlansNoStartDate = plansWithoutStartDate
                                                .Select(p => p.campaign_id)
                                                .ToList();

            return campaignsIdsPlansNoStartDate;
        }

        private List<plan> _GetFilteredPlansWithDates(DateTime? startDate, DateTime? endDate, PlanStatusEnum? planStatus, QueryHintBroadcastContext context)
        {
            var plansWithStartDate = (from p in context.plans
                                      where p.flight_start_date != null
                                      select p);

            if (startDate.HasValue && endDate.HasValue)
            {
                plansWithStartDate = plansWithStartDate.Where(p =>
                                            (p.flight_start_date != null &&
                                             p.flight_end_date == null &&
                                             p.flight_start_date >= startDate &&
                                             p.flight_start_date <= endDate) ||

                                            (p.flight_start_date != null &&
                                             p.flight_end_date != null &&
                                             p.flight_start_date <= endDate &&
                                             p.flight_end_date >= startDate));
            }

            if (planStatus.HasValue)
            {
                plansWithStartDate = plansWithStartDate.Where(p => p.status == (byte)planStatus);
            }

            return plansWithStartDate.ToList();
        }

        private CampaignListItemDto _MapToCampaignListItemDto(campaign c)
        {
            return new CampaignListItemDto
            {
                Id = c.id,
                Name = c.name,
                Advertiser = new AdvertiserDto { Id = c.advertiser_id },
                Agency = new AgencyDto { Id = c.agency_id },
                Notes = c.notes,
                ModifiedDate = c.modified_date,
                ModifiedBy = c.modified_by
            };
        }
    }
}
