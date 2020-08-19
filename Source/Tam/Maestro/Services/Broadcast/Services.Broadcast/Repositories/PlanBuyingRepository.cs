using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities;
using Tam.Maestro.Data.Entities;
using Services.Broadcast.Entities.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;

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
        /// <returns>PlanBuyingAllocationResult object</returns>
        PlanBuyingAllocationResult GetBuyingApiResultsByJobId(int jobId);

        /// <summary>
        /// Gets the plan buying band by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>PlanBuyingBandsDto object</returns>
        PlanBuyingBandsDto GetPlanBuyingBandByJobId(int jobId);

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
        /// <returns>PlanBuyingResultMarketsDto object</returns>
        PlanBuyingResultMarketsDto GetPlanBuyingResultMarketsByJobId(int jobId);

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
        /// <returns>BuyingProgramsResultDto object</returns>
        BuyingProgramsResultDto GetBuyingProgramsResultByJobId(int jobId);

        /// <summary>
        /// Gets the buying results by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>CurrentBuyingExecutionResultDto object</returns>
        CurrentBuyingExecutionResultDto GetBuyingResultsByJobId(int jobId);

        /// <summary>
        /// Gets the buying stations result by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>PlanBuyingStationResultDto object</returns>
        PlanBuyingStationResultDto GetBuyingStationsResultByJobId(int jobId);

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
        /// Saves the plan buying estimates.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="estimates">The estimates.</param>
        void SavePlanBuyingEstimates(int jobId, List<PlanBuyingEstimate> estimates);

        /// <summary>
        /// Gets the plan buying allocated spots by plan identifier.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>List of PlanBuyingAllocatedSpot objects</returns>
        List<PlanBuyingAllocatedSpot> GetPlanBuyingAllocatedSpotsByPlanId(int planId);

        /// <summary>
        /// Gets the plan buying allocated spots by plan version identifier.
        /// </summary>
        /// <param name="planVersionId">The plan version identifier.</param>
        /// <returns>List of PlanBuyingAllocatedSpot objects</returns>
        List<PlanBuyingAllocatedSpot> GetPlanBuyingAllocatedSpotsByPlanVersionId(int planVersionId);

        /// <summary>
        /// Updates the plan buying version identifier.
        /// </summary>
        /// <param name="versionId">The version identifier.</param>
        /// <param name="oldPlanVersionId">The old plan version identifier.</param>
        void UpdatePlanBuyingVersionId(int versionId, int oldPlanVersionId);
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
                    .Include(x => x.plan_version_buying_parameters_inventory_source_percentages)
                    .Include(x => x.plan_version_buying_parameters_inventory_source_type_percentages)
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
                    UnitCapType = (UnitCapEnum)e.unit_caps_type,
                    InventorySourcePercentages = e.plan_version_buying_parameters_inventory_source_percentages.Select(
                        s => new PlanInventorySourceDto
                        {
                            Id = s.inventory_source_id,
                            Name = s.inventory_sources.name,
                            Percentage = s.percentage
                        }).ToList(),
                    InventorySourceTypePercentages = e.plan_version_buying_parameters_inventory_source_type_percentages.Select(
                        s => new PlanInventorySourceTypeDto
                        {
                            Id = s.inventory_source_type,
                            Name = ((InventorySourceTypeEnum)s.inventory_source_type).GetDescriptionAttribute(),
                            Percentage = s.percentage
                        }).ToList(),
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
                    .Include(x => x.plan_version_buying_parameters_inventory_source_percentages)
                    .Include(x => x.plan_version_buying_parameters_inventory_source_type_percentages)
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
                InventorySourcePercentages = entity.plan_version_buying_parameters_inventory_source_percentages.Select(_MapPlanInventorySourceDto).ToList(),
                InventorySourceTypePercentages = entity.plan_version_buying_parameters_inventory_source_type_percentages.Select(_MapPlanInventorySourceTypeDto).ToList(),
                JobId = entity.plan_version_buying_job_id,
                PlanVersionId = entity.plan_version_id,
                AdjustedBudget = entity.budget_adjusted,
                AdjustedCPM = entity.cpm_adjusted,
                MarketGroup = (MarketGroupEnum)entity.market_group
            };
            return dto;
        }

        private PlanInventorySourceDto _MapPlanInventorySourceDto(
            plan_version_buying_parameters_inventory_source_percentages entity)
        {
            var dto = new PlanInventorySourceDto
            {
                Id = entity.inventory_source_id,
                Name = entity.inventory_sources.name,
                Percentage = entity.percentage
            };

            return dto;
        }

        private PlanInventorySourceTypeDto _MapPlanInventorySourceTypeDto(
            plan_version_buying_parameters_inventory_source_type_percentages entity)
        {
            var dto = new PlanInventorySourceTypeDto
            {
                Id = entity.inventory_source_type,
                Name = ((InventorySourceTypeEnum)entity.inventory_source_type).ToString(),
                Percentage = entity.percentage
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
                    market_group = (int)planBuyingParametersDto.MarketGroup
                };

                planBuyingParametersDto.InventorySourcePercentages.ForEach(s =>
                    planBuyingParameters.plan_version_buying_parameters_inventory_source_percentages.Add(
                        new plan_version_buying_parameters_inventory_source_percentages
                        {
                            inventory_source_id = s.Id,
                            percentage = s.Percentage
                        }));

                planBuyingParametersDto.InventorySourceTypePercentages.ForEach(s => 
                    planBuyingParameters.plan_version_buying_parameters_inventory_source_type_percentages.Add(
                        new plan_version_buying_parameters_inventory_source_type_percentages
                        {
                            inventory_source_type = (byte)s.Id,
                            percentage = s.Percentage
                        }));

                context.plan_version_buying_parameters.Add(planBuyingParameters);

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void SaveBuyingApiResults(PlanBuyingAllocationResult result)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };
                var planBuyingApiResult = new plan_version_buying_api_results
                {
                    optimal_cpm = result.BuyingCpm,
                    plan_version_buying_job_id = result.JobId,
                    buying_version = result.BuyingVersion
                };

                context.plan_version_buying_api_results.Add(planBuyingApiResult);

                context.SaveChanges();

                var pkSpots = context.plan_version_buying_api_result_spots.Any() ? context.plan_version_buying_api_result_spots.Max(x => x.id) + 1 : 0;

                var planBuyingApiResultSpots = new List<plan_version_buying_api_result_spots>();
                var planBuyingApiResultSpotFrequencies = new List<plan_version_buying_api_result_spot_frequencies>();

                foreach (var spot in result.Spots)
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
                                spots = x.Spots
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
        public PlanBuyingAllocationResult GetBuyingApiResultsByJobId(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var apiResult = context.plan_version_buying_api_results
                    .Include(x => x.plan_version_buying_api_result_spots)
                    .Include(x => x.plan_version_buying_api_result_spots.Select(y => y.plan_version_buying_api_result_spot_frequencies))
                    .Where(x => x.plan_version_buying_job_id == jobId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    return null;

                return new PlanBuyingAllocationResult
                {
                    BuyingCpm = apiResult.optimal_cpm,
                    JobId = apiResult.plan_version_buying_job_id,
                    Spots = apiResult.plan_version_buying_api_result_spots.Select(x => new PlanBuyingAllocatedSpot
                    {
                        Id = x.id,
                        StationInventoryManifestId = x.station_inventory_manifest_id,
                        // impressions are for :30 sec only for buying v3
                        Impressions30sec = x.impressions30sec,
                        SpotFrequencies = x.plan_version_buying_api_result_spot_frequencies.Select(y => new SpotFrequency
                        {
                            SpotLengthId = y.spot_length_id,
                            SpotCost = y.cost,
                            Spots = y.spots
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
                        StandardDaypart = _MapToDaypartDefaultDto(x.daypart_defaults)
                    }).ToList()
                };
            });
        }

        /// <inheritdoc/>
        public PlanBuyingBandsDto GetPlanBuyingBandByJobId(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_bands
                    .Include(x => x.plan_version_buying_band_details)
                    .Where(x => x.plan_version_buying_job_id == jobId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();
                
                if (result == null)
                    return null;

                return new PlanBuyingBandsDto
                {
                    Totals = new PlanBuyingBandTotalsDto
                    {
                        Cpm = result.total_cpm,
                        Budget = result.total_budget,
                        Impressions = result.total_impressions,
                        Spots = result.total_spots
                    },
                    Bands = result.plan_version_buying_band_details.Select(r => new PlanBuyingBandDetailDto
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

        /// <inheritdoc/>
        public void SavePlanBuyingBands(PlanBuyingBandsDto planBuyingBandDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_buying_bands.Add(new plan_version_buying_bands
                {
                    plan_version_buying_job_id = planBuyingBandDto.JobId,
                    total_budget = planBuyingBandDto.Totals.Budget,
                    total_cpm = planBuyingBandDto.Totals.Cpm,
                    total_impressions = planBuyingBandDto.Totals.Impressions,
                    total_spots = planBuyingBandDto.Totals.Spots,
                    plan_version_buying_band_details = planBuyingBandDto.Bands.Select(x =>
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
                        }).ToList()
                });

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public void SavePlanBuyingStations(PlanBuyingStationResultDto planBuyingStationResultDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.plan_version_buying_stations.Add(new plan_version_buying_stations
                {
                    plan_version_buying_job_id = planBuyingStationResultDto.JobId,
                    total_budget = planBuyingStationResultDto.Totals.Budget,
                    total_cpm = planBuyingStationResultDto.Totals.Cpm,
                    total_impressions = planBuyingStationResultDto.Totals.Impressions,
                    total_spots = planBuyingStationResultDto.Totals.Spots,
                    total_stations = planBuyingStationResultDto.Totals.Station,
                    plan_version_buying_station_details = planBuyingStationResultDto.Stations
                            .Select(stationDto => new plan_version_buying_station_details
                            {
                                cpm = stationDto.Cpm,
                                budget = stationDto.Budget,
                                spots = stationDto.Spots,
                                impressions = stationDto.Impressions,
                                impressions_percentage = stationDto.ImpressionsPercentage,
                                market = stationDto.Market,
                                station = stationDto.Station,
                            }).ToList()
                });

                context.SaveChanges();
            });
        }

        /// <inheritdoc/>
        public PlanBuyingResultMarketsDto GetPlanBuyingResultMarketsByJobId(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = context.plan_version_buying_markets
                    .Include(x => x.plan_version_buying_market_details)
                    .Where(x => x.plan_version_buying_job_id == jobId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();
                
                if (entity == null)
                    return null;

                var dto = new PlanBuyingResultMarketsDto
                {
                    Totals = new PlanBuyingResultMarketsTotalsDto
                    {
                        Markets = entity.total_markets,
                        CoveragePercent = entity.total_coverage_percent,
                        Stations = entity.total_stations,
                        Spots = entity.total_spots,
                        Impressions = entity.total_impressions,
                        Cpm = Convert.ToDecimal(entity.total_cpm),
                        Budget = Convert.ToDecimal(entity.total_budget)
                    },
                    MarketDetails = entity.plan_version_buying_market_details.Select(s => new PlanBuyingResultMarketDetailsDto
                    {
                        Rank = s.rank,
                        MarketCoveragePercent = s.market_coverage_percent,
                        Stations = s.stations,
                        Spots = s.spots,
                        Impressions = s.impressions,
                        Cpm = Convert.ToDecimal(s.cpm),
                        Budget = Convert.ToDecimal(s.budget),
                        ShareOfVoiceGoalPercentage = s.share_of_voice_goal_percentage,
                        ImpressionsPercentage = s.impressions_percentage
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
                context.plan_version_buying_markets.Add(new plan_version_buying_markets
                {
                    plan_version_buying_job_id = dto.BuyingJobId,
                    total_markets = dto.Totals.Markets,
                    total_coverage_percent = dto.Totals.CoveragePercent,
                    total_stations = dto.Totals.Stations,
                    total_spots = dto.Totals.Spots,
                    total_impressions = dto.Totals.Impressions,
                    total_cpm = Convert.ToDouble(dto.Totals.Cpm),
                    total_budget = Convert.ToDouble(dto.Totals.Budget),
                    plan_version_buying_market_details = dto.MarketDetails.Select(d => new plan_version_buying_market_details
                    {
                        market_coverage_percent = d.MarketCoveragePercent,
                        stations = d.Stations,
                        spots = d.Spots,
                        impressions = d.Impressions,
                        cpm = Convert.ToDouble(d.Cpm),
                        budget = Convert.ToDouble(d.Budget),
                        impressions_percentage = d.ImpressionsPercentage,
                        share_of_voice_goal_percentage = d.ShareOfVoiceGoalPercentage,
                        rank = d.Rank
                    }).ToList()
                });

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
                    total_spots = buyingResult.Totals.Spots
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
                        spots = program.Spots,
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
        public BuyingProgramsResultDto GetBuyingProgramsResultByJobId(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_result_spots)
                    .Where(x => x.plan_version_buying_job_id == jobId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new BuyingProgramsResultDto
                {
                    Totals = new BuyingProgramsResultTotalsDto
                    {
                        MarketCount = result.total_market_count,
                        StationCount = result.total_station_count,
                        AvgCpm = result.total_avg_cpm,
                        AvgImpressions = result.total_avg_impressions,
                        Budget = result.total_budget,
                        Spots = result.total_spots,
                        Impressions = result.total_impressions
                    },
                    Programs = result.plan_version_buying_result_spots.Select(r => new PlanBuyingProgramProgramDto
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

        /// <inheritdoc/>
        public CurrentBuyingExecutionResultDto GetBuyingResultsByJobId(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_results
                    .Include(x => x.plan_version_buying_result_spots)
                    .Where(x => x.plan_version_buying_job_id == jobId)
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
                    HasResults = result.plan_version_buying_result_spots.Any()
                };
            });
        }

        /// <inheritdoc/>
        public PlanBuyingStationResultDto GetBuyingStationsResultByJobId(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_version_buying_stations
                    .Include(p => p.plan_version_buying_station_details)
                    .Where(x => x.plan_version_buying_job_id == jobId)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (result == null)
                    return null;

                return new PlanBuyingStationResultDto
                {
                    Id = result.id,
                    JobId = result.plan_version_buying_job_id,
                    PlanVersionId = result.plan_version_buying_job.plan_version_id,
                    Totals = new PlanBuyingStationTotalsDto
                    {
                        Budget = result.total_budget,
                        Cpm = result.total_cpm,
                        Impressions = result.total_impressions,
                        ImpressionsPercentage = 100,
                        Spots = result.total_spots,
                        Station = result.total_stations
                    },
                    Stations = result.plan_version_buying_station_details.Select(d => new PlanBuyingStationDto
                    {
                        Budget = d.budget,
                        Cpm = d.cpm,
                        Impressions = d.impressions,
                        Id = d.id,
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
        public void SavePlanBuyingEstimates(int jobId, List<PlanBuyingEstimate> estimates)
        {
            _InReadUncommitedTransaction(context =>
            {
                var propertiesToIgnore = new List<string>() { "id" };

                var itemsToInsert = estimates
                    .Select(x => new plan_version_buying_job_inventory_source_estimates
                    {
                        media_week_id = x.MediaWeekId,
                        inventory_source_id = x.InventorySourceId,
                        inventory_source_type = (int?)x.InventorySourceType,
                        plan_version_buying_job_id = jobId,
                        impressions = x.Impressions,
                        cost = x.Cost
                    })
                    .ToList();

                BulkInsert(context, itemsToInsert, propertiesToIgnore);
            });
        }

        /// <inheritdoc/>
        public List<PlanBuyingAllocatedSpot> GetPlanBuyingAllocatedSpotsByPlanId(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var plan = context.plans.Single(x => x.id == planId);
                var planVersionId = plan.latest_version_id;

                var apiResult = (from job in context.plan_version_buying_job
                                 from apiResults in job.plan_version_buying_api_results
                                 where job.plan_version_id == planVersionId
                                 select apiResults)
                    .Include(x => x.plan_version_buying_api_result_spots)
                    .Include(x => x.plan_version_buying_api_result_spots.Select(s => s.inventory_media_week))
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    throw new Exception($"No buying runs were found for the plan {planId}");

                return apiResult.plan_version_buying_api_result_spots.Select(_MapToPlanBuyingAllocatedSpot).ToList();
            });
        }

        /// <inheritdoc/>
        public List<PlanBuyingAllocatedSpot> GetPlanBuyingAllocatedSpotsByPlanVersionId(int planVersionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var apiResult = (from job in context.plan_version_buying_job
                                 from apiResults in job.plan_version_buying_api_results
                                 where job.plan_version_id == planVersionId
                                 select apiResults)
                    .Include(x => x.plan_version_buying_api_result_spots)
                    .Include(x => x.plan_version_buying_api_result_spots.Select(s => s.inventory_media_week))
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault();

                if (apiResult == null)
                    throw new Exception($"No buying runs were found for the version {planVersionId}");

                return apiResult.plan_version_buying_api_result_spots.Select(_MapToPlanBuyingAllocatedSpot).ToList();
            });
        }

        private PlanBuyingAllocatedSpot _MapToPlanBuyingAllocatedSpot(plan_version_buying_api_result_spots spot)
        {
            return new PlanBuyingAllocatedSpot
            {
                Id = spot.id,
                StationInventoryManifestId = spot.station_inventory_manifest_id,
                Impressions30sec = spot.impressions30sec,
                SpotFrequencies = spot.plan_version_buying_api_result_spot_frequencies.Select(x => new SpotFrequency
                {
                    SpotLengthId = x.spot_length_id,
                    SpotCost = x.cost,
                    Spots = x.spots
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
                StandardDaypart = _MapToDaypartDefaultDto(spot.daypart_defaults)
            };
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
                if(job != null)
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
    }
}