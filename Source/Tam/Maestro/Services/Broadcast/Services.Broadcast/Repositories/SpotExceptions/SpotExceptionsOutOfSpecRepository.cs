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
using Common.Services.Extensions;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecRepository : IDataRepository
    {
        /// <summary>
        /// Gets the out of spec grouping to do asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecGroupingToDoAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec grouping done asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecGroupingDoneAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots to do asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecsToDoDto>> GetOutOfSpecSpotsToDoAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecsDoneDto>> GetOutOfSpecSpotsDoneAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots to do inventory sources asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotsToDoInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done inventory sources asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotsDoneInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exceptions out of spec reason codes asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecReasonCodeDto>> GetSpotExceptionsOutOfSpecReasonCodesAsync();

        /// <summary>
        /// Gets the out of spec spots to do markets asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotsToDoMarketsAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done markets asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotsDoneMarketsAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Finds the program from programs asynchronous.
        /// </summary>
        /// <param name="programSearchString">The program search string.</param>
        /// <returns></returns>
        Task<List<ProgramNameDto>> FindProgramFromProgramsAsync(string programSearchString);

        /// <summary>
        /// Finds the program from spot exception decisions asynchronous.
        /// </summary>
        /// <param name="programSearchString">The program search string.</param>
        /// <returns></returns>
        Task<List<ProgramNameDto>> FindProgramFromSpotExceptionDecisionsAsync(string programSearchString);

        /// <summary>
        /// Gets the out of spec spots to do advertisers asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<Guid?>> GetOutOfSpecSpotsToDoAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done advertisers asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<Guid?>> GetOutOfSpecSpotsDoneAdvertisersAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots to do stations asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotsToDoStationsAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done stations asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotsDoneStationsAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spot.
        /// </summary>
        /// <param name="todoId">The todo identifier.</param>
        /// <returns>
        /// </returns>
        Task<List<SpotExceptionsOutOfSpecsToDoDto>> GetOutOfSpecSpotsToDoByIds(List<int?> todoId);

        /// <summary>
        /// Adds the out of spec to done.
        /// </summary>
        /// <param name="doneOutOfSpecsToAdd">The done out of spec to add.</param>
        void AddOutOfSpecToDone(List<SpotExceptionsOutOfSpecsDoneDto> doneOutOfSpecsToAdd);

        /// <summary>
        /// Adds the out of spec edited to done.
        /// </summary>
        /// <param name="doneOutOfSpecsToAdd">The done out of specs to add.</param>
        void AddOutOfSpecEditedToDone(List<SpotExceptionsOutOfSpecsDoneDto> doneOutOfSpecsToAdd);

        /// <summary>
        /// Deletes the out of specs from to do.
        /// </summary>
        /// <param name="existingOutOfSpecsToDo">The existing out of specs to do.</param>
        void DeleteOutOfSpecsFromToDo(List<int> existingOutOfSpecsToDo);

        /// <summary>
        /// Saves the out of spec comments asynchronous.
        /// </summary>
        /// <param name="OutOfSpecsCommentsToAdd">The spot exceptions out of spec.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="decidedAt">The decided at.</param>
        /// <returns>
        /// </returns>
        Task<bool> SaveOutOfSpecCommentsAsync(List<SpotExceptionOutOfSpecCommentsDto> OutOfSpecsCommentsToAdd, string userName, DateTime decidedAt);

        /// <summary>
        /// Saves the spot exceptions out of spec done decisions asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecDoneDecisions">The spot exceptions out of spec done decisions.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="decidedAt">The decided at.</param>
        /// <returns></returns>
        Task<bool> SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(List<SpotExceptionsOutOfSpecDoneDecisionsDto> spotExceptionsOutOfSpecDoneDecisions, string userName, DateTime decidedAt);

        /// <summary>
        /// get the done plans by id
        /// </summary>
        /// <param name="doneId">done id</param>
        /// <returns>Done plans by id</returns>
        Task<List<SpotExceptionsOutOfSpecsDoneDto>> GetOutOfSpecSpotsDoneByIds(List<int?> doneId);
    }

    /// <inheritdoc />
    public class SpotExceptionsOutOfSpecRepository : BroadcastRepositoryBase, ISpotExceptionsOutOfSpecRepository
    {
        public SpotExceptionsOutOfSpecRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        /// <inheritdoc />
        public Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecGroupingToDoAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var outOfSpecDetailsToDo = context.spot_exceptions_out_of_specs
                    .Where(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.program_air_time >= weekStartDate
                    && spotExceptionsOutOfSpecToDoDb.program_air_time <= weekEndDate).ToList();

                var outOfSpecGroupingToDo = outOfSpecDetailsToDo.GroupBy(x => new { x.recommended_plan_id })
                    .Select(x =>
                    {
                        var first = x.First();
                        var recommendedPlanVersion = first.plan.plan_versions.Single(planVersion => planVersion.id == first.plan.latest_version_id);
                        var audience = first.audience;
                        return new SpotExceptionsOutOfSpecGroupingDto
                        {
                            PlanId = x.Key.recommended_plan_id ?? default,
                            AdvertiserMasterId = first.plan.campaign.advertiser_master_id,
                            PlanName = first.plan.name,
                            AffectedSpotsCount = x.Count(),
                            Impressions = x.Sum(y => y.impressions),
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AudienceName = _GetAudienceName(audience)
                        };
                    }).ToList();

                return outOfSpecGroupingToDo;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecGroupingDoneAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var outOfSpecDetailsDone = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.program_air_time >= weekStartDate
                    && spotExceptionsOutOfSpecDoneDb.program_air_time <= weekEndDate).Include(x => x.spot_exceptions_out_of_spec_done_decisions).ToList();

                var outOfSpecGroupingDone = outOfSpecDetailsDone.GroupBy(x => new { x.recommended_plan_id })
                    .Select(x =>
                    {
                        var first = x.First();
                        var recommendedPlanVersion = first.plan.plan_versions.Single(planVersion => planVersion.id == first.plan.latest_version_id);
                        var audience = first.audience;
                        var decisions = x.Select(y => y.spot_exceptions_out_of_spec_done_decisions).ToList();
                        return new SpotExceptionsOutOfSpecGroupingDto
                        {
                            PlanId = x.Key.recommended_plan_id ?? default,
                            AdvertiserMasterId = first.plan.campaign.advertiser_master_id,
                            PlanName = first.plan.name,
                            AffectedSpotsCount = x.Count(),
                            Impressions = x.Sum(y => y.impressions),
                            SyncedTimestamp = decisions.Max(d => d.Max(m => m.synced_at)),
                            FlightStartDate = recommendedPlanVersion.flight_start_date,
                            FlightEndDate = recommendedPlanVersion.flight_end_date,
                            SpotLengths = recommendedPlanVersion.plan_version_creative_lengths.Select(planVersionCreativeLength => _MapSpotLengthToDto(planVersionCreativeLength.spot_lengths)).ToList(),
                            AudienceName = _GetAudienceName(audience)
                        };
                    }).ToList();

                return outOfSpecGroupingDone;
            }));
        }

        /// <inheritdoc />
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
                var lstExecutionIdExternal = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpec.execution_id_external);
                var lstHashExternal = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpec.spot_unique_hash_external);
                var lstIsciName = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpec.isci_name).ToList();
                var lstProgramAir = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpec.program_air_time);
                var lstStationLegacy = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpec.station_legacy_call_letters);
                var lstReasonCode = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpec.reason_code_id);
                var lstRecomPlans = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpec.recommended_plan_id);
                var outOfSpecComment = context.spot_exceptions_out_of_spec_comments.Where(x => lstExecutionIdExternal.Contains(x.execution_id_external) &&
                                                                                      lstHashExternal.Contains(x.spot_unique_hash_external) &&
                                                                                      lstIsciName.Contains(x.isci_name) &&
                                                                                      lstProgramAir.Contains(x.program_air_time) &&
                                                                                      lstStationLegacy.Contains(x.station_legacy_call_letters) &&
                                                                                      lstReasonCode.Contains(x.reason_code_id) &&
                                                                                      lstRecomPlans.Contains(x.recommended_plan_id)).ToList();
                var spotExceptionsOutOfSpecPosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecToDoToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station, outOfSpecComment)).ToList();
                return spotExceptionsOutOfSpecPosts;

            });
        }

        /// <inheritdoc />
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
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsoutOfSpecDone = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var lstExecutionIdExternal = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpecDone.execution_id_external);
                var lstHashExternal = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpecDone.spot_unique_hash_external);
                var lstIsciName = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpecDone.isci_name).ToList();
                var lstProgramAir = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpecDone.program_air_time);
                var lstStationLegacy = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpecDone.station_legacy_call_letters);
                var lstReasonCode = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpecDone.reason_code_id);
                var lstRecomPlans = spotExceptionsOutOfSpecsEntities.Select(x => x.SpotExceptionsoutOfSpecDone.recommended_plan_id);
                var outOfSpecComment = context.spot_exceptions_out_of_spec_comments.Where(x => lstExecutionIdExternal.Contains(x.execution_id_external) &&
                                                                                      lstHashExternal.Contains(x.spot_unique_hash_external) &&
                                                                                      lstIsciName.Contains(x.isci_name) &&
                                                                                      lstProgramAir.Contains(x.program_air_time) &&
                                                                                      lstStationLegacy.Contains(x.station_legacy_call_letters) &&
                                                                                      lstReasonCode.Contains(x.reason_code_id) &&
                                                                                      lstRecomPlans.Contains(x.recommended_plan_id)).ToList();
                var spotExceptionsoutOfSpecDonePosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecDoneToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpecDone, spotExceptionsOutOfSpecEntity.Station, outOfSpecComment)).ToList();
                return spotExceptionsoutOfSpecDonePosts;

            });
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                        .Distinct()
                        .Select(
                            p => new ProgramNameDto
                            {
                                OfficialProgramName = p.program_name,
                                GenreId = p.genre_name == null ? context.genres.FirstOrDefault(x => x.name == p.genre_name).id : (int?)null
                            }).ToList();
                });
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Task<List<SpotExceptionsOutOfSpecsToDoDto>> GetOutOfSpecSpotsToDoByIds(List<int?> todoId)
        {
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var foundOutOfSpecTodoEntities = context.spot_exceptions_out_of_specs
                    .Where(x => todoId.Contains(x.id))
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
                var lstExecutionIdExternal = foundOutOfSpecTodoEntities.Select(x => x.SpotExceptionsoutOfSpec.execution_id_external);
                var lstHashExternal = foundOutOfSpecTodoEntities.Select(x => x.SpotExceptionsoutOfSpec.spot_unique_hash_external);
                var lstIsciName = foundOutOfSpecTodoEntities.Select(x => x.SpotExceptionsoutOfSpec.isci_name).ToList();
                var lstProgramAir = foundOutOfSpecTodoEntities.Select(x => x.SpotExceptionsoutOfSpec.program_air_time);
                var lstStationLegacy = foundOutOfSpecTodoEntities.Select(x => x.SpotExceptionsoutOfSpec.station_legacy_call_letters);
                var lstReasonCode = foundOutOfSpecTodoEntities.Select(x => x.SpotExceptionsoutOfSpec.reason_code_id);
                var lstRecomPlans = foundOutOfSpecTodoEntities.Select(x => x.SpotExceptionsoutOfSpec.recommended_plan_id);
                var outOfSpecComment = context.spot_exceptions_out_of_spec_comments.Where(x => lstExecutionIdExternal.Contains(x.execution_id_external) &&
                                                                                      lstHashExternal.Contains(x.spot_unique_hash_external)&&
                                                                                      lstIsciName.Contains(x.isci_name)&&
                                                                                      lstProgramAir.Contains(x.program_air_time)&&
                                                                                      lstStationLegacy.Contains(x.station_legacy_call_letters)&&
                                                                                      lstReasonCode.Contains(x.reason_code_id)&&
                                                                                      lstRecomPlans.Contains(x.recommended_plan_id)).ToList();
           
                var OutOfSpecToDo = foundOutOfSpecTodoEntities.Select(foundOutOfSpecTodoEntity => _MapOutOfSpecToDoToDto(foundOutOfSpecTodoEntity.SpotExceptionsoutOfSpec, foundOutOfSpecTodoEntity.Station, outOfSpecComment)).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details ToDo");
                return OutOfSpecToDo;
            }));
        }

        /// <inheritdoc />
        public Task<List<SpotExceptionsOutOfSpecsDoneDto>> GetOutOfSpecSpotsDoneByIds(List<int?> doneId)
        {
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var foundOutOfSpecDoneEntities = context.spot_exceptions_out_of_specs_done
                    .Where(x => doneId.Contains(x.id))
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
                        (spotExceptionsOutOfSpecDb, stationDb) => new { SpotExceptionsoutOfSpecDone = spotExceptionsOutOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .ToList();
                var lstExecutionIdExternal = foundOutOfSpecDoneEntities.Select(x => x.SpotExceptionsoutOfSpecDone.execution_id_external);
                var lstHashExternal = foundOutOfSpecDoneEntities.Select(x => x.SpotExceptionsoutOfSpecDone.spot_unique_hash_external);
                var lstIsciName = foundOutOfSpecDoneEntities.Select(x => x.SpotExceptionsoutOfSpecDone.isci_name).ToList();
                var lstProgramAir = foundOutOfSpecDoneEntities.Select(x => x.SpotExceptionsoutOfSpecDone.program_air_time);
                var lstStationLegacy = foundOutOfSpecDoneEntities.Select(x => x.SpotExceptionsoutOfSpecDone.station_legacy_call_letters);
                var lstReasonCode = foundOutOfSpecDoneEntities.Select(x => x.SpotExceptionsoutOfSpecDone.reason_code_id);
                var lstRecomPlans = foundOutOfSpecDoneEntities.Select(x => x.SpotExceptionsoutOfSpecDone.recommended_plan_id);
                var outOfSpecComment = context.spot_exceptions_out_of_spec_comments.Where(x => lstExecutionIdExternal.Contains(x.execution_id_external) &&
                                                                                      lstHashExternal.Contains(x.spot_unique_hash_external) &&
                                                                                      lstIsciName.Contains(x.isci_name) &&
                                                                                      lstProgramAir.Contains(x.program_air_time) &&
                                                                                      lstStationLegacy.Contains(x.station_legacy_call_letters) &&
                                                                                      lstReasonCode.Contains(x.reason_code_id) &&
                                                                                      lstRecomPlans.Contains(x.recommended_plan_id)).ToList();

                var OutOfSpecDone = foundOutOfSpecDoneEntities.Select(foundOutOfSpecTodoEntity => _MapOutOfSpecDoneToDto(foundOutOfSpecTodoEntity.SpotExceptionsoutOfSpecDone, foundOutOfSpecTodoEntity.Station, outOfSpecComment)).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details ToDo");
                return OutOfSpecDone;
            }));
        }

        /// <inheritdoc />
        public void AddOutOfSpecToDone(List<SpotExceptionsOutOfSpecsDoneDto> doneOutOfSpecsToAdd)
        {
            _InReadUncommitedTransaction(context =>
            {
                var outOfSpecDoneEntities = doneOutOfSpecsToAdd.Select(doneOutOfSpecToAdd => new spot_exceptions_out_of_specs_done
                {
                    spot_unique_hash_external = doneOutOfSpecToAdd.SpotUniqueHashExternal,
                    execution_id_external = doneOutOfSpecToAdd.ExecutionIdExternal,
                    reason_code_message = doneOutOfSpecToAdd.ReasonCodeMessage,
                    estimate_id = doneOutOfSpecToAdd.EstimateId,
                    isci_name = doneOutOfSpecToAdd.IsciName,
                    house_isci = doneOutOfSpecToAdd.HouseIsci,
                    recommended_plan_id = doneOutOfSpecToAdd.RecommendedPlanId,
                    program_name = doneOutOfSpecToAdd.ProgramName,
                    station_legacy_call_letters = doneOutOfSpecToAdd.StationLegacyCallLetters,
                    daypart_code = doneOutOfSpecToAdd.DaypartCode,
                    genre_name = doneOutOfSpecToAdd.GenreName,
                    spot_length_id = doneOutOfSpecToAdd.SpotLength.Id,
                    audience_id = doneOutOfSpecToAdd.Audience.Id,
                    program_network = doneOutOfSpecToAdd.ProgramNetwork,
                    program_air_time = doneOutOfSpecToAdd.ProgramAirTime,
                    reason_code_id = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecReasonCode.Id,
                    ingested_by = doneOutOfSpecToAdd.IngestedBy,
                    ingested_at = doneOutOfSpecToAdd.IngestedAt,
                    ingested_media_week_id = doneOutOfSpecToAdd.IngestedMediaWeekId,
                    impressions = doneOutOfSpecToAdd.Impressions,
                    market_code = doneOutOfSpecToAdd.MarketCode,
                    market_rank = doneOutOfSpecToAdd.MarketRank,
                    inventory_source_name = doneOutOfSpecToAdd.InventorySourceName,
                    spot_exceptions_out_of_spec_done_decisions = new List<spot_exceptions_out_of_spec_done_decisions>
                    {
                        new spot_exceptions_out_of_spec_done_decisions
                        {
                            accepted_as_in_spec = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.AcceptedAsInSpec,
                            decision_notes = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.AcceptedAsInSpec ? "In" : "Out",
                            program_name = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.ProgramName,
                            genre_name = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision?.GenreName,
                            daypart_code = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.DaypartCode,
                            decided_by = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.DecidedBy,
                            decided_at = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.DecidedAt
                        }
                    }

                }).ToList();               
                context.spot_exceptions_out_of_specs_done.AddRange(outOfSpecDoneEntities);
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void AddOutOfSpecEditedToDone(List<SpotExceptionsOutOfSpecsDoneDto> doneOutOfSpecsToAdd)
        {
            _InReadUncommitedTransaction(context =>
            {
                var outOfSpecDoneEntities = doneOutOfSpecsToAdd.Select(doneOutOfSpecToAdd => new spot_exceptions_out_of_specs_done
                {
                    spot_unique_hash_external = doneOutOfSpecToAdd.SpotUniqueHashExternal,
                    execution_id_external = doneOutOfSpecToAdd.ExecutionIdExternal,
                    reason_code_message = doneOutOfSpecToAdd.ReasonCodeMessage,
                    estimate_id = doneOutOfSpecToAdd.EstimateId,
                    isci_name = doneOutOfSpecToAdd.IsciName,
                    house_isci = doneOutOfSpecToAdd.HouseIsci,
                    recommended_plan_id = doneOutOfSpecToAdd.RecommendedPlanId,
                    program_name = doneOutOfSpecToAdd.ProgramName,
                    station_legacy_call_letters = doneOutOfSpecToAdd.StationLegacyCallLetters,
                    daypart_code = doneOutOfSpecToAdd.DaypartCode,
                    genre_name = doneOutOfSpecToAdd.GenreName,
                    spot_length_id = doneOutOfSpecToAdd.SpotLength.Id,
                    audience_id = doneOutOfSpecToAdd.Audience.Id,
                    program_network = doneOutOfSpecToAdd.ProgramNetwork,
                    program_air_time = doneOutOfSpecToAdd.ProgramAirTime,
                    reason_code_id = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecReasonCode.Id,
                    ingested_by = doneOutOfSpecToAdd.IngestedBy,
                    ingested_at = doneOutOfSpecToAdd.IngestedAt,
                    ingested_media_week_id = doneOutOfSpecToAdd.IngestedMediaWeekId,
                    impressions = doneOutOfSpecToAdd.Impressions,
                    market_code = doneOutOfSpecToAdd.MarketCode,
                    market_rank = doneOutOfSpecToAdd.MarketRank,
                    inventory_source_name = doneOutOfSpecToAdd.InventorySourceName,
                    spot_exceptions_out_of_spec_done_decisions = new List<spot_exceptions_out_of_spec_done_decisions>
                    {
                        new spot_exceptions_out_of_spec_done_decisions
                        {
                            accepted_as_in_spec = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.AcceptedAsInSpec,
                            decision_notes = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.AcceptedAsInSpec ? "In" : "Out",
                            program_name = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.ProgramName,
                            genre_name = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.GenreName,
                            daypart_code = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.DaypartCode,
                            decided_by = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.DecidedBy,
                            decided_at = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecDoneDecision.DecidedAt
                        }
                    }

                }).ToList();

                context.spot_exceptions_out_of_specs_done.AddRange(outOfSpecDoneEntities);
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void DeleteOutOfSpecsFromToDo(List<int> existingOutOfSpecsToDo)
        {
            _InReadUncommitedTransaction(context =>
            {
                var foundTodoOutOfSpecPlan = context.spot_exceptions_out_of_specs
                    .Where(x => existingOutOfSpecsToDo.Contains(x.id))
                    .ToList();

                context.spot_exceptions_out_of_specs.RemoveRange(foundTodoOutOfSpecPlan);

                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public Task<bool> SaveOutOfSpecCommentsAsync(List<SpotExceptionOutOfSpecCommentsDto> OutOfSpecsCommentsToAdd,string userName, DateTime decidedAt)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Saving Out Of Spec Comments");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                OutOfSpecsCommentsToAdd.ForEach(y =>
                {
                    var outOfSpecComments = context.spot_exceptions_out_of_spec_comments.FirstOrDefault(x => x.spot_unique_hash_external == y.SpotUniqueHashExternal
                                                                                 && x.execution_id_external == y.ExecutionIdExternal
                                                                                 && x.isci_name == y.IsciName
                                                                                 && x.program_air_time == y.ProgramAirTime
                                                                                 && x.station_legacy_call_letters == y.StationLegacyCallLetters
                                                                                 && x.reason_code_id == y.ReasonCode
                                                                                 && x.recommended_plan_id == y.RecommendedPlanId);
                    if (outOfSpecComments == null)
                    {
                        outOfSpecComments = new spot_exceptions_out_of_spec_comments();
                        outOfSpecComments.spot_unique_hash_external = y.SpotUniqueHashExternal;
                        outOfSpecComments.execution_id_external = y.ExecutionIdExternal;
                        outOfSpecComments.isci_name = y.IsciName;
                        outOfSpecComments.program_air_time = y.ProgramAirTime;
                        outOfSpecComments.station_legacy_call_letters = y.StationLegacyCallLetters;
                        outOfSpecComments.reason_code_id = y.ReasonCode;
                        outOfSpecComments.recommended_plan_id = y.RecommendedPlanId.Value;
                        outOfSpecComments.comment = y.Comments;
                        outOfSpecComments.added_by = userName;
                        outOfSpecComments.added_at = decidedAt;
                        context.spot_exceptions_out_of_spec_comments.Add(outOfSpecComments);
                    }
                    else
                    {
                        outOfSpecComments.comment = y.Comments;
                    }
                });

                isSaved = context.SaveChanges() > 1;
                _LogInfo($"Finished: Saving Out Of Spec Comments");

                return isSaved;
            }));
        }

        /// <inheritdoc />
        public Task<bool> SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(List<SpotExceptionsOutOfSpecDoneDecisionsDto> spotExceptionsOutOfSpecDoneDecisions, string userName, DateTime decidedAt)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Saving Out Of Spec Decisions to Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var decisionIds = spotExceptionsOutOfSpecDoneDecisions.Select(x => x.Id).ToList();
                var foundDecisions = context.spot_exceptions_out_of_spec_done_decisions.Where(x => decisionIds.Contains(x.spot_exceptions_out_of_spec_done_id)).ToList();
                foundDecisions.ForEach(decision =>
                {
                    var request = spotExceptionsOutOfSpecDoneDecisions.Where(x => x.Id == decision.spot_exceptions_out_of_spec_done_id).Single();

                    if (!(string.IsNullOrEmpty(request.ProgramName) && string.IsNullOrEmpty(request.GenreName) && string.IsNullOrEmpty(request.DaypartCode)))
                    {
                        decision.accepted_as_in_spec = request.AcceptedAsInSpec;
                        decision.decision_notes = request.AcceptedAsInSpec ? "In" : "Out";
                        decision.decided_by = userName;
                        decision.decided_at = decidedAt;
                        decision.program_name = request.ProgramName;
                        decision.genre_name = request.GenreName;
                        decision.daypart_code = request.DaypartCode;
                    }
                    else
                    {
                        decision.accepted_as_in_spec = request.AcceptedAsInSpec;
                        decision.decision_notes = request.AcceptedAsInSpec ? "In" : "Out";
                        decision.decided_by = userName;
                        decision.decided_at = decidedAt;
                    }
                });

                isSaved = context.SaveChanges() > 1;
                _LogInfo($"Finished: Saving Out Of Spec Decisions to Done");

                return isSaved;
            }));
        }
             
        private SpotExceptionsOutOfSpecsToDoDto _MapOutOfSpecToDoToDto(spot_exceptions_out_of_specs spotExceptionsOutOfSpecToDoEntity, station stationEntity, List<spot_exceptions_out_of_spec_comments> lstComments)
           {
            var planVersion = spotExceptionsOutOfSpecToDoEntity.plan?.plan_versions.First(v => v.id == spotExceptionsOutOfSpecToDoEntity.plan.latest_version_id);

            var outOfSpecComment = lstComments.FirstOrDefault(x => x.spot_unique_hash_external == spotExceptionsOutOfSpecToDoEntity.spot_unique_hash_external
                                                                                  && x.execution_id_external == spotExceptionsOutOfSpecToDoEntity.execution_id_external
                                                                                  && x.isci_name == spotExceptionsOutOfSpecToDoEntity.isci_name
                                                                                  && x.program_air_time == spotExceptionsOutOfSpecToDoEntity.program_air_time
                                                                                  && x.station_legacy_call_letters == spotExceptionsOutOfSpecToDoEntity.station_legacy_call_letters
                                                                                  && x.reason_code_id == spotExceptionsOutOfSpecToDoEntity.reason_code_id
                                                                                  && x.recommended_plan_id == spotExceptionsOutOfSpecToDoEntity.recommended_plan_id);
            var spotExceptionsOutOfSpec = new SpotExceptionsOutOfSpecsToDoDto
            {
                Id = spotExceptionsOutOfSpecToDoEntity.id,
                SpotUniqueHashExternal = spotExceptionsOutOfSpecToDoEntity.spot_unique_hash_external,
                ExecutionIdExternal = spotExceptionsOutOfSpecToDoEntity.execution_id_external,
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
                ProgramNetwork = spotExceptionsOutOfSpecToDoEntity.program_network,
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
                Comments = outOfSpecComment !=null ? outOfSpecComment.comment: null,
                InventorySourceName = spotExceptionsOutOfSpecToDoEntity.inventory_source_name
            };
            return spotExceptionsOutOfSpec;
        }

        private SpotExceptionsOutOfSpecsDoneDto _MapOutOfSpecDoneToDto(spot_exceptions_out_of_specs_done spotExceptionsOutOfSpecDoneEntity, station stationEntity, List<spot_exceptions_out_of_spec_comments> lstComments)
        {
            var planVersion = spotExceptionsOutOfSpecDoneEntity.plan?.plan_versions.First(v => v.id == spotExceptionsOutOfSpecDoneEntity.plan.latest_version_id);

            var outOfSpecComment = lstComments.FirstOrDefault(x => x.spot_unique_hash_external == spotExceptionsOutOfSpecDoneEntity.spot_unique_hash_external
                                                                                  && x.execution_id_external == spotExceptionsOutOfSpecDoneEntity.execution_id_external
                                                                                  && x.isci_name == spotExceptionsOutOfSpecDoneEntity.isci_name
                                                                                  && x.program_air_time == spotExceptionsOutOfSpecDoneEntity.program_air_time
                                                                                  && x.station_legacy_call_letters == spotExceptionsOutOfSpecDoneEntity.station_legacy_call_letters
                                                                                  && x.reason_code_id == spotExceptionsOutOfSpecDoneEntity.reason_code_id
                                                                                  && x.recommended_plan_id == spotExceptionsOutOfSpecDoneEntity.recommended_plan_id);

            var spotExceptionsOutOfSpec = new SpotExceptionsOutOfSpecsDoneDto
            {
                Id = spotExceptionsOutOfSpecDoneEntity.id,
                SpotUniqueHashExternal = spotExceptionsOutOfSpecDoneEntity.spot_unique_hash_external,
                ExecutionIdExternal = spotExceptionsOutOfSpecDoneEntity.execution_id_external,
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
                Comments = outOfSpecComment != null ? outOfSpecComment.comment : null,
                InventorySourceName = spotExceptionsOutOfSpecDoneEntity.inventory_source_name,
                SpotExceptionsOutOfSpecDoneDecision = spotExceptionsOutOfSpecDoneEntity.spot_exceptions_out_of_spec_done_decisions.Select(spotExceptionsOutOfSpecsDecisionDb => new SpotExceptionsOutOfSpecDoneDecisionsDto
                {
                    Id = spotExceptionsOutOfSpecsDecisionDb.id,
                    SpotExceptionsOutOfSpecDoneId = spotExceptionsOutOfSpecsDecisionDb.spot_exceptions_out_of_spec_done_id,
                    AcceptedAsInSpec = spotExceptionsOutOfSpecsDecisionDb.accepted_as_in_spec,
                    DecisionNotes = spotExceptionsOutOfSpecsDecisionDb.decision_notes,
                    DecidedBy = spotExceptionsOutOfSpecsDecisionDb.decided_by,
                    DecidedAt = spotExceptionsOutOfSpecsDecisionDb.decided_at,
                    SyncedBy = spotExceptionsOutOfSpecsDecisionDb.synced_by,
                    SyncedAt = spotExceptionsOutOfSpecsDecisionDb.synced_at,
                    ProgramName = spotExceptionsOutOfSpecsDecisionDb.program_name,
                    GenreName = spotExceptionsOutOfSpecsDecisionDb?.genre_name,
                    DaypartCode = spotExceptionsOutOfSpecsDecisionDb.daypart_code
                }).SingleOrDefault(),
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

        private string _GetAudienceName(audience audienceEntity)
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
            return audience.Name;
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
    }
}
