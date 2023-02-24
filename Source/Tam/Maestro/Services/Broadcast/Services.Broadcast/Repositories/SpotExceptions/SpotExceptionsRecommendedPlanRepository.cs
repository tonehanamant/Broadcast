using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    /// <summary></summary>
    public interface ISpotExceptionsRecommendedPlanRepository : IDataRepository
    {
        /// <summary>
        /// Gets the recommended plan grouping to do asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanGroupingDto>> GetRecommendedPlanGroupingToDoAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan grouping done asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanGroupingDto>> GetRecommendedPlanGroupingDoneAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan spots to do.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsDto>> GetRecommendedPlanSpotsToDo(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan spots queued.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns> </returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsDto>> GetRecommendedPlanSpotsQueued(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan spots synced.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsDto>> GetRecommendedPlanSpotsSynced(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan market filters asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetRecommendedPlanMarketFiltersAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan legacy call letter filters asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetRecommendedPlanLegacyCallLetterFiltersAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan inventory source filters asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetRecommendedPlanInventorySourceFiltersAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan details to do by identifier.
        /// </summary>
        /// <param name="detailsId">The recommended plan identifier.</param>
        /// <returns>
        /// </returns>
        Task<SpotExceptionsRecommendedPlanSpotsToDoDto> GetRecommendedPlanDetailsToDoById(int detailsId);

        /// <summary>
        /// Gets the recommended plan details done by identifier.
        /// </summary>
        /// <param name="detailsId">The recommended plan identifier.</param>
        /// <returns></returns>
        Task<SpotExceptionsRecommendedPlanSpotsDoneDto> GetRecommendedPlanDetailsDoneById(int detailsId);

        /// <summary>
        /// Gets the recommended plan advertisers to do asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<Guid>> GetRecommendedPlanAdvertisersToDoAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan advertisers done asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<Guid>> GetRecommendedPlanAdvertisersDoneAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan stations to do asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetRecommendedPlanStationsToDoAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan stations done asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns>
        /// </returns>
        Task<List<string>> GetRecommendedPlanStationsDoneAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan spots to do by ids.
        /// </summary>
        /// <param name="todoId">The todo identifier.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsToDoDto>> GetRecommendedPlanSpotsToDoByIds(List<int?> todoId);

        /// <summary>
        /// Adds the recommended plan to done.
        /// </summary>
        /// <param name="doneRecommendedPlansToAdd">The done recommended plans to add.</param>
        /// <param name="recommendedPlanId">The recommended plan identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="currentDate">The current date.</param>
        void AddRecommendedPlanToDone(List<SpotExceptionsRecommendedPlanSpotsDoneDto> doneRecommendedPlansToAdd, int recommendedPlanId, string userName, DateTime currentDate);

        /// <summary>
        /// Deletes the recommended plan from to do.
        /// </summary>
        /// <param name="existingRecommendedPlansToDo">The existing recommended plans to do.</param>
        void DeleteRecommendedPlanFromToDo(List<SpotExceptionsRecommendedPlanSpotsToDoDto> existingRecommendedPlansToDo);

        /// <summary>
        /// Gets the spot exception plan detail with decision.
        /// </summary>
        /// <param name="spotExceptionsId">The spot exceptions identifier.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsDoneDto>> GetSpotExceptionPlanDetailsWithDecision(List<int> spotExceptionsId);

        /// <summary>
        /// Adds the recommended plan done decisions asynchronous.
        /// </summary>
        /// <param name="decisionsToAdd">The decisions to add.</param>
        void UpdateRecommendedPlanDoneDecisionsAsync(List<SpotExceptionsRecommendedPlanSpotDecisionsDoneDto> decisionsToAdd);
    }

    public class SpotExceptionsRecommendedPlanRepository : BroadcastRepositoryBase, ISpotExceptionsRecommendedPlanRepository
    {
        public SpotExceptionsRecommendedPlanRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }


        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanGroupingDto>> GetRecommendedPlanGroupingToDoAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var planDetailsToDo = context.spot_exceptions_recommended_plan_details
                    .Where(recommendedPlanToDoDb => recommendedPlanToDoDb.spot_exceptions_recommended_plans.program_air_time >= weekStartDate
                    && recommendedPlanToDoDb.spot_exceptions_recommended_plans.program_air_time <= weekEndDate).ToList();

                var planGroupingToDo = planDetailsToDo.Where(x => x.is_recommended_plan).GroupBy(x => new { x.recommended_plan_id })
                    .Select(x =>
                    {
                        var first = x.First();
                        var recommendedPlanVersion = first.plan.plan_versions.Single(planVersion => planVersion.id == first.plan.latest_version_id);
                        return new SpotExceptionsRecommendedPlanGroupingDto
                        {
                            PlanId = x.Key.recommended_plan_id,
                            AdvertiserMasterId = first.plan.campaign.advertiser_master_id,
                            PlanName = first.plan.name,
                            AffectedSpotsCount = x.Count(),
                            Impressions = x.Sum(y => y.spot_delivered_impressions),
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AudienceName = first.audience_name
                        };
                    }).ToList();

                return planGroupingToDo;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanGroupingDto>> GetRecommendedPlanGroupingDoneAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var planDetailsDone = context.spot_exceptions_recommended_plan_details_done
                    .Where(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plans_done.program_air_time >= weekStartDate
                    && recommendedPlanDoneDb.spot_exceptions_recommended_plans_done.program_air_time <= weekEndDate
                    && recommendedPlanDoneDb.spot_exceptions_recommended_plan_done_decisions.Any()).ToList();

                var planGroupingDone = planDetailsDone.GroupBy(x => new { x.recommended_plan_id })
                    .Select(x =>
                    {
                        var first = x.First();
                        var recommendedPlanVersion = first.plan.plan_versions.Single(planVersion => planVersion.id == first.plan.latest_version_id);
                        var decisions = x.Select(y => y.spot_exceptions_recommended_plan_done_decisions).ToList();
                        return new SpotExceptionsRecommendedPlanGroupingDto
                        {
                            PlanId = x.Key.recommended_plan_id,
                            AdvertiserMasterId = first.plan.campaign.advertiser_master_id,
                            PlanName = first.plan.name,
                            AffectedSpotsCount = x.Count(),
                            Impressions = x.Sum(y => y.spot_delivered_impressions),
                            SyncedTimestamp = decisions.Max(d => d.Max(m => m.synced_at)),
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AudienceName = first.audience_name
                        };
                    }).ToList();

                return planGroupingDone;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsDto>> GetRecommendedPlanSpotsToDo(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Spots ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var planDetailsToDo = context.spot_exceptions_recommended_plans
                    .Join
                    (
                        context.spot_exceptions_recommended_plan_details
                        .Where(x => x.is_recommended_plan == true),
                        x => x.id, y => y.spot_exceptions_recommended_plan_id,
                        (x, y) => new { plan = x, detail = y }
                    )
                    .Join(context.spot_lengths, x => x.plan.spot_length_id, y => y.id, (x, y) => new { lengthPlan = x, spotLength = y })
                    .Where(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.lengthPlan.plan.program_air_time >= weekStartDate
                    && recommendedPlanSpotsToDoDb.lengthPlan.plan.program_air_time <= weekEndDate
                    && recommendedPlanSpotsToDoDb.lengthPlan.detail.recommended_plan_id == planId)
                    .GroupJoin(
                        context.stations,
                        recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.lengthPlan.plan.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (recommendedPlanSpotsToDoDb, stationDb) => new { recommendedPlansToDo = recommendedPlanSpotsToDoDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var planSpotsToDo = planDetailsToDo.Select(x =>
                {
                    return new SpotExceptionsRecommendedPlanSpotsDto
                    {
                        Id = x.recommendedPlansToDo.lengthPlan.plan.id,
                        EstimateId = x.recommendedPlansToDo.lengthPlan.plan.estimate_id,
                        IsciName = x.recommendedPlansToDo.lengthPlan.plan.house_isci,
                        ProgramAirTime = x.recommendedPlansToDo.lengthPlan.plan.program_air_time,
                        RecommendedPlanName = x.recommendedPlansToDo.lengthPlan.detail.plan.name,
                        PlanId = x.recommendedPlansToDo.lengthPlan.detail.recommended_plan_id,
                        Impressions = x.recommendedPlansToDo.lengthPlan.detail.spot_delivered_impressions,
                        SpotLength = x.recommendedPlansToDo.spotLength.length,
                        ProgramName = x.recommendedPlansToDo.lengthPlan.plan.program_name,
                        InventorySource = x.recommendedPlansToDo.lengthPlan.plan.inventory_source,
                        Affiliate = x.recommendedPlansToDo.lengthPlan.plan.affiliate,
                        MarketName = x.Station?.market?.geography_name,
                        Station = x.recommendedPlansToDo.lengthPlan.plan.station_legacy_call_letters
                    };

                }).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Spots ToDo");
                return planSpotsToDo;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsDto>> GetRecommendedPlanSpotsQueued(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Spots Queued");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var planDetailsDone = context.spot_exceptions_recommended_plans_done
                .Join
                (
                    context.spot_exceptions_recommended_plan_details_done
                    .Where(x => x.spot_exceptions_recommended_plan_done_decisions.Any()),
                    x => x.id, y => y.spot_exceptions_recommended_plan_done_id,
                    (x, y) => new { plan = x, detail = y }
                )
                .Join(context.spot_lengths, x => x.plan.spot_length_id, y => y.id, (x, y) => new { lengthPlan = x, spotLength = y })
                .Where(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.lengthPlan.plan.program_air_time >= weekStartDate
                && recommendedPlanSpotsDoneDb.lengthPlan.plan.program_air_time <= weekEndDate
                && recommendedPlanSpotsDoneDb.lengthPlan.detail.recommended_plan_id == planId
                && recommendedPlanSpotsDoneDb.lengthPlan.detail.spot_exceptions_recommended_plan_done_decisions.Select(x => x.synced_at).FirstOrDefault() == null)
                .GroupJoin(
                    context.stations,
                    recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.lengthPlan.plan.station_legacy_call_letters,
                    stationDb => stationDb.legacy_call_letters,
                    (recommendedPlanSpotsDoneDb, stationDb) => new { recommendedPlansDone = recommendedPlanSpotsDoneDb, Station = stationDb.FirstOrDefault() })
                .ToList();

                var planSpotsDone = planDetailsDone.Select(x =>
                {
                    return new SpotExceptionsRecommendedPlanSpotsDto
                    {
                        Id = x.recommendedPlansDone.lengthPlan.plan.id,
                        EstimateId = x.recommendedPlansDone.lengthPlan.plan.estimate_id.Value,
                        IsciName = x.recommendedPlansDone.lengthPlan.plan.house_isci,
                        ProgramAirTime = x.recommendedPlansDone.lengthPlan.plan.program_air_time,
                        RecommendedPlanName = x.recommendedPlansDone.lengthPlan.detail.plan.name,
                        PlanId = x.recommendedPlansDone.lengthPlan.detail.recommended_plan_id,
                        Impressions = x.recommendedPlansDone.lengthPlan.detail.spot_delivered_impressions,
                        SpotLength = x.recommendedPlansDone.spotLength.length,
                        ProgramName = x.recommendedPlansDone.lengthPlan.plan.program_name,
                        InventorySource = x.recommendedPlansDone.lengthPlan.plan.inventory_source,
                        Affiliate = x.recommendedPlansDone.lengthPlan.plan.affiliate,
                        MarketName = x.Station?.market?.geography_name,
                        Station = x.recommendedPlansDone.lengthPlan.plan.station_legacy_call_letters
                    };

                }).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Spots Queued");
                return planSpotsDone;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsDto>> GetRecommendedPlanSpotsSynced(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Spots Synced");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var planDetailsDone = context.spot_exceptions_recommended_plans_done
                .Join
                (
                    context.spot_exceptions_recommended_plan_details_done
                    .Where(x => x.spot_exceptions_recommended_plan_done_decisions.Any()),
                    x => x.id, y => y.spot_exceptions_recommended_plan_done_id,
                    (x, y) => new { plan = x, detail = y }
                )
                .Join(context.spot_lengths, x => x.plan.spot_length_id, y => y.id, (x, y) => new { lengthPlan = x, spotLength = y })
                .Where(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.lengthPlan.plan.program_air_time >= weekStartDate
                && recommendedPlanSpotsDoneDb.lengthPlan.plan.program_air_time <= weekEndDate
                && recommendedPlanSpotsDoneDb.lengthPlan.detail.recommended_plan_id == planId
                && recommendedPlanSpotsDoneDb.lengthPlan.detail.spot_exceptions_recommended_plan_done_decisions.Select(x => x.synced_at).FirstOrDefault() != null)
                .GroupJoin(
                    context.stations,
                    recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.lengthPlan.plan.station_legacy_call_letters,
                    stationDb => stationDb.legacy_call_letters,
                    (recommendedPlanSpotsDoneDb, stationDb) => new { recommendedPlansDone = recommendedPlanSpotsDoneDb, Station = stationDb.FirstOrDefault() })
                .ToList();

                var planSpotsDone = planDetailsDone.Select(x =>
                {
                    var decision = x.recommendedPlansDone.lengthPlan.detail.spot_exceptions_recommended_plan_done_decisions.First();
                    return new SpotExceptionsRecommendedPlanSpotsDto
                    {
                        Id = x.recommendedPlansDone.lengthPlan.plan.id,
                        EstimateId = x.recommendedPlansDone.lengthPlan.plan.estimate_id,
                        IsciName = x.recommendedPlansDone.lengthPlan.plan.house_isci,
                        ProgramAirTime = x.recommendedPlansDone.lengthPlan.plan.program_air_time,
                        PlanId = x.recommendedPlansDone.lengthPlan.detail.recommended_plan_id,
                        RecommendedPlanName = x.recommendedPlansDone.lengthPlan.detail.plan.name,
                        Impressions = x.recommendedPlansDone.lengthPlan.detail.spot_delivered_impressions,
                        SyncedTimestamp = decision.synced_at,
                        SpotLength = x.recommendedPlansDone.spotLength.length,
                        ProgramName = x.recommendedPlansDone.lengthPlan.plan.program_name,
                        InventorySource = x.recommendedPlansDone.lengthPlan.plan.inventory_source,
                        Affiliate = x.recommendedPlansDone.lengthPlan.plan.affiliate,
                        MarketName = x.Station?.market?.geography_name,
                        Station = x.recommendedPlansDone.lengthPlan.plan.station_legacy_call_letters
                    };

                }).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Spots Synced");
                return planSpotsDone;
            }));
        }

        /// <inheritdoc />
        public Task<List<string>> GetRecommendedPlanMarketFiltersAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Filters: Markets");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var marketFilters = context.spot_exceptions_recommended_plans
                    .Join(context.stations, x => x.station_legacy_call_letters, y => y.legacy_call_letters, (x, y) => new { plan = x, station = y })
                    .Where(x => x.plan.program_air_time >= weekStartDate && x.plan.program_air_time <= weekEndDate)
                    .Select(x => x.station.market.geography_name ?? "Unknown")
                    .Union(context.spot_exceptions_recommended_plans_done
                    .Join(context.stations, x => x.station_legacy_call_letters, y => y.legacy_call_letters, (x, y) => new { plan = x, station = y })
                    .Where(x => x.plan.program_air_time >= weekStartDate && x.plan.program_air_time <= weekEndDate)
                    .Select(x => x.station.market.geography_name ?? "Unknown"))
                    .ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Filters: Markets");
                return marketFilters;
            }));
        }

        /// <inheritdoc />
        public Task<List<string>> GetRecommendedPlanLegacyCallLetterFiltersAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Filters: Legacy Call Letters");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var legacyCallLetterFilters = context.spot_exceptions_recommended_plans
                    .Where(x => x.program_air_time >= weekStartDate && x.program_air_time <= weekEndDate)
                    .Select(x => x.station_legacy_call_letters)
                    .Union(context.spot_exceptions_recommended_plans_done
                    .Where(x => x.program_air_time >= weekStartDate && x.program_air_time <= weekEndDate)
                    .Select(x => x.station_legacy_call_letters))
                    .ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Filters: Legacy Call Letters");
                return legacyCallLetterFilters;
            }));
        }

        /// <inheritdoc />
        public Task<List<string>> GetRecommendedPlanInventorySourceFiltersAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Filters: Inventory Sources");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var inventorySourceFilters = context.spot_exceptions_recommended_plans
                    .Where(x => x.program_air_time >= weekStartDate && x.program_air_time <= weekEndDate)
                    .Select(x => x.inventory_source)
                    .Union(context.spot_exceptions_recommended_plans_done
                    .Where(x => x.program_air_time >= weekStartDate && x.program_air_time <= weekEndDate)
                    .Select(x => x.inventory_source))
                    .ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Filters: Inventory Sources");
                return inventorySourceFilters;
            }));
        }

        /// <inheritdoc />
        public Task<SpotExceptionsRecommendedPlanSpotsToDoDto> GetRecommendedPlanDetailsToDoById(int detailsId)
        {
            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Details ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanDetailsToDoEntity = context.spot_exceptions_recommended_plans
                    .Where(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.id == detailsId)
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details)
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .SingleOrDefault();

                if (recommendedPlanDetailsToDoEntity == null)
                {
                    return null;
                }

                var recommendedPlanDetailsToDo = _MapRecommendedPlanSpotsToDoToDto(recommendedPlanDetailsToDoEntity);

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details ToDo");
                return recommendedPlanDetailsToDo;
            }));
        }

        /// <inheritdoc />
        public Task<SpotExceptionsRecommendedPlanSpotsDoneDto> GetRecommendedPlanDetailsDoneById(int detailsId)
        {
            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Details Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanDetailsDoneEntity = context.spot_exceptions_recommended_plans_done
                    .Where(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.id == detailsId)
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done)
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.spot_exceptions_recommended_plan_done_decisions))
                    .SingleOrDefault();

                if (recommendedPlanDetailsDoneEntity == null)
                {
                    return null;
                }

                var recommendedPlanDetailsDone = _MapRecommendedPlanSpotsDoneToDto(recommendedPlanDetailsDoneEntity);

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details Done");
                return recommendedPlanDetailsDone;
            }));
        }

        /// <inheritdoc />
        public Task<List<Guid>> GetRecommendedPlanAdvertisersToDoAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<Guid> recommendedPlanAdvertiserMasterIdsPerWeek = null;

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Advertisers ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                recommendedPlanAdvertiserMasterIdsPerWeek = context.spot_exceptions_recommended_plan_details
                    .Where(recommendedPlanAdverisersToDoDb => recommendedPlanAdverisersToDoDb.spot_exceptions_recommended_plans.program_air_time >= weekStartDate && recommendedPlanAdverisersToDoDb.spot_exceptions_recommended_plans.program_air_time <= weekEndDate)
                    .Select(recommendedPlanAdverisersToDoDb => recommendedPlanAdverisersToDoDb.plan.campaign.advertiser_master_id ?? default).ToList();


                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Advertisers ToDo == Retrieved Count: {recommendedPlanAdvertiserMasterIdsPerWeek.Count()}");
                return recommendedPlanAdvertiserMasterIdsPerWeek;
            }));
        }

        /// <inheritdoc />
        public Task<List<Guid>> GetRecommendedPlanAdvertisersDoneAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            List<Guid> recommendedPlanAdvertiserMasterIdsPerWeek = null;

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Advertisers Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                recommendedPlanAdvertiserMasterIdsPerWeek = context.spot_exceptions_recommended_plan_details
                .Where(recommendedPlanAdverisersDoneDb => recommendedPlanAdverisersDoneDb.spot_exceptions_recommended_plans.program_air_time >= weekStartDate && recommendedPlanAdverisersDoneDb.spot_exceptions_recommended_plans.program_air_time <= weekEndDate)
                .Select(recommendedPlanAdverisersDoneDb => recommendedPlanAdverisersDoneDb.plan.campaign.advertiser_master_id ?? default).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Advertisers Done == Retrieved Count: {recommendedPlanAdvertiserMasterIdsPerWeek.Count()}");
                return recommendedPlanAdvertiserMasterIdsPerWeek;
            }));
        }

        /// <inheritdoc />
        public Task<List<string>> GetRecommendedPlanStationsToDoAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Stations ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanStationsToDo = context.spot_exceptions_recommended_plans
                                            .Where(recommendedPlanStationsToDoDb => recommendedPlanStationsToDoDb.program_air_time >= weekStartDate && recommendedPlanStationsToDoDb.program_air_time <= weekEndDate)
                                            .Select(rps => rps.station_legacy_call_letters ?? "Unknown")
                                            .ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Stations ToDo == Retrieved Count: {recommendedPlanStationsToDo.Count()}");
                return recommendedPlanStationsToDo;
            }));
        }

        /// <inheritdoc />
        public Task<List<string>> GetRecommendedPlanStationsDoneAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Stations Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanStationsDone = context.spot_exceptions_recommended_plans_done
                                            .Where(recommendedPlanStationsDoneDb => recommendedPlanStationsDoneDb.program_air_time >= weekStartDate && recommendedPlanStationsDoneDb.program_air_time <= weekEndDate)
                                            .Select(rps => rps.station_legacy_call_letters ?? "Unknown")
                                            .ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Stations ToDo == Retrieved Count: {recommendedPlanStationsDone.Count()}");
                return recommendedPlanStationsDone;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsToDoDto>> GetRecommendedPlanSpotsToDoByIds(List<int?> todoId)
        {
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var foundRecommendedPlanTodoEntities = context.spot_exceptions_recommended_plans
                    .Where(x => todoId.Contains(x.id))
                    .ToList();

                var recommendedPlanToDo = foundRecommendedPlanTodoEntities.Select(foundRecommendedPlanTodoEntity => _MapRecommendedPlanSpotsToDoToDto(foundRecommendedPlanTodoEntity)).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details ToDo");
                return recommendedPlanToDo;
            }));
        }

        /// <inheritdoc />
        public void AddRecommendedPlanToDone(List<SpotExceptionsRecommendedPlanSpotsDoneDto> doneRecommendedPlansToAdd, int recommendedPlanId, string userName, DateTime currentDate)
        {
            _InReadUncommitedTransaction(context =>
            {
                var recommendedPlanDoneEntity = doneRecommendedPlansToAdd.Select(doneRecommendedPlanToAdd => new spot_exceptions_recommended_plans_done
                {
                    spot_unique_hash_external = doneRecommendedPlanToAdd.SpotUniqueHashExternal,
                    ambiguity_code = doneRecommendedPlanToAdd.AmbiguityCode,
                    execution_id_external = doneRecommendedPlanToAdd.ExecutionIdExternal,
                    estimate_id = doneRecommendedPlanToAdd.EstimateId,
                    inventory_source = doneRecommendedPlanToAdd.InventorySource,
                    house_isci = doneRecommendedPlanToAdd.HouseIsci,
                    client_isci = doneRecommendedPlanToAdd.ClientIsci,
                    spot_length_id = doneRecommendedPlanToAdd.SpotLengthId,
                    program_air_time = doneRecommendedPlanToAdd.ProgramAirTime,
                    station_legacy_call_letters = doneRecommendedPlanToAdd.StationLegacyCallLetters,
                    affiliate = doneRecommendedPlanToAdd.Affiliate,
                    market_code = doneRecommendedPlanToAdd.MarketCode,
                    market_rank = doneRecommendedPlanToAdd.MarketRank,
                    program_name = doneRecommendedPlanToAdd.ProgramName,
                    program_genre = doneRecommendedPlanToAdd.ProgramGenre,
                    ingested_by = doneRecommendedPlanToAdd.IngestedBy,
                    ingested_at = doneRecommendedPlanToAdd.IngestedAt,
                    ingested_media_week_id = doneRecommendedPlanToAdd.IngestedMediaWeekId,
                    spot_exceptions_recommended_plan_details_done = doneRecommendedPlanToAdd.SpotExceptionsRecommendedPlanDetailsDone.Select(recommendedPlanDetailDoneDb =>
                    {
                        var recommendedPlanDetailDone = new spot_exceptions_recommended_plan_details_done
                        {
                            recommended_plan_id = recommendedPlanDetailDoneDb.RecommendedPlanId,
                            execution_trace_id = recommendedPlanDetailDoneDb.ExecutionTraceId,
                            rate = recommendedPlanDetailDoneDb.Rate,
                            audience_name = recommendedPlanDetailDoneDb.AudienceName,
                            contracted_impressions = recommendedPlanDetailDoneDb.ContractedImpressions,
                            delivered_impressions = recommendedPlanDetailDoneDb.DeliveredImpressions,
                            is_recommended_plan = recommendedPlanDetailDoneDb.IsRecommendedPlan,
                            plan_clearance_percentage = recommendedPlanDetailDoneDb.PlanClearancePercentage,
                            daypart_code = recommendedPlanDetailDoneDb.DaypartCode,
                            start_time = recommendedPlanDetailDoneDb.StartTime,
                            end_time = recommendedPlanDetailDoneDb.EndTime,
                            monday = recommendedPlanDetailDoneDb.Monday,
                            tuesday = recommendedPlanDetailDoneDb.Tuesday,
                            wednesday = recommendedPlanDetailDoneDb.Wednesday,
                            thursday = recommendedPlanDetailDoneDb.Thursday,
                            friday = recommendedPlanDetailDoneDb.Friday,
                            saturday = recommendedPlanDetailDoneDb.Saturday,
                            sunday = recommendedPlanDetailDoneDb.Sunday,
                            spot_delivered_impressions = recommendedPlanDetailDoneDb.SpotDeliveredImpressions,
                            plan_total_contracted_impressions = recommendedPlanDetailDoneDb.PlanTotalContractedImpressions,
                            plan_total_delivered_impressions = recommendedPlanDetailDoneDb.PlanTotalDeliveredImpressions,
                            ingested_media_week_id = recommendedPlanDetailDoneDb.IngestedMediaWeekId,
                            ingested_by = recommendedPlanDetailDoneDb.IngestedBy,
                            ingested_at = recommendedPlanDetailDoneDb.IngestedAt,
                            spot_unique_hash_external = recommendedPlanDetailDoneDb.SpotUniqueHashExternal,
                            execution_id_external = recommendedPlanDetailDoneDb.ExecutionIdExternal
                        };

                        if (recommendedPlanDetailDoneDb.RecommendedPlanId == recommendedPlanId)
                        {
                            recommendedPlanDetailDone.spot_exceptions_recommended_plan_done_decisions =
                            new List<spot_exceptions_recommended_plan_done_decisions>
                            {
                                new spot_exceptions_recommended_plan_done_decisions
                                {
                                    spot_exceptions_recommended_plan_details_done_id = recommendedPlanDetailDoneDb.Id,
                                    decided_by = userName,
                                    decided_at = currentDate
                                }
                            };
                        }

                        return recommendedPlanDetailDone;
                    }).ToList()

                });

                context.spot_exceptions_recommended_plans_done.AddRange(recommendedPlanDoneEntity);
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void DeleteRecommendedPlanFromToDo(List<SpotExceptionsRecommendedPlanSpotsToDoDto> existingRecommendedPlansToDo)
        {
            _InReadUncommitedTransaction(context =>
            {
                foreach (var plansToDelete in existingRecommendedPlansToDo)
                {
                    var foundPlansToDelete = context.spot_exceptions_recommended_plans
                    .Where(x => x.id == plansToDelete.Id)
                    .Single();

                    context.spot_exceptions_recommended_plans.Remove(foundPlansToDelete);
                }

                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsDoneDto>> GetSpotExceptionPlanDetailsWithDecision(List<int> spotExceptionsId)
        {
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var details = new List<SpotExceptionsRecommendedPlanSpotsDoneDto>();

                var planDetails = context.spot_exceptions_recommended_plans_done
                    .Where(x => spotExceptionsId.Contains(x.id))
                    .Include(x => x.spot_exceptions_recommended_plan_details_done)
                    .Include(x => x.spot_exceptions_recommended_plan_details_done.Select(y => y.spot_exceptions_recommended_plan_done_decisions))
                    .ToList();                

                if (planDetails != null)
                {
                    details = planDetails.Select(planDetail => _MapRecommendedPlanSpotsDoneToDto(planDetail)).ToList();
                }

                return details;
            }));
        }

        /// <inheritdoc />
        public void UpdateRecommendedPlanDoneDecisionsAsync(List<SpotExceptionsRecommendedPlanSpotDecisionsDoneDto> decisionsToAdd)
        {
            _InReadUncommitedTransaction(context =>
            {
                foreach (var decisionToAdd in decisionsToAdd)
                {
                    context.spot_exceptions_recommended_plan_done_decisions.AddOrUpdate(new spot_exceptions_recommended_plan_done_decisions
                    {
                        id = decisionToAdd.Id,
                        spot_exceptions_recommended_plan_details_done_id = decisionToAdd.SpotExceptionsRecommendedPlanDetailsDoneId,
                        decided_by = decisionToAdd.DecidedBy,
                        decided_at = decisionToAdd.DecidedAt,
                        synced_by = null,
                        synced_at = null
                    });
                }

                context.SaveChanges();
            });
        }

        private SpotExceptionsRecommendedPlanSpotsToDoDto _MapRecommendedPlanSpotsToDoToDto(spot_exceptions_recommended_plans recommendedPlanSpotsToDoEntity)
        {
            var recommendedPlanSpotsToDo = new SpotExceptionsRecommendedPlanSpotsToDoDto
            {
                Id = recommendedPlanSpotsToDoEntity.id,
                SpotUniqueHashExternal = recommendedPlanSpotsToDoEntity.spot_unique_hash_external,
                AmbiguityCode = recommendedPlanSpotsToDoEntity.ambiguity_code,
                ExecutionIdExternal = recommendedPlanSpotsToDoEntity.execution_id_external,
                EstimateId = recommendedPlanSpotsToDoEntity.estimate_id,
                InventorySource = recommendedPlanSpotsToDoEntity.inventory_source,
                HouseIsci = recommendedPlanSpotsToDoEntity.house_isci,
                ClientIsci = recommendedPlanSpotsToDoEntity.client_isci,
                SpotLengthId = recommendedPlanSpotsToDoEntity.spot_length_id,
                ProgramAirTime = recommendedPlanSpotsToDoEntity.program_air_time,
                StationLegacyCallLetters = recommendedPlanSpotsToDoEntity.station_legacy_call_letters,
                Affiliate = recommendedPlanSpotsToDoEntity.affiliate,
                MarketCode = recommendedPlanSpotsToDoEntity.market_code,
                MarketRank = recommendedPlanSpotsToDoEntity.market_rank,
                ProgramName = recommendedPlanSpotsToDoEntity.program_name,
                ProgramGenre = recommendedPlanSpotsToDoEntity.program_genre,
                IngestedBy = recommendedPlanSpotsToDoEntity.ingested_by,
                IngestedAt = recommendedPlanSpotsToDoEntity.ingested_at,
                IngestedMediaWeekId = recommendedPlanSpotsToDoEntity.ingested_media_week_id,
                SpotLength = _MapSpotLengthToDto(recommendedPlanSpotsToDoEntity.spot_lengths),
                SpotExceptionsRecommendedPlanDetailsToDo = recommendedPlanSpotsToDoEntity.spot_exceptions_recommended_plan_details.Select(recommendedPlanSpotDetailsToDoDb =>
                {
                    var recommendedPlan = recommendedPlanSpotDetailsToDoDb.plan;
                    var recommendedPlanVersion = recommendedPlan.plan_versions.Single(planVersion => planVersion.id == recommendedPlan.latest_version_id);
                    var recommendedPlanSpotDetailsToDo = new SpotExceptionsRecommendedPlanDetailsToDoDto
                    {
                        Id = recommendedPlanSpotDetailsToDoDb.id,
                        SpotExceptionsRecommendedPlanId = recommendedPlanSpotDetailsToDoDb.spot_exceptions_recommended_plan_id,
                        RecommendedPlanId = recommendedPlanSpotDetailsToDoDb.recommended_plan_id,
                        ExecutionTraceId = recommendedPlanSpotDetailsToDoDb.execution_trace_id,
                        Rate = recommendedPlanSpotDetailsToDoDb.rate,
                        AudienceName = recommendedPlanSpotDetailsToDoDb.audience_name,
                        ContractedImpressions = recommendedPlanSpotDetailsToDoDb.contracted_impressions,
                        DeliveredImpressions = recommendedPlanSpotDetailsToDoDb.delivered_impressions,
                        IsRecommendedPlan = recommendedPlanSpotDetailsToDoDb.is_recommended_plan,
                        PlanClearancePercentage = recommendedPlanSpotDetailsToDoDb.plan_clearance_percentage,
                        DaypartCode = recommendedPlanSpotDetailsToDoDb.daypart_code,
                        StartTime = recommendedPlanSpotDetailsToDoDb.start_time,
                        EndTime = recommendedPlanSpotDetailsToDoDb.end_time,
                        Monday = recommendedPlanSpotDetailsToDoDb.monday,
                        Tuesday = recommendedPlanSpotDetailsToDoDb.tuesday,
                        Wednesday = recommendedPlanSpotDetailsToDoDb.wednesday,
                        Thursday = recommendedPlanSpotDetailsToDoDb.thursday,
                        Friday = recommendedPlanSpotDetailsToDoDb.friday,
                        Saturday = recommendedPlanSpotDetailsToDoDb.saturday,
                        Sunday = recommendedPlanSpotDetailsToDoDb.sunday,
                        SpotDeliveredImpressions = recommendedPlanSpotDetailsToDoDb.spot_delivered_impressions,
                        PlanTotalContractedImpressions = recommendedPlanSpotDetailsToDoDb.plan_total_contracted_impressions,
                        PlanTotalDeliveredImpressions = recommendedPlanSpotDetailsToDoDb.plan_total_delivered_impressions,
                        IngestedMediaWeekId = recommendedPlanSpotDetailsToDoDb.ingested_media_week_id,
                        IngestedBy = recommendedPlanSpotDetailsToDoDb.ingested_by,
                        IngestedAt = recommendedPlanSpotDetailsToDoDb.ingested_at,
                        SpotUniqueHashExternal = recommendedPlanSpotDetailsToDoDb.spot_unique_hash_external,
                        ExecutionIdExternal = recommendedPlanSpotDetailsToDoDb.execution_id_external,
                        RecommendedPlanDetail = new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            Id = recommendedPlanSpotDetailsToDoDb.recommended_plan_id,
                            Name = recommendedPlan.name,
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AdvertiserMasterId = recommendedPlan.campaign.advertiser_master_id,
                            ProductMasterId = recommendedPlan.product_master_id
                        }
                    };
                    return recommendedPlanSpotDetailsToDo;
                }).ToList()
            };
            return recommendedPlanSpotsToDo;
        }

        private SpotExceptionsRecommendedPlanSpotsDoneDto _MapRecommendedPlanSpotsDoneToDto(spot_exceptions_recommended_plans_done recommendedPlanSpotsDoneEntity)
        {
            var recommendedPlanSpotsDone = new SpotExceptionsRecommendedPlanSpotsDoneDto
            {
                Id = recommendedPlanSpotsDoneEntity.id,
                SpotUniqueHashExternal = recommendedPlanSpotsDoneEntity.spot_unique_hash_external,
                AmbiguityCode = recommendedPlanSpotsDoneEntity.ambiguity_code,
                ExecutionIdExternal = recommendedPlanSpotsDoneEntity.execution_id_external,
                EstimateId = recommendedPlanSpotsDoneEntity.estimate_id,
                InventorySource = recommendedPlanSpotsDoneEntity.inventory_source,
                HouseIsci = recommendedPlanSpotsDoneEntity.house_isci,
                ClientIsci = recommendedPlanSpotsDoneEntity.client_isci,
                SpotLengthId = recommendedPlanSpotsDoneEntity.spot_length_id,
                ProgramAirTime = recommendedPlanSpotsDoneEntity.program_air_time,
                StationLegacyCallLetters = recommendedPlanSpotsDoneEntity.station_legacy_call_letters,
                Affiliate = recommendedPlanSpotsDoneEntity.affiliate,
                MarketCode = recommendedPlanSpotsDoneEntity.market_code,
                MarketRank = recommendedPlanSpotsDoneEntity.market_rank,
                ProgramName = recommendedPlanSpotsDoneEntity.program_name,
                ProgramGenre = recommendedPlanSpotsDoneEntity.program_genre,
                IngestedBy = recommendedPlanSpotsDoneEntity.ingested_by,
                IngestedAt = recommendedPlanSpotsDoneEntity.ingested_at,
                SpotLength = _MapSpotLengthToDto(recommendedPlanSpotsDoneEntity.spot_lengths),
                SpotExceptionsRecommendedPlanDetailsDone = recommendedPlanSpotsDoneEntity.spot_exceptions_recommended_plan_details_done.Select(recommendedPlanSpotDetailsDoneDb =>
                {
                    var recommendedPlan = recommendedPlanSpotDetailsDoneDb.plan;
                    var recommendedPlanVersion = recommendedPlan.plan_versions.Single(planVersion => planVersion.id == recommendedPlan.latest_version_id);
                    var recommendedPlanSpotDetailsDone = new SpotExceptionsRecommendedPlanDetailsDoneDto
                    {
                        Id = recommendedPlanSpotDetailsDoneDb.id,
                        SpotExceptionsRecommendedPlanId = recommendedPlanSpotDetailsDoneDb.spot_exceptions_recommended_plan_done_id,
                        RecommendedPlanId = recommendedPlanSpotDetailsDoneDb.recommended_plan_id,
                        ExecutionTraceId = recommendedPlanSpotDetailsDoneDb.execution_trace_id,
                        Rate = recommendedPlanSpotDetailsDoneDb.rate,
                        AudienceName = recommendedPlanSpotDetailsDoneDb.audience_name,
                        ContractedImpressions = recommendedPlanSpotDetailsDoneDb.contracted_impressions,
                        DeliveredImpressions = recommendedPlanSpotDetailsDoneDb.delivered_impressions,
                        IsRecommendedPlan = recommendedPlanSpotDetailsDoneDb.is_recommended_plan,
                        PlanClearancePercentage = recommendedPlanSpotDetailsDoneDb.plan_clearance_percentage,
                        DaypartCode = recommendedPlanSpotDetailsDoneDb.daypart_code,
                        StartTime = recommendedPlanSpotDetailsDoneDb.start_time,
                        EndTime = recommendedPlanSpotDetailsDoneDb.end_time,
                        Monday = recommendedPlanSpotDetailsDoneDb.monday,
                        Tuesday = recommendedPlanSpotDetailsDoneDb.tuesday,
                        Wednesday = recommendedPlanSpotDetailsDoneDb.wednesday,
                        Thursday = recommendedPlanSpotDetailsDoneDb.thursday,
                        Friday = recommendedPlanSpotDetailsDoneDb.friday,
                        Saturday = recommendedPlanSpotDetailsDoneDb.saturday,
                        Sunday = recommendedPlanSpotDetailsDoneDb.sunday,
                        SpotDeliveredImpressions = recommendedPlanSpotDetailsDoneDb.spot_delivered_impressions,
                        PlanTotalContractedImpressions = recommendedPlanSpotDetailsDoneDb.plan_total_contracted_impressions,
                        PlanTotalDeliveredImpressions = recommendedPlanSpotDetailsDoneDb.plan_total_delivered_impressions,
                        IngestedMediaWeekId = recommendedPlanSpotDetailsDoneDb.ingested_media_week_id,
                        IngestedBy = recommendedPlanSpotDetailsDoneDb.ingested_by,
                        IngestedAt = recommendedPlanSpotDetailsDoneDb.ingested_at,
                        SpotUniqueHashExternal = recommendedPlanSpotDetailsDoneDb.spot_unique_hash_external,
                        ExecutionIdExternal = recommendedPlanSpotDetailsDoneDb.execution_id_external,
                        RecommendedPlanDetail = new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            Id = recommendedPlanSpotDetailsDoneDb.recommended_plan_id,
                            Name = recommendedPlan.name,
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AdvertiserMasterId = recommendedPlan.campaign.advertiser_master_id,
                            ProductMasterId = recommendedPlan.product_master_id
                        },
                        SpotExceptionsRecommendedPlanDoneDecisions = recommendedPlanSpotDetailsDoneDb.spot_exceptions_recommended_plan_done_decisions.Select(recommendedPlanSpotDecisionsDoneDb => new SpotExceptionsRecommendedPlanSpotDecisionsDoneDto
                        {
                            Id = recommendedPlanSpotDecisionsDoneDb.id,
                            SpotExceptionsRecommendedPlanDetailsDoneId = recommendedPlanSpotDecisionsDoneDb.spot_exceptions_recommended_plan_details_done_id,
                            DecidedBy = recommendedPlanSpotDecisionsDoneDb.decided_by,
                            DecidedAt = recommendedPlanSpotDecisionsDoneDb.decided_at,
                            SyncedBy = recommendedPlanSpotDecisionsDoneDb.synced_by,
                            SyncedAt = recommendedPlanSpotDecisionsDoneDb.synced_at
                        }).SingleOrDefault()
                    };
                    return recommendedPlanSpotDetailsDone;
                }).ToList()
            };
            return recommendedPlanSpotsDone;
        }

        private SpotLengthDto _MapSpotLengthToDto(spot_lengths spotLengthEntity)
        {
            if (spotLengthEntity == null)
            {
                return null;
            }

            var spotLength = new SpotLengthDto
            {
                Id = spotLengthEntity.id,
                Length = spotLengthEntity.length
            };

            return spotLength;
        }
    }
}
