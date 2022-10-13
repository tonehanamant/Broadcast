using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsRecommendedPlanRepository : IDataRepository
    {
        Task<List<SpotExceptionsRecommendedPlansToDoDto>> GetRecommendedPlansToDoAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsRecommendedPlansDoneDto>> GetRecommendedPlansDoneAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<Guid>> GetRecommendedPlanSpotsToDoAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<Guid>> GetRecommendedPlanSpotsDoneAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<string>> GetSpotExceptionsRecommendedPlanToDoStationsAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<string>> GetSpotExceptionsRecommendedPlanDoneStationsAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsRecommendedPlansToDoDto>> GetSpotExceptionRecommendedPlanToDoSpots(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsRecommendedPlansDoneDto>> GetSpotExceptionRecommendedPlanDoneSpots(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<spot_exceptions_recommended_plans> GetSpotExceptionRecommendedPlanByDecisionAsync(SpotExceptionsRecommendedPlanSaveDto spotExceptionsRecommendedPlan);

        Task<int> AddSpotExceptionsRecommendedPlanToDoneAsync(spot_exceptions_recommended_plans spotExceptionsRecommendedPlanToDoEntity);

        Task<int> DeleteSpotExceptionsRecommendedPlanFromToDoAsync(int spotExceptionRecommendedPlanId, int spotExceptionRecommendedPlanDetailId);

        Task<bool> SaveSpotExceptionsRecommendedPlanDoneDecisionsAsync(SpotExceptionsRecommendedPlanDoneDecisionsDto spotExceptionsRecommendedPlanDoneDecision);

        Task<SpotExceptionsRecommendedPlansToDoDto> GetSpotExceptionsRecommendedPlanById(int spotExceptionsRecommendedPlanId);

        string GetMarketName(int marketCode);
    }

    public class SpotExceptionsRecommendedPlanRepository : BroadcastRepositoryBase, ISpotExceptionsRecommendedPlanRepository
    {
        public SpotExceptionsRecommendedPlanRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }


        /// <inheritdoc />
        public async Task<List<SpotExceptionsRecommendedPlansToDoDto>> GetRecommendedPlansToDoAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanToDoEntities = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanToDoDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details)
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_lengths)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanToDoDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanToDoDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlansDone = spotExceptionsRecommendedPlanToDoEntities.Select(spotExceptionsRecommendedPlanToDoEntity => _MapRecommendedPlansToDoToDto(spotExceptionsRecommendedPlanToDoEntity.SpotExceptionsRecommendedPlan)).ToList();
                
                return spotExceptionsRecommendedPlansDone;
            });
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionsRecommendedPlansDoneDto>> GetRecommendedPlansDoneAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanDoneEntities = context.spot_exceptions_recommended_plans_done
                    .Where(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDoneDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done)
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.spot_exceptions_recommended_plan_done_decisions))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_lengths)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDoneDb, stationDb) => new { SpotExceptionsRecommendedPlanDone = spotExceptionsRecommendedPlanDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlansDone = spotExceptionsRecommendedPlanDoneEntities.Select(spotExceptionsRecommendedPlanDoneEntity => _MapRecommendedPlansDoneToDto(spotExceptionsRecommendedPlanDoneEntity.SpotExceptionsRecommendedPlanDone)).ToList();
                
                return spotExceptionsRecommendedPlansDone;
            });
        }

        public async Task<List<Guid>> GetRecommendedPlanSpotsToDoAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<Guid> recommendedPlanAdvertiserMasterIdsPerWeek = null;

            return _InReadUncommitedTransaction(context =>
            {
                recommendedPlanAdvertiserMasterIdsPerWeek = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Select(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign.advertiser_master_id ?? default).FirstOrDefault())
                    .ToList();
                
                return recommendedPlanAdvertiserMasterIdsPerWeek;

            });
        }

        public async Task<List<Guid>> GetRecommendedPlanSpotsDoneAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            List<Guid> recommendedPlanAdvertiserMasterIdsPerWeek = null;

            return _InReadUncommitedTransaction(context =>
            {
                recommendedPlanAdvertiserMasterIdsPerWeek = context.spot_exceptions_recommended_plans_done
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details_done)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Select(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.campaign).Select(p => p.advertiser_master_id ?? default).FirstOrDefault())
                    .ToList();

                return recommendedPlanAdvertiserMasterIdsPerWeek;

            });
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsRecommendedPlanToDoStationsAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var recommendedPlanStations = context.spot_exceptions_recommended_plans
                                            .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                                            .Select(rps => rps.station_legacy_call_letters ?? "Unknown")
                                            .ToList();
                return recommendedPlanStations;
            });
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsRecommendedPlanDoneStationsAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var recommendedPlanStations = context.spot_exceptions_recommended_plans_done
                                            .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                                            .Select(rps => rps.station_legacy_call_letters ?? "Unknown")
                                            .ToList();
                return recommendedPlanStations;
            });
        }

        public async Task<List<SpotExceptionsRecommendedPlansToDoDto>> GetSpotExceptionRecommendedPlanToDoSpots(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanToDoEntities = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanToDoDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details).Where(p => p.spot_exceptions_recommended_plan_details.Any(x => x.recommended_plan_id == planId))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_lengths)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanToDoDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanToDoDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlansToDo = spotExceptionsRecommendedPlanToDoEntities.Select(spotExceptionsRecommendedPlanToDoEntity => _MapRecommendedPlansToDoToDto(spotExceptionsRecommendedPlanToDoEntity.SpotExceptionsRecommendedPlan)).ToList();

                return spotExceptionsRecommendedPlansToDo;
            });
        }

        public async Task<List<SpotExceptionsRecommendedPlansDoneDto>> GetSpotExceptionRecommendedPlanDoneSpots(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanDoneEntities = context.spot_exceptions_recommended_plans_done
                    .Where(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDoneDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done).Where(p => p.spot_exceptions_recommended_plan_details_done.Any(x => x.recommended_plan_id == planId))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.campaign))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_exceptions_recommended_plan_details_done.Select(serpd => serpd.spot_exceptions_recommended_plan_done_decisions))
                    .Include(spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.spot_lengths)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDoneDb => spotExceptionsRecommendedPlanDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDoneDb, stationDb) => new { SpotExceptionsRecommendedPlanDone = spotExceptionsRecommendedPlanDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsRecommendedPlansDone = spotExceptionsRecommendedPlanDoneEntities.Select(spotExceptionsRecommendedPlanDoneEntity => _MapRecommendedPlansDoneToDto(spotExceptionsRecommendedPlanDoneEntity.SpotExceptionsRecommendedPlanDone)).ToList();

                return spotExceptionsRecommendedPlansDone;
            });
        }

        public Task<spot_exceptions_recommended_plans> GetSpotExceptionRecommendedPlanByDecisionAsync(SpotExceptionsRecommendedPlanSaveDto spotExceptionsRecommendedPlan)
        {
            return _InReadUncommitedTransaction(async context =>
            {
                var spotExceptionsRecommendedPlanToDoEntities = await context.spot_exceptions_recommended_plans
                .Include(spotExceptionsRecommendedPlanToDoDb => spotExceptionsRecommendedPlanToDoDb.spot_exceptions_recommended_plan_details).Where(d => d.id == spotExceptionsRecommendedPlan.SelectedPlanId)
                .FirstAsync();

                return spotExceptionsRecommendedPlanToDoEntities;
            });
        }

        public Task<int> AddSpotExceptionsRecommendedPlanToDoneAsync(spot_exceptions_recommended_plans spotExceptionsRecommendedPlanToDoEntity)
        {
            return _InReadUncommitedTransaction(async context =>
            {
                var doneEntities = _MapRecommendedPlanDoneToDto(spotExceptionsRecommendedPlanToDoEntity);

                var countSaved = await context.SaveChangesAsync();

                return countSaved;
            });
        }

        public Task<int> DeleteSpotExceptionsRecommendedPlanFromToDoAsync(int spotExceptionRecommendedPlanId, int spotExceptionRecommendedPlanDetailId)
        {
            int planCountDelete = 0;

            return _InReadUncommitedTransaction(async context =>
            {
                var planDetailIdToDelete = context.spot_exceptions_recommended_plan_details.First(d => d.id == spotExceptionRecommendedPlanDetailId);

                if (planDetailIdToDelete != null)
                {
                    var result = context.spot_exceptions_recommended_plan_details.Remove(planDetailIdToDelete);
                }

                planCountDelete = await context.SaveChangesAsync();
                return planCountDelete;
            });
        }

        public Task<bool> SaveSpotExceptionsRecommendedPlanDoneDecisionsAsync(SpotExceptionsRecommendedPlanDoneDecisionsDto spotExceptionsRecommendedPlanDoneDecision)
        {
            return _InReadUncommitedTransaction(async context => 
            {
                var alreadyRecordExists = context.spot_exceptions_recommended_plan_done_decisions.SingleOrDefault(x =>
                    x.id == spotExceptionsRecommendedPlanDoneDecision.Id);

                bool isSpotExceptionsRecommendedPlanDecisionSaved = false;

                var spotExceptionsRecommendedPlanDoneDetails = context.spot_exceptions_recommended_plan_details_done
                    .Where(x => x.spot_exceptions_recommended_plan_done_id == spotExceptionsRecommendedPlanDoneDecision.SpotExceptionsId).ToList();
                
                if (spotExceptionsRecommendedPlanDoneDetails != null && spotExceptionsRecommendedPlanDoneDetails.Count() > 0)
                {
                    spotExceptionsRecommendedPlanDoneDecision.SpotExceptionsRecommendedPlanDetailsDoneId = spotExceptionsRecommendedPlanDoneDetails
                       .FirstOrDefault(x => x.spot_exceptions_recommended_plan_done_id == spotExceptionsRecommendedPlanDoneDecision.SpotExceptionsId
                       && x.recommended_plan_id == spotExceptionsRecommendedPlanDoneDecision.SpotExceptionsRecommendedPlanDetailsDoneId).id;

                    if (alreadyRecordExists == null)
                    {
                        context.spot_exceptions_recommended_plan_done_decisions.Add(new spot_exceptions_recommended_plan_done_decisions
                        {
                            spot_exceptions_recommended_plan_details_done_id = spotExceptionsRecommendedPlanDoneDecision.Id,
                            decided_by = spotExceptionsRecommendedPlanDoneDecision.DecidedBy,
                            decided_at = spotExceptionsRecommendedPlanDoneDecision.DecidedAt,
                            synced_by = spotExceptionsRecommendedPlanDoneDecision.SyncedBy,
                            synced_at = spotExceptionsRecommendedPlanDoneDecision.SyncedAt
                        });
                    }
                    else
                    {
                        alreadyRecordExists.spot_exceptions_recommended_plan_details_done_id = spotExceptionsRecommendedPlanDoneDecision.Id;
                        alreadyRecordExists.decided_by = spotExceptionsRecommendedPlanDoneDecision.DecidedBy;
                        alreadyRecordExists.decided_at = spotExceptionsRecommendedPlanDoneDecision.DecidedAt;
                        alreadyRecordExists.synced_at = spotExceptionsRecommendedPlanDoneDecision.SyncedAt;
                        alreadyRecordExists.synced_by = spotExceptionsRecommendedPlanDoneDecision.SyncedBy;
                    }

                    isSpotExceptionsRecommendedPlanDecisionSaved = context.SaveChanges() > 0;
                }
                return isSpotExceptionsRecommendedPlanDecisionSaved;
            });
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlansToDoDto> GetSpotExceptionsRecommendedPlanById(int spotExceptionsRecommendedPlanId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsRecommendedPlanEntity = context.spot_exceptions_recommended_plans
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.id == spotExceptionsRecommendedPlanId)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details)
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths)))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_exceptions_recommended_plan_details.Select(serpd => serpd.plan).Select(p => p.plan_versions.Select(pv => pv.plan_version_creative_lengths.Select(pvcl => pvcl.spot_lengths))))
                    .Include(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.spot_lengths)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsRecommendedPlanDb, stationDb) => new { SpotExceptionsRecommendedPlan = spotExceptionsRecommendedPlanDb, Station = stationDb.FirstOrDefault() })
                    .SingleOrDefault();

                if (spotExceptionsRecommendedPlanEntity == null)
                {
                    return null;
                }

                var spotExceptionsRecommendedPlan = _MapRecommendedPlansToDoToDto(spotExceptionsRecommendedPlanEntity.SpotExceptionsRecommendedPlan);
                return spotExceptionsRecommendedPlan;
            });
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

        private SpotExceptionsRecommendedPlansToDoDto _MapRecommendedPlansToDoToDto(spot_exceptions_recommended_plans spotExceptionsRecommendedPlanToDoEntity)
        {
            var spotExceptionsRecommendedPlanToDo = new SpotExceptionsRecommendedPlansToDoDto
            {
                Id = spotExceptionsRecommendedPlanToDoEntity.id,
                SpotUniqueHashExternal = spotExceptionsRecommendedPlanToDoEntity.spot_unique_hash_external,
                AmbiguityCode = spotExceptionsRecommendedPlanToDoEntity.ambiguity_code,
                ExecutionIdExternal = spotExceptionsRecommendedPlanToDoEntity.execution_id_external,
                EstimateId = spotExceptionsRecommendedPlanToDoEntity.estimate_id,
                InventorySource = spotExceptionsRecommendedPlanToDoEntity.inventory_source,
                HouseIsci = spotExceptionsRecommendedPlanToDoEntity.house_isci,
                ClientIsci = spotExceptionsRecommendedPlanToDoEntity.client_isci,
                SpotLengthId = spotExceptionsRecommendedPlanToDoEntity.spot_length_id,
                ProgramAirTime = spotExceptionsRecommendedPlanToDoEntity.program_air_time,
                StationLegacyCallLetters = spotExceptionsRecommendedPlanToDoEntity.station_legacy_call_letters,
                Affiliate = spotExceptionsRecommendedPlanToDoEntity.affiliate,
                MarketCode = spotExceptionsRecommendedPlanToDoEntity.market_code,
                MarketRank = spotExceptionsRecommendedPlanToDoEntity.market_rank,
                ProgramName = spotExceptionsRecommendedPlanToDoEntity.program_name,
                ProgramGenre = spotExceptionsRecommendedPlanToDoEntity.program_genre,
                IngestedBy = spotExceptionsRecommendedPlanToDoEntity.ingested_by,
                IngestedAt = spotExceptionsRecommendedPlanToDoEntity.ingested_at,
                IngestedMediaWeekId = spotExceptionsRecommendedPlanToDoEntity.ingested_media_week_id,
                SpotLength = _MapSpotLengthToDto(spotExceptionsRecommendedPlanToDoEntity.spot_lengths),
                SpotExceptionsRecommendedPlanDetailsToDo = spotExceptionsRecommendedPlanToDoEntity.spot_exceptions_recommended_plan_details.Select(spotExceptionsRecommendedPlanDetailToDoDb =>
                {
                    var recommendedPlan = spotExceptionsRecommendedPlanDetailToDoDb.plan;
                    var recommendedPlanVersion = recommendedPlan.plan_versions.Single(planVersion => planVersion.id == recommendedPlan.latest_version_id);
                    var spotExceptionsRecommendedPlanDetailToDo = new SpotExceptionsRecommendedPlanDetailsToDoDto
                    {
                        Id = spotExceptionsRecommendedPlanDetailToDoDb.id,
                        SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlanDetailToDoDb.spot_exceptions_recommended_plan_id,
                        RecommendedPlanId = spotExceptionsRecommendedPlanDetailToDoDb.recommended_plan_id,
                        ExecutionTraceId = spotExceptionsRecommendedPlanDetailToDoDb.execution_trace_id,
                        Rate = spotExceptionsRecommendedPlanDetailToDoDb.rate,
                        AudienceName = spotExceptionsRecommendedPlanDetailToDoDb.audience_name,
                        ContractedImpressions = spotExceptionsRecommendedPlanDetailToDoDb.contracted_impressions,
                        DeliveredImpressions = spotExceptionsRecommendedPlanDetailToDoDb.delivered_impressions,
                        IsRecommendedPlan = spotExceptionsRecommendedPlanDetailToDoDb.is_recommended_plan,
                        PlanClearancePercentage = spotExceptionsRecommendedPlanDetailToDoDb.plan_clearance_percentage,
                        DaypartCode = spotExceptionsRecommendedPlanDetailToDoDb.daypart_code,
                        StartTime = spotExceptionsRecommendedPlanDetailToDoDb.start_time,
                        EndTime = spotExceptionsRecommendedPlanDetailToDoDb.end_time,
                        Monday = spotExceptionsRecommendedPlanDetailToDoDb.monday,
                        Tuesday = spotExceptionsRecommendedPlanDetailToDoDb.tuesday,
                        Wednesday = spotExceptionsRecommendedPlanDetailToDoDb.wednesday,
                        Thursday = spotExceptionsRecommendedPlanDetailToDoDb.thursday,
                        Friday = spotExceptionsRecommendedPlanDetailToDoDb.friday,
                        Saturday = spotExceptionsRecommendedPlanDetailToDoDb.saturday,
                        Sunday = spotExceptionsRecommendedPlanDetailToDoDb.sunday,
                        SpotDeliveredImpressions = spotExceptionsRecommendedPlanDetailToDoDb.spot_delivered_impressions,
                        PlanTotalContractedImpressions = spotExceptionsRecommendedPlanDetailToDoDb.plan_total_contracted_impressions,
                        PlanTotalDeliveredImpressions = spotExceptionsRecommendedPlanDetailToDoDb.plan_total_delivered_impressions,
                        RecommendedPlanDetail = new RecommendedPlanDetailDto
                        {
                            Id = spotExceptionsRecommendedPlanDetailToDoDb.recommended_plan_id,
                            Name = recommendedPlan.name,
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AdvertiserMasterId = recommendedPlan.campaign.advertiser_master_id,
                            ProductMasterId = recommendedPlan.product_master_id
                        }
                    };
                    return spotExceptionsRecommendedPlanDetailToDo;
                }).ToList()
            };
            return spotExceptionsRecommendedPlanToDo;
        }

        private SpotExceptionsRecommendedPlansDoneDto _MapRecommendedPlansDoneToDto(spot_exceptions_recommended_plans_done spotExceptionsRecommendedPlanDoneEntity)
        {
            var spotExceptionsRecommendedPlanDone = new SpotExceptionsRecommendedPlansDoneDto
            {
                Id = spotExceptionsRecommendedPlanDoneEntity.id,
                SpotUniqueHashExternal = spotExceptionsRecommendedPlanDoneEntity.spot_unique_hash_external,
                AmbiguityCode = spotExceptionsRecommendedPlanDoneEntity.ambiguity_code,
                ExecutionIdExternal = spotExceptionsRecommendedPlanDoneEntity.execution_id_external,
                EstimateId = spotExceptionsRecommendedPlanDoneEntity.estimate_id,
                InventorySource = spotExceptionsRecommendedPlanDoneEntity.inventory_source,
                HouseIsci = spotExceptionsRecommendedPlanDoneEntity.house_isci,
                ClientIsci = spotExceptionsRecommendedPlanDoneEntity.client_isci,
                SpotLengthId = spotExceptionsRecommendedPlanDoneEntity.spot_length_id,
                ProgramAirTime = spotExceptionsRecommendedPlanDoneEntity.program_air_time,
                StationLegacyCallLetters = spotExceptionsRecommendedPlanDoneEntity.station_legacy_call_letters,
                Affiliate = spotExceptionsRecommendedPlanDoneEntity.affiliate,
                MarketCode = spotExceptionsRecommendedPlanDoneEntity.market_code,
                MarketRank = spotExceptionsRecommendedPlanDoneEntity.market_rank,
                ProgramName = spotExceptionsRecommendedPlanDoneEntity.program_name,
                ProgramGenre = spotExceptionsRecommendedPlanDoneEntity.program_genre,
                IngestedBy = spotExceptionsRecommendedPlanDoneEntity.ingested_by,
                IngestedAt = spotExceptionsRecommendedPlanDoneEntity.ingested_at,
                SpotLength = _MapSpotLengthToDto(spotExceptionsRecommendedPlanDoneEntity.spot_lengths),
                SpotExceptionsRecommendedPlanDetailsDone = spotExceptionsRecommendedPlanDoneEntity.spot_exceptions_recommended_plan_details_done.Select(SpotExceptionsRecommendedPlanDetailsDoneDb =>
                {
                    var recommendedPlan = SpotExceptionsRecommendedPlanDetailsDoneDb.plan;
                    var recommendedPlanVersion = recommendedPlan.plan_versions.Single(planVersion => planVersion.id == recommendedPlan.latest_version_id);
                    var spotExceptionsRecommendedPlanDetailDone = new SpotExceptionsRecommendedPlanDetailsDoneDto
                    {
                        Id = SpotExceptionsRecommendedPlanDetailsDoneDb.id,
                        SpotExceptionsRecommendedPlanId = SpotExceptionsRecommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_done_id,
                        RecommendedPlanId = SpotExceptionsRecommendedPlanDetailsDoneDb.recommended_plan_id,
                        ExecutionTraceId = SpotExceptionsRecommendedPlanDetailsDoneDb.execution_trace_id,
                        Rate = SpotExceptionsRecommendedPlanDetailsDoneDb.rate,
                        AudienceName = SpotExceptionsRecommendedPlanDetailsDoneDb.audience_name,
                        ContractedImpressions = SpotExceptionsRecommendedPlanDetailsDoneDb.contracted_impressions,
                        DeliveredImpressions = SpotExceptionsRecommendedPlanDetailsDoneDb.delivered_impressions,
                        IsRecommendedPlan = SpotExceptionsRecommendedPlanDetailsDoneDb.is_recommended_plan,
                        PlanClearancePercentage = SpotExceptionsRecommendedPlanDetailsDoneDb.plan_clearance_percentage,
                        DaypartCode = SpotExceptionsRecommendedPlanDetailsDoneDb.daypart_code,
                        StartTime = SpotExceptionsRecommendedPlanDetailsDoneDb.start_time,
                        EndTime = SpotExceptionsRecommendedPlanDetailsDoneDb.end_time,
                        Monday = SpotExceptionsRecommendedPlanDetailsDoneDb.monday,
                        Tuesday = SpotExceptionsRecommendedPlanDetailsDoneDb.tuesday,
                        Wednesday = SpotExceptionsRecommendedPlanDetailsDoneDb.wednesday,
                        Thursday = SpotExceptionsRecommendedPlanDetailsDoneDb.thursday,
                        Friday = SpotExceptionsRecommendedPlanDetailsDoneDb.friday,
                        Saturday = SpotExceptionsRecommendedPlanDetailsDoneDb.saturday,
                        Sunday = SpotExceptionsRecommendedPlanDetailsDoneDb.sunday,
                        SpotDeliveredImpressions = SpotExceptionsRecommendedPlanDetailsDoneDb.spot_delivered_impressions,
                        PlanTotalContractedImpressions = SpotExceptionsRecommendedPlanDetailsDoneDb.plan_total_contracted_impressions,
                        PlanTotalDeliveredImpressions = SpotExceptionsRecommendedPlanDetailsDoneDb.plan_total_delivered_impressions,
                        RecommendedPlanDetail = new RecommendedPlanDetailDto
                        {
                            Id = SpotExceptionsRecommendedPlanDetailsDoneDb.recommended_plan_id,
                            Name = recommendedPlan.name,
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AdvertiserMasterId = recommendedPlan.campaign.advertiser_master_id,
                            ProductMasterId = recommendedPlan.product_master_id
                        },
                        SpotExceptionsRecommendedPlanDoneDecisions = SpotExceptionsRecommendedPlanDetailsDoneDb.spot_exceptions_recommended_plan_done_decisions.Select(spotExceptionsRecommendedPlanDoneDecisionDb => new SpotExceptionsRecommendedPlanDoneDecisionsDto
                        {
                            Id = spotExceptionsRecommendedPlanDoneDecisionDb.id,
                            SpotExceptionsRecommendedPlanDetailsDoneId = spotExceptionsRecommendedPlanDoneDecisionDb.spot_exceptions_recommended_plan_details_done_id,
                            DecidedBy = spotExceptionsRecommendedPlanDoneDecisionDb.decided_by,
                            DecidedAt = spotExceptionsRecommendedPlanDoneDecisionDb.decided_at,
                            SyncedBy = spotExceptionsRecommendedPlanDoneDecisionDb.synced_by,
                            SyncedAt = spotExceptionsRecommendedPlanDoneDecisionDb.synced_at
                        }).SingleOrDefault()
                    };
                    return spotExceptionsRecommendedPlanDetailDone;
                }).ToList()
            };
            return spotExceptionsRecommendedPlanDone;
        }

        private spot_exceptions_recommended_plans_done _MapRecommendedPlanDoneToDto(spot_exceptions_recommended_plans spotExceptionsRecommendedPlanToDoEntity)
        {
            var spotExceptionsRecommendedPlanDoneEntity = new spot_exceptions_recommended_plans_done
            {
                id = spotExceptionsRecommendedPlanToDoEntity.id,
                spot_unique_hash_external = spotExceptionsRecommendedPlanToDoEntity.spot_unique_hash_external,
                ambiguity_code = spotExceptionsRecommendedPlanToDoEntity.ambiguity_code,
                execution_id_external = spotExceptionsRecommendedPlanToDoEntity.execution_id_external,
                estimate_id = spotExceptionsRecommendedPlanToDoEntity.estimate_id,
                inventory_source = spotExceptionsRecommendedPlanToDoEntity.inventory_source,
                house_isci = spotExceptionsRecommendedPlanToDoEntity.house_isci,
                client_isci = spotExceptionsRecommendedPlanToDoEntity.client_isci,
                spot_length_id = spotExceptionsRecommendedPlanToDoEntity.spot_length_id,
                program_air_time = spotExceptionsRecommendedPlanToDoEntity.program_air_time,
                station_legacy_call_letters = spotExceptionsRecommendedPlanToDoEntity.station_legacy_call_letters,
                affiliate = spotExceptionsRecommendedPlanToDoEntity.affiliate,
                market_code = spotExceptionsRecommendedPlanToDoEntity.market_code,
                market_rank = spotExceptionsRecommendedPlanToDoEntity.market_rank,
                program_name = spotExceptionsRecommendedPlanToDoEntity.program_name,
                program_genre = spotExceptionsRecommendedPlanToDoEntity.program_genre,
                ingested_by = spotExceptionsRecommendedPlanToDoEntity.ingested_by,
                ingested_at = spotExceptionsRecommendedPlanToDoEntity.ingested_at,
                ingested_media_week_id = spotExceptionsRecommendedPlanToDoEntity.ingested_media_week_id,
                spot_lengths = spotExceptionsRecommendedPlanToDoEntity.spot_lengths,
                spot_exceptions_recommended_plan_details_done = spotExceptionsRecommendedPlanToDoEntity.spot_exceptions_recommended_plan_details.Select(spotExceptionsRecommendedPlanDetailToDoDb =>
                {
                    var spotExceptionsRecommendedPlanDetailDone = new spot_exceptions_recommended_plan_details_done
                    {
                        id = spotExceptionsRecommendedPlanDetailToDoDb.id,
                        spot_exceptions_recommended_plan_done_id = spotExceptionsRecommendedPlanDetailToDoDb.spot_exceptions_recommended_plan_id,
                        recommended_plan_id = spotExceptionsRecommendedPlanDetailToDoDb.recommended_plan_id,
                        execution_trace_id = spotExceptionsRecommendedPlanDetailToDoDb.execution_trace_id,
                        rate = spotExceptionsRecommendedPlanDetailToDoDb.rate,
                        audience_name = spotExceptionsRecommendedPlanDetailToDoDb.audience_name,
                        contracted_impressions = spotExceptionsRecommendedPlanDetailToDoDb.contracted_impressions,
                        delivered_impressions = spotExceptionsRecommendedPlanDetailToDoDb.delivered_impressions,
                        is_recommended_plan = spotExceptionsRecommendedPlanDetailToDoDb.is_recommended_plan,
                        plan_clearance_percentage = spotExceptionsRecommendedPlanDetailToDoDb.plan_clearance_percentage,
                        daypart_code = spotExceptionsRecommendedPlanDetailToDoDb.daypart_code,
                        start_time = spotExceptionsRecommendedPlanDetailToDoDb.start_time,
                        end_time = spotExceptionsRecommendedPlanDetailToDoDb.end_time,
                        monday = spotExceptionsRecommendedPlanDetailToDoDb.monday,
                        tuesday = spotExceptionsRecommendedPlanDetailToDoDb.tuesday,
                        wednesday = spotExceptionsRecommendedPlanDetailToDoDb.wednesday,
                        thursday = spotExceptionsRecommendedPlanDetailToDoDb.thursday,
                        friday = spotExceptionsRecommendedPlanDetailToDoDb.friday,
                        saturday = spotExceptionsRecommendedPlanDetailToDoDb.saturday,
                        sunday = spotExceptionsRecommendedPlanDetailToDoDb.sunday,
                        spot_delivered_impressions = spotExceptionsRecommendedPlanDetailToDoDb.spot_delivered_impressions,
                        plan_total_contracted_impressions = spotExceptionsRecommendedPlanDetailToDoDb.plan_total_contracted_impressions,
                        plan_total_delivered_impressions = spotExceptionsRecommendedPlanDetailToDoDb.plan_total_delivered_impressions
                    };
                    return spotExceptionsRecommendedPlanDetailDone;
                }).ToList()
            };
            return spotExceptionsRecommendedPlanDoneEntity;
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
