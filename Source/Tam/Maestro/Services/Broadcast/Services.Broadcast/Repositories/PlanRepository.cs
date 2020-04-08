using Common.Services.Extensions;
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
        /// Saves the model that ran in the pricing guide.
        /// </summary>
        /// <param name="planPricingRunModel">Plan pricing model data</param>
        void SavePricingRequest(PlanPricingApiRequestParametersDto planPricingRunModel);

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
        void SavePricingApiResults(int planId, PlanPricingAllocationResult result);
        PlanPricingAllocationResult GetPricingApiResults(int planId);
        void SavePricingAggregateResults(int planId, PlanPricingResultBaseDto result);
        PlanPricingResultBaseDto GetPricingResults(int planId);

        PlanPricingJob GetPlanPricingJob(int jobId);

        PlanPricingParametersDto GetLatestParametersForPlanPricingJob(int jobId);

        void SavePlanPricingEstimates(int jobId, List<PricingEstimate> estimates);

        List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanId(int planId);

        List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanVersionId(int planVersionId);

        int GetPlanVersionIdByVersionNumber(int planId, int versionNumber);
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
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
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
                           .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
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
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks.Select(y => y.media_weeks)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters))           
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(y=>y.plan_version_pricing_parameters_inventory_source_percentages)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(y => y.plan_version_pricing_parameters_inventory_source_type_percentages)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(y => y.plan_version_pricing_parameters_inventory_source_percentages.Select(r=> r.inventory_sources))))
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
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks.Select(y => y.media_weeks)))
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
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_weeks.Select(y => y.media_weeks)))
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
                SpotLengthId = latestPlanVersion.spot_length_id,
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
                WeeklyBreakdownWeeks = latestPlanVersion.plan_version_weeks.Select(_MapWeeklyBreakdownWeeks).ToList(),
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
                PricingParameters = _MapPricingParameters(latestPlanVersion.plan_version_pricing_parameters.SingleOrDefault()),
                IsDraft = latestPlanVersion.is_draft,
                VersionNumber = latestPlanVersion.version_number,
                VersionId = latestPlanVersion.id,
                IsAduEnabled = latestPlanVersion.is_adu_enabled
            };
            return dto;
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
                Margin = arg.margin
            };
        }

        private WeeklyBreakdownWeek _MapWeeklyBreakdownWeeks(plan_version_weeks arg)
        {
            return new WeeklyBreakdownWeek
            {
                ActiveDays = arg.active_days_label,
                EndDate = arg.media_weeks.end_date,
                WeeklyImpressions = arg.weekly_impressions,
                WeeklyRatings = arg.weekly_rating_points,
                NumberOfActiveDays = arg.number_active_days,
                WeeklyImpressionsPercentage = arg.weekly_impressions_percentage,
                StartDate = arg.media_weeks.start_date,
                MediaWeekId = arg.media_weeks.id,
                WeeklyBudget = arg.weekly_budget,
                WeeklyAdu = arg.weekly_adu
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

            version.spot_length_id = planDto.SpotLengthId;
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

            _MapPlanAudienceInfo(version, planDto);
            _MapPlanBudget(version, planDto);
            _MapPlanFlightDays(version, planDto, context);
            _MapPlanFlightHiatus(version, planDto, context);
            _MapDayparts(version, planDto, context);
            _MapPlanSecondaryAudiences(version, planDto, context);
            _MapPlanMarkets(version, planDto, context);
            _MapWeeklyBreakdown(version, planDto, context);
        }

        private static void _SetCreatedDate(plan_versions version, string createdBy, DateTime createdDate)
        {
            version.created_by = createdBy; //when editing the draft; the created properties have to be removed in order to not overwrite the initial creator of the draft
            version.created_date = createdDate;
        }

        private static void _MapWeeklyBreakdown(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_weeks.RemoveRange(entity.plan_version_weeks);
            planDto.WeeklyBreakdownWeeks.ForEach(d =>
            {
                entity.plan_version_weeks.Add(new plan_version_weeks
                {
                    active_days_label = d.ActiveDays,
                    number_active_days = d.NumberOfActiveDays,
                    weekly_impressions = d.WeeklyImpressions,
                    weekly_rating_points = d.WeeklyRatings,
                    weekly_impressions_percentage = d.WeeklyImpressionsPercentage,
                    media_week_id = d.MediaWeekId,
                    weekly_budget = d.WeeklyBudget,
                    weekly_adu = d.WeeklyAdu
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

        public void SavePricingRequest(PlanPricingApiRequestParametersDto planPricingRunModel)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var plan = context.plans.Single(x => x.id == planPricingRunModel.PlanId);
                    var planVersionId = plan.latest_version_id;

                    var planPricingExecution = new plan_version_pricing_executions
                    {
                        plan_version_id = planVersionId,
                        min_cpm = planPricingRunModel.MinCpm,
                        max_cpm = planPricingRunModel.MaxCpm,
                        impressions_goal = planPricingRunModel.ImpressionsGoal,
                        budget_goal = planPricingRunModel.BudgetGoal,
                        cpm_goal = planPricingRunModel.CpmGoal,
                        proprietary_blend = planPricingRunModel.ProprietaryBlend,
                        competition_factor = planPricingRunModel.CompetitionFactor,
                        inflation_factor = planPricingRunModel.InflationFactor,
                        coverage_goal = planPricingRunModel.CoverageGoalPercent,
                        unit_caps_type = (int)planPricingRunModel.UnitCapType,
                        unit_caps = planPricingRunModel.UnitCaps,
                        margin = planPricingRunModel.Margin
                    };

                    foreach (var market in planPricingRunModel.Markets)
                    {
                        planPricingExecution.plan_version_pricing_execution_markets.Add(new plan_version_pricing_execution_markets
                        {
                            market_code = (short)market.MarketId,
                            share_of_voice_percent = market.MarketShareOfVoice
                        });
                    }

                    planPricingRunModel.InventorySourcePercentages.ForEach(s => planPricingExecution.plan_version_pricing_inventory_source_percentages.Add(
                        new plan_version_pricing_inventory_source_percentages
                        {
                            inventory_source_id = s.Id,
                            percentage = s.Percentage
                        }));

                    planPricingRunModel.InventorySourceTypePercentages.ForEach(s => planPricingExecution.plan_version_pricing_inventory_source_type_percentages.Add(
                        new plan_version_pricing_inventory_source_type_percentages
                        {
                            inventory_source_type = (byte)s.Id,
                            percentage = s.Percentage
                        }));
                    
                    context.plan_version_pricing_executions.Add(planPricingExecution);
                    context.SaveChanges();
                }
            );
        }

        public List<PlanPricingApiRequestParametersDto> GetPlanPricingRuns(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var executions = context
                    .plan_version_pricing_executions
                    .Include(x => x.plan_versions)
                    .Include(x => x.plan_version_pricing_execution_markets)
                    .Include(x => x.plan_version_pricing_inventory_source_percentages)
                    .Include(x => x.plan_version_pricing_inventory_source_type_percentages)
                    .Where(p => p.plan_versions.plan_id == planId);

                return executions.ToList().Select(e => new PlanPricingApiRequestParametersDto
                {
                    PlanId = e.plan_versions.plan_id,
                    BudgetGoal = e.budget_goal,
                    CompetitionFactor = e.competition_factor,
                    CoverageGoalPercent = e.coverage_goal,
                    CpmGoal = e.cpm_goal,
                    ImpressionsGoal = e.impressions_goal,
                    InflationFactor = e.inflation_factor,
                    Markets = e.plan_version_pricing_execution_markets.Select(m => new PlanPricingMarketDto
                    {
                        MarketId = m.market_code,
                        MarketShareOfVoice = m.share_of_voice_percent
                    }).ToList(),
                    MaxCpm = e.max_cpm,
                    MinCpm = e.min_cpm,
                    ProprietaryBlend = e.proprietary_blend,
                    UnitCaps = e.unit_caps,
                    UnitCapType = (UnitCapEnum)e.unit_caps_type,
                    InventorySourcePercentages = e.plan_version_pricing_inventory_source_percentages.Select(
                        s => new PlanPricingInventorySourceDto
                        {
                            Id = s.inventory_source_id,
                            Name = s.inventory_sources.name,
                            Percentage = s.percentage
                        }).ToList(),
                    InventorySourceTypePercentages = e.plan_version_pricing_inventory_source_type_percentages.Select(
                        s => new PlanPricingInventorySourceTypeDto
                        {
                            Id = s.inventory_source_type,
                            Name = ((InventorySourceTypeEnum)s.inventory_source_type).GetDescriptionAttribute(),
                            Percentage = s.percentage
                        }).ToList()
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
                    .Single("Latest pricing job parameters not found.");

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
                InventorySourceTypePercentages = entity.plan_version_pricing_parameters_inventory_source_type_percentages.Select(_MapPlanPricingInventorySourceTypeDto).ToList()
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
                var plan = context.plans.Single(x => x.id == planPricingParametersDto.PlanId);
                var planVersionId = plan.latest_version_id;
                var previousParameters = context.plan_version_pricing_parameters.SingleOrDefault(x => x.plan_version_id == planVersionId);

                if (previousParameters != null)
                {
                    context.plan_version_pricing_parameters.Remove(previousParameters);
                }

                var planPricingParameters = new plan_version_pricing_parameters
                {
                    plan_version_id = planVersionId,
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
                    margin = planPricingParametersDto.Margin
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

        public void SavePricingApiResults(int planId, PlanPricingAllocationResult result)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;
                var previousResults = context.plan_version_pricing_api_results.Where(x => x.plan_version_id == planVersionId);

                if (previousResults != null)
                {
                    context.plan_version_pricing_api_results.RemoveRange(previousResults);
                }

                var planPricingApiResult = new plan_version_pricing_api_results
                {
                    plan_version_id = planVersionId,
                    optimal_cpm = result.PricingCpm
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
                    .Single(p => p.plan_version_id == planVersionId);

                return new PlanPricingAllocationResult
                {
                    PricingCpm = apiResult.optimal_cpm,
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

        public void SavePricingAggregateResults(int planId, PlanPricingResultBaseDto pricingResult)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;
                var previousResults = context.plan_version_pricing_results.Where(x => x.plan_version_id == planVersionId);

                if (previousResults != null)
                {
                    context.plan_version_pricing_results.RemoveRange(previousResults);
                }

                var planPricingResult = new plan_version_pricing_results
                {
                    plan_version_id = planVersionId,
                    optimal_cpm = pricingResult.OptimalCpm,
                    total_market_count = pricingResult.Totals.MarketCount,
                    total_station_count = pricingResult.Totals.StationCount,
                    total_avg_cpm = pricingResult.Totals.AvgCpm,
                    total_avg_impressions = pricingResult.Totals.AvgImpressions,
                    goal_fulfilled_by_proprietary = pricingResult.GoalFulfilledByProprietary
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
                        station_count = program.StationCount
                    };

                    spots.Add(planPricingResultSpots);
                }

                if (spots.Any())
                {
                    BulkInsert(context, spots, propertiesToIgnore);
                }
            });
        }

        public PlanPricingResultBaseDto GetPricingResults(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;
                var result = context.plan_version_pricing_results.SingleOrDefault(p => p.plan_version_id == planVersionId);
                if (result == null)
                    return null;
                return new PlanPricingResultBaseDto
                {
                    OptimalCpm = result.optimal_cpm,
                    GoalFulfilledByProprietary = result.goal_fulfilled_by_proprietary,
                    Totals = new PlanPricingTotalsDto
                    {
                        MarketCount = result.total_market_count,
                        StationCount = result.total_station_count,
                        AvgCpm = result.total_avg_cpm,
                        AvgImpressions = result.total_avg_impressions
                    },
                    Programs = result.plan_version_pricing_result_spots.Select(r => new PlanPricingProgramDto
                    {
                        Id = r.id,
                        ProgramName = r.program_name,
                        Genre = r.genre,
                        AvgCpm = r.avg_cpm,
                        AvgImpressions = r.avg_impressions,
                        PercentageOfBuy = r.percentage_of_buy,
                        MarketCount = r.market_count,
                        StationCount = r.station_count
                    }).OrderByDescending(p => p.PercentageOfBuy)
                       .ThenByDescending(p => p.AvgCpm)
                       .ThenBy(p => p.ProgramName).ToList()
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
                    .Single(p => p.plan_version_id == planVersionId, $"No prcing runs were found for the version {planVersionId}");

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
                    .Single(p => p.plan_version_id == planVersionId, $"No prcing runs were found for the version {planVersionId}");

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
    }
}