using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Buying;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IPlanBuyingRepository : IDataRepository
    {
        /// <summary>
        /// Gets the plan buying runs.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>List of PlanBuyingApiRequestParametersDto objects</returns>
        List<PlanBuyingApiRequestParametersDto> GetPlanBuyingRuns(int planId);

        /// <summary>
        /// Adds the plan buying job.
        /// </summary>
        /// <param name="planBuyingJob">The plan buying job.</param>
        /// <returns>New id</returns>
        int AddPlanBuyingJob(PlanBuyingJob planBuyingJob);

        /// <summary>
        /// Updates the plan buying job.
        /// </summary>
        /// <param name="planBuyingJob">The plan buying job.</param>
        void UpdatePlanBuyingJob(PlanBuyingJob planBuyingJob);

        /// <summary>
        /// Updates the plan buying job with hangfire job identifier.
        /// </summary>
        /// <param name="jobId">The buying job identifier.</param>
        /// <param name="hangfireJobId">The hangfire job identifier.</param>
        void UpdateJobHangfireId(int jobId, string hangfireJobId);

        /// <summary>
        /// Updates the plan buying job with the inventory raw file url.
        /// </summary>
        /// <param name="jobId">The buying job identifier.</param>
        /// <param name="fileName">The name of the file.</param>
        void UpdateFileName(int jobId, string fileName);

        /// <summary>
        /// Gets the latest buying job.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>PlanBuyingJob object</returns>
        PlanBuyingJob GetLatestBuyingJob(int planId);

        /// <summary>
        /// Gets the Optimal CPM.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">Spot Allocation Model Mode.</param>
        /// <param name="postingType">Posting Type.</param>
        /// <returns>Optimal CPM</returns>
        decimal GetOptimalCPM(int jobId, SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType);

        /// <summary>
        /// Gets the plan buying job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>PlanBuyingJob object</returns>
        PlanBuyingJob GetPlanBuyingJob(int jobId);

        /// <summary>
        /// Gets the plan buying job by plan version identifier.
        /// </summary>
        /// <param name="planVersionId">The plan version identifier.</param>
        /// <returns>PlanBuyingJob object</returns>
        PlanBuyingJob GetPlanBuyingJobByPlanVersionId(int planVersionId);

        /// <summary>
        /// Gets the latest parameters for plan buying job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns></returns>
        PlanBuyingParametersDto GetLatestParametersForPlanBuyingJob(int jobId);

        /// <summary>
        /// Saves the plan buying parameters.
        /// </summary>
        /// <param name="planBuyingRequestDto">The plan buying request dto.</param>
        void SavePlanBuyingParameters(PlanBuyingParametersDto planBuyingRequestDto);

        /// <summary>
        /// Saves the buying API results.
        /// </summary>
        /// <param name="result">The result.</param>
        void SaveBuyingApiResults(PlanBuyingAllocationResult result);

        /// <summary>
        /// Gets the buying API results by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="postingType"></param>
        /// <returns></returns>
        PlanBuyingAllocationResult GetBuyingApiResultsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency, 
            PostingTypeEnum postingType = PostingTypeEnum.NSI);

        /// <summary>
        /// Gets the plan buying band by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingBandsDto GetPlanBuyingBandByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Gets the plan buying band inventory station.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>The plan buying band station details</returns>
        PlanBuyingBandInventoryStationsDto GetPlanBuyingBandInventoryStations(int jobId);

        /// <summary>
        /// Saves the plan buying bands.
        /// </summary>
        /// <param name="planBuyingBandDto">The plan buying band dto.</param>
        /// <param name="postingType">The Posting Type.</param>
        void SavePlanBuyingBands(PlanBuyingBandsDto planBuyingBandDto, PostingTypeEnum postingType);

        /// <summary>
        /// Saves the plan buying band inventory stations.
        /// </summary>
        /// <param name="planBuyingBandInventoryStations">The plan buying band stations dto.</param>
        void SavePlanBuyingBandInventoryStations(PlanBuyingBandInventoryStationsDto planBuyingBandInventoryStations);

        /// <summary>
        /// Saves the plan buying stations.
        /// </summary>
        /// <param name="planBuyingStationResultDto">The plan buying station result dto.</param>
        /// <param name="postingType">The plan buying posting type.</param>
        void SavePlanBuyingStations(PlanBuyingStationResultDto planBuyingStationResultDto, PostingTypeEnum postingType);


        /// <summary>
        /// Gets the plan buying result markets by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultMarketsDto GetPlanBuyingResultMarketsByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Saves the plan buying market results.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// /// <param name="postingType">The plan buying posting type.</param>
        void SavePlanBuyingMarketResults(PlanBuyingResultMarketsDto dto, PostingTypeEnum postingType);

        /// <summary>
        /// Saves the buying aggregate results.
        /// </summary>
        /// <param name="planBuyingResult">The plan buying result</param>
        /// <returns>The id of saved plan buying results</returns>
        int SaveBuyingAggregateResults(PlanBuyingResultBaseDto planBuyingResult);

        /// <summary>
        /// Save the plan buying spots
        /// </summary>
        /// <param name="planVersionBuyingResultId">The plan version buying result id</param>
        /// <param name="planBuyingResult">The plan buying result</param>
        void SavePlanBuyingResultSpots(int planVersionBuyingResultId, PlanBuyingResultBaseDto planBuyingResult);

        /// <summary>
        /// Save the plan buying spot stations
        /// </summary>
        /// <param name="planVersionBuyingResultId">The plan version buying result id</param>
        /// <param name="planBuyingResult">The plan buying result</param>
        void SavePlanBuyingResultSpotStations(int planVersionBuyingResultId, PlanBuyingResultBaseDto planBuyingResult);

        /// <summary>
        /// Gets the buying programs result by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultProgramsDto GetBuyingProgramsResultByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode);
        /// <summary>
        /// Gets the buying programs result by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultProgramsDto GetBuyingProgramsResultByJobId_V2(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Gets the buying results by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <returns></returns>
        CurrentBuyingExecutionResultDto GetBuyingResultsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType);

        /// <summary>
        /// Gets the buying results by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <returns></returns>
        List<CurrentBuyingExecutionResultDto> GetBuyingResultsByJobId(int jobId, PostingTypeEnum postingType);

        /// <summary>
        /// Gets the buying stations result by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingStationResultDto GetBuyingStationsResultByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Gets the goal CPM.
        /// </summary>
        /// <param name="planVersionId">The plan version identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType"></param>
        /// <returns>Decimal Goal CPM</returns>
        decimal GetGoalCpm(int planVersionId, int jobId, PostingTypeEnum postingType);

        /// <summary>
        /// Gets the goal CPM.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType"></param>
        /// <returns>Decimal Goal CPM</returns>
        decimal GetGoalCpm(int jobId, PostingTypeEnum postingType);

        /// <summary>
        /// Updates the plan buying version identifier.
        /// </summary>
        /// <param name="versionId">The version identifier.</param>
        /// <param name="oldPlanVersionId">The old plan version identifier.</param>
        void UpdatePlanBuyingVersionId(int versionId, int oldPlanVersionId);


        /// <summary>
        /// Saves the plan buying ownership group results.
        /// </summary>
        /// <param name="aggregateOwnershipGroupResultsTaskResult">The aggregate ownership group results task result.</param>
        /// /// <param name="postingType">The plan buying posting type.</param>
        void SavePlanBuyingOwnershipGroupResults(PlanBuyingResultOwnershipGroupDto aggregateOwnershipGroupResultsTaskResult, PostingTypeEnum postingType);

        /// <summary>
        /// Gets the buying ownership groups by job identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="postingType">The Posting Type.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroupsByJobId(int id, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Saves the plan buying rep firm results.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <param name="postingType">The plan buying posting type.</param>
        void SavePlanBuyingRepFirmResults(PlanBuyingResultRepFirmDto dto, PostingTypeEnum postingType);


        /// <summary>
        /// Gets the buying rep firms by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="postingType">The Posting Type</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultRepFirmDto GetBuyingRepFirmsByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Get the allocated spot for program lineup.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The type of posting</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        List<PlanBuyingAllocatedSpot> GetPlanBuyingAllocatedSpotsByPlanId(int planId, PostingTypeEnum? postingType = null,
            SpotAllocationModelMode? spotAllocationModelMode = SpotAllocationModelMode.Efficiency);

        /// <summary>
        /// Gets the proprietary inventory for program lineup.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>List of ProgramLineupProprietaryInventory objects</returns>
        List<ProgramLineupProprietaryInventory> GetProprietaryInventoryForBuyingProgramLineup(int jobId);


        /// <summary>
        /// Deletes the saved buying data.
        /// </summary>
        /// <returns></returns>
        bool DeleteSavedBuyingData();
    }

    public class PlanBuyingRepository : BroadcastRepositoryBase, IPlanBuyingRepository
    {
        public PlanBuyingRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        /// <inheritdoc/>
        public List<PlanBuyingApiRequestParametersDto> GetPlanBuyingRuns(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var executions = context
                    .plan_version_buying_parameters
                    .Include(x => x.plan_versions)
                    .Include(x => x.plan_versions.plan_version_available_markets)
                    .Where(p => p.plan_versions.plan_id == planId);

                return executions.ToList().Select(e => new PlanBuyingApiRequestParametersDto
                {
                    PlanId = e.plan_versions.plan_id,
                    BudgetGoal = e.budget_adjusted,
                    CompetitionFactor = e.competition_factor,
                    CoverageGoalPercent = e.plan_versions.coverage_goal_percent ?? 0,
                    CpmGoal = e.cpm_adjusted,
                    ImpressionsGoal = e.impressions_goal,
                    InflationFactor = e.inflation_factor,
                    Markets = e.plan_versions.plan_version_available_markets.Select(m => new PlanBuyingMarketDto
                    {
                        MarketId = m.market_code,
                        MarketShareOfVoice = m.share_of_voice_percent
                    }).ToList(),
                    MaxCpm = e.max_cpm,
                    MinCpm = e.min_cpm,
                    ProprietaryBlend = e.proprietary_blend,
                    UnitCaps = e.unit_caps,
                    UnitCapsType = (UnitCapEnum)e.unit_caps_type,
                    JobId = e.plan_version_buying_job_id,
                    Margin = e.margin
                }).ToList();
            });
        }

        /// <inheritdoc/>
        public int AddPlanBuyingJob(PlanBuyingJob planBuyingJob)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planBuyingJobDb = new plan_version_buying_job
                {
                    plan_version_id = planBuyingJob.PlanVersionId,
                    status = (int)planBuyingJob.Status,
                    queued_at = planBuyingJob.Queued,
                    completed_at = planBuyingJob.Completed
                };

                context.plan_version_buying_job.Add(planBuyingJobDb);

                context.SaveChanges();

                return planBuyingJobDb.id;
            });
        }

        /// <inheritdoc/>
        public void UpdatePlanBuyingJob(PlanBuyingJob planBuyingJob)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_buying_job.Single(x => x.id == planBuyingJob.Id);

                job.status = (int)planBuyingJob.Status;
                job.completed_at = planBuyingJob.Completed;
                job.error_message = planBuyingJob.ErrorMessage;
                job.diagnostic_result = planBuyingJob.DiagnosticResult;

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void UpdateJobHangfireId(int jobId, string hangfireJobId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_buying_job.Single(x => x.id == jobId);

                job.hangfire_job_id = hangfireJobId;

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void UpdateFileName(int jobId, string fileName)
        {
            _InReadUncommitedTransaction(context =>
            {
                var job = context.plan_version_buying_job.Single(x => x.id == jobId);

                job.inventory_raw_file = fileName;

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public PlanBuyingJob GetLatestBuyingJob(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var latestJob = (from pvpj in context.plan_version_buying_job
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

                return new PlanBuyingJob
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

        public decimal GetOptimalCPM(int jobId, SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = (from pvbr in context.plan_version_buying_results
                              where pvbr.plan_version_buying_job_id == jobId
                                 && pvbr.spot_allocation_model_mode == (int)spotAllocationModelMode
                                 && pvbr.posting_type == (int)postingType
                              select pvbr).Select(p => p.optimal_cpm).Single();

                return result;
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
                    DiagnosticResult = job.diagnostic_result,
                    InventoryRawFile = job.inventory_raw_file
                };
            });
        }

        /// <inheritdoc/>
        public PlanBuyingJob GetPlanBuyingJobByPlanVersionId(int planVersionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                // get the latest succeeded job.
                var job = context.plan_version_buying_job
                .Where(x => x.plan_version_id == planVersionId && x.status == (int)BackgroundJobProcessingStatus.Succeeded)
                .OrderByDescending(x => x.id)
                .FirstOrDefault();
                
                if (job == null)
                {
                    throw new InvalidOperationException($"Job for plan_version_id '{planVersionId}' not found.");
                }

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
        public PlanBuyingParametersDto GetLatestParametersForPlanBuyingJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planVersionId = context.plan_version_buying_job
                    .Single(x => x.id == jobId, $"Job id {jobId} not found.")
                    .plan_version_id;

                var latestParametersId = context.plan_version_buying_parameters
                    .Where(p => p.plan_version_id == planVersionId)
                    .Select(p => p.id)
                    .Max();

                var latestParameters = context.plan_version_buying_parameters
                    .Include(x => x.plan_versions)
                    .Where(x => x.id == latestParametersId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (latestParameters == null)
                    throw new Exception("Latest buying job parameters not found.");

                var dto = _MapPlanBuyingParameters(latestParameters);

                return dto;
            });
        }

        private PlanBuyingParametersDto _MapPlanBuyingParameters(plan_version_buying_parameters entity)
        {
            var dto = new PlanBuyingParametersDto
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
                JobId = entity.plan_version_buying_job_id,
                PlanVersionId = entity.plan_version_id,
                AdjustedBudget = entity.budget_adjusted,
                AdjustedCPM = entity.cpm_adjusted,
                MarketGroup = (MarketGroupEnum)entity.market_group,
                PostingType = (PostingTypeEnum)entity.posting_type,
                BudgetCpmLever = (BudgetCpmLeverEnum)entity.budget_cpm_lever
            };
            return dto;
        }

        private CurrentBuyingExecutionResultDto _MapToCurrentBuyingExecutionResultDto(plan_version_buying_results entity)
        {
            var dto = new CurrentBuyingExecutionResultDto
            {
                OptimalCpm = entity.optimal_cpm,
                JobId = entity.plan_version_buying_job_id,
                PlanVersionId = entity.plan_version_buying_job.plan_version_id,
                GoalFulfilledByProprietary = entity.goal_fulfilled_by_proprietary,
                HasResults = entity.plan_version_buying_result_spots.Any(),
                SpotAllocationModelMode = (SpotAllocationModelMode)entity.spot_allocation_model_mode,
                PostingType = (PostingTypeEnum)entity.posting_type
            };
            return dto;
        }

        /// <inheritdoc/>
        public void SavePlanBuyingParameters(PlanBuyingParametersDto planBuyingParametersDto)
        {
           _InReadUncommitedTransaction(context =>
            {
                var planBuyingParameters = new plan_version_buying_parameters
                {
                    plan_version_id = planBuyingParametersDto.PlanVersionId,
                    min_cpm = planBuyingParametersDto.MinCpm,
                    max_cpm = planBuyingParametersDto.MaxCpm,
                    impressions_goal = planBuyingParametersDto.DeliveryImpressions,
                    budget_goal = planBuyingParametersDto.Budget,
                    cpm_goal = planBuyingParametersDto.CPM,
                    proprietary_blend = planBuyingParametersDto.ProprietaryBlend,
                    competition_factor = planBuyingParametersDto.CompetitionFactor,
                    inflation_factor = planBuyingParametersDto.InflationFactor,
                    unit_caps_type = (int)planBuyingParametersDto.UnitCapsType,
                    unit_caps = planBuyingParametersDto.UnitCaps,
                    cpp = planBuyingParametersDto.CPP,
                    currency = (int)planBuyingParametersDto.Currency,
                    rating_points = planBuyingParametersDto.DeliveryRatingPoints,
                    margin = planBuyingParametersDto.Margin,
                    plan_version_buying_job_id = planBuyingParametersDto.JobId,
                    budget_adjusted = planBuyingParametersDto.AdjustedBudget,
                    cpm_adjusted = planBuyingParametersDto.AdjustedCPM,
                    market_group = (int)planBuyingParametersDto.MarketGroup,
                    plan_version_buying_parameter_inventory_proprietary_summaries = planBuyingParametersDto.ProprietaryInventory
                        .Select(x => new plan_version_buying_parameter_inventory_proprietary_summaries
                        {
                            inventory_proprietary_summary_id = x.Id
                        })
                        .ToList(),
                    posting_type = (int)planBuyingParametersDto.PostingType,
                    budget_cpm_lever = (int)planBuyingParametersDto.BudgetCpmLever,
                    share_book_id = planBuyingParametersDto.ShareBookId,
                    hut_book_id = planBuyingParametersDto.HUTBookId,
                    fluidity_percentage = planBuyingParametersDto.FluidityPercentage
                };

                context.plan_version_buying_parameters.Add(planBuyingParameters);

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void SaveBuyingApiResults(PlanBuyingAllocationResult result)
        {
            var spots = result.AllocatedSpots;

            var planBuyingApiResultSpots = spots.Select(spot => new plan_version_buying_api_result_spots
            {
                station_inventory_manifest_id = spot.Id,
                contract_media_week_id = spot.ContractMediaWeek.Id,
                inventory_media_week_id = spot.InventoryMediaWeek.Id,
                impressions30sec = spot.Impressions30sec,
                standard_daypart_id = spot.StandardDaypart.Id,
                plan_version_buying_api_result_spot_frequencies = spot.SpotFrequencies.Select(x => new plan_version_buying_api_result_spot_frequencies()
                {
                    spot_length_id = x.SpotLengthId,
                    cost = x.SpotCost,
                    spots = x.Spots,
                    impressions = x.Impressions
                }).ToList()
            }).ToList();

            var planBuyingApiResult = new plan_version_buying_api_results
            {
                optimal_cpm = result.BuyingCpm,
                plan_version_buying_job_id = result.JobId,
                buying_version = result.BuyingVersion,
                spot_allocation_model_mode = (int)result.SpotAllocationModelMode,
                posting_type = (int)result.PostingType,
            };

            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_buying_api_results.Add(planBuyingApiResult);
                context.SaveChanges();
            });
            var newId = planBuyingApiResult.id;

            var chunksToSave = planBuyingApiResultSpots.GetChunks<plan_version_buying_api_result_spots>(10000);
            foreach (var chunk in chunksToSave)
            {
                chunk.ForEach(s => s.plan_version_buying_api_results_id = newId);

                _InReadUncommitedTransaction(context =>
                {
                    context.plan_version_buying_api_result_spots.AddRange(chunk);
                    context.SaveChanges();
                });
            }
        }

        /// <inheritdoc/>
        public PlanBuyingAllocationResult GetBuyingApiResultsByJobId(int jobId,
        SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency,
        PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var apiResult = context.plan_version_buying_api_results
                    .Include(x => x.plan_version_buying_api_result_spots)
                    .Include(x => x.plan_version_buying_api_result_spots.Select(y => y.plan_version_buying_api_result_spot_frequencies))
                    .Include(x => x.plan_version_buying_api_result_spots.Select(y => y.station_inventory_manifest))
                    .Where(x => x.plan_version_buying_job_id == jobId && x.spot_allocation_model_mode == (int)spotAllocationModelMode
                        && x.posting_type == (int)postingType)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    return null;

                var planVersionId = apiResult.plan_version_buying_job.plan_version_id.HasValue
                    ? apiResult.plan_version_buying_job.plan_version_id.Value
                    : 0;

                var resultSpotLists = _MapToResultSpotLists(apiResult.plan_version_buying_api_result_spots);

                var allocationResult = new PlanBuyingAllocationResult
                {
                    BuyingCpm = apiResult.optimal_cpm,
                    JobId = apiResult.plan_version_buying_job_id,
                    BuyingVersion = apiResult.buying_version,
                    PlanVersionId = planVersionId,
                    AllocatedSpots = resultSpotLists.Allocated,
                    UnallocatedSpots = resultSpotLists.Unallocated,
                    SpotAllocationModelMode = (SpotAllocationModelMode)apiResult.spot_allocation_model_mode,
                    PostingType = (PostingTypeEnum)apiResult.posting_type,
                };

                return allocationResult;
            });
        }

        /// <inheritdoc/>
        public PlanBuyingBandsDto GetPlanBuyingBandByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_band_details)
                    .Where(x => x.plan_version_buying_job_id == jobId && x.posting_type == (int)postingType &&
                    x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingBandsDto
                {
                    BuyingJobId = result.plan_version_buying_job_id,
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
                    PostingType = (PostingTypeEnum)result.posting_type,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        AvgCpm = result.total_avg_cpm,
                        Budget = result.total_budget,
                        Impressions = result.total_impressions,
                        SpotCount = result.total_spots
                    },
                    Details = result.plan_version_buying_band_details.Select(r => new PlanBuyingBandDetailDto
                    {
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

        /// <inheritdoc/>
        public PlanBuyingBandInventoryStationsDto GetPlanBuyingBandInventoryStations(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planBuyingResult = context.plan_version_buying_job
                                    .Include(x => x.plan_version_buying_band_inventory_stations)
                                    .Include(x => x.plan_version_buying_band_inventory_stations.Select(y => y.station))
                                    .Include(x => x.plan_version_buying_band_inventory_stations.Select(y => y.plan_version_buying_band_inventory_station_dayparts))
                                    .Where(x => x.id == jobId)
                                    .OrderByDescending(x => x.id)
                                    .FirstOrDefault();
                if (planBuyingResult == null)
                {
                    return null;
                }

                var planBuyingBandInventoryStations = new PlanBuyingBandInventoryStationsDto
                {
                    BuyingJobId = planBuyingResult.id,
                    PostingType = (PostingTypeEnum)planBuyingResult.plan_version_buying_band_inventory_stations.FirstOrDefault(x => x.plan_version_buying_job_id == planBuyingResult.id).posting_type_id,
                    Totals = planBuyingResult.plan_version_buying_band_inventory_stations.Select(planBuyingBandStationTotals => new PlanBuyingProgramTotalsDto
                    {
                    }).FirstOrDefault(),
                    Details = planBuyingResult.plan_version_buying_band_inventory_stations.Select(planBuyingBandStationDetail => new PlanBuyingBandStationDetailDto
                    {

                        StationId = planBuyingBandStationDetail.station_id,
                        Impressions = planBuyingBandStationDetail.impressions,
                        Cost = planBuyingBandStationDetail.cost,
                        ManifestWeeksCount = planBuyingBandStationDetail.manifest_weeks_count,
                        RepFirm = planBuyingBandStationDetail.station.rep_firm_name,
                        OwnerName = planBuyingBandStationDetail.station.owner_name,
                        LegacyCallLetters = planBuyingBandStationDetail.station.legacy_call_letters,
                        MarketCode = planBuyingBandStationDetail.station.market_code,
                        PostingTypeConversionRate = planBuyingBandStationDetail.posting_type_conversion_rate,
                        PlanBuyingBandInventoryStationDayparts = planBuyingBandStationDetail.plan_version_buying_band_inventory_station_dayparts.Select(planBuyingBandInventoryStationDaypart => new PlanBuyingBandInventoryStationDaypartDto
                        {
                            ActiveDays = planBuyingBandInventoryStationDaypart.active_days,
                            Hours = planBuyingBandInventoryStationDaypart.hours
                        }).ToList()
                    }).ToList()
                };
                return planBuyingBandInventoryStations;
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingBands(PlanBuyingBandsDto dto, PostingTypeEnum postingType)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode &&
                                 x.posting_type == (int)postingType);

                buyingResults.plan_version_buying_band_details = dto.Details.Select(x =>
                        new plan_version_buying_band_details
                        {
                            cpm = x.Cpm,
                            max_band = x.MaxBand,
                            min_band = x.MinBand,
                            budget = x.Budget,
                            spots = x.Spots,
                            impressions = x.Impressions,
                            impressions_percentage = x.ImpressionsPercentage,
                            available_inventory_percentage = x.AvailableInventoryPercent
                        }).ToList();

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingBandInventoryStations(PlanBuyingBandInventoryStationsDto planBuyingBandInventoryStations)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_job
                    .Single(x => x.id == planBuyingBandInventoryStations.BuyingJobId,
                                 $"Unable to find results for given job id {planBuyingBandInventoryStations.BuyingJobId}");

                buyingResults.plan_version_buying_band_inventory_stations = planBuyingBandInventoryStations.Details.Select(x =>
                        new plan_version_buying_band_inventory_stations
                        {
                            posting_type_id = (int)planBuyingBandInventoryStations.PostingType,
                            station_id = x.StationId,
                            impressions = x.Impressions,
                            cost = x.Cost,
                            manifest_weeks_count = x.ManifestWeeksCount,
                            posting_type_conversion_rate = x.PostingTypeConversionRate,
                            plan_version_buying_band_inventory_station_dayparts = x.PlanBuyingBandInventoryStationDayparts.Select(y => new plan_version_buying_band_inventory_station_dayparts
                            {
                                active_days = y.ActiveDays,
                                hours = y.Hours
                            }).ToList()
                        }).ToList();

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingStations(PlanBuyingStationResultDto dto, PostingTypeEnum postingType)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode &&
                                 x.posting_type == (int)postingType);

                buyingResults.plan_version_buying_station_details = dto.Details
                            .Select(stationDto => new plan_version_buying_station_details
                            {
                                cpm = stationDto.Cpm,
                                budget = stationDto.Budget,
                                spots = stationDto.Spots,
                                impressions = stationDto.Impressions,
                                impressions_percentage = stationDto.ImpressionsPercentage,
                                market = stationDto.Market,
                                station = stationDto.Station
                            }).ToList();

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public PlanBuyingResultMarketsDto GetPlanBuyingResultMarketsByJobId(int jobId, PostingTypeEnum? postingType,
            SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_market_details)
                    .Where(x => x.plan_version_buying_job_id == jobId &&
                        x.posting_type == (int)postingType &&
                        x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (entity == null)
                    return null;

                var dto = new PlanBuyingResultMarketsDto
                {
                    BuyingJobId = entity.plan_version_buying_job_id,
                    PlanVersionId = entity.plan_version_buying_job.plan_version_id.Value,
                    SpotAllocationModelMode = (SpotAllocationModelMode)entity.spot_allocation_model_mode,
                    PostingType = (PostingTypeEnum)entity.posting_type,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        MarketCount = entity.total_market_count,
                        MarketCoveragePercent = entity.total_market_coverage_percent,
                        StationCount = entity.total_station_count,
                        SpotCount = entity.total_spots,
                        Impressions = entity.total_impressions,
                        AvgCpm = Convert.ToDecimal(entity.total_avg_cpm),
                        Budget = Convert.ToDecimal(entity.total_budget)
                    },
                    Details = entity.plan_version_buying_market_details.Select(s => new PlanBuyingResultMarketDetailsDto
                    {
                        Rank = s.rank,
                        MarketCoveragePercent = s.market_coverage_percent,
                        StationCount = s.stations,
                        SpotCount = s.spots,
                        Impressions = s.impressions,
                        Cpm = Convert.ToDecimal(s.cpm),
                        Budget = Convert.ToDecimal(s.budget),
                        ShareOfVoiceGoalPercentage = s.share_of_voice_goal_percentage,
                        ImpressionsPercentage = s.impressions_percentage,
                        MarketName = s.market_name
                    }).ToList()
                };
                return dto;
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingMarketResults(PlanBuyingResultMarketsDto dto, PostingTypeEnum postingType)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode &&
                                 x.posting_type == (int)postingType);

                //we only need to populate the coverage. the other totals are saved on the programs call
                buyingResults.total_market_coverage_percent = dto.Totals.MarketCoveragePercent;

                buyingResults.plan_version_buying_market_details = dto.Details.Select(d => new plan_version_buying_market_details
                {
                    market_coverage_percent = d.MarketCoveragePercent,
                    stations = d.StationCount,
                    spots = d.SpotCount,
                    impressions = d.Impressions,
                    cpm = Convert.ToDouble(d.Cpm),
                    budget = Convert.ToDouble(d.Budget),
                    impressions_percentage = d.ImpressionsPercentage,
                    share_of_voice_goal_percentage = d.ShareOfVoiceGoalPercentage,
                    rank = d.Rank,
                    market_name = d.MarketName
                }).ToList();

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

        /// <inheritdoc/>
        public int SaveBuyingAggregateResults(PlanBuyingResultBaseDto planBuyingResult)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planBuyingResultEntity = new plan_version_buying_results
                {
                    optimal_cpm = planBuyingResult.OptimalCpm,
                    total_market_count = planBuyingResult.Totals.MarketCount,
                    total_station_count = planBuyingResult.Totals.StationCount,
                    total_avg_cpm = planBuyingResult.Totals.AvgCpm,
                    total_avg_impressions = planBuyingResult.Totals.AvgImpressions,
                    total_budget = planBuyingResult.Totals.Budget,
                    total_impressions = planBuyingResult.Totals.Impressions,
                    plan_version_buying_job_id = planBuyingResult.JobId,
                    goal_fulfilled_by_proprietary = planBuyingResult.GoalFulfilledByProprietary,
                    total_spots = planBuyingResult.Totals.SpotCount,
                    total_market_coverage_percent = planBuyingResult.Totals.MarketCoveragePercent,
                    spot_allocation_model_mode = (int)planBuyingResult.SpotAllocationModelMode,
                    posting_type = (int)planBuyingResult.PostingType,
                };
                context.plan_version_buying_results.Add(planBuyingResultEntity);
                context.SaveChanges();
                return planBuyingResultEntity.id;
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingResultSpots(int planVersionBuyingResultId, PlanBuyingResultBaseDto planBuyingResult)
        {
            _InReadUncommitedTransaction(context =>
            {
                var planBuyingResultSpots = planBuyingResult.Programs.Select(program => new plan_version_buying_result_spots
                {
                    plan_version_buying_result_id = planVersionBuyingResultId,
                    program_name = program.ProgramName,
                    genre = program.Genre,
                    avg_impressions = program.AvgImpressions,
                    avg_cpm = program.AvgCpm,
                    percentage_of_buy = program.PercentageOfBuy,
                    market_count = program.MarketCount,
                    station_count = program.StationCount,
                    budget = program.Budget,
                    spots = program.SpotCount,
                    impressions = program.Impressions
                }).ToList();
                if (planBuyingResultSpots.Any())
                {
                    context.plan_version_buying_result_spots.AddRange(planBuyingResultSpots);
                    context.SaveChanges();
                }
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingResultSpotStations(int planVersionBuyingResultId, PlanBuyingResultBaseDto planBuyingResult)
        {
            _InReadUncommitedTransaction(context =>
            {
                var planBuyingResultSpotStations = planBuyingResult.Programs.Select(program => new plan_version_buying_result_spot_stations
                {
                    plan_version_buying_result_id = planVersionBuyingResultId,
                    program_name = program.ProgramName,
                    genre = program.Genre,
                    station = program.Station,
                    impressions = program.Impressions,
                    spots = program.SpotCount,
                    budget = program.Budget
                }).ToList();
                if (planBuyingResultSpotStations.Any())
                {
                    context.plan_version_buying_result_spot_stations.AddRange(planBuyingResultSpotStations);
                    context.SaveChanges();
                }
            });
        }

        /// <inheritdoc/>
        public PlanBuyingResultProgramsDto GetBuyingProgramsResultByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_result_spots)
                    .Where(x => x.plan_version_buying_job_id == jobId &&
                        x.spot_allocation_model_mode == (int)spotAllocationModelMode && x.posting_type == (int)postingType)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingResultProgramsDto
                {
                    PostingType = (PostingTypeEnum)result.posting_type,
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        MarketCount = result.total_market_count,
                        StationCount = result.total_station_count,
                        AvgCpm = result.total_avg_cpm,
                        AvgImpressions = result.total_avg_impressions,
                        Budget = result.total_budget,
                        SpotCount = result.total_spots,
                        Impressions = result.total_impressions
                    },
                    Details = result.plan_version_buying_result_spots.Select(r => new PlanBuyingProgramProgramDto
                    {
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
        /// <inheritdoc/>
        public PlanBuyingResultProgramsDto GetBuyingProgramsResultByJobId_V2(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var buyingResult = context.plan_version_buying_results
                   .Include(x => x.plan_version_buying_result_spot_stations)
                   .Single(x => x.plan_version_buying_job_id == jobId &&
                                x.spot_allocation_model_mode == (int)spotAllocationModelMode &&
                                x.posting_type == (int)postingType,
                                $"Unable to find results for given job id {jobId}, posting type {postingType} and allocation mode {spotAllocationModelMode}");

                return new PlanBuyingResultProgramsDto
                {
                    PostingType = (PostingTypeEnum)buyingResult.posting_type,
                    SpotAllocationModelMode = (SpotAllocationModelMode)buyingResult.spot_allocation_model_mode,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        MarketCount = buyingResult.total_market_count,
                        StationCount = buyingResult.total_station_count,
                        AvgCpm = buyingResult.total_avg_cpm,
                        AvgImpressions = buyingResult.total_avg_impressions,
                        Budget = buyingResult.total_budget,
                        SpotCount = buyingResult.total_spots,
                        Impressions = buyingResult.total_impressions
                    },
                    Details = (from planVersionBuyingResultSpotStations in buyingResult.plan_version_buying_result_spot_stations
                               join stations in context.stations
                               on planVersionBuyingResultSpotStations.station equals stations.legacy_call_letters
                               select new PlanBuyingProgramProgramDto
                               {
                                   ProgramName = planVersionBuyingResultSpotStations.program_name,
                                   Genre = planVersionBuyingResultSpotStations.genre,
                                   MarketCode = stations.market_code,
                                   Station = planVersionBuyingResultSpotStations.station,
                                   Impressions = planVersionBuyingResultSpotStations.impressions,
                                   Budget = planVersionBuyingResultSpotStations.budget,
                                   Spots = planVersionBuyingResultSpotStations.spots,
                                   RepFirm = stations.rep_firm_name,
                                   OwnerName = stations.owner_name,
                                   LegacyCallLetters = stations.legacy_call_letters
                               }).ToList()
                };
            });
        }

        /// <inheritdoc/>
        public CurrentBuyingExecutionResultDto GetBuyingResultsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode, PostingTypeEnum postingType)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_result_spots)
                    .Where(x => x.plan_version_buying_job_id == jobId && x.spot_allocation_model_mode == (int)spotAllocationModelMode
                    && x.posting_type == (int)postingType)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                var BuyingResult = new CurrentBuyingExecutionResultDto
                {
                    OptimalCpm = result.optimal_cpm,
                    JobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id,
                    GoalFulfilledByProprietary = result.goal_fulfilled_by_proprietary,
                    HasResults = result.plan_version_buying_result_spots.Any(),
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
                    PostingType = (PostingTypeEnum)result.posting_type
                };
                return BuyingResult;
            });
        }

        public List<CurrentBuyingExecutionResultDto> GetBuyingResultsByJobId(int jobId, PostingTypeEnum postingType)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_result_spots)
                    .Where(x => x.plan_version_buying_job_id == jobId
                    && x.posting_type == (int)postingType)
                    .OrderByDescending(p => p.id)
                    .ToList();

                if (result == null)
                    return null;

                var BuyingResult = result.Select(_MapToCurrentBuyingExecutionResultDto).ToList();

                return BuyingResult;
            });
        }

        /// <inheritdoc/>
        public PlanBuyingStationResultDto GetBuyingStationsResultByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(p => p.plan_version_buying_station_details)
                    .Where(x => x.plan_version_buying_job_id == jobId &&
                    x.posting_type == (int)postingType &&
                    x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingStationResultDto
                {
                    Id = result.id,
                    BuyingJobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id,
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
                    PostingType = (PostingTypeEnum)result.posting_type,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        Budget = result.total_budget,
                        AvgCpm = result.total_avg_cpm,
                        Impressions = result.total_impressions,
                        SpotCount = result.total_spots,
                        StationCount = result.total_station_count
                    },
                    Details = (from d in result.plan_version_buying_station_details
                               join st in context.stations
                               on d.station equals st.legacy_call_letters
                               select new PlanBuyingStationDto
                               {
                                   Budget = d.budget,
                                   Cpm = d.cpm,
                                   Impressions = d.impressions,
                                   ImpressionsPercentage = d.impressions_percentage,
                                   Market = d.market,
                                   Spots = d.spots,
                                   Station = d.station,
                                   Affiliate = st.affiliation,
                                   RepFirm = st.rep_firm_name,
                                   OwnerName = st.owner_name,
                                   LegacyCallLetters = st.legacy_call_letters
                               }).OrderByDescending(p => p.ImpressionsPercentage).ToList()
                };
            });
        }

        /// <inheritdoc/>
        public decimal GetGoalCpm(int planVersionId, int jobId, PostingTypeEnum postingType)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_parameters
                    .Where(p => p.plan_version_id == planVersionId && p.plan_version_buying_job_id == jobId && p.posting_type == (int)postingType)
                    .Select(p => p.cpm_goal)
                    .FirstOrDefault();

                return result;
            });
        }

        /// <inheritdoc/>
        public decimal GetGoalCpm(int jobId, PostingTypeEnum postingType)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_parameters
                    .Where(p => p.plan_version_buying_job_id == jobId && p.posting_type == (int)postingType)
                    .Select(p => p.cpm_goal)
                    .FirstOrDefault();

                return result;
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
                    context.SaveChanges();
                }

                var parameter = (from p in context.plan_version_buying_parameters
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

        /// <inheritdoc/>
        public void SavePlanBuyingOwnershipGroupResults(PlanBuyingResultOwnershipGroupDto dto, PostingTypeEnum postingType)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode &&
                                 x.posting_type == (int)postingType);
                buyingResults.plan_version_buying_ownership_group_details = dto.Details.Select(d => new plan_version_buying_ownership_group_details
                {
                    stations = d.StationCount,
                    spots = d.SpotCount,
                    impressions = d.Impressions,
                    cpm = d.Cpm,
                    budget = d.Budget,
                    impressions_percentage = d.ImpressionsPercentage,
                    ownership_group_name = d.OwnershipGroupName,
                    markets = d.MarketCount
                }).ToList();

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroupsByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(p => p.plan_version_buying_ownership_group_details)
                    .Where(x => x.plan_version_buying_job_id == jobId &&
                        x.posting_type == (int)postingType &&
                        x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingResultOwnershipGroupDto
                {
                    BuyingJobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id.Value,
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
                    PostingType = (PostingTypeEnum)result.posting_type,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        Budget = result.total_budget,
                        AvgCpm = result.total_avg_cpm,
                        Impressions = result.total_impressions,
                        MarketCount = result.total_market_count,
                        StationCount = result.total_station_count,
                        SpotCount = result.total_spots,
                        MarketCoveragePercent = result.total_market_coverage_percent
                    },
                    Details = result.plan_version_buying_ownership_group_details.Select(d => new PlanBuyingResultOwnershipGroupDetailsDto
                    {
                        Budget = d.budget,
                        Cpm = d.cpm,
                        Impressions = d.impressions,
                        ImpressionsPercentage = d.impressions_percentage,
                        MarketCount = d.markets,
                        SpotCount = d.spots,
                        StationCount = d.stations,
                        OwnershipGroupName = d.ownership_group_name
                    }).OrderByDescending(p => p.ImpressionsPercentage).ThenByDescending(p => p.Budget).ToList()
                };
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingRepFirmResults(PlanBuyingResultRepFirmDto dto, PostingTypeEnum postingType)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                        x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode &&
                        x.posting_type == (int)postingType
                        );

                buyingResults.plan_version_buying_rep_firm_details = dto.Details.Select(d => new plan_version_buying_rep_firm_details
                {
                    stations = d.StationCount,
                    spots = d.SpotCount,
                    impressions = d.Impressions,
                    cpm = d.Cpm,
                    budget = d.Budget,
                    impressions_percentage = d.ImpressionsPercentage,
                    rep_firm_name = d.RepFirmName,
                    markets = d.MarketCount
                }).ToList();

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public PlanBuyingResultRepFirmDto GetBuyingRepFirmsByJobId(int jobId, PostingTypeEnum? postingType, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(p => p.plan_version_buying_rep_firm_details)
                    .Where(x => x.plan_version_buying_job_id == jobId &&
                    x.posting_type == (int)postingType &&
                    x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingResultRepFirmDto
                {
                    BuyingJobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id.Value,
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode,
                    PostingType = (PostingTypeEnum)result.posting_type,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        Budget = result.total_budget,
                        AvgCpm = result.total_avg_cpm,
                        Impressions = result.total_impressions,
                        MarketCount = result.total_market_count,
                        StationCount = result.total_station_count,
                        SpotCount = result.total_spots,
                        MarketCoveragePercent = result.total_market_coverage_percent
                    },
                    Details = result.plan_version_buying_rep_firm_details.Select(d => new PlanBuyingResultRepFirmDetailsDto
                    {
                        Budget = d.budget,
                        Cpm = d.cpm,
                        Impressions = d.impressions,
                        ImpressionsPercentage = d.impressions_percentage,
                        MarketCount = d.markets,
                        SpotCount = d.spots,
                        StationCount = d.stations,
                        RepFirmName = d.rep_firm_name
                    }).OrderByDescending(p => p.ImpressionsPercentage).ThenByDescending(p => p.Budget).ToList()
                };
            });
        }

        private ResultSpotLists _MapToResultSpotLists(IEnumerable<plan_version_buying_api_result_spots> allSpots)
        {
            var resultLists = new ResultSpotLists();

            foreach (var spot in allSpots)
            {
                var allocatedFreqs = new List<plan_version_buying_api_result_spot_frequencies>();
                var unallocatedFreqs = new List<plan_version_buying_api_result_spot_frequencies>();

                spot.plan_version_buying_api_result_spot_frequencies.ForEach(f =>
                {
                    if (f.spots > 0)
                    {
                        allocatedFreqs.Add(f);
                    }
                    else
                    {
                        unallocatedFreqs.Add(f);
                    }
                });

                if (allocatedFreqs.Any())
                {
                    var resultSpot = _MapToPlanBuyingAllocatedSpot(spot, allocatedFreqs);
                    resultLists.Allocated.Add(resultSpot);
                }

                if (unallocatedFreqs.Any())
                {
                    var resultSpot = _MapToPlanBuyingAllocatedSpot(spot, unallocatedFreqs);
                    resultLists.Unallocated.Add(resultSpot);
                }
            }

            return resultLists;
        }

        private PlanBuyingAllocatedSpot _MapToPlanBuyingAllocatedSpot(plan_version_buying_api_result_spots spot, List<plan_version_buying_api_result_spot_frequencies> spotLengths)
        {
            // HACK : When the entity is retrieved the ContractWeek and InventoryWeek entities are sometimes getting flipped.
            // Be sure to use the correct one by referencing the explicitly stored id.
            var weeks = new List<media_weeks> { spot.inventory_media_week, spot.contract_media_week };
            // they both may be same week, so don't use single().
            var inventoryWeek = weeks.First(w => w.id == spot.inventory_media_week_id);
            var contractWeek = weeks.First(w => w.id == spot.contract_media_week_id);

            var resultSpot = new PlanBuyingAllocatedSpot
            {
                Id = spot.id,
                StationInventoryManifestId = spot.station_inventory_manifest_id,
                LegacyCallLetters = spot.station_inventory_manifest.station?.legacy_call_letters,
                RepFirm = spot.station_inventory_manifest.station?.rep_firm_name,
                OwnerName = spot.station_inventory_manifest.station?.owner_name,
                // impressions are for :30 sec only for buying v3
                Impressions30sec = spot.impressions30sec,
                SpotFrequencies = spotLengths
                    .Select(y => new SpotFrequency
                    {
                        SpotLengthId = y.spot_length_id,
                        SpotCost = y.cost,
                        Spots = y.spots,
                        Impressions = y.impressions
                    }).ToList(),
                InventoryMediaWeek = new MediaWeek
                {
                    Id = inventoryWeek.id,
                    MediaMonthId = inventoryWeek.media_month_id,
                    WeekNumber = inventoryWeek.week_number,
                    StartDate = inventoryWeek.start_date,
                    EndDate = inventoryWeek.end_date
                },
                ContractMediaWeek = new MediaWeek
                {
                    Id = contractWeek.id,
                    MediaMonthId = contractWeek.media_month_id,
                    WeekNumber = contractWeek.week_number,
                    StartDate = contractWeek.start_date,
                    EndDate = contractWeek.end_date
                },
                StandardDaypart = _MapToStandardDaypartDto(spot.standard_dayparts)
            };
            return resultSpot;
        }

        private class ResultSpotLists
        {
            public List<PlanBuyingAllocatedSpot> Allocated = new List<PlanBuyingAllocatedSpot>();
            public List<PlanBuyingAllocatedSpot> Unallocated = new List<PlanBuyingAllocatedSpot>();
        }

        public List<PlanBuyingAllocatedSpot> GetPlanBuyingAllocatedSpotsByPlanId(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode? spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;

                var apiResult = (from job in context.plan_version_buying_job
                                 from apiResults in job.plan_version_buying_api_results
                                 where job.plan_version_id == planVersionId
                                    && (!postingType.HasValue || apiResults.posting_type == (int)postingType.Value)
                                    && (!spotAllocationModelMode.HasValue || apiResults.spot_allocation_model_mode == (int)spotAllocationModelMode.Value)
                                 select apiResults)

                    .Include(x => x.plan_version_buying_api_result_spots)
                    .Include(x => x.plan_version_buying_api_result_spots.Select(s => s.inventory_media_week))
                    .Include(x => x.plan_version_buying_api_result_spots.Select(s => s.plan_version_buying_api_result_spot_frequencies))
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    throw new Exception($"No buying runs were found for the plan {planId}");

                return apiResult.plan_version_buying_api_result_spots.Where(x => x.plan_version_buying_api_result_spot_frequencies.Any(y => y.spots > 0)).Select(_MapToPlanBuyingAllocatedSpot).ToList();
            });
        }

        private PlanBuyingAllocatedSpot _MapToPlanBuyingAllocatedSpot(plan_version_buying_api_result_spots spot)
        {
            // HACK : When the entity is retrieved the ContractWeek and InventoryWeek entities are sometimes getting flipped.
            // Be sure to use the correct one by referencing the explicitly stored id.
            var weeks = new List<media_weeks> { spot.inventory_media_week, spot.contract_media_week };
            // they both may be same week, so don't use single().
            var inventoryWeek = weeks.First(w => w.id == spot.inventory_media_week_id);
            var contractWeek = weeks.First(w => w.id == spot.contract_media_week_id);

            var resultSpot = new PlanBuyingAllocatedSpot
            {
                Id = spot.id,
                StationInventoryManifestId = spot.station_inventory_manifest_id,
                // impressions are for :30 sec only for buying v3
                Impressions30sec = spot.impressions30sec,
                SpotFrequencies = spot.plan_version_buying_api_result_spot_frequencies.Select(x => new SpotFrequency
                {
                    SpotLengthId = x.spot_length_id,
                    SpotCost = x.cost,
                    Spots = x.spots,
                    Impressions = x.impressions
                }).ToList(),
                InventoryMediaWeek = new MediaWeek
                {
                    Id = inventoryWeek.id,
                    MediaMonthId = inventoryWeek.media_month_id,
                    WeekNumber = inventoryWeek.week_number,
                    StartDate = inventoryWeek.start_date,
                    EndDate = inventoryWeek.end_date
                },
                ContractMediaWeek = new MediaWeek
                {
                    Id = contractWeek.id,
                    MediaMonthId = contractWeek.media_month_id,
                    WeekNumber = contractWeek.week_number,
                    StartDate = contractWeek.start_date,
                    EndDate = contractWeek.end_date
                },
                StandardDaypart = _MapToStandardDaypartDto(spot.standard_dayparts)
            };
            return resultSpot;
        }

        public List<ProgramLineupProprietaryInventory> GetProprietaryInventoryForBuyingProgramLineup(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planVersion = (from buyingJob in context.plan_version_buying_job
                                   join pv in context.plan_versions on buyingJob.plan_version_id equals pv.id
                                   where buyingJob.id == jobId
                                   select pv).Single($"Could not find plan for job id = {jobId}");

                //get all summary ids selected on the plan
                var summaryIds = (from pv_ips in context.plan_version_buying_parameter_inventory_proprietary_summaries
                                  join pv_pp in context.plan_version_buying_parameters
                                    on pv_ips.plan_version_buying_parameter_id equals pv_pp.id
                                  where pv_pp.plan_version_buying_job_id == jobId
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

        public bool DeleteSavedBuyingData()
        {
            using (var context = CreateDBContext(false))
            {
                context.Database.ExecuteSqlCommand(
                "DELETE FROM plan_version_buying_api_result_spot_frequencies; "
                + "DBCC CHECKIDENT('plan_version_buying_api_result_spot_frequencies', RESEED, 0); "

                + "DELETE FROM plan_version_buying_api_result_spots; "
                + "DBCC CHECKIDENT('plan_version_buying_api_result_spots', RESEED, 0); "

                + "DELETE FROM plan_version_buying_api_results; "
                + "DBCC CHECKIDENT('plan_version_buying_api_results', RESEED, 0); "

                + "DELETE FROM plan_version_buying_result_spots; "
                + "DBCC CHECKIDENT('plan_version_buying_result_spots', RESEED, 0); "

                + "DELETE FROM plan_version_buying_result_spot_stations; "
                + "DBCC CHECKIDENT('plan_version_buying_result_spot_stations', RESEED, 0); "

                + "DELETE FROM plan_version_buying_band_station_dayparts; "
                + "DBCC CHECKIDENT('plan_version_buying_band_station_dayparts', RESEED, 0); "

                + "DELETE FROM plan_version_buying_band_stations; "
                + "DBCC CHECKIDENT('plan_version_buying_band_stations', RESEED, 0); "

                + "DELETE FROM plan_version_buying_results; "
                + "DBCC CHECKIDENT('plan_version_buying_results', RESEED, 0); "

                + "DELETE FROM plan_version_buying_job; "
                + "DBCC CHECKIDENT('plan_version_buying_job', RESEED, 0); "

                + "DELETE FROM plan_version_buying_band_details; "
                + "DBCC CHECKIDENT('plan_version_buying_band_details', RESEED, 0); "

                + "DELETE FROM plan_version_buying_band_inventory_station_dayparts; "
                + "DBCC CHECKIDENT('plan_version_buying_band_inventory_station_dayparts', RESEED, 0); "

                + "DELETE FROM plan_version_buying_band_inventory_stations; "
                + "DBCC CHECKIDENT('plan_version_buying_band_inventory_stations', RESEED, 0); "

                + "DELETE FROM plan_version_buying_market_details; "
                + "DBCC CHECKIDENT('plan_version_buying_market_details', RESEED, 0); "

                + "DELETE FROM plan_version_buying_ownership_group_details; "
                + "DBCC CHECKIDENT('plan_version_buying_ownership_group_details', RESEED, 0); "

                + "DELETE FROM plan_version_buying_parameter_inventory_proprietary_summaries; "
                + "DBCC CHECKIDENT('plan_version_buying_parameter_inventory_proprietary_summaries', RESEED, 0); "

                + "DELETE FROM plan_version_buying_parameters; "
                + "DBCC CHECKIDENT('plan_version_buying_parameters', RESEED, 0); "

                + "DELETE FROM plan_version_buying_rep_firm_details; "
                + "DBCC CHECKIDENT('plan_version_buying_rep_firm_details', RESEED, 0);"

                + "DELETE FROM plan_version_buying_station_details; "
                + "DBCC CHECKIDENT('plan_version_buying_station_details', RESEED, 0); ");
            }

            return true;
        }
    }
}
