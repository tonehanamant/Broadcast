﻿using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.PlanPricing;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities;
using static Services.Broadcast.Entities.Campaign.ProgramLineupReportData;
using Tam.Maestro.Data.Entities;
using Common.Services;

namespace Services.Broadcast.Repositories
{
    public interface IPlanRepository : IDataRepository
    {
        /// <summary>
        /// Saves the new plan.
        /// </summary>
        /// <param name="planDto">The plan.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="planDto">The plan dto.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void SavePlan(PlanDto planDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="versionId">Optional: version id. If nothing is passed, it will return the latest version</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId, int? versionId = null);

        /// <summary>
        /// Gets the plans of a campaign excluding Canceled and Rejected.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        List<PlanDto> GetPlansForCampaign(int campaignId);

        /// <summary>
        /// Creates the or update draft.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void CreateOrUpdateDraft(PlanDto plan, string createdBy, DateTime createdDate);

        /// <summary>
        /// Gets the plan history.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>List of PlanHistoryDto objects</returns>
        List<PlanVersion> GetPlanHistory(int planId);

        /// <summary>
        /// Checks if a draft exist on the plan and returns the draft id
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>Dravt id</returns>
        int CheckIfDraftExists(int planId);

        /// <summary>
        /// Deletes the plan draft.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        void DeletePlanDraft(int planId);

        List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId);

        /// <summary>
        /// Gets the plan name by identifier.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>The name of the plan</returns>
        string GetPlanNameById(int planId);

        /// <summary>
        /// Gets the plans for automatic transition.
        /// </summary>
        /// <param name="transitionDate">The transition date.</param>
        /// <returns></returns>
        List<PlanDto> GetPlansForAutomaticTransition(DateTime transitionDate);

        /// <summary>
        /// Gets the latest version number for plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>Numeric value representing the plans latest version number</returns>
        int GetLatestVersionNumberForPlan(int planId);

        int AddPlanPricingJob(PlanPricingJob planPricingJob);
        void UpdatePlanPricingJob(PlanPricingJob planPricingJob);

        /// <summary>
        /// Updates the plan pricing job with hangfire job identifier.
        /// </summary>
        /// <param name="jobId">The pricing job identifier.</param>
        /// <param name="hangfireJobId">The hangfire job identifier.</param>
        void UpdateJobHangfireId(int jobId, string hangfireJobId);
        PlanPricingJob GetLatestPricingJob(int planId);
        void SavePlanPricingParameters(PlanPricingParametersDto planPricingRequestDto);
        void SavePricingApiResults(PlanPricingAllocationResult result);
        PlanPricingAllocationResult GetPricingApiResults(int planId);
        void SavePricingAggregateResults(PlanPricingResultBaseDto result);
        CurrentPricingExecutionResultDto GetPricingResults(int planId);

        PlanPricingJob GetPlanPricingJob(int jobId);

        PlanPricingParametersDto GetLatestParametersForPlanPricingJob(int jobId);

        void SavePlanPricingEstimates(int jobId, List<PricingEstimate> estimates);

        List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanId(int planId);

        List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanVersionId(int planVersionId);

        int GetPlanVersionIdByVersionNumber(int planId, int versionNumber);

        PricingProgramsResultDto GetPricingProgramsResult(int planId);

        /// <summary>
        /// TO BE DELETED ON NEXT RELEASE: Loads the plans for weekly breakdown remapping.
        /// </summary>
        /// <returns></returns>
        List<PlanDto> LoadPlansForWeeklyBreakdownRemapping();
        
        /// <summary>
        ///  TO BE DELETED ON NEXT RELEASE: Saves the weekly breakdown distribution.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void SaveWeeklyBreakdownDistribution(PlanDto plan);

        PlanPricingBandDto GetPlanPricingBand(int planId);

        void SavePlanPricingBands(PlanPricingBandDto planPricingBandDto);
        
        /// <summary>
        ///  TO BE DELETED ON NEXT RELEASE: Saves the creative lengthsdistribution.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void SaveCreativeLengths(PlanDto plan);
    }

    public class PlanRepository : BroadcastRepositoryBase, IPlanRepository
    {
        public PlanRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
        {
        }

        public int GetPlanVersionIdByVersionNumber(int planId, int versionNumber)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planVersion = context.plan_versions
                    .Where(x => x.plan_id == planId && x.version_number == versionNumber)
                    .SingleOrDefault();

                if (planVersion == null)
                    throw new ApplicationException($"Can not find version {versionNumber} for plan {planId}");

                return planVersion.id;
            });
        }

        /// <inheritdoc/>
        public void SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var newPlan = new plan();
                    var version = new plan_versions();
                    newPlan.plan_versions.Add(version);

                    _MapFromDto(planDto, context, newPlan, version);
                    _SetCreatedDate(version, createdBy, createdDate);

                    context.plans.Add(newPlan);
                    context.SaveChanges();

                    _UpdateLatestVersionId(newPlan, context);

                    planDto.Id = newPlan.id;
                    planDto.VersionId = newPlan.latest_version_id;
                });
        }

        /// <inheritdoc />
        public void SavePlan(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var plan = context.plans
                        .Include(p => p.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown))
                        .Where(x => x.id == planDto.Id)
                        .Single(p => p.id == planDto.Id, "Invalid plan id.");
                    var version = new plan_versions();
                    plan.plan_versions.Add(version);

                    _MapFromDto(planDto, context, plan, version);
                    _SetCreatedDate(version, createdBy, createdDate);

                    context.SaveChanges();

                    _UpdateLatestVersionId(plan, context);

                    planDto.VersionId = plan.latest_version_id;
                });
        }

        /// <inheritdoc/>
        public void CreateOrUpdateDraft(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(
                   context =>
                   {
                       var plan = (from p in context.plans
                                   where p.id == planDto.Id
                                   select p)
                           .Include(p => p.plan_versions)
                           .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                           .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown))
                       .Single(x => x.id == planDto.Id, "Invalid plan id");

                       //there can be only 1 draft on a plan, so we're doing Single here
                       var draftVersion = plan.plan_versions.Where(x => x.is_draft == true).SingleOrDefault();
                       if (draftVersion == null)
                       {
                           //there is no draft on the plan, so we create a new version as the draft
                           draftVersion = new plan_versions();
                           plan.plan_versions.Add(draftVersion);
                           _SetCreatedDate(draftVersion, createdBy, createdDate);
                       }
                       else
                       {
                           draftVersion.modified_by = createdBy;
                           draftVersion.modified_date = createdDate;
                       }

                       _MapFromDto(planDto, context, plan, draftVersion);

                       context.SaveChanges();
                   });
        }

        /// <inheritdoc />
        public void DeletePlanDraft(int planId)
        {
            _InReadUncommitedTransaction(
                   context =>
                   {
                       //there can be only 1 draft on a plan, so we're doing Single here
                       var draftVersion = (from v in context.plan_versions
                                           where v.plan_id == planId && v.is_draft == true
                                           select v)
                       .Single("Cannot delete invalid draft.");

                       context.plan_versions.Remove(draftVersion);
                       context.SaveChanges();
                   });
        }

        /// <inheritdoc />
        public PlanDto GetPlan(int planId, int? versionId = null)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var markets = context.markets.ToList();
                    var entity = (from plan in context.plans
                                  where plan.id == planId
                                  select plan)
                        .Include(x => x.campaign)
                        .Include(x => x.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_creative_lengths))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_days.Select(d => d.day)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_show_type_restrictions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_show_type_restrictions.Select(r => r.show_types))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_genre_restrictions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_genre_restrictions.Select(r => r.genre))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_program_restrictions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_program_restrictions.Select(r => r.genre))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_affiliate_restrictions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(d => d.plan_version_daypart_affiliate_restrictions.Select(r => r.affiliate))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown.Select(y => y.media_weeks)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(y => y.plan_version_pricing_parameters_inventory_source_percentages)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(y => y.plan_version_pricing_parameters_inventory_source_type_percentages)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(y => y.plan_version_pricing_parameters_inventory_source_percentages.Select(r => r.inventory_sources))))
                        .Single(s => s.id == planId, "Invalid plan id.");
                    return _MapToDto(entity, markets, versionId);
                });
        }

        /// <inheritdoc />
        public string GetPlanNameById(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planName = (from plan in context.plans
                                where plan.id == planId
                                select plan.name)
                            .Single("Invalid plan id.");
                return planName;
            });
        }

        public int CheckIfDraftExists(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = (from plan in context.plans
                              where plan.id == planId
                              select plan)
                              .Include(x => x.plan_versions)
                    .Single(s => s.id == planId, "Invalid plan id.");
                return entity.plan_versions.Where(x => x.is_draft == true).Select(x => x.id).SingleOrDefault();
            });
        }

        /// <inheritdoc/>
        public List<PlanVersion> GetPlanHistory(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return (from version in context.plan_versions
                        where version.plan_id == planId
                        select version)
                    .Include(p => p.plan_version_dayparts)
                    .Include(p => p.plan_version_flight_hiatus_days)
                    .Select(x => new PlanVersion
                    {
                        VersionId = x.id,
                        VersionNumber = x.version_number,
                        Budget = x.budget,
                        TargetCPM = x.target_cpm,
                        TargetImpressions = x.target_impression,
                        FlightEndDate = x.flight_end_date,
                        FlightStartDate = x.flight_start_date,
                        IsDraft = x.is_draft,
                        ModifiedBy = x.modified_by ?? x.created_by,
                        ModifiedDate = x.modified_date ?? x.created_date,
                        Status = x.status,
                        TargetAudienceId = x.target_audience_id,
                        Dayparts = x.plan_version_dayparts.Select(y => new PlanDaypartDto
                        {
                            DaypartCodeId = y.daypart_default_id,
                            EndTimeSeconds = y.end_time_seconds,
                            IsEndTimeModified = y.is_end_time_modified,
                            IsStartTimeModified = y.is_start_time_modified,
                            StartTimeSeconds = y.start_time_seconds,
                            WeightingGoalPercent = y.weighting_goal_percent
                        }).ToList(),
                        HiatusDays = x.plan_version_flight_hiatus_days.Select(y => y.hiatus_day).ToList()
                    }).ToList();
            });
        }

        /// <inheritdoc />
        public List<PlanDto> GetPlansForCampaign(int campaignId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var markets = context.markets.ToList();
                var entitiesRaw = (from plan in context.plans
                                   join planVersion in context.plan_versions on plan.id equals planVersion.plan_id
                                   where plan.campaign_id == campaignId && plan.latest_version_id == planVersion.id
                                   select plan)
                        .Include(x => x.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_days.Select(y => y.day)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown.Select(y => y.media_weeks)))
                    .ToList();

                return entitiesRaw.Select(e => _MapToDto(e, markets)).ToList();
            });
        }

        /// <inheritdoc />
        public List<PlanDto> GetPlansForAutomaticTransition(DateTime transitionDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planIdsThatHaveDrafts =
                    context.plan_versions.Where(v => v.is_draft == true).Select(v => v.plan_id);

                var markets = context.markets.ToList();
                var entitiesRaw = (from plan in context.plans
                                   join planVersion in context.plan_versions on plan.id equals planVersion.plan_id
                                   where !planIdsThatHaveDrafts.Contains(plan.id) &&
                                   plan.latest_version_id == planVersion.id &&
                                   (
                                    (planVersion.flight_start_date <= transitionDate && planVersion.status == (int)PlanStatusEnum.Contracted) ||
                                    (planVersion.flight_end_date <= transitionDate && planVersion.status == (int)PlanStatusEnum.Live)
                                   )
                                   select plan)
                        .Include(x => x.plan_versions)
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_days.Select(y => y.day)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_secondary_audiences))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_available_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_blackout_markets))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weekly_breakdown.Select(y => y.media_weeks)))
                    .ToList();

                return entitiesRaw.Select(e => _MapToDto(e, markets)).ToList();
            });
        }

        /// <inheritdoc />
        public int GetLatestVersionNumberForPlan(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var latestPlanByVersion =
                    context.plan_versions
                        .Where(p => p.plan_id == planId && p.is_draft == false)
                        .OrderByDescending(p => p.version_number)
                        .FirstOrDefault();

                return latestPlanByVersion.version_number.Value;
            });
        }

        private void _UpdateLatestVersionId(plan newPlan, QueryHintBroadcastContext context)
        {
            newPlan.latest_version_id = newPlan.plan_versions.Max(x => x.id);
            context.SaveChanges();
        }

        private PlanDto _MapToDto(plan entity, List<market> markets, int? versionId = null)
        {
            var latestPlanVersion = versionId != null
                ? entity.plan_versions.Where(x => x.id == versionId).Single($"There is no version {versionId} available")
                : entity.plan_versions.Where(x => x.id == entity.latest_version_id).Single("There is no latest version available");

            //drafts don't have summary, so we're doing SingleOrDefault
            var planSummary = latestPlanVersion.plan_version_summaries.SingleOrDefault();
            var dto = new PlanDto
            {
                Id = entity.id,
                CampaignId = entity.campaign.id,
                CampaignName = entity.campaign.name,
                Name = entity.name,
                CreativeLengths = latestPlanVersion.plan_version_creative_lengths.Select(_MapCreativeLengthsToDto).ToList(),
                Equivalized = latestPlanVersion.equivalized,
                Status = EnumHelper.GetEnum<PlanStatusEnum>(latestPlanVersion.status),
                ProductId = entity.product_id,
                FlightDays = latestPlanVersion.plan_version_flight_days.Select(flightDay => flightDay.day.id).ToList(),
                FlightStartDate = latestPlanVersion.flight_start_date,
                FlightEndDate = latestPlanVersion.flight_end_date,
                FlightNotes = latestPlanVersion.flight_notes,
                AudienceId = latestPlanVersion.target_audience_id,
                AudienceType = EnumHelper.GetEnum<AudienceTypeEnum>(latestPlanVersion.audience_type),
                HUTBookId = latestPlanVersion.hut_book_id,
                ShareBookId = latestPlanVersion.share_book_id,
                PostingType = EnumHelper.GetEnum<PostingTypeEnum>(latestPlanVersion.posting_type),
                FlightHiatusDays = latestPlanVersion.plan_version_flight_hiatus_days.Select(h => h.hiatus_day).ToList(),
                Budget = latestPlanVersion.budget,
                TargetImpressions = latestPlanVersion.target_impression,
                TargetCPM = latestPlanVersion.target_cpm,
                TargetRatingPoints = latestPlanVersion.target_rating_points,
                TargetCPP = latestPlanVersion.target_cpp,
                Currency = EnumHelper.GetEnum<PlanCurrenciesEnum>(latestPlanVersion.currency),
                GoalBreakdownType = EnumHelper.GetEnum<PlanGoalBreakdownTypeEnum>(latestPlanVersion.goal_breakdown_type),
                SecondaryAudiences = latestPlanVersion.plan_version_secondary_audiences.Select(_MapSecondaryAudiences).ToList(),
                Dayparts = latestPlanVersion.plan_version_dayparts.Select(_MapPlanDaypartDto).ToList(),
                CoverageGoalPercent = latestPlanVersion.coverage_goal_percent,
                AvailableMarkets = latestPlanVersion.plan_version_available_markets.Select(e => _MapAvailableMarketDto(e, markets)).ToList(),
                BlackoutMarkets = latestPlanVersion.plan_version_blackout_markets.Select(e => _MapBlackoutMarketDto(e, markets)).ToList(),
                WeeklyBreakdownWeeks = latestPlanVersion.plan_version_weekly_breakdown.Select(_MapWeeklyBreakdownWeeks).ToList(),
                ModifiedBy = latestPlanVersion.modified_by ?? latestPlanVersion.created_by,
                ModifiedDate = latestPlanVersion.modified_date ?? latestPlanVersion.created_date,
                Vpvh = latestPlanVersion.target_vpvh,
                TargetUniverse = latestPlanVersion.target_universe,
                HHCPM = latestPlanVersion.hh_cpm,
                HHCPP = latestPlanVersion.hh_cpp,
                HHImpressions = latestPlanVersion.hh_impressions,
                HHRatingPoints = latestPlanVersion.hh_rating_points,
                HHUniverse = latestPlanVersion.hh_universe,
                AvailableMarketsWithSovCount = planSummary?.available_market_with_sov_count ?? null,
                BlackoutMarketCount = planSummary?.blackout_market_count ?? null,
                BlackoutMarketTotalUsCoveragePercent = planSummary?.blackout_market_total_us_coverage_percent ?? null,
                PricingParameters = _MapPricingParameters(latestPlanVersion.plan_version_pricing_parameters.OrderByDescending(p => p.id).FirstOrDefault()),
                IsDraft = latestPlanVersion.is_draft,
                VersionNumber = latestPlanVersion.version_number,
                VersionId = latestPlanVersion.id,
                IsAduEnabled = latestPlanVersion.is_adu_enabled
            };
            return dto;
        }

        private CreativeLength _MapCreativeLengthsToDto(plan_version_creative_lengths x)
        {
            return new CreativeLength
            {
                SpotLengthId = x.spot_length_id,
                Weight = x.weight
            };
        }

        private PlanPricingParametersDto _MapPricingParameters(plan_version_pricing_parameters arg)
        {
            if (arg == null)
                return null;

            var sortedSourcePercents = PlanPricingInventorySourceSortEngine.SortInventorySourcePercents(
                arg.plan_version_pricing_parameters_inventory_source_percentages.Select(i =>
                new PlanPricingInventorySourceDto
                {
                    Id = i.inventory_source_id,
                    Name = i.inventory_sources.name,
                    Percentage = i.percentage
                }).ToList());

            var sortedSourceTypePercents = PlanPricingInventorySourceSortEngine.SortInventorySourceTypePercents(
                arg.plan_version_pricing_parameters_inventory_source_type_percentages.Select(i =>
                new PlanPricingInventorySourceTypeDto
                {
                    Id = i.inventory_source_type,
                    Name = ((InventorySourceTypeEnum)i.inventory_source_type).GetDescriptionAttribute(),
                    Percentage = i.percentage
                }).ToList());

            return new PlanPricingParametersDto
            {
                PlanId = arg.plan_versions.plan_id,
                Budget = arg.budget_goal,
                CompetitionFactor = arg.competition_factor,
                CPM = arg.cpm_goal,
                DeliveryImpressions = arg.impressions_goal,
                InflationFactor = arg.inflation_factor,
                MaxCpm = arg.max_cpm,
                MinCpm = arg.min_cpm,
                ProprietaryBlend = arg.proprietary_blend,
                CPP = arg.cpp,
                Currency = (PlanCurrenciesEnum)arg.currency,
                DeliveryRatingPoints = arg.rating_points,
                InventorySourcePercentages = sortedSourcePercents,
                InventorySourceTypePercentages = sortedSourceTypePercents,
                UnitCaps = arg.unit_caps,
                UnitCapsType = (UnitCapEnum)arg.unit_caps_type,
                Margin = arg.margin,
                JobId = arg.plan_version_pricing_job_id,
                PlanVersionId = arg.plan_version_id,
                AdjustedCPM = arg.cpm_adjusted,
                AdjustedBudget = arg.budget_adjusted,
                MarketGroup = (PricingMarketGroupEnum)arg.market_group
            };
        }

        private WeeklyBreakdownWeek _MapWeeklyBreakdownWeeks(plan_version_weekly_breakdown arg)
        {
            return new WeeklyBreakdownWeek
            {
                ActiveDays = arg.active_days_label,
                EndDate = arg.media_weeks.end_date,
                WeeklyImpressions = arg.impressions,
                WeeklyRatings = arg.rating_points,
                NumberOfActiveDays = arg.number_active_days,
                WeeklyImpressionsPercentage = arg.impressions_percentage,
                StartDate = arg.media_weeks.start_date,
                MediaWeekId = arg.media_weeks.id,
                WeeklyBudget = arg.budget,
                AduImpressions = arg.adu_impressions,
                SpotLengthId = arg.spot_length_id,
                DaypartCodeId = arg.daypart_default_id,
                PercentageOfWeek = arg.percentage_of_week
            };
        }

        private static PlanAudienceDto _MapSecondaryAudiences(plan_version_secondary_audiences x)
        {
            return new PlanAudienceDto
            {
                AudienceId = x.audience_id,
                Type = (AudienceTypeEnum)x.audience_type,
                Vpvh = x.vpvh,
                RatingPoints = x.rating_points,
                Impressions = x.impressions,
                CPM = x.cpm,
                CPP = (decimal?)x.cpp,
                Universe = x.universe
            };
        }

        private void _MapFromDto(PlanDto planDto, QueryHintBroadcastContext context, plan plan, plan_versions version)
        {
            plan.name = planDto.Name;
            plan.product_id = planDto.ProductId;
            plan.campaign_id = planDto.CampaignId;

            version.equivalized = planDto.Equivalized;
            version.status = (int)planDto.Status;
            version.flight_start_date = planDto.FlightStartDate.Value;
            version.flight_end_date = planDto.FlightEndDate.Value;
            version.flight_notes = planDto.FlightNotes;
            version.coverage_goal_percent = planDto.CoverageGoalPercent.Value;
            version.goal_breakdown_type = (int)planDto.GoalBreakdownType;
            version.target_vpvh = planDto.Vpvh;
            version.target_universe = planDto.TargetUniverse;
            version.hh_cpm = planDto.HHCPM;
            version.hh_cpp = planDto.HHCPP;
            version.hh_impressions = planDto.HHImpressions;
            version.hh_rating_points = planDto.HHRatingPoints;
            version.hh_universe = planDto.HHUniverse;
            version.is_draft = planDto.IsDraft;
            version.version_number = planDto.VersionNumber;
            version.is_adu_enabled = planDto.IsAduEnabled;

            _MapCreativeLengths(version, planDto, context);
            _MapPlanAudienceInfo(version, planDto);
            _MapPlanBudget(version, planDto);
            _MapPlanFlightDays(version, planDto, context);
            _MapPlanFlightHiatus(version, planDto, context);
            _MapDayparts(version, planDto, context);
            _MapPlanSecondaryAudiences(version, planDto, context);
            _MapPlanMarkets(version, planDto, context);
            _MapWeeklyBreakdown(version, planDto, context);
        }

        private void _MapCreativeLengths(plan_versions version, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_creative_lengths.RemoveRange(version.plan_version_creative_lengths);

            planDto.CreativeLengths.ForEach(d =>
            {
                version.plan_version_creative_lengths.Add(new plan_version_creative_lengths
                {
                    spot_length_id = d.SpotLengthId,
                    weight = d.Weight
                });
            });
        }

        private static void _SetCreatedDate(plan_versions version, string createdBy, DateTime createdDate)
        {
            version.created_by = createdBy; //when editing the draft; the created properties have to be removed in order to not overwrite the initial creator of the draft
            version.created_date = createdDate;
        }

        private static void _MapWeeklyBreakdown(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_weekly_breakdown.RemoveRange(entity.plan_version_weekly_breakdown);
            planDto.WeeklyBreakdownWeeks.ForEach(d =>
            {
                entity.plan_version_weekly_breakdown.Add(new plan_version_weekly_breakdown
                {
                    active_days_label = d.ActiveDays,
                    number_active_days = d.NumberOfActiveDays,
                    impressions = d.WeeklyImpressions,
                    rating_points = d.WeeklyRatings,
                    impressions_percentage = d.WeeklyImpressionsPercentage,
                    media_week_id = d.MediaWeekId,
                    budget = d.WeeklyBudget,
                    adu_impressions = d.AduImpressions,
                    spot_length_id = d.SpotLengthId,
                    daypart_default_id = d.DaypartCodeId,
                    percentage_of_week = d.PercentageOfWeek
                });
            });
        }

        private static void _MapPlanAudienceInfo(plan_versions entity, PlanDto planDto)
        {
            entity.target_audience_id = planDto.AudienceId;
            entity.audience_type = (int)planDto.AudienceType;
            entity.hut_book_id = planDto.HUTBookId;
            entity.share_book_id = planDto.ShareBookId;
            entity.posting_type = (int)planDto.PostingType;
        }

        private static void _MapPlanBudget(plan_versions entity, PlanDto planDto)
        {
            entity.budget = planDto.Budget.Value;
            entity.target_impression = planDto.TargetImpressions.Value;
            entity.target_cpm = planDto.TargetCPM.Value;
            entity.target_rating_points = planDto.TargetRatingPoints.Value;
            entity.target_cpp = planDto.TargetCPP.Value;
            entity.currency = (int)planDto.Currency;
        }

        private void _MapPlanFlightDays(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_flight_days.RemoveRange(entity.plan_version_flight_days);
            planDto.FlightDays.ForEach(d =>
            {
                entity.plan_version_flight_days.Add(new plan_version_flight_days { day_id = d });
            });
        }

        private static void _MapPlanFlightHiatus(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_flight_hiatus_days.RemoveRange(entity.plan_version_flight_hiatus_days);
            planDto.FlightHiatusDays.ForEach(d =>
            {
                entity.plan_version_flight_hiatus_days.Add(new plan_version_flight_hiatus_days { hiatus_day = d });
            });
        }

        private static PlanDaypartDto _MapPlanDaypartDto(plan_version_dayparts entity)
        {
            var dto = new PlanDaypartDto
            {
                DaypartCodeId = entity.daypart_default_id,
                DaypartTypeId = EnumHelper.GetEnum<DaypartTypeEnum>(entity.daypart_type),
                StartTimeSeconds = entity.start_time_seconds,
                IsStartTimeModified = entity.is_start_time_modified,
                EndTimeSeconds = entity.end_time_seconds,
                IsEndTimeModified = entity.is_end_time_modified,
                WeightingGoalPercent = entity.weighting_goal_percent
            };

            // if the contain type has ever been set
            if (entity.show_type_restrictions_contain_type.HasValue)
            {
                dto.Restrictions.ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                {
                    ContainType = (ContainTypeEnum)entity.show_type_restrictions_contain_type.Value,
                    ShowTypes = entity.plan_version_daypart_show_type_restrictions.Select(x => _MapToLookupDto(x.show_types)).ToList()
                };
            }

            // if the contain type has ever been set
            if (entity.genre_restrictions_contain_type.HasValue)
            {
                dto.Restrictions.GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                {
                    ContainType = (ContainTypeEnum)entity.genre_restrictions_contain_type.Value,
                    Genres = entity.plan_version_daypart_genre_restrictions.Select(x => _MapToLookupDto(x.genre)).ToList()
                };
            }

            if (entity.program_restrictions_contain_type.HasValue)
            {
                dto.Restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                {
                    ContainType = (ContainTypeEnum)entity.program_restrictions_contain_type.Value,
                    Programs = entity.plan_version_daypart_program_restrictions.Select(_MapToProgramDto).ToList()
                };
            }

            if (entity.affiliate_restrictions_contain_type.HasValue)
            {
                dto.Restrictions.AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                {
                    ContainType = (ContainTypeEnum)entity.affiliate_restrictions_contain_type.Value,
                    Affiliates = entity.plan_version_daypart_affiliate_restrictions.Select(x => _MapToLookupDto(x.affiliate)).ToList()
                };
            }

            return dto;
        }

        private static LookupDto _MapToLookupDto(show_types show_Type)
        {
            return new LookupDto
            {
                Id = show_Type.id,
                Display = show_Type.name
            };
        }

        private static LookupDto _MapToLookupDto(affiliate affiliate)
        {
            return new LookupDto
            {
                Id = affiliate.id,
                Display = affiliate.name
            };
        }

        private static ProgramDto _MapToProgramDto(plan_version_daypart_program_restrictions programRestriction)
        {
            return new ProgramDto
            {
                Genre = new LookupDto
                {
                    Id = programRestriction.genre.id,
                    Display = programRestriction.genre.name
                },
                ContentRating = programRestriction.content_rating,
                Name = programRestriction.program_name
            };
        }

        private static LookupDto _MapToLookupDto(genre genre)
        {
            return new LookupDto
            {
                Id = genre.id,
                Display = genre.name
            };
        }

        private static void _MapDayparts(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_dayparts.RemoveRange(entity.plan_version_dayparts);

            foreach (var daypart in planDto.Dayparts)
            {
                var newDaypart = new plan_version_dayparts
                {
                    daypart_default_id = daypart.DaypartCodeId,
                    daypart_type = (int)daypart.DaypartTypeId,
                    start_time_seconds = daypart.StartTimeSeconds,
                    is_start_time_modified = daypart.IsStartTimeModified,
                    end_time_seconds = daypart.EndTimeSeconds,
                    is_end_time_modified = daypart.IsEndTimeModified,
                    weighting_goal_percent = daypart.WeightingGoalPercent
                };

                if (daypart.Restrictions != null)
                {
                    _HydrateShowTypeRestrictions(newDaypart, daypart.Restrictions.ShowTypeRestrictions);
                    _HydrateGenreRestrictions(newDaypart, daypart.Restrictions.GenreRestrictions);
                    _HydrateProgramRestrictions(newDaypart, daypart.Restrictions.ProgramRestrictions);
                    _HydrateAffiliateRestrictions(newDaypart, daypart.Restrictions.AffiliateRestrictions);
                }

                entity.plan_version_dayparts.Add(newDaypart);
            }
        }

        private static void _HydrateShowTypeRestrictions(
            plan_version_dayparts daypart,
            PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto showTypeRestrictions)
        {
            if (showTypeRestrictions == null)
                return;

            daypart.show_type_restrictions_contain_type = (int)showTypeRestrictions.ContainType;

            if (!showTypeRestrictions.ShowTypes.IsEmpty())
            {
                foreach (var showTypeRestriction in showTypeRestrictions.ShowTypes)
                {
                    daypart.plan_version_daypart_show_type_restrictions.Add(new plan_version_daypart_show_type_restrictions
                    {
                        show_type_id = showTypeRestriction.Id
                    });
                }
            }
        }

        private static void _HydrateGenreRestrictions(
            plan_version_dayparts daypart,
            PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto genreRestrictions)
        {
            if (genreRestrictions == null)
                return;

            daypart.genre_restrictions_contain_type = (int)genreRestrictions.ContainType;

            if (!genreRestrictions.Genres.IsEmpty())
            {
                foreach (var genreRestriction in genreRestrictions.Genres)
                {
                    daypart.plan_version_daypart_genre_restrictions.Add(new plan_version_daypart_genre_restrictions
                    {
                        genre_id = genreRestriction.Id
                    });
                }
            }
        }

        private static void _HydrateProgramRestrictions(
            plan_version_dayparts daypart,
            PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto programRestrictions)
        {
            if (programRestrictions == null)
                return;

            daypart.program_restrictions_contain_type = (int)programRestrictions.ContainType;

            if (!programRestrictions.Programs.IsEmpty())
            {
                foreach (var programRestriction in programRestrictions.Programs)
                {
                    daypart.plan_version_daypart_program_restrictions.Add(new plan_version_daypart_program_restrictions
                    {
                        genre_id = programRestriction.Genre.Id,
                        content_rating = programRestriction.ContentRating,
                        program_name = programRestriction.Name
                    });
                }
            }
        }

        private static void _HydrateAffiliateRestrictions(
            plan_version_dayparts newDaypart,
            PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto affiliateRestrictions)
        {
            if (affiliateRestrictions == null)
                return;

            newDaypart.affiliate_restrictions_contain_type = (int)affiliateRestrictions.ContainType;

            if (!affiliateRestrictions.Affiliates.IsEmpty())
            {
                foreach (var affiliateRestriction in affiliateRestrictions.Affiliates)
                {
                    newDaypart.plan_version_daypart_affiliate_restrictions.Add(new plan_version_daypart_affiliate_restrictions
                    {
                        affiliate_id = affiliateRestriction.Id
                    });
                }
            }
        }

        private static void _MapPlanSecondaryAudiences(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_secondary_audiences.RemoveRange(entity.plan_version_secondary_audiences);
            planDto.SecondaryAudiences.ForEach(d =>
            {
                entity.plan_version_secondary_audiences.Add(new plan_version_secondary_audiences
                {
                    audience_id = d.AudienceId,
                    audience_type = (int)d.Type,
                    vpvh = d.Vpvh,
                    rating_points = d.RatingPoints.Value,
                    impressions = d.Impressions.Value,
                    cpm = d.CPM.Value,
                    cpp = (double)d.CPP.Value,
                    universe = d.Universe
                });
            });
        }

        private static void _MapPlanMarkets(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_available_markets.RemoveRange(entity.plan_version_available_markets);
            context.plan_version_blackout_markets.RemoveRange(entity.plan_version_blackout_markets);

            planDto.AvailableMarkets.ForEach(m =>
            {
                entity.plan_version_available_markets.Add(new plan_version_available_markets
                {
                    market_code = m.MarketCode,
                    market_coverage_File_id = m.MarketCoverageFileId,
                    rank = m.Rank,
                    percentage_of_us = m.PercentageOfUS,
                    share_of_voice_percent = m.ShareOfVoicePercent
                }
                );
            });

            planDto.BlackoutMarkets.ForEach(m =>
            {
                entity.plan_version_blackout_markets.Add(new plan_version_blackout_markets()
                {
                    market_code = m.MarketCode,
                    market_coverage_file_id = m.MarketCoverageFileId,
                    rank = m.Rank,
                    percentage_of_us = m.PercentageOfUS,
                });
            });
        }

        private static PlanAvailableMarketDto _MapAvailableMarketDto(plan_version_available_markets entity, List<market> markets)
        {
            var marketName = markets.Where(s => s.market_code == entity.market_code).Select(s => s.geography_name)
                .Single($"More than one market found with code '{entity.market_code}'.");

            var dto = new PlanAvailableMarketDto
            {
                Id = entity.id,
                MarketCode = entity.market_code,
                Market = marketName,
                MarketCoverageFileId = entity.market_coverage_File_id,
                Rank = entity.rank,
                PercentageOfUS = entity.percentage_of_us,
                ShareOfVoicePercent = entity.share_of_voice_percent,
            };
            return dto;
        }

        private static PlanBlackoutMarketDto _MapBlackoutMarketDto(plan_version_blackout_markets entity, List<market> markets)
        {
            var marketName = markets.Where(s => s.market_code == entity.market_code).Select(s => s.geography_name)
                .Single($"More than one market found with code '{entity.market_code}'.");

            var dto = new PlanBlackoutMarketDto
            {
                Id = entity.id,
                MarketCode = entity.market_code,
                Market = marketName,
                MarketCoverageFileId = entity.market_coverage_file_id,
                Rank = entity.rank,
                PercentageOfUS = entity.percentage_of_us,
            };
            return dto;
        }

        public List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var executions = context
                    .plan_version_pricing_parameters
                    .Include(x => x.plan_versions)
                    .Include(x => x.plan_versions.plan_version_available_markets)
                    .Include(x => x.plan_version_pricing_parameters_inventory_source_percentages)
                    .Include(x => x.plan_version_pricing_parameters_inventory_source_type_percentages)
                    .Where(p => p.plan_versions.plan_id == planId);

                return executions.ToList().Select(e => new PlanPricingApiRequestParametersDto
                {
                    PlanId = e.plan_versions.plan_id,
                    BudgetGoal = e.budget_adjusted,
                    CompetitionFactor = e.competition_factor,
                    CoverageGoalPercent = e.plan_versions.coverage_goal_percent,
                    CpmGoal = e.cpm_adjusted,
                    ImpressionsGoal = e.impressions_goal,
                    InflationFactor = e.inflation_factor,
                    Markets = e.plan_versions.plan_version_available_markets.Select(m => new PlanPricingMarketDto
                    {
                        MarketId = m.market_code,
                        MarketShareOfVoice = m.share_of_voice_percent
                    }).ToList(),
                    MaxCpm = e.max_cpm,
                    MinCpm = e.min_cpm,
                    ProprietaryBlend = e.proprietary_blend,
                    UnitCaps = e.unit_caps,
                    UnitCapType = (UnitCapEnum)e.unit_caps_type,
                    InventorySourcePercentages = e.plan_version_pricing_parameters_inventory_source_percentages.Select(
                        s => new PlanPricingInventorySourceDto
                        {
                            Id = s.inventory_source_id,
                            Name = s.inventory_sources.name,
                            Percentage = s.percentage
                        }).ToList(),
                    InventorySourceTypePercentages = e.plan_version_pricing_parameters_inventory_source_type_percentages.Select(
                        s => new PlanPricingInventorySourceTypeDto
                        {
                            Id = s.inventory_source_type,
                            Name = ((InventorySourceTypeEnum)s.inventory_source_type).GetDescriptionAttribute(),
                            Percentage = s.percentage
                        }).ToList(),
                    JobId = e.plan_version_pricing_job_id,
                    Margin = e.margin
                }).ToList();
            });
        }

        public int AddPlanPricingJob(PlanPricingJob planPricingJob)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planPricingJobDb = new plan_version_pricing_job
                {
                    plan_version_id = planPricingJob.PlanVersionId,
                    status = (int)planPricingJob.Status,
                    queued_at = planPricingJob.Queued,
                    completed_at = planPricingJob.Completed
                };

                context.plan_version_pricing_job.Add(planPricingJobDb);

                context.SaveChanges();

                return planPricingJobDb.id;
            });
        }

        public void UpdatePlanPricingJob(PlanPricingJob planPricingJob)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_pricing_job.Single(x => x.id == planPricingJob.Id);

                job.status = (int)planPricingJob.Status;
                job.completed_at = planPricingJob.Completed;
                job.error_message = planPricingJob.ErrorMessage;
                job.diagnostic_result = planPricingJob.DiagnosticResult;

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void UpdateJobHangfireId(int jobId, string hangfireJobId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_pricing_job.Single(x => x.id == jobId);

                job.hangfire_job_id = hangfireJobId;

                context.SaveChanges();
            });
        }

        public PlanPricingJob GetLatestPricingJob(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var latestJob = (from pvpj in context.plan_version_pricing_job
                                 where pvpj.plan_versions.plan_id == planId && pvpj.plan_version_id == pvpj.plan_versions.plan.latest_version_id
                                 select pvpj)
                                // ignore canceled runs
                                .Where(x => x.status != (int)BackgroundJobProcessingStatus.Canceled)
                                // take jobs with status Queued or Processing first
                                .OrderByDescending(x => x.status == (int)BackgroundJobProcessingStatus.Queued || x.status == (int)BackgroundJobProcessingStatus.Processing)
                                // then take latest completed
                                .ThenByDescending(x => x.completed_at)
                                .FirstOrDefault();

                if (latestJob == null)
                    return null;

                return new PlanPricingJob
                {
                    Id = latestJob.id,
                    HangfireJobId = latestJob.hangfire_job_id,
                    PlanVersionId = latestJob.plan_version_id,
                    Status = (BackgroundJobProcessingStatus)latestJob.status,
                    Queued = latestJob.queued_at,
                    Completed = latestJob.completed_at,
                    ErrorMessage = latestJob.error_message,
                    DiagnosticResult = latestJob.diagnostic_result
                };
            });
        }

        public PlanPricingJob GetPlanPricingJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_pricing_job
                    .Single(x => x.id == jobId, $"Job id {jobId} not found.");

                return new PlanPricingJob
                {
                    Id = job.id,
                    HangfireJobId = job.hangfire_job_id,
                    PlanVersionId = job.plan_version_id,
                    Status = (BackgroundJobProcessingStatus)job.status,
                    Queued = job.queued_at,
                    Completed = job.completed_at,
                    ErrorMessage = job.error_message,
                    DiagnosticResult = job.diagnostic_result
                };
            });
        }

        public PlanPricingParametersDto GetLatestParametersForPlanPricingJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planVersionId = context.plan_version_pricing_job
                    .Single(x => x.id == jobId, $"Job id {jobId} not found.")
                    .plan_version_id;

                var latestParametersId = context.plan_version_pricing_parameters
                    .Where(p => p.plan_version_id == planVersionId)
                    .Select(p => p.id)
                    .Max();

                var latestParameters = context.plan_version_pricing_parameters
                    .Include(x => x.plan_version_pricing_parameters_inventory_source_percentages)
                    .Include(x => x.plan_version_pricing_parameters_inventory_source_type_percentages)
                    .Include(x => x.plan_versions)
                    .Where(x => x.id == latestParametersId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (latestParameters == null)
                    throw new Exception("Latest pricing job parameters not found.");

                var dto = _MapPlanPricingParameters(latestParameters);

                return dto;
            });
        }

        private PlanPricingParametersDto _MapPlanPricingParameters(plan_version_pricing_parameters entity)
        {
            var dto = new PlanPricingParametersDto
            {
                PlanId = entity.plan_versions.plan_id,
                MinCpm = entity.min_cpm,
                MaxCpm = entity.max_cpm,
                DeliveryImpressions = entity.impressions_goal,
                Budget = entity.budget_goal,
                ProprietaryBlend = entity.proprietary_blend,
                CPM = entity.cpm_goal,
                CompetitionFactor = entity.competition_factor,
                InflationFactor = entity.inflation_factor,
                UnitCaps = entity.unit_caps,
                UnitCapsType = (UnitCapEnum)entity.unit_caps_type,
                Currency = (PlanCurrenciesEnum)entity.currency,
                CPP = entity.cpp,
                DeliveryRatingPoints = entity.rating_points,
                Margin = entity.margin,
                InventorySourcePercentages = entity.plan_version_pricing_parameters_inventory_source_percentages.Select(_MapPlanPricingInventorySourceDto).ToList(),
                InventorySourceTypePercentages = entity.plan_version_pricing_parameters_inventory_source_type_percentages.Select(_MapPlanPricingInventorySourceTypeDto).ToList(),
                JobId = entity.plan_version_pricing_job_id,
                PlanVersionId = entity.plan_version_id,
                AdjustedBudget = entity.budget_adjusted,
                AdjustedCPM = entity.cpm_adjusted,
                MarketGroup = (PricingMarketGroupEnum)entity.market_group
            };
            return dto;
        }

        private PlanPricingInventorySourceDto _MapPlanPricingInventorySourceDto(
            plan_version_pricing_parameters_inventory_source_percentages entity)
        {
            var dto = new PlanPricingInventorySourceDto
            {
                Id = entity.inventory_source_id,
                Name = entity.inventory_sources.name,
                Percentage = entity.percentage
            };

            return dto;
        }

        private PlanPricingInventorySourceTypeDto _MapPlanPricingInventorySourceTypeDto(
            plan_version_pricing_parameters_inventory_source_type_percentages entity)
        {
            var dto = new PlanPricingInventorySourceTypeDto
            {
                Id = entity.inventory_source_type,
                Name = ((InventorySourceTypeEnum)entity.inventory_source_type).ToString(),
                Percentage = entity.percentage
            };

            return dto;
        }

        public void SavePlanPricingParameters(PlanPricingParametersDto planPricingParametersDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var planPricingParameters = new plan_version_pricing_parameters
                {
                    plan_version_id = planPricingParametersDto.PlanVersionId,
                    min_cpm = planPricingParametersDto.MinCpm,
                    max_cpm = planPricingParametersDto.MaxCpm,
                    impressions_goal = planPricingParametersDto.DeliveryImpressions,
                    budget_goal = planPricingParametersDto.Budget,
                    cpm_goal = planPricingParametersDto.CPM,
                    proprietary_blend = planPricingParametersDto.ProprietaryBlend,
                    competition_factor = planPricingParametersDto.CompetitionFactor,
                    inflation_factor = planPricingParametersDto.InflationFactor,
                    unit_caps_type = (int)planPricingParametersDto.UnitCapsType,
                    unit_caps = planPricingParametersDto.UnitCaps,
                    cpp = planPricingParametersDto.CPP,
                    currency = (int)planPricingParametersDto.Currency,
                    rating_points = planPricingParametersDto.DeliveryRatingPoints,
                    margin = planPricingParametersDto.Margin,
                    plan_version_pricing_job_id = planPricingParametersDto.JobId,
                    budget_adjusted = planPricingParametersDto.AdjustedBudget,
                    cpm_adjusted = planPricingParametersDto.AdjustedCPM,
                    market_group = (int)planPricingParametersDto.MarketGroup
                };

                planPricingParametersDto.InventorySourcePercentages.ForEach(s => planPricingParameters.plan_version_pricing_parameters_inventory_source_percentages.Add(
                    new plan_version_pricing_parameters_inventory_source_percentages
                    {
                        inventory_source_id = s.Id,
                        percentage = s.Percentage
                    }));

                planPricingParametersDto.InventorySourceTypePercentages.ForEach(s => planPricingParameters.plan_version_pricing_parameters_inventory_source_type_percentages.Add(
                    new plan_version_pricing_parameters_inventory_source_type_percentages
                    {
                        inventory_source_type = (byte)s.Id,
                        percentage = s.Percentage
                    }));

                context.plan_version_pricing_parameters.Add(planPricingParameters);

                context.SaveChanges();
            });
        }

        public void SavePricingApiResults(PlanPricingAllocationResult result)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };
                var planPricingApiResult = new plan_version_pricing_api_results
                {
                    plan_version_id = result.PlanVersionId,
                    optimal_cpm = result.PricingCpm,
                    plan_version_pricing_job_id = result.JobId
                };

                context.plan_version_pricing_api_results.Add(planPricingApiResult);

                context.SaveChanges();

                var planPricingApiResultSpots = new List<plan_version_pricing_api_result_spots>();

                foreach (var spot in result.Spots)
                {
                    var planPricingApiResultSpot = new plan_version_pricing_api_result_spots
                    {
                        plan_version_pricing_api_results_id = planPricingApiResult.id,
                        station_inventory_manifest_id = spot.Id,
                        contract_media_week_id = spot.ContractMediaWeek.Id,
                        inventory_media_week_id = spot.InventoryMediaWeek.Id,
                        impressions = spot.Impressions,
                        standard_daypart_id = spot.StandardDaypart.Id,
                        cost = spot.SpotCost,
                        spots = spot.Spots
                    };

                    planPricingApiResultSpots.Add(planPricingApiResultSpot);
                }

                BulkInsert(context, planPricingApiResultSpots, propertiesToIgnore);
            });
        }

        public PlanPricingAllocationResult GetPricingApiResults(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;
                var apiResult = context.plan_version_pricing_api_results
                    .Include(x => x.plan_version_pricing_api_result_spots)
                    .Where(p => p.plan_version_id == planVersionId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    return null;

                return new PlanPricingAllocationResult
                {
                    PricingCpm = apiResult.optimal_cpm,
                    JobId = apiResult.plan_version_pricing_job_id,
                    PlanVersionId = apiResult.plan_version_id,
                    Spots = apiResult.plan_version_pricing_api_result_spots.Select(x => new PlanPricingAllocatedSpot
                    {
                        Id = x.id,
                        StationInventoryManifestId = x.station_inventory_manifest_id,
                        Impressions = x.impressions,
                        SpotCost = x.cost,
                        Spots = x.spots,
                        InventoryMediaWeek = new MediaWeek
                        {
                            Id = x.inventory_media_week.id,
                            MediaMonthId = x.inventory_media_week.media_month_id,
                            WeekNumber = x.inventory_media_week.week_number,
                            StartDate = x.inventory_media_week.start_date,
                            EndDate = x.inventory_media_week.end_date
                        },
                        ContractMediaWeek = new MediaWeek
                        {
                            Id = x.contract_media_week.id,
                            MediaMonthId = x.contract_media_week.media_month_id,
                            WeekNumber = x.contract_media_week.week_number,
                            StartDate = x.contract_media_week.start_date,
                            EndDate = x.contract_media_week.end_date
                        },
                        StandardDaypart = _MapToDaypartDefaultDto(x.daypart_defaults)
                    }).ToList()
                };
            });
        }

        public PlanPricingBandDto GetPlanPricingBand(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;

                var result = context.plan_version_pricing_bands.Where(p => p.plan_version_id == planVersionId).OrderByDescending(p => p.id).FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanPricingBandDto
                {
                    Totals = new PlanPricingBandTotalsDto
                    {
                        TotalCpm = result.total_cpm,
                        TotalBudget = result.total_budget,
                        TotalImpressions = result.total_impressions, 
                        TotalSpots = result.total_spots
                    },
                    Bands = result.plan_version_pricing_band_details.Select(r => new PlanPricingBandDetailDto
                    {
                        Id = r.id,
                        MinBand = r.min_band,
                        MaxBand = r.max_band,
                        Spots = r.spots,
                        Impressions = r.impressions,
                        Budget = r.budget,
                        Cpm = r.cpm,
                        ImpressionsPercentage = r.impressions_percentage,
                        AvailableInventoryPercent = r.available_inventory_percentage
                    }).OrderBy(p => p.MinBand).ToList()
                };
            });
        }

        public void SavePlanPricingBands(PlanPricingBandDto planPricingBandDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var planPricingBand = new plan_version_pricing_bands
                {
                    plan_version_id = planPricingBandDto.PlanVersionId,
                    plan_version_pricing_job_id = planPricingBandDto.JobId,
                    total_budget = planPricingBandDto.Totals.TotalBudget,
                    total_cpm = planPricingBandDto.Totals.TotalCpm,
                    total_impressions = planPricingBandDto.Totals.TotalImpressions,
                    total_spots = planPricingBandDto.Totals.TotalSpots
                };

                foreach(var bandDto in planPricingBandDto.Bands)
                {
                    var band = new plan_version_pricing_band_details
                    {
                        cpm = bandDto.Cpm,
                        max_band = bandDto.MaxBand,
                        min_band = bandDto.MinBand,
                        budget = bandDto.Budget,
                        spots = bandDto.Spots,
                        impressions = bandDto.Impressions,
                        impressions_percentage = bandDto.ImpressionsPercentage,
                        available_inventory_percentage = bandDto.AvailableInventoryPercent
                    };

                    planPricingBand.plan_version_pricing_band_details.Add(band);
                }

                context.plan_version_pricing_bands.Add(planPricingBand);

                context.SaveChanges();
            });
        }

        private DaypartDefaultDto _MapToDaypartDefaultDto(daypart_defaults daypartDefault)
        {
            if (daypartDefault == null)
                return null;

            return new DaypartDefaultDto
            {
                Id = daypartDefault.id,
                Code = daypartDefault.code,
                FullName = daypartDefault.name
            };
        }

        public void SavePricingAggregateResults(PlanPricingResultBaseDto pricingResult)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };

                var planPricingResult = new plan_version_pricing_results
                {
                    plan_version_id = pricingResult.PlanVersionId,
                    optimal_cpm = pricingResult.OptimalCpm,
                    total_market_count = pricingResult.Totals.MarketCount,
                    total_station_count = pricingResult.Totals.StationCount,
                    total_avg_cpm = pricingResult.Totals.AvgCpm,
                    total_avg_impressions = pricingResult.Totals.AvgImpressions,
                    total_budget = pricingResult.Totals.Budget,
                    total_impressions = pricingResult.Totals.Impressions,
                    plan_version_pricing_job_id = pricingResult.JobId,
                    goal_fulfilled_by_proprietary = pricingResult.GoalFulfilledByProprietary,
                    total_spots = pricingResult.Totals.Spots
                };

                context.plan_version_pricing_results.Add(planPricingResult);

                context.SaveChanges();

                var spots = new List<plan_version_pricing_result_spots>();

                foreach (var program in pricingResult.Programs)
                {
                    var planPricingResultSpots = new plan_version_pricing_result_spots
                    {
                        plan_version_pricing_result_id = planPricingResult.id,
                        program_name = program.ProgramName,
                        genre = program.Genre,
                        avg_impressions = program.AvgImpressions,
                        avg_cpm = program.AvgCpm,
                        percentage_of_buy = program.PercentageOfBuy,
                        market_count = program.MarketCount,
                        station_count = program.StationCount,
                        budget = program.Budget,
                        spots = program.Spots,
                        impressions = program.Impressions
                    };

                    spots.Add(planPricingResultSpots);
                }

                if (spots.Any())
                {
                    BulkInsert(context, spots, propertiesToIgnore);
                }
            });
        }

        public PricingProgramsResultDto GetPricingProgramsResult(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;

                var result = context.plan_version_pricing_results.Where(p => p.plan_version_id == planVersionId).OrderByDescending(p => p.id).FirstOrDefault();
                if (result == null)
                    return null;

                return new PricingProgramsResultDto
                {
                    Totals = new PricingProgramsResultTotalsDto
                    {
                        MarketCount = result.total_market_count,
                        StationCount = result.total_station_count,
                        AvgCpm = result.total_avg_cpm,
                        AvgImpressions = result.total_avg_impressions,
                        Budget = result.total_budget,
                        Spots = result.total_spots,
                        Impressions = result.total_impressions
                    },
                    Programs = result.plan_version_pricing_result_spots.Select(r => new PlanPricingProgramProgramDto
                    {
                        Id = r.id,
                        ProgramName = r.program_name,
                        Genre = r.genre,
                        AvgCpm = r.avg_cpm,
                        AvgImpressions = r.avg_impressions,
                        ImpressionsPercentage = r.percentage_of_buy,
                        MarketCount = r.market_count,
                        StationCount = r.station_count,
                        Impressions = r.impressions,
                        Budget = r.budget,
                        Spots = r.spots
                    }).OrderByDescending(p => p.ImpressionsPercentage)
                       .ThenByDescending(p => p.AvgCpm)
                       .ThenBy(p => p.ProgramName).ToList()
                };
            });
        }

        public CurrentPricingExecutionResultDto GetPricingResults(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;
                var result = context.plan_version_pricing_results.Where(p => p.plan_version_id == planVersionId).OrderByDescending(p => p.id).FirstOrDefault();
                if (result == null)
                    return null;
                return new CurrentPricingExecutionResultDto
                {
                    OptimalCpm = result.optimal_cpm,
                    JobId = result.plan_version_pricing_job_id,
                    PlanVersionId = result.plan_version_id,
                    GoalFulfilledByProprietary = result.goal_fulfilled_by_proprietary
                };
            });
        }

        public void SavePlanPricingEstimates(int jobId, List<PricingEstimate> estimates)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };

                var itemsToInsert = estimates
                    .Select(x => new plan_version_pricing_job_inventory_source_estimates
                    {
                        media_week_id = x.MediaWeekId,
                        inventory_source_id = x.InventorySourceId,
                        inventory_source_type = (int?)x.InventorySourceType,
                        plan_version_pricing_job_id = jobId,
                        impressions = x.Impressions,
                        cost = x.Cost
                    })
                    .ToList();

                BulkInsert(context, itemsToInsert, propertiesToIgnore);
            });
        }

        public List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanId(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;
                var apiResult = context.plan_version_pricing_api_results
                    .Include(x => x.plan_version_pricing_api_result_spots)
                    .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.inventory_media_week))
                    .Where(p => p.plan_version_id == planVersionId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    throw new Exception($"No pricing runs were found for the version {planVersionId}");

                return apiResult.plan_version_pricing_api_result_spots.Select(_MapToPlanPricingAllocatedSpot).ToList();
            });
        }

        public List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanVersionId(int planVersionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var apiResult = context.plan_version_pricing_api_results
                    .Include(x => x.plan_version_pricing_api_result_spots)
                    .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.inventory_media_week))
                    .Where(p => p.plan_version_id == planVersionId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    throw new Exception($"No pricing runs were found for the version {planVersionId}");

                return apiResult.plan_version_pricing_api_result_spots.Select(_MapToPlanPricingAllocatedSpot).ToList();
            });
        }

        private PlanPricingAllocatedSpot _MapToPlanPricingAllocatedSpot(plan_version_pricing_api_result_spots spot)
        {
            return new PlanPricingAllocatedSpot
            {
                Id = spot.id,
                StationInventoryManifestId = spot.station_inventory_manifest_id,
                Impressions = spot.impressions,
                SpotCost = spot.cost,
                Spots = spot.spots,
                InventoryMediaWeek = new MediaWeek
                {
                    Id = spot.inventory_media_week.id,
                    MediaMonthId = spot.inventory_media_week.media_month_id,
                    WeekNumber = spot.inventory_media_week.week_number,
                    StartDate = spot.inventory_media_week.start_date,
                    EndDate = spot.inventory_media_week.end_date
                },
                ContractMediaWeek = new MediaWeek
                {
                    Id = spot.contract_media_week.id,
                    MediaMonthId = spot.contract_media_week.media_month_id,
                    WeekNumber = spot.contract_media_week.week_number,
                    StartDate = spot.contract_media_week.start_date,
                    EndDate = spot.contract_media_week.end_date
                },
                StandardDaypart = _MapToDaypartDefaultDto(spot.daypart_defaults)
            };
        }

        public List<PlanDto> LoadPlansForWeeklyBreakdownRemapping()
        {
            return _InReadUncommitedTransaction(context =>
            {
                List<PlanDto> result = new List<PlanDto>();
                var queryData = (from plan in context.plans
                                 from planVersion in plan.plan_versions
                                 let planWeeks = planVersion.plan_version_weekly_breakdown_duplicate
                                 let planDayparts = planVersion.plan_version_dayparts
                                 let planCreativeLengths = planVersion.plan_version_creative_lengths
                                 where planWeeks.All(x => x.daypart_default_id == null && x.spot_length_id == null)
                                 select new
                                 {
                                     PlanVersionId = planVersion.id,
                                     PlanId = plan.id,
                                     GoalBreakdownType = planVersion.goal_breakdown_type,
                                     TargetImpression = planVersion.target_impression,
                                     Budget = planVersion.budget,
                                     TargetRatingPoints = planVersion.target_rating_points,
                                     Weeks = planWeeks,
                                     Dayparts = planDayparts,
                                     CreativeLengths = planCreativeLengths
                                 }).ToList();
                queryData.ForEach(x =>
                  result.Add(new PlanDto
                  {
                      VersionId = x.PlanVersionId,
                      Id = x.PlanId,
                      TargetImpressions = x.TargetImpression,
                      Budget = x.Budget,
                      TargetRatingPoints = x.TargetRatingPoints,
                      GoalBreakdownType = (PlanGoalBreakdownTypeEnum)x.GoalBreakdownType,
                      CreativeLengths = x.CreativeLengths.Select(y => new CreativeLength
                      {
                          SpotLengthId = y.spot_length_id,
                          Weight = y.weight
                      }).ToList(),
                      Dayparts = x.Dayparts.Select(y => new PlanDaypartDto
                      {
                          DaypartCodeId = y.daypart_default_id,
                          DaypartTypeId = EnumHelper.GetEnum<DaypartTypeEnum>(y.daypart_type),
                          StartTimeSeconds = y.start_time_seconds,
                          IsStartTimeModified = y.is_start_time_modified,
                          EndTimeSeconds = y.end_time_seconds,
                          IsEndTimeModified = y.is_end_time_modified,
                          WeightingGoalPercent = y.weighting_goal_percent
                      }).ToList(),
                      WeeklyBreakdownWeeks = x.Weeks.Select(y => new WeeklyBreakdownWeek
                      {
                          ActiveDays = y.active_days_label,
                          WeeklyImpressions = y.impressions,
                          WeeklyRatings = y.rating_points,
                          NumberOfActiveDays = y.number_active_days,
                          WeeklyImpressionsPercentage = y.impressions_percentage,
                          MediaWeekId = y.media_week_id,
                          WeeklyBudget = y.budget,
                          WeeklyAdu = y.adu
                      }).ToList()
                  }));
                return result;
            });
        }

        public void SaveWeeklyBreakdownDistribution(PlanDto plan)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_weekly_breakdown
                    .RemoveRange(context.plan_version_weekly_breakdown.Where(x => x.plan_version_id == plan.VersionId));
                context.plan_version_weekly_breakdown.AddRange(
                    plan.WeeklyBreakdownWeeks.Select(x => new plan_version_weekly_breakdown
                    {
                        plan_version_id = plan.VersionId,
                        active_days_label = x.ActiveDays,
                        adu_impressions = x.AduImpressions,
                        budget = x.WeeklyBudget,
                        daypart_default_id = x.DaypartCodeId,
                        impressions = x.WeeklyImpressions,
                        impressions_percentage = x.WeeklyImpressionsPercentage,
                        media_week_id = x.MediaWeekId,
                        number_active_days = x.NumberOfActiveDays,
                        percentage_of_week = x.PercentageOfWeek,
                        rating_points = x.WeeklyRatings,
                        spot_length_id = x.SpotLengthId
                    }).ToList());
                context.SaveChanges();
            });
        }
        public void SaveCreativeLengths(PlanDto plan)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_creative_lengths
                    .RemoveRange(context.plan_version_creative_lengths.Where(x => x.plan_version_id == plan.VersionId));
                context.plan_version_creative_lengths.AddRange(
                    plan.CreativeLengths.Select(x => new plan_version_creative_lengths
                    {
                        plan_version_id = plan.VersionId,
                        spot_length_id = x.SpotLengthId,
                        weight = x.Weight
                    }).ToList());
                context.SaveChanges();
            });
        }
    }
}