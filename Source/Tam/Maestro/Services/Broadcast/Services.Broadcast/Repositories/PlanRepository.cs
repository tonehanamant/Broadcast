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
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities;
using Tam.Maestro.Data.Entities;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Campaign;

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

        void SetPricingPlanVersionId(int jobId, int planVersionId);

        /// <summary>
        /// Updates the plan pricing job with hangfire job identifier.
        /// </summary>
        /// <param name="jobId">The pricing job identifier.</param>
        /// <param name="hangfireJobId">The hangfire job identifier.</param>
        void UpdateJobHangfireId(int jobId, string hangfireJobId);
        PlanPricingJob GetPricingJobForLatestPlanVersion(int planId);
        PlanPricingJob GetPricingJobForPlanVersion(int planVersionId);

        //PlanPricingJob GetPricingJobForLatestPlanVersions(int planId);
        //PlanPricingJob GetPricingJobForPlanVersions(int planVersionId);

        void SavePlanPricingParameters(PlanPricingParametersDto planPricingRequestDto);
        CurrentPricingExecutionResultDto GetPricingResultsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        List<CurrentPricingExecutionResultDto> GetAllPricingResultsByJobIds(int jobId);

        PlanPricingJob GetPlanPricingJob(int jobId);
        PlanBuyingJob GetPlanBuyingJob(int jobId);

        /// <summary>
        /// Get the plan id for the pricing job.  Null if there was not plan id for the job.
        /// </summary>
        int? GetPlanIdFromPricingJob(int jobId);

        PlanPricingParametersDto GetLatestParametersForPlanPricingJob(int jobId);

        List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanId(int planId,
            PostingTypeEnum postingType, SpotAllocationModelMode spotAllocationModelMode);

        List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanVersionId(int planVersionId);

        int GetPlanVersionIdByVersionNumber(int planId, int versionNumber);

        /// <summary>
        /// Get Goal CPM value
        /// </summary>
        /// <param name="planVersionId"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        decimal GetGoalCpm(int planVersionId, int jobId);

        /// <summary>
        /// Get Goal CPM value
        /// </summary>
        /// <param name="jobId">The job identifier</param>
        /// <returns></returns>
        decimal GetGoalCpm(int jobId);
        /// <summary>Get average calculated VPVH from plan_version_pricing_results_dayparts table
        /// </summary>
        /// <param name="PlanVersionPricingResultId">The plan_version_pricing_results identifier</param>
        /// <returns></returns>
        List<PlanPricingResultsDaypartDto> GetPricingResultsDayparts(int PlanVersionPricingResultId);
        /// <summary>
        /// Updates plan pricing version to point to the previous version of the plan pricing data
        /// </summary>
        /// <param name="versionId">Current version id</param>
        /// <param name="oldPlanVersionId">Previous version id</param>
        void UpdatePlanPricingVersionId(int versionId, int oldPlanVersionId);

        void UpdatePlanBuyingVersionId(int versionId, int oldPlanVersionId);

        void SetBuyingPlanVersionId(int jobId, int planVersionId);

        PlanPricingParametersDto GetPricingParametersForVersion(int versionId);
        PlanPricingParametersDto GetLatestPricingParameters(int planId);

        /// <summary>
        /// Gets the proprietary inventory for program lineup.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>List of ProgramLineupProprietaryInventory objects</returns>
        List<ProgramLineupProprietaryInventory> GetProprietaryInventoryForProgramLineup(int jobId);

        List<PlanPricingJob> GetSuccessfulPricingJobs(int planVersionId);

        void SavePricingApiResults(PlanPricingAllocationResult result);

        PlanPricingAllocationResult GetPricingApiResultsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        PlanPricingAllocationResultDto GetPricingApiResultsByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);


        void SavePricingAggregateResults(PlanPricingResultBaseDto result);
        PricingProgramsResultDto GetPricingProgramsResultByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);
        PricingProgramsResultDto_v2 GetPricingProgramsResultByJobId_v2(int id,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        void SavePlanPricingBands(PlanPricingBand planPricingBand);
        PlanPricingBandDto GetPlanPricingBandByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);
        PlanPricingBandDto_v2 GetPlanPricingBandByJobId_v2(int id,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        void SavePlanPricingStations(PlanPricingStationResult planPricingStationResult);
        PlanPricingStationResultDto GetPricingStationsResultByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);
        PlanPricingStationResultDto_v2 GetPricingStationsResultByJobId_v2(int id,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        void SavePlanPricingMarketResults(PlanPricingResultMarkets planPricingResultMarkets);
        PlanPricingResultMarketsDto GetPlanPricingResultMarketsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);
        PlanPricingResultMarketsDto_v2 GetPlanPricingResultMarketsByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        double GetNsiToNtiConversionRate(List<PlanDaypartDto> planDayparts);
        List<PlanPricingResultsDaypartDto> GetPlanPricingResultsDaypartsByPlanPricingResultId(int planPricingResultId);        
    }

    public class PlanRepository : BroadcastRepositoryBase, IPlanRepository
    {
        public PlanRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper)
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

                       planDto.VersionId = draftVersion.id;
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
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_versions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_version_pricing_parameter_inventory_proprietary_summaries)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_version_pricing_parameter_inventory_proprietary_summaries.Select(r => r.inventory_proprietary_summary))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_buying_parameters))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_audience_daypart_vpvh))
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
                            DaypartCodeId = y.standard_daypart_id,
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
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_versions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_version_pricing_parameter_inventory_proprietary_summaries)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_version_pricing_parameter_inventory_proprietary_summaries.Select(r => r.inventory_proprietary_summary))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_buying_parameters))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_audience_daypart_vpvh))
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
                                    (planVersion.flight_end_date < transitionDate && planVersion.status == (int)PlanStatusEnum.Live)
                                   )
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
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_versions)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_version_pricing_parameter_inventory_proprietary_summaries)))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_pricing_parameters.Select(pp => pp.plan_version_pricing_parameter_inventory_proprietary_summaries.Select(r => r.inventory_proprietary_summary))))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_buying_parameters))
                        .Include(p => p.plan_versions.Select(x => x.plan_version_audience_daypart_vpvh))
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
            var planVersion = versionId != null
                ? entity.plan_versions.Where(x => x.id == versionId).Single($"There is no version {versionId} available")
                : entity.plan_versions.Where(x => x.id == entity.latest_version_id).Single("There is no latest version available");

            //drafts don't have summary, so we're doing SingleOrDefault
            var planSummary = planVersion.plan_version_summaries.SingleOrDefault();
            var dto = new PlanDto
            {
                Id = entity.id,
                CampaignId = entity.campaign.id,
                CampaignName = entity.campaign.name,
                Name = entity.name,
                SpotAllocationModelMode = EnumHelper.GetEnum<SpotAllocationModelMode>(entity.spot_allocation_model_mode ?? (int)SpotAllocationModelMode.Quality),
                CreativeLengths = planVersion.plan_version_creative_lengths.Select(_MapCreativeLengthsToDto).ToList(),
                Equivalized = planVersion.equivalized,
                Status = EnumHelper.GetEnum<PlanStatusEnum>(planVersion.status),
                ProductId = entity.product_id,
                ProductMasterId = entity.product_master_id,
                FlightDays = planVersion.plan_version_flight_days.Select(flightDay => flightDay.day.id).ToList(),
                FlightStartDate = planVersion.flight_start_date,
                FlightEndDate = planVersion.flight_end_date,
                FlightNotes = planVersion.flight_notes,
                FlightNotesInternal = planVersion.flight_notes_internal,
                AudienceId = planVersion.target_audience_id,
                AudienceType = EnumHelper.GetEnum<AudienceTypeEnum>(planVersion.audience_type),
                HUTBookId = planVersion.hut_book_id,
                ShareBookId = planVersion.share_book_id,
                PostingType = EnumHelper.GetEnum<PostingTypeEnum>(planVersion.posting_type),
                FlightHiatusDays = planVersion.plan_version_flight_hiatus_days.Select(h => h.hiatus_day).ToList(),
                Budget = planVersion.budget,
                TargetImpressions = planVersion.target_impression,
                TargetCPM = planVersion.target_cpm,
                TargetRatingPoints = planVersion.target_rating_points,
                TargetCPP = planVersion.target_cpp,
                Currency = EnumHelper.GetEnum<PlanCurrenciesEnum>(planVersion.currency),
                GoalBreakdownType = EnumHelper.GetEnum<PlanGoalBreakdownTypeEnum>(planVersion.goal_breakdown_type),
                SecondaryAudiences = planVersion.plan_version_secondary_audiences.Select(_MapSecondaryAudiences).ToList(),
                Dayparts = planVersion.plan_version_dayparts.Select(d => _MapPlanDaypartDto(d, planVersion)).ToList(),
                CoverageGoalPercent = planVersion.coverage_goal_percent,
                AvailableMarkets = planVersion.plan_version_available_markets.Select(e => _MapAvailableMarketDto(e, markets)).ToList(),
                BlackoutMarkets = planVersion.plan_version_blackout_markets.Select(e => _MapBlackoutMarketDto(e, markets)).ToList(),
                WeeklyBreakdownWeeks = planVersion.plan_version_weekly_breakdown.Select(_MapWeeklyBreakdownWeeks).ToList(),
                ModifiedBy = planVersion.modified_by ?? planVersion.created_by,
                ModifiedDate = planVersion.modified_date ?? planVersion.created_date,
                Vpvh = planVersion.target_vpvh,
                TargetUniverse = planVersion.target_universe,
                HHCPM = planVersion.hh_cpm,
                HHCPP = planVersion.hh_cpp,
                HHImpressions = planVersion.hh_impressions,
                HHRatingPoints = planVersion.hh_rating_points,
                HHUniverse = planVersion.hh_universe,
                AvailableMarketsWithSovCount = planSummary?.available_market_with_sov_count ?? null,
                BlackoutMarketCount = planSummary?.blackout_market_count ?? null,
                BlackoutMarketTotalUsCoveragePercent = planSummary?.blackout_market_total_us_coverage_percent ?? null,
                PricingParameters = _MapPricingParameters(planVersion.plan_version_pricing_parameters.OrderByDescending(p => p.id).FirstOrDefault()),
                IsDraft = planVersion.is_draft,
                VersionNumber = planVersion.version_number,
                VersionId = planVersion.id,
                IsAduEnabled = planVersion.is_adu_enabled,
                ImpressionsPerUnit = planVersion.impressions_per_unit ?? 0,
                BuyingParameters = _MapBuyingParameters(planVersion.plan_version_buying_parameters.OrderByDescending(p => p.id).FirstOrDefault()),
            };

            if (dto.PricingParameters != null)
                dto.JobId = dto.PricingParameters.JobId;

            return dto;
        }

        private PlanBuyingParametersDto _MapBuyingParameters(plan_version_buying_parameters arg)
        {
            if (arg == null)
                return null;

            return new PlanBuyingParametersDto
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
                UnitCaps = arg.unit_caps,
                UnitCapsType = (UnitCapEnum)arg.unit_caps_type,
                Margin = arg.margin,
                JobId = arg.plan_version_buying_job_id,
                PlanVersionId = arg.plan_version_id,
                AdjustedCPM = arg.cpm_adjusted,
                AdjustedBudget = arg.budget_adjusted,
                MarketGroup = (MarketGroupEnum)arg.market_group,
                PostingType = (PostingTypeEnum)arg.posting_type
            };
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

            return new PlanPricingParametersDto
            {
                PlanId = arg.plan_versions?.plan_id,
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
                UnitCaps = arg.unit_caps,
                UnitCapsType = (UnitCapEnum)arg.unit_caps_type,
                Margin = arg.margin,
                JobId = arg.plan_version_pricing_job_id,
                PlanVersionId = arg.plan_version_id,
                AdjustedCPM = arg.cpm_adjusted,
                AdjustedBudget = arg.budget_adjusted,
                MarketGroup = (MarketGroupEnum)arg.market_group,
                ProprietaryInventory = arg.plan_version_pricing_parameter_inventory_proprietary_summaries
                                .Where(x => x.inventory_proprietary_summary.is_active)
                .Select(p => new Entities.InventoryProprietary.InventoryProprietarySummary
                {
                    NumberOfUnit = p.unit_number,
                    Id = p.inventory_proprietary_summary_id
                }).ToList(),
                PostingType = (PostingTypeEnum)arg.posting_type
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
                DaypartCodeId = arg.standard_daypart_id,
                PercentageOfWeek = arg.percentage_of_week,
                UnitImpressions = arg.unit_impressions ?? 0
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
            plan.product_master_id = planDto.ProductMasterId;
            plan.campaign_id = planDto.CampaignId;
            plan.spot_allocation_model_mode = (int)planDto.SpotAllocationModelMode;
            version.equivalized = planDto.Equivalized;
            version.status = (int)planDto.Status;
            version.flight_start_date = planDto.FlightStartDate.Value;
            version.flight_end_date = planDto.FlightEndDate.Value;
            version.flight_notes = planDto.FlightNotes;
            version.flight_notes_internal = planDto.FlightNotesInternal;
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
            version.impressions_per_unit = planDto.ImpressionsPerUnit;

            _MapCreativeLengths(version, planDto, context);
            _MapPlanAudienceInfo(version, planDto);
            _MapPlanBudget(version, planDto);
            _MapPlanFlightDays(version, planDto, context);
            _MapPlanFlightHiatus(version, planDto, context);
            _MapDayparts(version, planDto, context);
            _MapAudienceDaypartVpvh(version, planDto, context);
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
                    standard_daypart_id = d.DaypartCodeId,
                    percentage_of_week = d.PercentageOfWeek,
                    unit_impressions = d.UnitImpressions
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

        private static PlanDaypartDto _MapPlanDaypartDto(plan_version_dayparts entity, plan_versions planVersion)
        {
            var dto = new PlanDaypartDto
            {
                DaypartCodeId = entity.standard_daypart_id,
                DaypartTypeId = EnumHelper.GetEnum<DaypartTypeEnum>(entity.daypart_type),
                StartTimeSeconds = entity.start_time_seconds,
                IsStartTimeModified = entity.is_start_time_modified,
                EndTimeSeconds = entity.end_time_seconds,
                IsEndTimeModified = entity.is_end_time_modified,
                WeightingGoalPercent = entity.weighting_goal_percent,
                WeekdaysWeighting = entity.weekdays_weighting,
                WeekendWeighting = entity.weekend_weighting,
                VpvhForAudiences = planVersion.plan_version_audience_daypart_vpvh
                    .Where(x => x.standard_daypart_id == entity.standard_daypart_id)
                    .Select(x => new PlanDaypartVpvhForAudienceDto
                    {
                        AudienceId = x.audience_id,
                        Vpvh = x.vpvh_value,
                        VpvhType = (VpvhTypeEnum)x.vpvh_type,
                        StartingPoint = x.starting_point
                    })
                    .ToList()
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
                    standard_daypart_id = daypart.DaypartCodeId,
                    daypart_type = (int)daypart.DaypartTypeId,
                    start_time_seconds = daypart.StartTimeSeconds,
                    is_start_time_modified = daypart.IsStartTimeModified,
                    end_time_seconds = daypart.EndTimeSeconds,
                    is_end_time_modified = daypart.IsEndTimeModified,
                    weighting_goal_percent = daypart.WeightingGoalPercent,
                    weekdays_weighting = daypart.WeekdaysWeighting,
                    weekend_weighting = daypart.WeekendWeighting
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

        private static void _MapAudienceDaypartVpvh(plan_versions entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_version_audience_daypart_vpvh.RemoveRange(entity.plan_version_audience_daypart_vpvh);

            foreach (var daypart in planDto.Dayparts)
            {
                foreach (var vpvhForAudience in daypart.VpvhForAudiences)
                {
                    entity.plan_version_audience_daypart_vpvh.Add(new plan_version_audience_daypart_vpvh
                    {
                        audience_id = vpvhForAudience.AudienceId,
                        standard_daypart_id = daypart.DaypartCodeId,
                        vpvh_type = (int)vpvhForAudience.VpvhType,
                        vpvh_value = vpvhForAudience.Vpvh,
                        starting_point = vpvhForAudience.StartingPoint
                    });
                }
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
                        genre_id = programRestriction.Genre?.Id,
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
                    share_of_voice_percent = m.ShareOfVoicePercent,
                    is_user_share_of_voice_percent = m.IsUserShareOfVoicePercent
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
                IsUserShareOfVoicePercent = entity.is_user_share_of_voice_percent
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
                    UnitCapsType = (UnitCapEnum)e.unit_caps_type,
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

        public void SetPricingPlanVersionId(int jobId, int planVersionId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_pricing_job.Single(x => x.id == jobId);

                if (job != null)
                    job.plan_version_id = planVersionId;

                var parameters = context.plan_version_pricing_parameters.Single(x => x.plan_version_pricing_job_id == jobId);
                if (parameters != null)
                    parameters.plan_version_id = planVersionId;

                context.SaveChanges();
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

        /// <inheritdoc/>
        public PlanPricingJob GetPricingJobForLatestPlanVersion(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var latestJob = (from pvpj in context.plan_version_pricing_job
                                 where pvpj.plan_versions.plan_id == planId &&
                                       pvpj.plan_version_id == pvpj.plan_versions.plan.latest_version_id
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

        public PlanPricingJob GetPricingJobForPlanVersion(int planVersionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var latestJob = (from pvpj in context.plan_version_pricing_job
                                 where pvpj.plan_versions.id == planVersionId
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

        /// <inheritdoc/>
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


        /// <inheritdoc/>
        public PlanBuyingJob GetPlanBuyingJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_buying_job
                    .Single(x => x.id == jobId, $"Job id {jobId} not found.");

                return new PlanBuyingJob
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

        /// <inheritdoc/>
        public int? GetPlanIdFromPricingJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_pricing_job
                    .Include(x => x.plan_versions)
                    .Single(x => x.id == jobId, $"Job id {jobId} not found.");

                // job.plan_versions will be null if ran while creating a plan.
                var result = job.plan_versions?.plan_id;
                return result;
            });
        }

        /// <inheritdoc/>
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
                    .Include(x => x.plan_versions)
                    .Include(x => x.plan_version_pricing_parameter_inventory_proprietary_summaries)
                    .Include(x => x.plan_version_pricing_parameter_inventory_proprietary_summaries.Select(y => y.inventory_proprietary_summary))
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
                JobId = entity.plan_version_pricing_job_id,
                PlanVersionId = entity.plan_version_id,
                AdjustedBudget = entity.budget_adjusted,
                AdjustedCPM = entity.cpm_adjusted,
                MarketGroup = (MarketGroupEnum)entity.market_group,
                ProprietaryInventory = entity.plan_version_pricing_parameter_inventory_proprietary_summaries.Where(p => p.inventory_proprietary_summary.is_active)
                    .Select(x => new Entities.InventoryProprietary.InventoryProprietarySummary { Id = x.inventory_proprietary_summary_id, NumberOfUnit = x.unit_number })
                    .ToList()
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
                    market_group = (int)planPricingParametersDto.MarketGroup,
                    plan_version_pricing_parameter_inventory_proprietary_summaries = planPricingParametersDto.ProprietaryInventory
                        .Select(x => new plan_version_pricing_parameter_inventory_proprietary_summaries
                        {
                            inventory_proprietary_summary_id = x.Id,
                            unit_number = x.NumberOfUnit.GetValueOrDefault()
                        })
                        .ToList(),
                    posting_type = (int)planPricingParametersDto.PostingType
                };

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
                    optimal_cpm = result.PricingCpm,
                    plan_version_pricing_job_id = result.JobId,
                    pricing_version = result.PricingVersion,
                    spot_allocation_model_mode = (int)(result.SpotAllocationModelMode),
                    posting_type = (int)result.PostingType
                };

                context.plan_version_pricing_api_results.Add(planPricingApiResult);

                context.SaveChanges();

                var pkSpots = context.plan_version_pricing_api_result_spots.Any() ? context.plan_version_pricing_api_result_spots.Max(x => x.id) + 1 : 0;

                var planPricingApiResultSpots = new List<plan_version_pricing_api_result_spots>();
                var planPricingApiResultSpotFrequencies = new List<plan_version_pricing_api_result_spot_frequencies>();

                foreach (var spot in result.Spots)
                {
                    var currentSpotId = pkSpots++;

                    var planPricingApiResultSpot = new plan_version_pricing_api_result_spots
                    {
                        id = currentSpotId,
                        plan_version_pricing_api_results_id = planPricingApiResult.id,
                        station_inventory_manifest_id = spot.Id,
                        contract_media_week_id = spot.ContractMediaWeek.Id,
                        inventory_media_week_id = spot.InventoryMediaWeek.Id,
                        impressions30sec = spot.Impressions30sec,
                        standard_daypart_id = spot.StandardDaypart.Id,
                    };

                    var frequencies = spot.SpotFrequencies
                            .Select(x => new plan_version_pricing_api_result_spot_frequencies
                            {
                                plan_version_pricing_api_result_spot_id = currentSpotId,
                                spot_length_id = x.SpotLengthId,
                                cost = x.SpotCost,
                                spots = x.Spots,
                                impressions = x.Impressions
                            })
                            .ToList();

                    planPricingApiResultSpots.Add(planPricingApiResultSpot);
                    planPricingApiResultSpotFrequencies.AddRange(frequencies);
                }

                BulkInsert(context, planPricingApiResultSpots);
                BulkInsert(context, planPricingApiResultSpotFrequencies, propertiesToIgnore);
            });
        }

        public PlanPricingAllocationResult GetPricingApiResultsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var postingType = context.plan_version_pricing_job
                        .Include(x => x.plan_versions)
                        .First(x => x.id == jobId)
                        .plan_versions?.posting_type ?? (int)PostingTypeEnum.NSI; //If there is no saved plan yet default to NSI;

                var entities = context.plan_version_pricing_api_results
                    .Include(x => x.plan_version_pricing_api_result_spots)
                    .Include(x => x.plan_version_pricing_api_result_spots.Select(y => y.plan_version_pricing_api_result_spot_frequencies))
                    .Where(x => x.plan_version_pricing_job_id == jobId
                        && x.spot_allocation_model_mode == (int)spotAllocationModelMode
                        && x.posting_type == postingType)
                    .OrderByDescending(p => p.id);

                return _GetPostingTypePlanPricingAllocationResult(entities, (PostingTypeEnum)postingType);
            });
        }

        public PlanPricingAllocationResultDto GetPricingApiResultsByJobId_v2(int jobId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entities = context.plan_version_pricing_api_results
                    .Include(x => x.plan_version_pricing_api_result_spots)
                    .Include(x => x.plan_version_pricing_api_result_spots.Select(y => y.plan_version_pricing_api_result_spot_frequencies))
                    .Where(x => x.plan_version_pricing_job_id == jobId
                        && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id);

                if (entities == null || !entities.Any())
                    return null;

                return new PlanPricingAllocationResultDto
                {
                    NsiResults = _GetPostingTypePlanPricingAllocationResult(entities, PostingTypeEnum.NSI),
                    NtiResults = _GetPostingTypePlanPricingAllocationResult(entities, PostingTypeEnum.NTI)
                };
            });
        }

        private PlanPricingAllocationResult _GetPostingTypePlanPricingAllocationResult(IQueryable<plan_version_pricing_api_results> entities, PostingTypeEnum postingType)
        {
            var apiResult = entities.FirstOrDefault(x => x.posting_type == (int)postingType);

            if (apiResult == null)
                return null;

            return new PlanPricingAllocationResult
            {
                SpotAllocationModelMode = (SpotAllocationModelMode)apiResult.spot_allocation_model_mode,
                PostingType = (PostingTypeEnum)apiResult.spot_allocation_model_mode,
                PricingCpm = apiResult.optimal_cpm,
                JobId = apiResult.plan_version_pricing_job_id,
                Spots = apiResult.plan_version_pricing_api_result_spots.Select(x => new PlanPricingAllocatedSpot
                {
                    Id = x.id,
                    StationInventoryManifestId = x.station_inventory_manifest_id,
                    // impressions are for :30 sec only for pricing v3
                    Impressions30sec = x.impressions30sec,
                    SpotFrequencies = x.plan_version_pricing_api_result_spot_frequencies.Select(y => new SpotFrequency
                    {
                        SpotLengthId = y.spot_length_id,
                        SpotCost = y.cost,
                        Spots = y.spots,
                        Impressions = y.impressions
                    }).ToList(),
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
                    StandardDaypart = _MapToStandardDaypartDto(x.standard_dayparts)
                }).ToList()
            };
        }

        public PlanPricingBandDto GetPlanPricingBandByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var postingType = context.plan_version_pricing_job
                                        .Include(x => x.plan_versions)
                                        .First(x => x.id == jobId)
                                        .plan_versions?.posting_type ?? (int)PostingTypeEnum.NSI; //If there is no saved plan yet dafult to NSI;

                var result = context.plan_version_pricing_bands
                    .Include(x => x.plan_version_pricing_band_details)
                    .Where(x => x.plan_version_pricing_job_id == jobId
                                && x.posting_type == postingType
                                && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanPricingBandDto
                {
                    PostingType = (PostingTypeEnum)result.posting_type,
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
                    Totals = new PlanPricingBandTotalsDto
                    {
                        Cpm = result.total_cpm,
                        Budget = result.total_budget,
                        Impressions = result.total_impressions,
                        Spots = result.total_spots
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
                    }).ToList()
                };
            });
        }

        public PlanPricingBandDto_v2 GetPlanPricingBandByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entities = context.plan_version_pricing_bands
                    .Include(x => x.plan_version_pricing_band_details)
                    .Where(x => x.plan_version_pricing_job_id == jobId
                        && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id);

                if (!entities.Any())
                    return null;

                return new PlanPricingBandDto_v2
                {
                    NsiResults = _GetPostingTypePlanPricingResultsBands(entities, PostingTypeEnum.NSI),
                    NtiResults = _GetPostingTypePlanPricingResultsBands(entities, PostingTypeEnum.NTI),
                    SpotAllocationModelMode = spotAllocationModelMode
                };
            });
        }

        private PostingTypePlanPricingResultBands _GetPostingTypePlanPricingResultsBands(IOrderedQueryable<plan_version_pricing_bands> entities, PostingTypeEnum postingType)
        {
            var entity = entities.FirstOrDefault(x => x.posting_type == (int)postingType);

            if (entity == null)
                return null;

            var result = new PostingTypePlanPricingResultBands
            {
                Totals = new PlanPricingBandTotalsDto
                {
                    Cpm = entity.total_cpm,
                    Budget = entity.total_budget,
                    Impressions = entity.total_impressions,
                    Spots = entity.total_spots
                },
                BandsDetails = entity.plan_version_pricing_band_details.Select(r => new PlanPricingBandDetailDto
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
                }).ToList()
            };

            return result;
        }

        public void SavePlanPricingBands(PlanPricingBand planPricingBand)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_pricing_bands.Add(new plan_version_pricing_bands
                {
                    plan_version_pricing_job_id = planPricingBand.JobId,
                    total_budget = planPricingBand.Totals.Budget,
                    total_cpm = planPricingBand.Totals.Cpm,
                    total_impressions = planPricingBand.Totals.Impressions,
                    total_spots = planPricingBand.Totals.Spots,
                    posting_type = (int)planPricingBand.PostingType,
                    spot_allocation_model_mode = (int)planPricingBand.SpotAllocationModelMode,
                    plan_version_pricing_band_details = planPricingBand.Bands.Select(x =>
                        new plan_version_pricing_band_details
                        {
                            cpm = x.Cpm,
                            max_band = x.MaxBand,
                            min_band = x.MinBand,
                            budget = x.Budget,
                            spots = x.Spots,
                            impressions = x.Impressions,
                            impressions_percentage = x.ImpressionsPercentage,
                            available_inventory_percentage = x.AvailableInventoryPercent,
                            is_proprietary = x.IsProprietary
                        }).ToList()
                });

                context.SaveChanges();
            });
        }

        public void SavePlanPricingStations(PlanPricingStationResult planPricingStationResult)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_pricing_stations.Add(new plan_version_pricing_stations
                {
                    plan_version_pricing_job_id = planPricingStationResult.JobId,
                    total_budget = planPricingStationResult.Totals.Budget,
                    total_cpm = planPricingStationResult.Totals.Cpm,
                    total_impressions = planPricingStationResult.Totals.Impressions,
                    total_spots = planPricingStationResult.Totals.Spots,
                    total_stations = planPricingStationResult.Totals.Station,
                    posting_type = (int)planPricingStationResult.PostingType,
                    spot_allocation_model_mode = (int)planPricingStationResult.SpotAllocationModelMode,
                    plan_version_pricing_station_details = planPricingStationResult.Stations
                            .Select(stationDto => new plan_version_pricing_station_details
                            {
                                cpm = stationDto.Cpm,
                                budget = stationDto.Budget,
                                spots = stationDto.Spots,
                                impressions = stationDto.Impressions,
                                impressions_percentage = stationDto.ImpressionsPercentage,
                                market = stationDto.Market,
                                station = stationDto.Station,
                                is_proprietary = stationDto.IsProprietary
                            }).ToList()
                });

                context.SaveChanges();
            });
        }

        public PlanPricingResultMarketsDto GetPlanPricingResultMarketsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                //Temporary
                var postingType = context.plan_version_pricing_job
                                        .Include(x => x.plan_versions)
                                        .First(x => x.id == jobId)
                                        .plan_versions?.posting_type ?? (int)PostingTypeEnum.NSI; //If there is no saved plan yet dafult to NSI;

                var entity = context.plan_version_pricing_markets
                    .Include(x => x.plan_version_pricing_market_details)
                    .Where(x => x.plan_version_pricing_job_id == jobId && x.posting_type == postingType
                    && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (entity == null)
                    return null;

                var dto = new PlanPricingResultMarketsDto
                {
                    PostingType = (PostingTypeEnum)entity.posting_type,
                    SpotAllocationModelMode = spotAllocationModelMode,
                    Totals = new PlanPricingResultMarketsTotalsDto
                    {
                        Markets = entity.total_markets,
                        CoveragePercent = entity.total_coverage_percent,
                        Stations = entity.total_stations,
                        Spots = entity.total_spots,
                        Impressions = entity.total_impressions,
                        Cpm = Convert.ToDecimal(entity.total_cpm),
                        Budget = Convert.ToDecimal(entity.total_budget)
                    },
                    MarketDetails = entity.plan_version_pricing_market_details.Select(s => new PlanPricingResultMarketDetailsDto
                    {
                        MarketName = s.market_name,
                        Rank = s.rank,
                        MarketCoveragePercent = s.market_coverage_percent,
                        Stations = s.stations_per_market,
                        Spots = s.spots,
                        Impressions = s.impressions,
                        Budget = Convert.ToDecimal(s.budget),
                        ShareOfVoiceGoalPercentage = s.share_of_voice_goal_percentage,
                        ImpressionsPercentage = s.impressions_percentage
                    }).ToList()
                };
                return dto;
            });
        }

        public PlanPricingResultMarketsDto_v2 GetPlanPricingResultMarketsByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entities = context.plan_version_pricing_markets
                     .Include(x => x.plan_version_pricing_market_details)
                     .Where(x => x.plan_version_pricing_job_id == jobId &&
                                 x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                     .OrderByDescending(p => p.id);

                if (!entities.Any())
                    return null;

                var dto = new PlanPricingResultMarketsDto_v2
                {
                    NsiResults = _GetPostingTypePlanPricingResultMarkets(entities, PostingTypeEnum.NSI),
                    NtiResults = _GetPostingTypePlanPricingResultMarkets(entities, PostingTypeEnum.NTI),
                    SpotAllocationModelMode = spotAllocationModelMode
                };
                return dto;
            });
        }

        public double GetNsiToNtiConversionRate(List<PlanDaypartDto> planDayParts)
        {
            var standardDayPartIds = planDayParts.Select(x => x.DaypartCodeId).ToList();
            return _InReadUncommitedTransaction(context =>
            {
                var latestMediaMonth = context.nti_to_nsi_conversion_rates
                         .Select(x => x.media_month_id)
                         .ToList()
                         .Max();

                var conversionRate = context.nti_to_nsi_conversion_rates
                    .Where(x => standardDayPartIds.Contains(x.standard_daypart_id) && x.media_month_id == latestMediaMonth)
                    .Average(x => x.conversion_rate);

                return conversionRate;
            });
        }

        private PostingTypePlanPricingResultMarkets _GetPostingTypePlanPricingResultMarkets(IQueryable<plan_version_pricing_markets> entities, PostingTypeEnum postingType)
        {
            var entity = entities.FirstOrDefault(x => x.posting_type == (int)postingType);

            if (entity == null) return null;

            var result = new PostingTypePlanPricingResultMarkets
            {
                Totals = new PlanPricingResultMarketsTotalsDto
                {
                    Markets = entity.total_markets,
                    CoveragePercent = entity.total_coverage_percent,
                    Stations = entity.total_stations,
                    Spots = entity.total_spots,
                    Impressions = entity.total_impressions,
                    Cpm = Convert.ToDecimal(entity.total_cpm),
                    Budget = Convert.ToDecimal(entity.total_budget)
                },
                MarketDetails = entity.plan_version_pricing_market_details.Select(s => new PlanPricingResultMarketDetailsDto
                {
                    MarketName = s.market_name,
                    Rank = s.rank,
                    MarketCoveragePercent = s.market_coverage_percent,
                    Stations = s.stations_per_market,
                    Spots = s.spots,
                    Impressions = s.impressions,
                    Budget = Convert.ToDecimal(s.budget),
                    ShareOfVoiceGoalPercentage = s.share_of_voice_goal_percentage,
                    ImpressionsPercentage = s.impressions_percentage
                }).ToList()
            };

            return result;
        }

        public void SavePlanPricingMarketResults(PlanPricingResultMarkets planPricingResultMarkets)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_pricing_markets.Add(new plan_version_pricing_markets
                {
                    plan_version_pricing_job_id = planPricingResultMarkets.PricingJobId,
                    total_markets = planPricingResultMarkets.Totals.Markets,
                    total_coverage_percent = planPricingResultMarkets.Totals.CoveragePercent,
                    total_stations = planPricingResultMarkets.Totals.Stations,
                    total_spots = planPricingResultMarkets.Totals.Spots,
                    total_impressions = planPricingResultMarkets.Totals.Impressions,
                    total_cpm = Convert.ToDouble(planPricingResultMarkets.Totals.Cpm),
                    total_budget = Convert.ToDouble(planPricingResultMarkets.Totals.Budget),
                    posting_type = (int)planPricingResultMarkets.PostingType,
                    spot_allocation_model_mode = (int)planPricingResultMarkets.SpotAllocationModelMode,
                    plan_version_pricing_market_details = planPricingResultMarkets.MarketDetails.Select(d => new plan_version_pricing_market_details
                    {
                        market_name = d.MarketName,
                        market_coverage_percent = d.MarketCoveragePercent,
                        stations = d.Stations,
                        stations_per_market = d.StationsPerMarket,
                        spots = d.Spots,
                        impressions = d.Impressions,
                        budget = Convert.ToDouble(d.Budget),
                        impressions_percentage = d.ImpressionsPercentage,
                        share_of_voice_goal_percentage = d.ShareOfVoiceGoalPercentage,
                        rank = d.Rank,
                        is_proprietary = d.IsProprietary
                    }).ToList()
                });

                context.SaveChanges();
            });
        }

        private StandardDaypartDto _MapToStandardDaypartDto(standard_dayparts standardDaypart)
        {
            if (standardDaypart == null)
                return null;

            return new StandardDaypartDto
            {
                Id = standardDaypart.id,
                Code = standardDaypart.code,
                FullName = standardDaypart.name
            };
        }

        public void SavePricingAggregateResults(PlanPricingResultBaseDto pricingResult)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };

                var planPricingResult = new plan_version_pricing_results
                {
                    optimal_cpm = pricingResult.OptimalCpm,
                    total_market_count = pricingResult.Totals.MarketCount,
                    total_station_count = pricingResult.Totals.StationCount,
                    total_avg_cpm = pricingResult.Totals.AvgCpm,
                    total_avg_impressions = pricingResult.Totals.AvgImpressions,
                    total_budget = pricingResult.Totals.Budget,
                    total_impressions = pricingResult.Totals.Impressions,
                    plan_version_pricing_job_id = pricingResult.JobId,
                    goal_fulfilled_by_proprietary = pricingResult.GoalFulfilledByProprietary,
                    total_spots = pricingResult.Totals.Spots,
                    posting_type = (int)pricingResult.PostingType,
                    spot_allocation_model_mode = (int)pricingResult.SpotAllocationModelMode                   
                };
                foreach (var planVersionPricingDaypart in pricingResult.PlanVersionPricingDaypartsList)
                {
                    var planVersionPricingDayparts = new plan_version_pricing_results_dayparts
                    {
                        standard_daypart_id = planVersionPricingDaypart.StandardDaypartId,
                        calculated_vpvh = planVersionPricingDaypart.CalculatedVpvh
                    };

                    planPricingResult.plan_version_pricing_results_dayparts.Add(planVersionPricingDayparts);
                }
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
                        impressions = program.Impressions,
                        is_proprietary = program.IsProprietary,
                    };

                    spots.Add(planPricingResultSpots);
                }

                if (spots.Any())
                {
                    BulkInsert(context, spots, propertiesToIgnore);
                }
            });
        }

        public PricingProgramsResultDto GetPricingProgramsResultByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                //Temporary
                var postingType = context.plan_version_pricing_job
                                        .Include(x => x.plan_versions)
                                        .First(x => x.id == jobId)
                                        .plan_versions?.posting_type ?? (int)PostingTypeEnum.NSI; //If there is no saved plan yet dafult to NSI

                var result = context.plan_version_pricing_results
                    .Include(x => x.plan_version_pricing_result_spots)
                    .Where(x => x.plan_version_pricing_job_id == jobId && x.posting_type == postingType
                    && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PricingProgramsResultDto
                {
                    PostingType = (PostingTypeEnum)result.posting_type,
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
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

        public PricingProgramsResultDto_v2 GetPricingProgramsResultByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entities = context.plan_version_pricing_results
                    .Include(x => x.plan_version_pricing_result_spots)
                    .Where(x => x.plan_version_pricing_job_id == jobId
                        && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id);

                if (!entities.Any()) return null;

                var dto = new PricingProgramsResultDto_v2
                {
                    NsiResults = _GetPostingTypePlanPricingResultPrograms(entities, PostingTypeEnum.NSI),
                    NtiResults = _GetPostingTypePlanPricingResultPrograms(entities, PostingTypeEnum.NTI),
                    SpotAllocationModelMode = spotAllocationModelMode
                };

                return dto;
            });
        }

        private PostingTypePlanPricingResultPrograms _GetPostingTypePlanPricingResultPrograms(IOrderedQueryable<plan_version_pricing_results> entities,
            PostingTypeEnum postingType)
        {
            var entity = entities.FirstOrDefault(x => x.posting_type == (int)postingType);

            if (entity == null) return null;

            var programResult = new PostingTypePlanPricingResultPrograms
            {
                Totals = new PricingProgramsResultTotalsDto
                {
                    MarketCount = entity.total_market_count,
                    StationCount = entity.total_station_count,
                    AvgCpm = entity.total_avg_cpm,
                    AvgImpressions = entity.total_avg_impressions,
                    Budget = entity.total_budget,
                    Spots = entity.total_spots,
                    Impressions = entity.total_impressions
                },
                ProgramDetails = entity.plan_version_pricing_result_spots.Select(r => new PlanPricingProgramProgramDto
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

            return programResult;
        }

        public CurrentPricingExecutionResultDto GetPricingResultsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                //Temporary
                var postingType = context.plan_version_pricing_job
                                        .Include(x => x.plan_versions)
                                        .First(x => x.id == jobId)
                                        .plan_versions?.posting_type ?? (int)PostingTypeEnum.NSI; //If there is no saved plan yet dafult to NSI;

                var result = context.plan_version_pricing_results
                    .Include(x => x.plan_version_pricing_result_spots)
                    .Where(x => x.plan_version_pricing_job_id == jobId && x.posting_type == postingType
                        && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                var dtoResult = _MapToCurrentPricingExecutionResultDto(result);

                return dtoResult;
            });
        }

        public List<CurrentPricingExecutionResultDto> GetAllPricingResultsByJobIds(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_pricing_results
               .Include(x => x.plan_version_pricing_result_spots)
               .Where(x => x.plan_version_pricing_job_id == jobId)
               .OrderByDescending(p => p.id)
               .ToList();

                if (result == null)
                    return null;

                var results = result.Select(_MapToCurrentPricingExecutionResultDto).ToList();
                return results;
            });
        }

        private CurrentPricingExecutionResultDto _MapToCurrentPricingExecutionResultDto(plan_version_pricing_results entity)
        {
            var dto = new CurrentPricingExecutionResultDto
            {
                Id = entity.id,
                PostingType = (PostingTypeEnum)entity.posting_type,
                OptimalCpm = entity.optimal_cpm,
                JobId = entity.plan_version_pricing_job_id,
                PlanVersionId = entity.plan_version_pricing_job.plan_version_id,
                GoalFulfilledByProprietary = entity.goal_fulfilled_by_proprietary,
                HasResults = entity.plan_version_pricing_result_spots.Any(),
                SpotAllocationModelMode = (SpotAllocationModelMode)entity.spot_allocation_model_mode,
                TotalBudget = entity.total_budget,
                TotalImpressions = entity.total_impressions
            };
            return dto;
        }

        public PlanPricingStationResultDto GetPricingStationsResultByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                //Temporary
                var postingType = context.plan_version_pricing_job
                                        .Include(x => x.plan_versions)
                                        .First(x => x.id == jobId)
                                        .plan_versions?.posting_type ?? (int)PostingTypeEnum.NSI; //If there is no saved plan yet dafult to NSI;

                var result = context.plan_version_pricing_stations
                    .Include(p => p.plan_version_pricing_station_details)
                    .Where(x => x.plan_version_pricing_job_id == jobId
                        && x.posting_type == postingType
                        && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanPricingStationResultDto
                {
                    Id = result.id,
                    JobId = result.plan_version_pricing_job_id,
                    PlanVersionId = result.plan_version_pricing_job.plan_version_id,
                    PostingType = (PostingTypeEnum)result.posting_type,
                    SpotAllocationModelMode = spotAllocationModelMode,
                    Totals = new PlanPricingStationTotalsDto
                    {
                        Budget = result.total_budget,
                        Cpm = result.total_cpm,
                        Impressions = result.total_impressions,
                        ImpressionsPercentage = 100,
                        Spots = result.total_spots,
                        Station = result.total_stations
                    },
                    Stations = result.plan_version_pricing_station_details.Select(d => new PlanPricingStationDto
                    {
                        Budget = d.budget,
                        Cpm = d.cpm,
                        Impressions = d.impressions,
                        Id = d.id,
                        ImpressionsPercentage = d.impressions_percentage,
                        Market = d.market,
                        Spots = d.spots,
                        Station = d.station
                    }).ToList()
                };
            });
        }

        public PlanPricingStationResultDto_v2 GetPricingStationsResultByJobId_v2(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entities = context.plan_version_pricing_stations
                    .Include(p => p.plan_version_pricing_station_details)
                    .Where(x => x.plan_version_pricing_job_id == jobId
                    && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id);

                if (!entities.Any()) return null;

                var result = new PlanPricingStationResultDto_v2
                {
                    Id = entities.First().id,
                    JobId = jobId,
                    PlanVersionId = entities.First().plan_version_pricing_job.plan_version_id,
                    NsiResults = _GetPostingTypePlanPricingResultStations(entities, PostingTypeEnum.NSI),
                    NtiResults = _GetPostingTypePlanPricingResultStations(entities, PostingTypeEnum.NTI),
                    SpotAllocationModelMode = spotAllocationModelMode
                };

                return result;
            });
        }

        private PostingTypePlanPricingResultStations _GetPostingTypePlanPricingResultStations(IOrderedQueryable<plan_version_pricing_stations> entities, PostingTypeEnum postingType)
        {
            var entity = entities.FirstOrDefault(x => x.posting_type == (int)postingType);

            if (entity == null) return null;

            var result = new PostingTypePlanPricingResultStations
            {
                Totals = new PlanPricingStationTotalsDto
                {
                    Budget = entity.total_budget,
                    Cpm = entity.total_cpm,
                    Impressions = entity.total_impressions,
                    ImpressionsPercentage = 100,
                    Spots = entity.total_spots,
                    Station = entity.total_stations
                },
                StationDetails = entity.plan_version_pricing_station_details.Select(d => new PlanPricingStationDto
                {
                    Budget = d.budget,
                    Cpm = d.cpm,
                    Impressions = d.impressions,
                    Id = d.id,
                    ImpressionsPercentage = d.impressions_percentage,
                    Market = d.market,
                    Spots = d.spots,
                    Station = d.station
                }).ToList()
            };

            return result;
        }

        public decimal GetGoalCpm(int planVersionId, int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_pricing_parameters.Where(p =>
                        p.plan_version_id == planVersionId && p.plan_version_pricing_job_id == jobId)
                    .Select(p => p.cpm_goal).FirstOrDefault();

                return result;
            });
        }

        public decimal GetGoalCpm(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_pricing_parameters.Where(p => p.plan_version_pricing_job_id == jobId)
                    .Select(p => p.cpm_goal).FirstOrDefault();

                return result;
            });
        }
        public List<PlanPricingResultsDaypartDto> GetPricingResultsDayparts(int PlanVersionPricingResultId)
        {
           return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_pricing_results_dayparts.Where(p => p.plan_version_pricing_result_id == PlanVersionPricingResultId)
                .Select(d => new PlanPricingResultsDaypartDto
                {
                    Id = d.id,
                    PlanVersionPricingResultId = d.plan_version_pricing_result_id,
                    CalculatedVpvh = d.calculated_vpvh,
                    StandardDaypartId = d.standard_daypart_id
                }).ToList();               
                return result;
            });
        }

        public List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanId(int planId, PostingTypeEnum postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;

                // get the latest Job
                var jobEntity = context.plan_version_pricing_job
                    .Where(j => j.plan_version_id == planVersionId)
                    .OrderByDescending(j => j.id)
                    .FirstOrDefault();

                if (jobEntity == null)
                {
                    throw new InvalidOperationException($"No pricing runs were found for the plan {planId}.");
                }

                plan_version_pricing_api_results apiResult = null;

                apiResult = context.plan_version_pricing_api_results
                        .Include(x => x.plan_version_pricing_api_result_spots)
                        .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.inventory_media_week))
                        .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.plan_version_pricing_api_result_spot_frequencies))
                        .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.standard_dayparts))
                        .SingleOrDefault(r => r.plan_version_pricing_job_id == jobEntity.id
                            && r.posting_type == (int)postingType
                            && r.spot_allocation_model_mode == (int)spotAllocationModelMode);

                if (apiResult == null)
                    throw new Exception($"No pricing runs were found for the plan {planId} for posting type {postingType}");

                var results = apiResult.plan_version_pricing_api_result_spots.Select(_MapToPlanPricingAllocatedSpot).ToList();

                return results;
            });
        }

        public List<PlanPricingAllocatedSpot> GetPlanPricingAllocatedSpotsByPlanVersionId(int planVersionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var apiResult = (from job in context.plan_version_pricing_job
                                 from apiResults in job.plan_version_pricing_api_results
                                 where job.plan_version_id == planVersionId
                                 select apiResults)
                    .Include(x => x.plan_version_pricing_api_result_spots)
                    .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.inventory_media_week))
                    .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.plan_version_pricing_api_result_spot_frequencies))
                    .Include(x => x.plan_version_pricing_api_result_spots.Select(s => s.standard_dayparts))
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
                Impressions30sec = spot.impressions30sec,
                SpotFrequencies = spot.plan_version_pricing_api_result_spot_frequencies.Select(x => new SpotFrequency
                {
                    SpotLengthId = x.spot_length_id,
                    SpotCost = x.cost,
                    Spots = x.spots,
                    Impressions = x.impressions
                }).ToList(),
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
                StandardDaypart = _MapToStandardDaypartDto(spot.standard_dayparts)
            };
        }

        /// <inheritdoc/>
        public void UpdatePlanPricingVersionId(int versionId, int oldPlanVersionId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = (from j in context.plan_version_pricing_job
                           where j.plan_version_id == oldPlanVersionId
                           orderby j.id descending
                           select j).FirstOrDefault();
                if (job != null)
                {
                    job.plan_version_id = versionId;
                    context.SaveChanges();
                }

                var parameter = (from p in context.plan_version_pricing_parameters
                                 where p.plan_version_id == oldPlanVersionId
                                 orderby p.id descending
                                 select p).FirstOrDefault();
                if (parameter != null)
                {
                    parameter.plan_version_id = versionId;
                    context.SaveChanges();
                }
            });
        }

        public void SetBuyingPlanVersionId(int jobId, int planVersionId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_buying_job.OrderByDescending(x => x.id).FirstOrDefault(x => x.id == jobId);
                if (job != null)
                {
                    job.plan_version_id = planVersionId;
                }

                var parameters = context.plan_version_buying_parameters.FirstOrDefault(x => x.plan_version_buying_job_id == jobId);
                if (parameters != null)
                {
                    parameters.plan_version_id = planVersionId;
                }
                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void UpdatePlanBuyingVersionId(int versionId, int oldPlanVersionId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = (from j in context.plan_version_buying_job
                           where j.plan_version_id == oldPlanVersionId
                           orderby j.id descending
                           select j).FirstOrDefault();

                if (job != null)
                {
                    job.plan_version_id = versionId;
                }

                var parameter = (from p in context.plan_version_buying_parameters
                                 where p.plan_version_id == oldPlanVersionId
                                 orderby p.id descending
                                 select p).FirstOrDefault();

                if (parameter != null)
                {
                    parameter.plan_version_id = versionId;
                }
                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public PlanPricingParametersDto GetPricingParametersForVersion(int versionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = (from parameters in context.plan_version_pricing_parameters
                              where parameters.plan_version_id == versionId
                              orderby parameters.id descending
                              select parameters);

                return _MapPricingParameters(entity.FirstOrDefault());
            });
        }

        /// <inheritdoc/>
        public PlanPricingParametersDto GetLatestPricingParameters(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = (from parameters in context.plan_version_pricing_parameters
                              where parameters.plan_versions.plan_id == planId &&
                                    parameters.plan_version_id == parameters.plan_versions.plan.latest_version_id
                              orderby parameters.id descending
                              select parameters);

                return _MapPricingParameters(entity.FirstOrDefault());
            });
        }

        public List<PlanPricingJob> GetSuccessfulPricingJobs(int planVersionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var jobs = context.plan_version_pricing_job
                    .Where(j => j.plan_version_id == planVersionId &&
                                j.status == (int)BackgroundJobProcessingStatus.Succeeded)
                    .ToList();

                var results = jobs.Select(j =>
                    new PlanPricingJob
                    {
                        Id = j.id,
                        HangfireJobId = j.hangfire_job_id,
                        PlanVersionId = j.plan_version_id,
                        Status = (BackgroundJobProcessingStatus)j.status,
                        Queued = j.queued_at,
                        Completed = j.completed_at,
                    }).ToList();
                return results;
            });
        }

        /// <inheritdoc/>
        public List<ProgramLineupProprietaryInventory> GetProprietaryInventoryForProgramLineup(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planVersion = (from pricingJob in context.plan_version_pricing_job
                                   join pv in context.plan_versions on pricingJob.plan_version_id equals pv.id
                                   where pricingJob.id == jobId
                                   select pv).Single($"Could not find plan for job id = {jobId}");

                //get all summary ids selected on the plan
                var summaryIds = (from pv_ips in context.plan_version_pricing_parameter_inventory_proprietary_summaries
                                  join pv_pp in context.plan_version_pricing_parameters
                                    on pv_ips.plan_version_pricing_parameter_id equals pv_pp.id
                                  where pv_pp.plan_version_pricing_job_id == jobId
                                  select pv_ips.inventory_proprietary_summary_id).ToList();

                //get all records based on the summary ids and plan target audience
                var result = (from ips in context.inventory_proprietary_summary
                              join ssa in context.inventory_proprietary_summary_station_audiences
                                 on ips.id equals ssa.inventory_proprietary_summary_id
                              join dpm in context.inventory_proprietary_daypart_program_mappings
                                 on ips.inventory_proprietary_daypart_program_mappings_id equals dpm.id
                              join dp in context.inventory_proprietary_daypart_programs
                                 on dpm.inventory_proprietary_daypart_programs_id equals dp.id
                              join aa in context.audience_audiences on ssa.audience_id equals aa.rating_audience_id
                              join dd in context.standard_dayparts on dpm.standard_daypart_id equals dd.id
                              join g in context.genres on dp.genre_id equals g.id
                              join s in context.stations on ssa.station_id equals s.id
                              where summaryIds.Contains(ips.id)
                                    && aa.rating_category_group_id == BroadcastConstants.RatingsGroupId
                                    && aa.custom_audience_id == planVersion.target_audience_id
                              select new
                              {
                                  ssa.station_id,
                                  s.market_code,
                                  dpm.standard_daypart_id,
                                  dpm.inventory_proprietary_daypart_programs_id,
                                  ssa.impressions,
                                  dp.program_name,
                                  dd.daypart_id,
                                  genre_name = g.name,
                                  s.affiliation,
                                  s.legacy_call_letters
                              }).ToList();

                if (result == null)
                    throw new Exception($"No proprietary inventory summary were found for the plan {planVersion.plan_id}");

                return result.GroupBy(x => new { x.station_id, x.standard_daypart_id, x.inventory_proprietary_daypart_programs_id })
                .Select(x =>
                {
                    var first = x.First();
                    return new ProgramLineupProprietaryInventory
                    {
                        Station = new MarketCoverageByStation.Station
                        {
                            Id = x.Key.station_id,
                            Affiliation = first.affiliation,
                            LegacyCallLetters = first.legacy_call_letters
                        },
                        InventoryProprietaryDaypartProgramId = x.Key.inventory_proprietary_daypart_programs_id,
                        ImpressionsPerWeek = x.Sum(y => y.impressions),
                        ProgramName = first.program_name,
                        Genre = first.genre_name,
                        DaypartId = first.daypart_id,
                        MarketCode = first.market_code.Value,
                    };
                })
                .ToList();
            });
        }

        public List<PlanPricingResultsDaypartDto> GetPlanPricingResultsDaypartsByPlanPricingResultId(int planPricingResultId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_pricing_results_dayparts
                    .Where(x => x.plan_version_pricing_result_id == planPricingResultId)
                    .Select(x => new PlanPricingResultsDaypartDto
                    {
                        Id = x.id,
                        PlanVersionPricingResultId = x.plan_version_pricing_result_id,
                        StandardDaypartId = x.standard_daypart_id,
                        CalculatedVpvh = x.calculated_vpvh
                    }).ToList();

                return result;
            });
        }
    }
}