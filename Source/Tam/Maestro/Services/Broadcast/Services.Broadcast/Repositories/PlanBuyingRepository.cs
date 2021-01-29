﻿using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Buying;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
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
        /// Gets the latest buying job.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>PlanBuyingJob object</returns>
        PlanBuyingJob GetLatestBuyingJob(int planId);

        /// <summary>
        /// Gets the plan buying job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>PlanBuyingJob object</returns>
        PlanBuyingJob GetPlanBuyingJob(int jobId);

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
        /// <returns></returns>
        PlanBuyingAllocationResult GetBuyingApiResultsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality);

        /// <summary>
        /// Gets the plan buying band by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingBandsDto GetPlanBuyingBandByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Saves the plan buying bands.
        /// </summary>
        /// <param name="planBuyingBandDto">The plan buying band dto.</param>
        void SavePlanBuyingBands(PlanBuyingBandsDto planBuyingBandDto);

        /// <summary>
        /// Saves the plan buying stations.
        /// </summary>
        /// <param name="planBuyingStationResultDto">The plan buying station result dto.</param>
        void SavePlanBuyingStations(PlanBuyingStationResultDto planBuyingStationResultDto);

        /// <summary>
        /// Gets the plan buying result markets by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultMarketsDto GetPlanBuyingResultMarketsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Saves the plan buying market results.
        /// </summary>
        /// <param name="dto">The dto.</param>
        void SavePlanBuyingMarketResults(PlanBuyingResultMarketsDto dto);

        /// <summary>
        /// Saves the buying aggregate results.
        /// </summary>
        /// <param name="result">The result.</param>
        void SaveBuyingAggregateResults(PlanBuyingResultBaseDto result);

        /// <summary>
        /// Gets the buying programs result by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultProgramsDto GetBuyingProgramsResultByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Gets the buying results by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        CurrentBuyingExecutionResultDto GetBuyingResultsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Gets the buying stations result by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingStationResultDto GetBuyingStationsResultByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Gets the goal CPM.
        /// </summary>
        /// <param name="planVersionId">The plan version identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>Decimal Goal CPM</returns>
        decimal GetGoalCpm(int planVersionId, int jobId);

        /// <summary>
        /// Gets the goal CPM.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>Decimal Goal CPM</returns>
        decimal GetGoalCpm(int jobId);

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
        void SavePlanBuyingOwnershipGroupResults(PlanBuyingResultOwnershipGroupDto aggregateOwnershipGroupResultsTaskResult);

        /// <summary>
        /// Gets the buying ownership groups by job identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroupsByJobId(int id, SpotAllocationModelMode spotAllocationModelMode);

        /// <summary>
        /// Saves the plan buying rep firm results.
        /// </summary>
        /// <param name="dto">The dto.</param>
        void SavePlanBuyingRepFirmResults(PlanBuyingResultRepFirmDto dto);

        /// <summary>
        /// Gets the buying rep firms by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="spotAllocationModelMode">The spot allocation model mode.</param>
        /// <returns></returns>
        PlanBuyingResultRepFirmDto GetBuyingRepFirmsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode);
    }

    public class PlanBuyingRepository : BroadcastRepositoryBase, IPlanBuyingRepository
    {
        public PlanBuyingRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
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
                    CoverageGoalPercent = e.plan_versions.coverage_goal_percent,
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
                MarketGroup = (MarketGroupEnum)entity.market_group
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
                        .ToList()
                };

                context.plan_version_buying_parameters.Add(planBuyingParameters);

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void SaveBuyingApiResults(PlanBuyingAllocationResult result)
        {
            // merge allocated and unallocated into one list
            // figure we'll have less allocated than unallocated.
            var spots = result.UnallocatedSpots;
            foreach (var allocated in result.AllocatedSpots)
            {
                var existingSpot = spots.SingleOrDefault(s => 
                    s.Id == allocated.Id && 
                    s.ContractMediaWeek == allocated.ContractMediaWeek);

                if (existingSpot == null)
                {
                    spots.Add(allocated);
                }
                else
                {
                    existingSpot.SpotFrequencies.AddRange(allocated.SpotFrequencies);
                }
            }

            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };
                var planBuyingApiResult = new plan_version_buying_api_results
                {
                    optimal_cpm = result.BuyingCpm,
                    plan_version_buying_job_id = result.JobId,
                    buying_version = result.BuyingVersion,
                    spot_allocation_model_mode = (int)result.SpotAllocationModelMode
                };

                context.plan_version_buying_api_results.Add(planBuyingApiResult);

                context.SaveChanges();

                var pkSpots = context.plan_version_buying_api_result_spots.Any() ? context.plan_version_buying_api_result_spots.Max(x => x.id) + 1 : 1;

                var planBuyingApiResultSpots = new List<plan_version_buying_api_result_spots>();
                var planBuyingApiResultSpotFrequencies = new List<plan_version_buying_api_result_spot_frequencies>();

                foreach (var spot in spots)
                {
                    var currentSpotId = pkSpots++;

                    var planBuyingApiResultSpot = new plan_version_buying_api_result_spots
                    {
                        id = currentSpotId,
                        plan_version_buying_api_results_id = planBuyingApiResult.id,
                        station_inventory_manifest_id = spot.Id,
                        contract_media_week_id = spot.ContractMediaWeek.Id,
                        inventory_media_week_id = spot.InventoryMediaWeek.Id,
                        impressions30sec = spot.Impressions30sec,
                        standard_daypart_id = spot.StandardDaypart.Id,
                    };

                    var frequencies = spot.SpotFrequencies
                            .Select(x => new plan_version_buying_api_result_spot_frequencies
                            {
                                plan_version_buying_api_result_spot_id = currentSpotId,
                                spot_length_id = x.SpotLengthId,
                                cost = x.SpotCost,
                                spots = x.Spots,
                                impressions = x.Impressions
                            })
                            .ToList();

                    planBuyingApiResultSpots.Add(planBuyingApiResultSpot);
                    planBuyingApiResultSpotFrequencies.AddRange(frequencies);
                }

                BulkInsert(context, planBuyingApiResultSpots);
                BulkInsert(context, planBuyingApiResultSpotFrequencies, propertiesToIgnore);
            });
        }

        /// <inheritdoc/>
        public PlanBuyingAllocationResult GetBuyingApiResultsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var apiResult = context.plan_version_buying_api_results
                    .Include(x => x.plan_version_buying_api_result_spots)
                    .Include(x => x.plan_version_buying_api_result_spots.Select(y => y.plan_version_buying_api_result_spot_frequencies))
                    .Where(x => x.plan_version_buying_job_id == jobId && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
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
                    SpotAllocationModelMode = (SpotAllocationModelMode)apiResult.spot_allocation_model_mode
                };

                return allocationResult;
            });
        }

        /// <inheritdoc/>
        public PlanBuyingBandsDto GetPlanBuyingBandByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_band_details)
                    .Where(x => x.plan_version_buying_job_id == jobId && 
                    x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingBandsDto
                {
                    BuyingJobId= result.plan_version_buying_job_id,
                    SpotAllocationModelMode = spotAllocationModelMode,
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
        public void SavePlanBuyingBands(PlanBuyingBandsDto dto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode);

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
        public void SavePlanBuyingStations(PlanBuyingStationResultDto dto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode);

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
        public PlanBuyingResultMarketsDto GetPlanBuyingResultMarketsByJobId(int jobId,
            SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_market_details)
                    .Where(x => x.plan_version_buying_job_id == jobId && 
                        x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (entity == null)
                    return null;

                var dto = new PlanBuyingResultMarketsDto
                {
                    BuyingJobId = entity.plan_version_buying_job_id,
                    PlanVersionId = entity.plan_version_buying_job.plan_version_id.Value,
                    SpotAllocationModelMode = spotAllocationModelMode,
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
        public void SavePlanBuyingMarketResults(PlanBuyingResultMarketsDto dto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode);

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
        public void SaveBuyingAggregateResults(PlanBuyingResultBaseDto buyingResult)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };

                var planBuyingResult = new plan_version_buying_results
                {
                    optimal_cpm = buyingResult.OptimalCpm,
                    total_market_count = buyingResult.Totals.MarketCount,
                    total_station_count = buyingResult.Totals.StationCount,
                    total_avg_cpm = buyingResult.Totals.AvgCpm,
                    total_avg_impressions = buyingResult.Totals.AvgImpressions,
                    total_budget = buyingResult.Totals.Budget,
                    total_impressions = buyingResult.Totals.Impressions,
                    plan_version_buying_job_id = buyingResult.JobId,
                    goal_fulfilled_by_proprietary = buyingResult.GoalFulfilledByProprietary,
                    total_spots = buyingResult.Totals.SpotCount,
                    total_market_coverage_percent = buyingResult.Totals.MarketCoveragePercent,
                    spot_allocation_model_mode = (int)buyingResult.SpotAllocationModelMode
                };

                context.plan_version_buying_results.Add(planBuyingResult);

                context.SaveChanges();

                var spots = new List<plan_version_buying_result_spots>();

                foreach (var program in buyingResult.Programs)
                {
                    var planBuyingResultSpots = new plan_version_buying_result_spots
                    {
                        plan_version_buying_result_id = planBuyingResult.id,
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
                    };

                    spots.Add(planBuyingResultSpots);
                }

                if (spots.Any())
                {
                    BulkInsert(context, spots, propertiesToIgnore);
                }
            });
        }

        /// <inheritdoc/>
        public PlanBuyingResultProgramsDto GetBuyingProgramsResultByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_result_spots)
                    .Where(x => x.plan_version_buying_job_id == jobId &&
                        x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingResultProgramsDto
                {
                    SpotAllocationModelMode = spotAllocationModelMode,
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
        public CurrentBuyingExecutionResultDto GetBuyingResultsByJobId(int jobId, 
            SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_result_spots)
                    .Where(x => x.plan_version_buying_job_id == jobId && x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new CurrentBuyingExecutionResultDto
                {
                    OptimalCpm = result.optimal_cpm,
                    JobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id,
                    GoalFulfilledByProprietary = result.goal_fulfilled_by_proprietary,
                    HasResults = result.plan_version_buying_result_spots.Any(),
                    SpotAllocationModelMode = (SpotAllocationModelMode)result.spot_allocation_model_mode
                };
            });
        }

        /// <inheritdoc/>
        public PlanBuyingStationResultDto GetBuyingStationsResultByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(p => p.plan_version_buying_station_details)
                    .Where(x => x.plan_version_buying_job_id == jobId && 
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
                    SpotAllocationModelMode = spotAllocationModelMode,
                    Totals = new PlanBuyingProgramTotalsDto
                    {
                        Budget = result.total_budget,
                        AvgCpm = result.total_avg_cpm,
                        Impressions = result.total_impressions,
                        SpotCount = result.total_spots,
                        StationCount = result.total_station_count
                    },
                    Details = result.plan_version_buying_station_details.Select(d => new PlanBuyingStationDto
                    {
                        Budget = d.budget,
                        Cpm = d.cpm,
                        Impressions = d.impressions,
                        ImpressionsPercentage = d.impressions_percentage,
                        Market = d.market,
                        Spots = d.spots,
                        Station = d.station
                    }).OrderByDescending(p => p.ImpressionsPercentage).ToList()
                };
            });
        }

        /// <inheritdoc/>
        public decimal GetGoalCpm(int planVersionId, int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_parameters.Where(p =>
                        p.plan_version_id == planVersionId && p.plan_version_buying_job_id == jobId)
                    .Select(p => p.cpm_goal).FirstOrDefault();

                return result;
            });
        }

        /// <inheritdoc/>
        public decimal GetGoalCpm(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_parameters.Where(p => p.plan_version_buying_job_id == jobId)
                    .Select(p => p.cpm_goal).FirstOrDefault();

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
        public void SavePlanBuyingOwnershipGroupResults(PlanBuyingResultOwnershipGroupDto dto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId &&
                                 x.spot_allocation_model_mode == (int)dto.SpotAllocationModelMode);
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
        public PlanBuyingResultOwnershipGroupDto GetBuyingOwnershipGroupsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(p => p.plan_version_buying_ownership_group_details)
                    .Where(x => x.plan_version_buying_job_id == jobId &&
                        x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingResultOwnershipGroupDto
                {
                    BuyingJobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id.Value,
                    SpotAllocationModelMode = spotAllocationModelMode,
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
        public void SavePlanBuyingRepFirmResults(PlanBuyingResultRepFirmDto dto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var buyingResults = context.plan_version_buying_results
                    .Single(x => x.plan_version_buying_job_id == dto.BuyingJobId && 
                        x.spot_allocation_model_mode == (int) dto.SpotAllocationModelMode);

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
        public PlanBuyingResultRepFirmDto GetBuyingRepFirmsByJobId(int jobId, SpotAllocationModelMode spotAllocationModelMode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(p => p.plan_version_buying_rep_firm_details)
                    .Where(x => x.plan_version_buying_job_id == jobId && 
                    x.spot_allocation_model_mode == (int)spotAllocationModelMode)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingResultRepFirmDto
                {
                    BuyingJobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id.Value,
                    SpotAllocationModelMode = spotAllocationModelMode,
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
            var weeks = new List<media_weeks> {spot.inventory_media_week, spot.contract_media_week};
            // they both may be same week, so don't use single().
            var inventoryWeek = weeks.First(w => w.id == spot.inventory_media_week_id);
            var contractWeek = weeks.First(w => w.id == spot.contract_media_week_id);

            var resultSpot = new PlanBuyingAllocatedSpot
            {
                Id = spot.id,
                StationInventoryManifestId = spot.station_inventory_manifest_id,
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
    }
}