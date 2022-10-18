using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using static log4net.Appender.RollingFileAppender;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsRecommendedPlanRepository : IDataRepository
    {
        /// <summary>
        /// Gets the recommended plans to do asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsToDoDto>> GetRecommendedPlansToDoAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plans done asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsDoneDto>> GetRecommendedPlansDoneAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan spots to do.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsToDoDto>> GetRecommendedPlanSpotsToDo(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the recommended plan spots done.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsRecommendedPlanSpotsDoneDto>> GetRecommendedPlanSpotsDone(int planId, DateTime weekStartDate, DateTime weekEndDate);

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
        /// Gets the recommended plan details to do by identifier.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="recommendedPlanId">The recommended plan identifier.</param>
        /// <returns>
        /// </returns>
        Task<SpotExceptionsRecommendedPlanSpotsToDoDto> GetRecommendedPlanDetailsToDoByRecommendedPlanId(int planId, int recommendedPlanId);

        /// <summary>
        /// Gets the recommended plan details done by recommended plan identifier.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="recommendedPlanId">The recommended plan identifier.</param>
        /// <returns></returns>
        Task<SpotExceptionsRecommendedPlanSpotsDoneDto> GetRecommendedPlanDetailsDoneByRecommendedPlanId(int planId, int recommendedPlanId);

        Task<bool> SaveRecommendedPlanToDoDecisionsAsync(SpotExceptionsRecommendedPlanSaveDto planToSave, SpotExceptionsRecommendedPlanSpotsToDoDto recommendedPlansToDo, string userName, DateTime decided_at);

        /// <summary>
        /// Saves the recommended plan done decisions asynchronous.
        /// </summary>
        /// <param name="recommendedPlanDoneDecision">The recommended plan done decision.</param>
        /// <returns>
        /// </returns>
        Task<bool> SaveRecommendedPlanDoneDecisionsAsync(SpotExceptionsRecommendedPlanSpotDecisionsDoneDto recommendedPlanDoneDecision);

        /// <summary>
        /// Gets the name of the market.
        /// </summary>
        /// <param name="marketCode">The market code.</param>
        /// <returns>
        /// </returns>
        string GetMarketName(int marketCode);
    }

    public class SpotExceptionsRecommendedPlanRepository : BroadcastRepositoryBase, ISpotExceptionsRecommendedPlanRepository
    {
        public SpotExceptionsRecommendedPlanRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }


        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsToDoDto>> GetRecommendedPlansToDoAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plans ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanToDoEntities = context.spot_exceptions_recommended_plans
                    .Where(recommendedPlanToDoDb => recommendedPlanToDoDb.program_air_time >= weekStartDate && recommendedPlanToDoDb.program_air_time <= weekEndDate)
                    .Include(recommendedPlanToDoDb => recommendedPlanToDoDb.spot_exceptions_recommended_plan_details)
                    .Include(recommendedPlanToDoDb => recommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(recommendedPlanToDoDb => recommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(recommendedPlanToDoDb => recommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanToDoDb => recommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanToDoDb => recommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        recommendedPlanToDoDb => recommendedPlanToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (recommendedPlanToDoDb, stationDb) => new { SpotExceptionsRecommendedPlan = recommendedPlanToDoDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var recommendedPlansDone = recommendedPlanToDoEntities.Select(recommendedPlanToDoEntity => _MapRecommendedPlanSpotsToDoToDto(recommendedPlanToDoEntity.SpotExceptionsRecommendedPlan)).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plans ToDo == Retrieved Count: {recommendedPlansDone.Count()}");
                return recommendedPlansDone;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsDoneDto>> GetRecommendedPlansDoneAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plans Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanDoneEntities = context.spot_exceptions_recommended_plans_done
                    .Where(recommendedPlanDoneDb => recommendedPlanDoneDb.program_air_time >= weekStartDate && recommendedPlanDoneDb.program_air_time <= weekEndDate)
                    .Include(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done)
                    .Include(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Include(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(recommendedPlanDoneDb => recommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.spot_exceptions_recommended_plan_done_decisions))
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDoneDb, stationDb) => new { SpotExceptionsRecommendedPlanDone = spotExceptionsRecommendedPlanDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var recommendedPlansDone = recommendedPlanDoneEntities.Select(recommendedPlanDoneEntity => _MapRecommendedPlanSpotsDoneToDto(recommendedPlanDoneEntity.SpotExceptionsRecommendedPlanDone)).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plans Done == Retrieved Count: {recommendedPlansDone.Count()}");
                return recommendedPlansDone;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsToDoDto>> GetRecommendedPlanSpotsToDo(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Spots ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanSpotsToDoEntities = context.spot_exceptions_recommended_plans
                    .Where(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.program_air_time >= weekStartDate && recommendedPlanSpotsToDoDb.program_air_time <= weekEndDate)
                    .Include(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.spot_exceptions_recommended_plan_details).Where(p => p.spot_exceptions_recommended_plan_details.Any(x => x.recommended_plan_id == planId))
                    .Include(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        recommendedPlanSpotsToDoDb => recommendedPlanSpotsToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (recommendedPlanSpotsToDoDb, stationDb) => new { SpotExceptionsRecommendedPlan = recommendedPlanSpotsToDoDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var recommendedPlanSpotsToDo = recommendedPlanSpotsToDoEntities.Select(recommendedPlanSpotsToDoEntity => _MapRecommendedPlanSpotsToDoToDto(recommendedPlanSpotsToDoEntity.SpotExceptionsRecommendedPlan)).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Spots ToDo");
                return recommendedPlanSpotsToDo;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsRecommendedPlanSpotsDoneDto>> GetRecommendedPlanSpotsDone(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Spots Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanSpotsDoneEntities = context.spot_exceptions_recommended_plans_done
                    .Where(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.program_air_time >= weekStartDate && recommendedPlanSpotsDoneDb.program_air_time <= weekEndDate)
                    .Include(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.spot_exceptions_recommended_plan_details_done).Where(p => p.spot_exceptions_recommended_plan_details_done.Any(x => x.recommended_plan_id == planId))
                    .Include(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Include(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.spot_exceptions_recommended_plan_done_decisions))
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        recommendedPlanSpotsDoneDb => recommendedPlanSpotsDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (recommendedPlanSpotsDoneDb, stationDb) => new { SpotExceptionsRecommendedPlanDone = recommendedPlanSpotsDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var recommendedPlansDone = recommendedPlanSpotsDoneEntities.Select(recommendedPlanSpotsDoneEntity => _MapRecommendedPlanSpotsDoneToDto(recommendedPlanSpotsDoneEntity.SpotExceptionsRecommendedPlanDone)).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Spots Done");
                return recommendedPlansDone;
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
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (recommendedPlanDetailsToDoDb, stationDb) => new { SpotExceptionsRecommendedPlan = recommendedPlanDetailsToDoDb, Station = stationDb.FirstOrDefault() })
                    .SingleOrDefault();

                if (recommendedPlanDetailsToDoEntity == null)
                {
                    return null;
                }

                var recommendedPlanDetailsToDo = _MapRecommendedPlanSpotsToDoToDto(recommendedPlanDetailsToDoEntity.SpotExceptionsRecommendedPlan);

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
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanDb, Station = stationDb.FirstOrDefault() })
                    .SingleOrDefault();

                if (recommendedPlanDetailsDoneEntity == null)
                {
                    return null;
                }

                var recommendedPlanDetailsDone = _MapRecommendedPlanSpotsDoneToDto(recommendedPlanDetailsDoneEntity.SpotExceptionsRecommendedPlan);

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
                recommendedPlanAdvertiserMasterIdsPerWeek = context.spot_exceptions_recommended_plans
                    .Where(recommendedPlanAdverisersToDoDb => recommendedPlanAdverisersToDoDb.program_air_time >= weekStartDate && recommendedPlanAdverisersToDoDb.program_air_time <= weekEndDate)
                    .Include(recommendedPlanAdverisersToDoDb => recommendedPlanAdverisersToDoDb.spot_exceptions_recommended_plan_details)
                    .Include(recommendedPlanAdverisersToDoDb => recommendedPlanAdverisersToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Select(recommendedPlanAdverisersToDoDb => recommendedPlanAdverisersToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign.advertiser_master_id ?? default).FirstOrDefault())
                    .ToList();

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
                recommendedPlanAdvertiserMasterIdsPerWeek = context.spot_exceptions_recommended_plans_done
                    .Where(recommendedPlanAdverisersDoneDb => recommendedPlanAdverisersDoneDb.program_air_time >= weekStartDate && recommendedPlanAdverisersDoneDb.program_air_time <= weekEndDate)
                    .Include(recommendedPlanAdverisersDoneDb => recommendedPlanAdverisersDoneDb.spot_exceptions_recommended_plan_details_done)
                    .Include(recommendedPlanAdverisersDoneDb => recommendedPlanAdverisersDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Select(recommendedPlanAdverisersDoneDb => recommendedPlanAdverisersDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.campaign).Select(p => p.advertiser_master_id ?? default).FirstOrDefault())
                    .ToList();

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
                                            .Where(rRecommendedPlanStationsToDoDb => rRecommendedPlanStationsToDoDb.program_air_time >= weekStartDate && rRecommendedPlanStationsToDoDb.program_air_time <= weekEndDate)
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
        public Task<SpotExceptionsRecommendedPlanSpotsToDoDto> GetRecommendedPlanDetailsToDoByRecommendedPlanId(int planId, int recommendedPlanId)
        {
            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Details ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanDetailsToDoEntity = context.spot_exceptions_recommended_plans
                    .Where(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.id == planId)
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details)
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (recommendedPlanDetailsToDoDb, stationDb) => new { SpotExceptionsRecommendedPlan = recommendedPlanDetailsToDoDb, Station = stationDb.FirstOrDefault() })
                    .SingleOrDefault();

                if (recommendedPlanDetailsToDoEntity == null)
                {
                    return null;
                }

                var recommendedPlanDetailsToDo = _MapRecommendedPlanSpotsToDoToDto(recommendedPlanDetailsToDoEntity.SpotExceptionsRecommendedPlan);

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details ToDo");
                return recommendedPlanDetailsToDo;
            }));
        }

        /// <inheritdoc />
        public Task<SpotExceptionsRecommendedPlanSpotsDoneDto> GetRecommendedPlanDetailsDoneByRecommendedPlanId(int planId, int recommendedPlanId)
        {
            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Details Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanDetailsDoneEntity = context.spot_exceptions_recommended_plans_done
                    .Where(recommendedPlanDetailsToDoDb => recommendedPlanDetailsToDoDb.id == planId)
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done)
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(recommendedPlanDetailsDoneDb => recommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.spot_exceptions_recommended_plan_done_decisions))
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanDb, Station = stationDb.FirstOrDefault() })
                    .SingleOrDefault();

                if (recommendedPlanDetailsDoneEntity == null)
                {
                    return null;
                }

                var recommendedPlanDetailsDone = _MapRecommendedPlanSpotsDoneToDto(recommendedPlanDetailsDoneEntity.SpotExceptionsRecommendedPlan);

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details Done");
                return recommendedPlanDetailsDone;
            }));
        }

        /// <inheritdoc />
        public Task<bool> SaveRecommendedPlanToDoDecisionsAsync(SpotExceptionsRecommendedPlanSaveDto planToSave, SpotExceptionsRecommendedPlanSpotsToDoDto recommendedPlansToDo, string userName, DateTime currentDate)
        {
            bool isMoved = false;

            _LogInfo($"Starting: Moving Spot Exceptions Recommended Plan To Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var recommendedPlanDoneDecisions = _MapRecommendedPlanDoneToDto(recommendedPlansToDo);
                var doneEntitiesToAdd = _MapRecommendedPlanSpotsDoneToEntity(recommendedPlanDoneDecisions);

                foreach(var entity in doneEntitiesToAdd.spot_exceptions_recommended_plan_details_done)
                {
                    entity.spot_exceptions_recommended_plan_done_decisions.Add(new spot_exceptions_recommended_plan_done_decisions()
                    {
                        decided_by = userName,
                        decided_at = currentDate,
                        synced_by = null,
                        synced_at = null
                    });

                    context.spot_exceptions_recommended_plan_details_done.AddOrUpdate(entity);
                }
                context.spot_exceptions_recommended_plans_done.Add(doneEntitiesToAdd);

                var todoEntitiesToRemove = context.spot_exceptions_recommended_plans.Where(x => x.id == recommendedPlansToDo.Id).First();
                context.spot_exceptions_recommended_plans.Remove(todoEntitiesToRemove);

                context.SaveChanges();
                isMoved = true;

                _LogInfo($"Finished: Moving Spot Exceptions Recommended Plan To Done");
                return isMoved;
            }));
        }

        /// <inheritdoc />
        public Task<bool> SaveRecommendedPlanDoneDecisionsAsync(SpotExceptionsRecommendedPlanSpotDecisionsDoneDto recommendedPlanDoneDecision)
        {
            _LogInfo($"Starting: Adding Recommended Plan Decision");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                bool isSpotExceptionsRecommendedPlanDecisionSaved = false;
                var spotExceptionsRecommendedPlanDoneDetails = context.spot_exceptions_recommended_plan_details_done
                    .Where(x => x.spot_exceptions_recommended_plan_done_id == recommendedPlanDoneDecision.SpotExceptionsRecommendedPlanDetailsDoneId).ToList();

                if (spotExceptionsRecommendedPlanDoneDetails != null && spotExceptionsRecommendedPlanDoneDetails.Count() > 0)
                {
                    recommendedPlanDoneDecision.SpotExceptionsRecommendedPlanDetailsDoneId = spotExceptionsRecommendedPlanDoneDetails
                       .FirstOrDefault(x => x.spot_exceptions_recommended_plan_done_id == recommendedPlanDoneDecision.SpotExceptionsId
                       && x.recommended_plan_id == recommendedPlanDoneDecision.SpotExceptionsRecommendedPlanId).id;

                    List<int> planDetailIds = spotExceptionsRecommendedPlanDoneDetails.Select(x => x.id).ToList();

                    var existSpotExceptionsRecommendedPlanDoneDecision = context.spot_exceptions_recommended_plan_done_decisions.
                    Where(x => planDetailIds.Contains(x.spot_exceptions_recommended_plan_details_done_id)).FirstOrDefault();

                    if (existSpotExceptionsRecommendedPlanDoneDecision == null)
                    {
                        context.spot_exceptions_recommended_plan_done_decisions.Add(new spot_exceptions_recommended_plan_done_decisions
                        {
                            spot_exceptions_recommended_plan_details_done_id = recommendedPlanDoneDecision.SpotExceptionsRecommendedPlanDetailsDoneId,
                            decided_by = recommendedPlanDoneDecision.DecidedBy,
                            decided_at = recommendedPlanDoneDecision.DecidedAt,
                            synced_by = recommendedPlanDoneDecision.SyncedBy,
                            synced_at = recommendedPlanDoneDecision.SyncedAt
                        });
                    }
                    else
                    {
                        existSpotExceptionsRecommendedPlanDoneDecision.spot_exceptions_recommended_plan_details_done_id = recommendedPlanDoneDecision.SpotExceptionsRecommendedPlanDetailsDoneId;
                        existSpotExceptionsRecommendedPlanDoneDecision.decided_by = recommendedPlanDoneDecision.DecidedBy;
                        existSpotExceptionsRecommendedPlanDoneDecision.decided_at = recommendedPlanDoneDecision.DecidedAt;
                        existSpotExceptionsRecommendedPlanDoneDecision.synced_by = recommendedPlanDoneDecision.SyncedBy;
                        existSpotExceptionsRecommendedPlanDoneDecision.synced_at = recommendedPlanDoneDecision.SyncedAt;
                    }
                    isSpotExceptionsRecommendedPlanDecisionSaved = context.SaveChanges() > 0;
                }
                return isSpotExceptionsRecommendedPlanDecisionSaved;
            }));
        }

        /// <inheritdoc />
        public string GetMarketName(int marketCode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var marketName = context.markets
                                .Where(m => m.market_code == marketCode)
                                .Select(m => m.geography_name)
                                .Single();
                return marketName;
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
                        RecommendedPlanDetail = new RecommendedPlanDetailDto
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
                        RecommendedPlanDetail = new RecommendedPlanDetailDto
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

        private spot_exceptions_recommended_plans_done _MapRecommendedPlanSpotsDoneToEntity(SpotExceptionsRecommendedPlanSpotsDoneDto recommendedPlanSpotsDone)
        {
            var recommendedPlanSpotsDoneEntity = new spot_exceptions_recommended_plans_done
            {
                spot_unique_hash_external = recommendedPlanSpotsDone.SpotUniqueHashExternal,
                ambiguity_code = recommendedPlanSpotsDone.AmbiguityCode,
                execution_id_external = recommendedPlanSpotsDone.ExecutionIdExternal,
                estimate_id = recommendedPlanSpotsDone.EstimateId,
                inventory_source = recommendedPlanSpotsDone.InventorySource,
                house_isci = recommendedPlanSpotsDone.HouseIsci,
                client_isci = recommendedPlanSpotsDone.ClientIsci,
                spot_length_id = recommendedPlanSpotsDone.SpotLengthId,
                program_air_time = recommendedPlanSpotsDone.ProgramAirTime,
                station_legacy_call_letters = recommendedPlanSpotsDone.StationLegacyCallLetters,
                affiliate = recommendedPlanSpotsDone.Affiliate,
                market_code = recommendedPlanSpotsDone.MarketCode,
                market_rank = recommendedPlanSpotsDone.MarketRank,
                program_name = recommendedPlanSpotsDone.ProgramName,
                program_genre = recommendedPlanSpotsDone.ProgramGenre,
                ingested_by = recommendedPlanSpotsDone.IngestedBy,
                ingested_at = recommendedPlanSpotsDone.IngestedAt,
                spot_exceptions_recommended_plan_details_done = recommendedPlanSpotsDone.SpotExceptionsRecommendedPlanDetailsDone.Select(recommendedPlanDetailsDone =>
                {
                    var recommendedPlanSpotDetailsDone = new spot_exceptions_recommended_plan_details_done
                    {
                        spot_exceptions_recommended_plan_done_id = recommendedPlanDetailsDone.SpotExceptionsRecommendedPlanId,
                        recommended_plan_id = recommendedPlanDetailsDone.RecommendedPlanId,
                        execution_trace_id = recommendedPlanDetailsDone.ExecutionTraceId,
                        rate = recommendedPlanDetailsDone.Rate,
                        audience_name = recommendedPlanDetailsDone.AudienceName,
                        contracted_impressions = recommendedPlanDetailsDone.ContractedImpressions,
                        delivered_impressions = recommendedPlanDetailsDone.DeliveredImpressions,
                        is_recommended_plan = recommendedPlanDetailsDone.IsRecommendedPlan,
                        plan_clearance_percentage = recommendedPlanDetailsDone.PlanClearancePercentage,
                        daypart_code = recommendedPlanDetailsDone.DaypartCode,
                        start_time = recommendedPlanDetailsDone.StartTime,
                        end_time = recommendedPlanDetailsDone.EndTime,
                        monday = recommendedPlanDetailsDone.Monday,
                        tuesday = recommendedPlanDetailsDone.Tuesday,
                        wednesday = recommendedPlanDetailsDone.Wednesday,
                        thursday = recommendedPlanDetailsDone.Thursday,
                        saturday = recommendedPlanDetailsDone.Saturday,
                        sunday = recommendedPlanDetailsDone.Sunday,
                        spot_delivered_impressions = recommendedPlanDetailsDone.SpotDeliveredImpressions,
                        plan_total_contracted_impressions = recommendedPlanDetailsDone.PlanTotalContractedImpressions,
                        plan_total_delivered_impressions = recommendedPlanDetailsDone.PlanTotalDeliveredImpressions,
                        ingested_media_week_id = recommendedPlanDetailsDone.IngestedMediaWeekId,
                        ingested_by = recommendedPlanDetailsDone.IngestedBy,
                        ingested_at = recommendedPlanDetailsDone.IngestedAt,
                        spot_unique_hash_external = recommendedPlanDetailsDone.SpotUniqueHashExternal,
                        execution_id_external = recommendedPlanDetailsDone.ExecutionIdExternal
                    };
                    return recommendedPlanSpotDetailsDone;
                }).ToList()
            };
            return recommendedPlanSpotsDoneEntity;
        }

        private SpotExceptionsRecommendedPlanSpotsDoneDto _MapRecommendedPlanDoneToDto(SpotExceptionsRecommendedPlanSpotsToDoDto recommendedPlanSpotsToDoEntity)
        {
            var recommendedPlanDoneEntity = new SpotExceptionsRecommendedPlanSpotsDoneDto
            {
                SpotUniqueHashExternal = recommendedPlanSpotsToDoEntity.SpotUniqueHashExternal,
                AmbiguityCode = recommendedPlanSpotsToDoEntity.AmbiguityCode,
                ExecutionIdExternal = recommendedPlanSpotsToDoEntity.ExecutionIdExternal,
                EstimateId = recommendedPlanSpotsToDoEntity.EstimateId,
                InventorySource = recommendedPlanSpotsToDoEntity.InventorySource,
                HouseIsci = recommendedPlanSpotsToDoEntity.HouseIsci,
                ClientIsci = recommendedPlanSpotsToDoEntity.ClientIsci,
                SpotLengthId = recommendedPlanSpotsToDoEntity.SpotLengthId,
                ProgramAirTime = recommendedPlanSpotsToDoEntity.ProgramAirTime,
                StationLegacyCallLetters = recommendedPlanSpotsToDoEntity.StationLegacyCallLetters,
                Affiliate = recommendedPlanSpotsToDoEntity.Affiliate,
                MarketCode = recommendedPlanSpotsToDoEntity.MarketCode,
                MarketRank = recommendedPlanSpotsToDoEntity.MarketRank,
                ProgramName = recommendedPlanSpotsToDoEntity.ProgramName,
                ProgramGenre = recommendedPlanSpotsToDoEntity.ProgramGenre,
                IngestedBy = recommendedPlanSpotsToDoEntity.IngestedBy,
                IngestedAt = recommendedPlanSpotsToDoEntity.IngestedAt,
                IngestedMediaWeekId = recommendedPlanSpotsToDoEntity.IngestedMediaWeekId,
                SpotLength = recommendedPlanSpotsToDoEntity.SpotLength,
                SpotExceptionsRecommendedPlanDetailsDone = recommendedPlanSpotsToDoEntity.SpotExceptionsRecommendedPlanDetailsToDo.Select(recommendedPlanDetailToDoDb =>
                {
                    var recommendedPlanDetailDone = new SpotExceptionsRecommendedPlanDetailsDoneDto
                    {
                        SpotExceptionsRecommendedPlanId = recommendedPlanDetailToDoDb.SpotExceptionsRecommendedPlanId,
                        RecommendedPlanId = recommendedPlanDetailToDoDb.RecommendedPlanId,
                        ExecutionTraceId = recommendedPlanDetailToDoDb.ExecutionTraceId,
                        Rate = recommendedPlanDetailToDoDb.Rate,
                        AudienceName = recommendedPlanDetailToDoDb.AudienceName,
                        ContractedImpressions = recommendedPlanDetailToDoDb.ContractedImpressions,
                        DeliveredImpressions = recommendedPlanDetailToDoDb.DeliveredImpressions,
                        IsRecommendedPlan = recommendedPlanDetailToDoDb.IsRecommendedPlan,
                        PlanClearancePercentage = recommendedPlanDetailToDoDb.PlanClearancePercentage,
                        DaypartCode = recommendedPlanDetailToDoDb.DaypartCode,
                        StartTime = recommendedPlanDetailToDoDb.StartTime,
                        EndTime = recommendedPlanDetailToDoDb.EndTime,
                        Monday = recommendedPlanDetailToDoDb.Monday,
                        Tuesday = recommendedPlanDetailToDoDb.Tuesday,
                        Wednesday = recommendedPlanDetailToDoDb.Wednesday,
                        Thursday = recommendedPlanDetailToDoDb.Thursday,
                        Friday = recommendedPlanDetailToDoDb.Friday,
                        Saturday = recommendedPlanDetailToDoDb.Saturday,
                        Sunday = recommendedPlanDetailToDoDb.Sunday,
                        SpotDeliveredImpressions = recommendedPlanDetailToDoDb.SpotDeliveredImpressions,
                        PlanTotalContractedImpressions = recommendedPlanDetailToDoDb.PlanTotalContractedImpressions,
                        PlanTotalDeliveredImpressions = recommendedPlanDetailToDoDb.PlanTotalDeliveredImpressions,
                        IngestedMediaWeekId = recommendedPlanDetailToDoDb.IngestedMediaWeekId,
                        IngestedBy = recommendedPlanDetailToDoDb.IngestedBy,
                        IngestedAt = recommendedPlanDetailToDoDb.IngestedAt,
                        SpotUniqueHashExternal = recommendedPlanDetailToDoDb.SpotUniqueHashExternal,
                        ExecutionIdExternal = recommendedPlanDetailToDoDb.ExecutionIdExternal,
                    };
                    return recommendedPlanDetailDone;
                }).ToList()
            };
            return recommendedPlanDoneEntity;
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
