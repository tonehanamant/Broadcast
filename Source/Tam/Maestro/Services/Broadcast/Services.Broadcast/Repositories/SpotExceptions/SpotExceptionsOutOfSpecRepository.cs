using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using System.Data.Entity;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Entities.Plan;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecRepository : IDataRepository
    {
        Task<List<SpotExceptionsOutOfSpecsToDoDto>> GetOutOfSpecToDoAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsOutOfSpecsDoneDto>> GetOutOfSpecDoneAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsOutOfSpecsToDoDto>> GetOutOfSpecSpotsToDoAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsOutOfSpecsDoneDto>> GetOutOfSpecSpotsDoneAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<List<string>> GetOutOfSpecSpotsToDoInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<List<string>> GetOutOfSpecSpotsDoneInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsOutOfSpecReasonCodeDto>> GetSpotExceptionsOutOfSpecReasonCodesAsync();

        Task<List<string>> GetOutOfSpecSpotsToDoMarketsAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<List<string>> GetOutOfSpecSpotsDoneMarketsAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        Task<List<ProgramNameDto>> FindProgramFromProgramsAsync(string programSearchString);

        Task<List<ProgramNameDto>> FindProgramFromSpotExceptionDecisionsAsync(string programSearchString);

        Task<List<Guid?>> GetOutOfSpecSpotsToDoAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<Guid?>> GetOutOfSpecSpotsDoneAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<string>> GetOutOfSpecSpotsToDoStationsAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<string>> GetOutOfSpecSpotsDoneStationsAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<SpotExceptionsOutOfSpecsToDoDto> GetSpotExceptionOutOfSpecByDecisionAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpec);

        Task<int> AddSpotExceptionsOutOfSpecToDoneAsync(SpotExceptionsOutOfSpecsDoneDto todoPlan);

        Task<int> DeleteSpotExceptionsOutOfSpecFromToDoAsync(int spotExceptionOutOfSpecId);

        Task<bool> SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpecDoneDecision, string userName, DateTime decidedAt);
    }

    /// <inheritdoc />
    public class SpotExceptionsOutOfSpecRepository : BroadcastRepositoryBase, ISpotExceptionsOutOfSpecRepository
    {
        public SpotExceptionsOutOfSpecRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        public async Task<List<SpotExceptionsOutOfSpecsToDoDto>> GetOutOfSpecToDoAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsToDoEntities = context.spot_exceptions_out_of_specs
                    .Where(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecToDoDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.plan)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.plan.campaign)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.plan.plan_versions)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.spot_lengths)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.audience)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecToDoDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecToDoDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsoutOfSpecToDoPosts = spotExceptionsOutOfSpecsToDoEntities.Select(spotExceptionsOutOfSpecToDoEntity => _MapOutOfSpecToDto(spotExceptionsOutOfSpecToDoEntity.SpotExceptionsOutOfSpec, spotExceptionsOutOfSpecToDoEntity.Station)).ToList();
                
                return spotExceptionsoutOfSpecToDoPosts;
            });
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionsOutOfSpecsDoneDto>> GetOutOfSpecDoneAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsDoneEntities = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.plan)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.plan.campaign)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.plan.plan_versions)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.spot_lengths)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.audience)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsOutOfSpecDone = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsoutOfSpecDonePosts = spotExceptionsOutOfSpecsDoneEntities.Select(spotExceptionsOutOfSpecDoneEntity => _MapOutOfSpecDoneToDto(spotExceptionsOutOfSpecDoneEntity.SpotExceptionsOutOfSpecDone, spotExceptionsOutOfSpecDoneEntity.Station)).ToList();
                
                return spotExceptionsoutOfSpecDonePosts;
            });
        }

        public async Task<List<SpotExceptionsOutOfSpecsToDoDto>> GetOutOfSpecSpotsToDoAsync(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsEntities = context.spot_exceptions_out_of_specs
                    .Where(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.recommended_plan_id == planId &&
                            spotExceptionsOutOfSpecDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDb.program_air_time <= weekEndDate)
                    .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan)
                    .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan.campaign)
                    .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan.plan_versions)
                    .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan.plan_versions.Select(x => x.plan_version_dayparts))
                    .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.spot_lengths)
                    .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.audience)
                    .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDb, stationDb) => new { SpotExceptionsoutOfSpec = spotExceptionsOutOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsOutOfSpecPosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station)).ToList();
                return spotExceptionsOutOfSpecPosts;

            });
        }

        public async Task<List<SpotExceptionsOutOfSpecsDoneDto>> GetOutOfSpecSpotsDoneAsync(int planId, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsEntities = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.recommended_plan_id == planId &&
                            spotExceptionsOutOfSpecDoneDb.program_air_time >= startDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= endDate)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.spot_exceptions_out_of_spec_done_decisions)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.plan)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.plan.campaign)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.plan.plan_versions)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.plan.plan_versions.Select(x => x.plan_version_dayparts))
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.spot_lengths)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.audience)
                    .Include(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsoutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsoutOfSpecDonePosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecDoneToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station)).ToList();
                return spotExceptionsoutOfSpecDonePosts;

            });
        }

        public async Task<List<string>> GetOutOfSpecSpotsToDoInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsToDoInventorySources = new List<string>();

            return _InReadUncommitedTransaction(context =>
            {
                 spotExceptionsToDoInventorySources = context.spot_exceptions_out_of_specs
                    .Where(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.recommended_plan_id == planId &&
                            spotExceptionsOutOfSpecDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDb.program_air_time <= weekEndDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.SpotExceptionsOutOfSpec.inventory_source_name ?? "Unknown")
                    .ToList();

                return spotExceptionsToDoInventorySources;

            });
        }

        public async Task<List<string>> GetOutOfSpecSpotsDoneInventorySourcesAsync(int planId, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsDoneInventorySources = new List<string>();

            return _InReadUncommitedTransaction(context =>
            {
                spotExceptionsDoneInventorySources = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.recommended_plan_id == planId &&
                            spotExceptionsOutOfSpecDoneDb.program_air_time >= startDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= endDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.SpotExceptionsOutOfSpec.inventory_source_name ?? "Unknown")
                    .ToList();

                return spotExceptionsDoneInventorySources;

            });
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionsOutOfSpecReasonCodeDto>> GetSpotExceptionsOutOfSpecReasonCodesAsync()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecReasonCodesEntities = context.spot_exceptions_out_of_spec_reason_codes.ToList();

                var spotExceptionsOutOfSpecReasonCodes = spotExceptionsOutOfSpecReasonCodesEntities.Select(spotExceptionsOutOfSpecReasonCodesEntity => new SpotExceptionsOutOfSpecReasonCodeDto
                {
                    Id = spotExceptionsOutOfSpecReasonCodesEntity.id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCodesEntity.reason_code,
                    Reason = spotExceptionsOutOfSpecReasonCodesEntity.reason,
                    Label = spotExceptionsOutOfSpecReasonCodesEntity.label
                }).ToList();

                return spotExceptionsOutOfSpecReasonCodes;
            });
        }

        public async Task<List<string>> GetOutOfSpecSpotsToDoMarketsAsync(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsToDoMarkets = new List<string>();

            return _InReadUncommitedTransaction(context =>
            {
                spotExceptionsToDoMarkets = context.spot_exceptions_out_of_specs
                   .Where(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.recommended_plan_id == planId &&
                           spotExceptionsOutOfSpecDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDb.program_air_time <= weekEndDate)
                   .GroupJoin(
                       context.stations
                       .Include(stationDb => stationDb.market),
                       spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.station_legacy_call_letters,
                       stationDb => stationDb.legacy_call_letters,
                       (spotExceptionsOutOfSpecDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDb, Station = stationDb.FirstOrDefault() })
                   .Select(r => r.Station.market.geography_name ?? "Unknown")
                   .ToList();

                return spotExceptionsToDoMarkets;

            });
        }

        public async Task<List<string>> GetOutOfSpecSpotsDoneMarketsAsync(int planId, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsDoneMarkets = new List<string>();

            return _InReadUncommitedTransaction(context =>
            {
                spotExceptionsDoneMarkets = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.recommended_plan_id == planId &&
                            spotExceptionsOutOfSpecDoneDb.program_air_time >= startDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= endDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.Station.market.geography_name ?? "Unknown")
                    .ToList();

                return spotExceptionsDoneMarkets;

            });
        }

        /// <inheritdoc />
        public async Task<List<ProgramNameDto>> FindProgramFromProgramsAsync(string programSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.programs
                        .Where(p => p.name.ToLower().Contains(programSearchString.ToLower()))
                        .OrderBy(p => p.name)
                        .Select(
                            p => new ProgramNameDto
                            {
                                OfficialProgramName = p.name,
                                GenreId = p.genre_id
                            }).ToList();
                });
        }

        /// <inheritdoc />
        public async Task<List<ProgramNameDto>> FindProgramFromSpotExceptionDecisionsAsync(string programSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.spot_exceptions_out_of_spec_done_decisions
                        .Where(p => p.program_name.ToLower().Contains(programSearchString.ToLower()))
                        .OrderBy(p => p.program_name)
                        .Select(
                            p => new ProgramNameDto
                            {
                                OfficialProgramName = p.program_name,
                                GenreId = context.genres.FirstOrDefault(x => x.name == p.genre_name).id
                            }).ToList();
                });
        }

        public async Task<List<Guid?>> GetOutOfSpecSpotsToDoAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<Guid?> spotExceptionsToDoAdvertisers = null;

            return _InReadUncommitedTransaction(context =>
            {
                spotExceptionsToDoAdvertisers = context.spot_exceptions_out_of_specs
                   .Where(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecToDoDb.program_air_time <= weekEndDate)
                   .GroupJoin(
                       context.stations
                       .Include(stationDb => stationDb.market),
                       spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.station_legacy_call_letters,
                       stationDb => stationDb.legacy_call_letters,
                       (spotExceptionsOutOfSpecToDoDb, stationDb) => new { SpotExceptionsOutOfSpecToDo = spotExceptionsOutOfSpecToDoDb, Station = stationDb.FirstOrDefault() })
                   .Select(r => r.SpotExceptionsOutOfSpecToDo.plan.campaign.advertiser_master_id)
                   .ToList();

                return spotExceptionsToDoAdvertisers;

            });
        }

        public async Task<List<Guid?>> GetOutOfSpecSpotsDoneAdvertisersAsync(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);
            List<Guid?> spotExceptionsDoneAdvertisers = null;

            return _InReadUncommitedTransaction(context =>
            {
                spotExceptionsDoneAdvertisers = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.program_air_time >= startDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= endDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.SpotExceptionsOutOfSpec.plan.campaign.advertiser_master_id)
                    .ToList();

                return spotExceptionsDoneAdvertisers;

            });
        }

        public async Task<List<string>> GetOutOfSpecSpotsToDoStationsAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsToDoStations = null;

            return _InReadUncommitedTransaction(context =>
            {
                spotExceptionsToDoStations = context.spot_exceptions_out_of_specs
                   .Where(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecToDoDb.program_air_time <= weekEndDate)
                   .GroupJoin(
                       context.stations
                       .Include(stationDb => stationDb.market),
                       spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.station_legacy_call_letters,
                       stationDb => stationDb.legacy_call_letters,
                       (spotExceptionsOutOfSpecToDoDb, stationDb) => new { SpotExceptionsOutOfSpecToDo = spotExceptionsOutOfSpecToDoDb, Station = stationDb.FirstOrDefault() })
                   .Select(r => r.Station.station_call_letters ?? "Unknown")
                   .ToList();

                return spotExceptionsToDoStations;

            });
        }

        public async Task<List<string>> GetOutOfSpecSpotsDoneStationsAsync(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsDoneAdvertisers = null;

            return _InReadUncommitedTransaction(context =>
            {
                spotExceptionsDoneAdvertisers = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.program_air_time >= startDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= endDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.Station.legacy_call_letters ?? "Unknown")
                    .ToList();

                return spotExceptionsDoneAdvertisers;

            });
        }

        public async Task<SpotExceptionsOutOfSpecsToDoDto> GetSpotExceptionOutOfSpecByDecisionAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpec)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecToDoEntities = context.spot_exceptions_out_of_specs
                    .Where(o => o.id == spotExceptionsOutOfSpec.Id)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.plan)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.plan.campaign)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.plan.plan_versions)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.spot_lengths)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.audience)
                    .Include(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.spot_exceptions_out_of_spec_reason_codes)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecToDoDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecToDoDb, Station = stationDb.FirstOrDefault() }).ToList()
                    .ToList();

                var spotExceptionsoutOfSpecToDoPosts = spotExceptionsOutOfSpecToDoEntities.Select(spotExceptionsOutOfSpecToDoEntity => _MapOutOfSpecToDto(spotExceptionsOutOfSpecToDoEntity.SpotExceptionsOutOfSpec, spotExceptionsOutOfSpecToDoEntity.Station))
                    .First();

                return spotExceptionsoutOfSpecToDoPosts;
            });
        }

        public Task<int> AddSpotExceptionsOutOfSpecToDoneAsync(SpotExceptionsOutOfSpecsDoneDto todoPlan)
        {
            return _InReadUncommitedTransaction(async context =>
            {
                var doneEntities = _MapOutOfSpecEntity(todoPlan);

                var countSaved = await context.SaveChangesAsync();

                return countSaved;
            });
        }

        public Task<int> DeleteSpotExceptionsOutOfSpecFromToDoAsync(int spotExceptionOutOfSpecId)
        {
            int deleteCount = 0;

            return _InReadUncommitedTransaction(async context =>
            {
                var outOfSpecIdToDelete = context.spot_exceptions_out_of_specs.First(d => d.id == spotExceptionOutOfSpecId);

                if (outOfSpecIdToDelete != null)
                {
                    var result = context.spot_exceptions_out_of_specs.Remove(outOfSpecIdToDelete);
                }

                deleteCount = await context.SaveChangesAsync();
                return deleteCount;
            });
        }

        public async Task<bool> SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpecDoneDecision, string userName, DateTime decidedAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var alreadyRecordExists = context.spot_exceptions_out_of_spec_done_decisions.SingleOrDefault(x =>
                    x.spot_exceptions_out_of_spec_done_id == spotExceptionsOutOfSpecDoneDecision.Id);

                var outOfSpecId = context.spot_exceptions_out_of_specs.SingleOrDefault(x =>
                    x.id == spotExceptionsOutOfSpecDoneDecision.Id);

                if (!(string.IsNullOrEmpty(spotExceptionsOutOfSpecDoneDecision.ProgramName) && string.IsNullOrEmpty(spotExceptionsOutOfSpecDoneDecision.GenreName) && string.IsNullOrEmpty(spotExceptionsOutOfSpecDoneDecision.DaypartCode)))
                {
                    if (alreadyRecordExists == null)
                    {
                        context.spot_exceptions_out_of_spec_done_decisions.Add(new spot_exceptions_out_of_spec_done_decisions
                        {
                            spot_exceptions_out_of_spec_done_id = spotExceptionsOutOfSpecDoneDecision.Id,
                            accepted_as_in_spec = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec,
                            decision_notes = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec ? "In" : "Out",
                            decided_by = userName,
                            decided_at = decidedAt,
                            program_name = spotExceptionsOutOfSpecDoneDecision.ProgramName,
                            genre_name = spotExceptionsOutOfSpecDoneDecision.GenreName,
                            daypart_code = spotExceptionsOutOfSpecDoneDecision.DaypartCode
                        });
                    }
                    else
                    {
                        alreadyRecordExists.accepted_as_in_spec = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec;
                        alreadyRecordExists.decided_by = userName;
                        alreadyRecordExists.decided_at = decidedAt;
                        alreadyRecordExists.decision_notes = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec ? "In" : "Out";
                        alreadyRecordExists.synced_at = null;
                        alreadyRecordExists.synced_by = null;
                        alreadyRecordExists.program_name = spotExceptionsOutOfSpecDoneDecision.ProgramName;
                        alreadyRecordExists.genre_name = spotExceptionsOutOfSpecDoneDecision.GenreName;
                        alreadyRecordExists.daypart_code = spotExceptionsOutOfSpecDoneDecision.DaypartCode;
                    }
                }
                else if (spotExceptionsOutOfSpecDoneDecision.Comments == null)
                {
                    if (alreadyRecordExists == null)
                    {
                        context.spot_exceptions_out_of_spec_done_decisions.Add(new spot_exceptions_out_of_spec_done_decisions
                        {
                            spot_exceptions_out_of_spec_done_id = spotExceptionsOutOfSpecDoneDecision.Id,
                            accepted_as_in_spec = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec,
                            decision_notes = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec ? "In" : "Out",
                            decided_by = userName,
                            decided_at = decidedAt
                        });
                    }
                    else
                    {
                        alreadyRecordExists.accepted_as_in_spec = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec;
                        alreadyRecordExists.decided_by = userName;
                        alreadyRecordExists.decided_at = decidedAt;
                        alreadyRecordExists.decision_notes = spotExceptionsOutOfSpecDoneDecision.AcceptAsInSpec ? "In" : "Out";
                        alreadyRecordExists.synced_at = null;
                        alreadyRecordExists.synced_by = null;
                    }
                }
                else
                {
                    outOfSpecId.comment = spotExceptionsOutOfSpecDoneDecision.Comments;
                }

                bool isSpotExceptionsOutOfSpecDecisionSaved = false;
                int recordCount = 0;
                recordCount = context.SaveChanges();
                if (recordCount > 0)
                {
                    isSpotExceptionsOutOfSpecDecisionSaved = true;
                }

                return isSpotExceptionsOutOfSpecDecisionSaved;
            });
        }

        private SpotExceptionsOutOfSpecsToDoDto _MapOutOfSpecToDto(spot_exceptions_out_of_specs spotExceptionsOutOfSpecToDoEntity, station stationEntity)
        {
            var planVersion = spotExceptionsOutOfSpecToDoEntity.plan?.plan_versions.First(v => v.id == spotExceptionsOutOfSpecToDoEntity.plan.latest_version_id);

            var spotExceptionsOutOfSpec = new SpotExceptionsOutOfSpecsToDoDto
            {
                Id = spotExceptionsOutOfSpecToDoEntity.id,
                SpotUniqueHashExternal = spotExceptionsOutOfSpecToDoEntity.spot_unique_hash_external,
                ReasonCodeMessage = spotExceptionsOutOfSpecToDoEntity.reason_code_message,
                EstimateId = spotExceptionsOutOfSpecToDoEntity.estimate_id,
                IsciName = spotExceptionsOutOfSpecToDoEntity.isci_name,
                HouseIsci = spotExceptionsOutOfSpecToDoEntity.house_isci,
                RecommendedPlanId = spotExceptionsOutOfSpecToDoEntity.recommended_plan_id,
                RecommendedPlanName = spotExceptionsOutOfSpecToDoEntity.plan?.name,
                ProgramName = spotExceptionsOutOfSpecToDoEntity.program_name,
                StationLegacyCallLetters = spotExceptionsOutOfSpecToDoEntity.station_legacy_call_letters,
                DaypartCode = spotExceptionsOutOfSpecToDoEntity.daypart_code,
                GenreName = spotExceptionsOutOfSpecToDoEntity.genre_name,
                Affiliate = stationEntity?.affiliation,
                Market = stationEntity?.market?.geography_name,
                SpotLength = _MapSpotLengthToDto(spotExceptionsOutOfSpecToDoEntity.spot_lengths),
                Audience = _MapAudienceToDto(spotExceptionsOutOfSpecToDoEntity.audience),
                ProgramAirTime = spotExceptionsOutOfSpecToDoEntity.program_air_time,
                IngestedBy = spotExceptionsOutOfSpecToDoEntity.ingested_by,
                IngestedAt = spotExceptionsOutOfSpecToDoEntity.ingested_at,
                IngestedMediaWeekId = spotExceptionsOutOfSpecToDoEntity.ingested_media_week_id,
                Impressions = spotExceptionsOutOfSpecToDoEntity.impressions,
                PlanId = spotExceptionsOutOfSpecToDoEntity.recommended_plan_id ?? 0,
                FlightStartDate = planVersion?.flight_start_date,
                FlightEndDate = planVersion?.flight_end_date,
                AdvertiserMasterId = spotExceptionsOutOfSpecToDoEntity.plan?.campaign.advertiser_master_id,
                Product = null,
                SpotExceptionsOutOfSpecReasonCode = _MapSpotExceptionsOutOfSpecReasonCodeToDto(spotExceptionsOutOfSpecToDoEntity.spot_exceptions_out_of_spec_reason_codes),
                MarketCode = spotExceptionsOutOfSpecToDoEntity.market_code,
                MarketRank = spotExceptionsOutOfSpecToDoEntity.market_rank,
                Comments = spotExceptionsOutOfSpecToDoEntity.comment,
                InventorySourceName = spotExceptionsOutOfSpecToDoEntity.inventory_source_name
            };
            return spotExceptionsOutOfSpec;
        }

        private SpotExceptionsOutOfSpecsDoneDto _MapOutOfSpecDoneToDto(spot_exceptions_out_of_specs_done spotExceptionsOutOfSpecDoneEntity, station stationEntity)
        {
            var planVersion = spotExceptionsOutOfSpecDoneEntity.plan?.plan_versions.First(v => v.id == spotExceptionsOutOfSpecDoneEntity.plan.latest_version_id);

            var spotExceptionsOutOfSpec = new SpotExceptionsOutOfSpecsDoneDto
            {
                Id = spotExceptionsOutOfSpecDoneEntity.id,
                SpotUniqueHashExternal = spotExceptionsOutOfSpecDoneEntity.spot_unique_hash_external,
                ReasonCodeMessage = spotExceptionsOutOfSpecDoneEntity.reason_code_message,
                EstimateId = spotExceptionsOutOfSpecDoneEntity.estimate_id,
                IsciName = spotExceptionsOutOfSpecDoneEntity.isci_name,
                HouseIsci = spotExceptionsOutOfSpecDoneEntity.house_isci,
                RecommendedPlanId = spotExceptionsOutOfSpecDoneEntity.recommended_plan_id,
                RecommendedPlanName = spotExceptionsOutOfSpecDoneEntity.plan?.name,
                ProgramName = spotExceptionsOutOfSpecDoneEntity.program_name,
                StationLegacyCallLetters = spotExceptionsOutOfSpecDoneEntity.station_legacy_call_letters,
                DaypartCode = spotExceptionsOutOfSpecDoneEntity.daypart_code,
                GenreName = spotExceptionsOutOfSpecDoneEntity.genre_name,
                Affiliate = stationEntity?.affiliation,
                Market = stationEntity?.market?.geography_name,
                SpotLength = _MapSpotLengthToDto(spotExceptionsOutOfSpecDoneEntity.spot_lengths),
                Audience = _MapAudienceToDto(spotExceptionsOutOfSpecDoneEntity.audience),
                ProgramAirTime = spotExceptionsOutOfSpecDoneEntity.program_air_time,
                IngestedBy = spotExceptionsOutOfSpecDoneEntity.ingested_by,
                IngestedAt = spotExceptionsOutOfSpecDoneEntity.ingested_at,
                IngestedMediaWeekId = spotExceptionsOutOfSpecDoneEntity.ingested_media_week_id,
                Impressions = spotExceptionsOutOfSpecDoneEntity.impressions,
                PlanId = spotExceptionsOutOfSpecDoneEntity.recommended_plan_id ?? 0,
                FlightStartDate = planVersion?.flight_start_date,
                FlightEndDate = planVersion?.flight_end_date,
                AdvertiserMasterId = spotExceptionsOutOfSpecDoneEntity.plan?.campaign.advertiser_master_id,
                Product = null,
                SpotExceptionsOutOfSpecReasonCode = _MapSpotExceptionsOutOfSpecReasonCodeToDto(spotExceptionsOutOfSpecDoneEntity.spot_exceptions_out_of_spec_reason_codes),
                MarketCode = spotExceptionsOutOfSpecDoneEntity.market_code,
                MarketRank = spotExceptionsOutOfSpecDoneEntity.market_rank,
                Comments = spotExceptionsOutOfSpecDoneEntity.comment,
                InventorySourceName = spotExceptionsOutOfSpecDoneEntity.inventory_source_name
            };
            return spotExceptionsOutOfSpec;
        }

        private spot_exceptions_out_of_specs_done _MapOutOfSpecEntity(SpotExceptionsOutOfSpecsDoneDto outOfSpecDone)
        {
            var spotExceptionsOutOfSpec = new spot_exceptions_out_of_specs_done
            {
                id = outOfSpecDone.Id,
                spot_unique_hash_external = outOfSpecDone.SpotUniqueHashExternal,
                reason_code_message = outOfSpecDone.ReasonCodeMessage,
                estimate_id = outOfSpecDone.EstimateId,
                isci_name = outOfSpecDone.IsciName,
                house_isci = outOfSpecDone.HouseIsci,
                recommended_plan_id = outOfSpecDone.RecommendedPlanId,
                program_name = outOfSpecDone.ProgramName,
                station_legacy_call_letters = outOfSpecDone.StationLegacyCallLetters,
                daypart_code = outOfSpecDone.DaypartCode,
                genre_name = outOfSpecDone.GenreName,
                spot_length_id = outOfSpecDone.SpotLength.Id,
                audience_id = outOfSpecDone.Audience.Id,
                program_air_time = outOfSpecDone.ProgramAirTime,
                ingested_by = outOfSpecDone.IngestedBy,
                ingested_at = outOfSpecDone.IngestedAt,
                ingested_media_week_id = outOfSpecDone.IngestedMediaWeekId,
                impressions = outOfSpecDone.Impressions,
                reason_code_id = outOfSpecDone.SpotExceptionsOutOfSpecReasonCode.Id,
                market_code = outOfSpecDone.MarketCode,
                market_rank = outOfSpecDone.MarketRank,
                comment = outOfSpecDone.Comments,
                inventory_source_name = outOfSpecDone.InventorySourceName
            };
            return spotExceptionsOutOfSpec;
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

        private AudienceDto _MapAudienceToDto(audience audienceEntity)
        {
            if (audienceEntity == null)
            {
                return null;
            }

            var audience = new AudienceDto
            {
                Id = audienceEntity.id,
                Code = audienceEntity.code,
                Name = audienceEntity.name
            };
            return audience;
        }

        private SpotExceptionsOutOfSpecReasonCodeDto _MapSpotExceptionsOutOfSpecReasonCodeToDto(spot_exceptions_out_of_spec_reason_codes spotExceptionsOutOfSpecReasonCodesEntity)
        {
            if (spotExceptionsOutOfSpecReasonCodesEntity == null)
            {
                return null;
            }

            var spotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
            {
                Id = spotExceptionsOutOfSpecReasonCodesEntity.id,
                ReasonCode = spotExceptionsOutOfSpecReasonCodesEntity.reason_code,
                Reason = spotExceptionsOutOfSpecReasonCodesEntity.reason,
                Label = spotExceptionsOutOfSpecReasonCodesEntity.label
            };
            return spotExceptionsOutOfSpecReasonCode;
        }







        //    return _InReadUncommitedTransaction(context =>
        //    {
        //        foreach (var spotExceptionsOutOfSpecId in spotExceptionSaveDecisionsPlansRequest.Decisions)
        //        {
        //            var alreadyRecordExists = context.spot_exceptions_out_of_spec_decisions.SingleOrDefault(x =>
        //                x.spot_exceptions_out_of_spec_id == spotExceptionsOutOfSpecId.Id);
        //            var outOfSpecId = context.spot_exceptions_out_of_specs.SingleOrDefault(x =>
        //                x.id == spotExceptionsOutOfSpecId.Id);
        //            if (!(string.IsNullOrEmpty(spotExceptionsOutOfSpecId.ProgramName) && string.IsNullOrEmpty(spotExceptionsOutOfSpecId.GenreName) && string.IsNullOrEmpty(spotExceptionsOutOfSpecId.DaypartCode)))
        //            {
        //                if (alreadyRecordExists == null)
        //                {
        //                    context.spot_exceptions_out_of_spec_decisions.Add(new spot_exceptions_out_of_spec_decisions
        //                    {
        //                        spot_exceptions_out_of_spec_id = spotExceptionsOutOfSpecId.Id,
        //                        accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec,
        //                        decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out",
        //                        username = userName,
        //                        created_at = createdAt,
        //                        program_name = spotExceptionsOutOfSpecId.ProgramName,
        //                        genre_name = spotExceptionsOutOfSpecId.GenreName,
        //                        daypart_code = spotExceptionsOutOfSpecId.DaypartCode
        //                    });
        //                }
        //                else
        //                {
        //                    alreadyRecordExists.accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec;
        //                    alreadyRecordExists.username = userName;
        //                    alreadyRecordExists.created_at = createdAt;
        //                    alreadyRecordExists.decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out";
        //                    alreadyRecordExists.synced_at = null;
        //                    alreadyRecordExists.synced_by = null;
        //                    alreadyRecordExists.program_name = spotExceptionsOutOfSpecId.ProgramName;
        //                    alreadyRecordExists.genre_name = spotExceptionsOutOfSpecId.GenreName;
        //                    alreadyRecordExists.daypart_code = spotExceptionsOutOfSpecId.DaypartCode;
        //                }
        //            }
        //            else if (spotExceptionsOutOfSpecId.Comments == null)
        //            {
        //                if (alreadyRecordExists == null)
        //                {
        //                    context.spot_exceptions_out_of_spec_decisions.Add(new spot_exceptions_out_of_spec_decisions
        //                    {
        //                        spot_exceptions_out_of_spec_id = spotExceptionsOutOfSpecId.Id,
        //                        accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec,
        //                        decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out",
        //                        username = userName,
        //                        created_at = createdAt
        //                    });
        //                }
        //                else
        //                {
        //                    alreadyRecordExists.accepted_as_in_spec = spotExceptionsOutOfSpecId.AcceptAsInSpec;
        //                    alreadyRecordExists.username = userName;
        //                    alreadyRecordExists.created_at = createdAt;
        //                    alreadyRecordExists.decision_notes = spotExceptionsOutOfSpecId.AcceptAsInSpec ? "In" : "Out";
        //                    alreadyRecordExists.synced_at = null;
        //                    alreadyRecordExists.synced_by = null;
        //                }
        //            }
        //            else
        //            {
        //                outOfSpecId.comment = spotExceptionsOutOfSpecId.Comments;
        //            }
        //        }

        //        bool isSpotExceptionsOutOfSpecDecisionSaved = false;
        //        int recordCount = 0;
        //        recordCount = context.SaveChanges();
        //        if (recordCount > 0)
        //        {
        //            isSpotExceptionsOutOfSpecDecisionSaved = true;
        //        }

        //        return isSpotExceptionsOutOfSpecDecisionSaved;
        //    });
        //}
    }
}
