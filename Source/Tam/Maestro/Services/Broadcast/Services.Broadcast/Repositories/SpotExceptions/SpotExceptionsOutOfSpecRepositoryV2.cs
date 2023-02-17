using Common.Services.Repositories;
using Common.Services.Extensions;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    /// <summary>
    /// Defines the interface for method defines for the SpotExceptionsOutOfSpecRepositoryV2
    /// </summary>
    public interface ISpotExceptionsOutOfSpecRepositoryV2 : IDataRepository
    {
        /// <summary>
        /// Gets the list of out of spec to do 
        /// </summary>
        /// <param name="inventorySource">The inventory source array</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecPlansToDoAsync(List<string> inventorySource, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// gets the done plans basis of inventory source filter
        /// </summary>
        /// <param name="weekStartDate">week start date</param>
        /// <param name="weekEndDate">week end date</param>
        /// <param name="InventorySources">inventory sources</param>
        /// <returns>List of done plans</returns>
        Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecPlansDoneAsync(DateTime weekStartDate, DateTime weekEndDate, List<string> InventorySources);

        /// <summary>
        /// Gets the out of spec spots to do inventory sources asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecPlanToDoInventorySourcesAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done inventory sources asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecPlanDoneInventorySourcesAsync(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots to do.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<OutOfSpecSpotsToDoDto> GetOutOfSpecSpotsToDo(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Adds the out of spec to done.
        /// </summary>
        /// <param name="doneOutOfSpecsToAdd">The done out of spec to add.</param>
        void AddOutOfSpecToDone(List<SpotExceptionsOutOfSpecsDoneDto> doneOutOfSpecsToAdd);

        /// <summary>
        /// Deletes the out of specs from to do.
        /// </summary>
        /// <param name="existingOutOfSpecsToDo">The existing out of specs to do.</param>
        void DeleteOutOfSpecsFromToDo(List<int> existingOutOfSpecsToDo);

        /// <summary>
        /// Gets the out of spec spots to do inventory sources asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotToDoInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done inventory sources asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecSpotDoneInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exceptions out of spec to do reason codes v2.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<OutOfSpecSpotReasonCodesDto>> GetOutOfSpecSpotToDoReasonCodesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exceptions out of spec done reason codes v2.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        Task<List<OutOfSpecSpotReasonCodesDto>> GetOutOfSpecSpotDoneReasonCodesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate);

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
        /// Gets the out of spec decision queued count asynchronous.
        /// </summary>
        /// <returns></returns>
        int GetOutOfSpecDecisionQueuedCountAsync();
        /// <summary>
        ///  Gets the out of spec done spot.
        // </summary>
        List<SpotExceptionsOutOfSpecsDoneDto> GetOutOfSpecSpotsDoneByIds(List<int> spotId);
        /// <summary>
        /// Save comment for out of spec
        /// </summary>
        bool SaveOutOfSpecComments(List<SpotExceptionOutOfSpecCommentsDto> OutOfSpecsCommentsToAdd, string userName, DateTime decidedAt);

        /// <summary>
        /// Saves the spot exceptions out of spec done decisions .
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecDoneDecisions">The spot exceptions out of spec done decisions.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="decidedAt">The decided at.</param>
        /// <returns></returns>
        bool SaveSpotExceptionsOutOfSpecDoneDecisions(List<SpotExceptionsOutOfSpecDoneDecisionsDto> spotExceptionsOutOfSpecDoneDecisions, string userName, DateTime decidedAt);
    }

    /// <summary>
    /// spot exception repository which interacts with the database
    /// </summary>
    public class SpotExceptionsOutOfSpecRepositoryV2 : BroadcastRepositoryBase, ISpotExceptionsOutOfSpecRepositoryV2
    {
        /// <summary>
        /// constructor of the class
        /// </summary>
        public SpotExceptionsOutOfSpecRepositoryV2(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        /// <inheritdoc />
        public Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecPlansToDoAsync(List<string> inventorySource, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var outOfSpecDetailsToDo = new List<spot_exceptions_out_of_specs>();
                if (inventorySource == null || inventorySource.Count == 0)
                {
                    outOfSpecDetailsToDo = context.spot_exceptions_out_of_specs
                   .Where(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.program_air_time >= weekStartDate
                   && spotExceptionsOutOfSpecToDoDb.program_air_time <= weekEndDate).ToList();
                }
                else
                {
                    outOfSpecDetailsToDo = context.spot_exceptions_out_of_specs
                   .Where(spotExceptionsOutOfSpecToDoDb => spotExceptionsOutOfSpecToDoDb.program_air_time >= weekStartDate
                   && spotExceptionsOutOfSpecToDoDb.program_air_time <= weekEndDate
                   && inventorySource.Contains(spotExceptionsOutOfSpecToDoDb.inventory_source_name)).ToList();
                }


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
        public Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecPlansDoneAsync(DateTime weekStartDate, DateTime weekEndDate, List<string> InventorySources)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            var outOfSpecDetailsDone = new List<spot_exceptions_out_of_specs_done>();

            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                if (InventorySources.Count != 0)
                {
                    outOfSpecDetailsDone = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.program_air_time >= weekStartDate
                    && spotExceptionsOutOfSpecDoneDb.program_air_time <= weekEndDate && InventorySources.Contains(spotExceptionsOutOfSpecDoneDb.inventory_source_name)).Include(x => x.spot_exceptions_out_of_spec_done_decisions).ToList();
                }
                else
                {
                    outOfSpecDetailsDone = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.program_air_time >= weekStartDate
                    && spotExceptionsOutOfSpecDoneDb.program_air_time <= weekEndDate).Include(x => x.spot_exceptions_out_of_spec_done_decisions).ToList();
                }
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
        public async Task<List<string>> GetOutOfSpecPlanToDoInventorySourcesAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsToDoInventorySources = new List<string>();

            return await _InReadUncommitedTransaction(context =>
            {
                spotExceptionsToDoInventorySources = context.spot_exceptions_out_of_specs
                   .Where(spotExceptionsOutOfSpecDb =>
                           spotExceptionsOutOfSpecDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDb.program_air_time <= weekEndDate)
                   .GroupJoin(
                       context.stations
                       .Include(stationDb => stationDb.market),
                       spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.station_legacy_call_letters,
                       stationDb => stationDb.legacy_call_letters,
                       (spotExceptionsOutOfSpecDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDb, Station = stationDb.FirstOrDefault() })
                   .Select(r => r.SpotExceptionsOutOfSpec.inventory_source_name ?? "Unknown")
                   .ToList();

                return Task.FromResult(spotExceptionsToDoInventorySources);

            });
        }

        /// <inheritdoc />
        public async Task<List<string>> GetOutOfSpecPlanDoneInventorySourcesAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsDoneInventorySources = new List<string>();

            return await _InReadUncommitedTransaction(context =>
            {
                spotExceptionsDoneInventorySources = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb =>
                            spotExceptionsOutOfSpecDoneDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= weekEndDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.SpotExceptionsOutOfSpec.inventory_source_name ?? "Unknown")
                    .ToList();

                return Task.FromResult(spotExceptionsDoneInventorySources);

            });
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotsToDoDto> GetOutOfSpecSpotsToDo(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
            var spotExceptionsOutOfSpecsEntities = context.spot_exceptions_out_of_specs
                .Where(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDb.program_air_time <= weekEndDate && spotExceptionsOutOfSpecDb.recommended_plan_id == planId)
                .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan)
                .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan.campaign)
                .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan.plan_versions)
                .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.plan.plan_versions.Select(x => x.plan_version_dayparts))
                .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.spot_lengths)
                .Include(spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.audience)
                .Include(spotExceptionsoutOfSpecDb => spotExceptionsoutOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
                .GroupJoin
                (
                    context.spot_exceptions_out_of_spec_comments,
                    x => new { a = x.spot_unique_hash_external, b = x.execution_id_external, c = x.isci_name, d = x.program_air_time, e = x.reason_code_id, f = x.recommended_plan_id.Value }, 
                    y => new { a = y.spot_unique_hash_external, b = y.execution_id_external, c = y.isci_name, d = y.program_air_time, e = y.reason_code_id, f = y.recommended_plan_id },
                    (x, y) => new { OutOfSpecSpotsToDo = x, Comments = y.FirstOrDefault() }
                )
                .GroupJoin(
                    context.stations
                    .Include(stationDb => stationDb.market),
                    spotExceptionsOutOfSpecDb => spotExceptionsOutOfSpecDb.OutOfSpecSpotsToDo.station_legacy_call_letters,
                    stationDb => stationDb.legacy_call_letters,
                    (spotExceptionsOutOfSpecDb, stationDb) => new { SpotExceptionsoutOfSpec = spotExceptionsOutOfSpecDb, Station = stationDb.FirstOrDefault() })
                .ToList();

                var outOfSpecSpotsToDo = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecSpotsToDoToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.OutOfSpecSpotsToDo, spotExceptionsOutOfSpecEntity.Station, spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec.Comments)).ToList();
                return outOfSpecSpotsToDo;
            });
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
        public async Task<List<string>> GetOutOfSpecSpotToDoInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsToDoInventorySources = new List<string>();

            return await _InReadUncommitedTransaction(context =>
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

                return Task.FromResult(spotExceptionsToDoInventorySources);

            });
        }

        /// <inheritdoc />
        public async Task<List<string>> GetOutOfSpecSpotDoneInventorySourcesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsDoneInventorySources = new List<string>();

            return await _InReadUncommitedTransaction(context =>
            {
                spotExceptionsDoneInventorySources = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.recommended_plan_id == planId &&
                            spotExceptionsOutOfSpecDoneDb.program_air_time >= weekStartDate && spotExceptionsOutOfSpecDoneDb.program_air_time <= weekEndDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        spotExceptionsOutOfSpecDoneDb => spotExceptionsOutOfSpecDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsOutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.SpotExceptionsOutOfSpec.inventory_source_name ?? "Unknown")
                    .ToList();

                return Task.FromResult(spotExceptionsDoneInventorySources);

            });
        }

        /// <inheritdoc />
        public async Task<List<OutOfSpecSpotReasonCodesDto>> GetOutOfSpecSpotToDoReasonCodesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            return await _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecReasonCodesEntities = from oos in context.spot_exceptions_out_of_specs
                                                                 join reasoncode in context.spot_exceptions_out_of_spec_reason_codes on
                                                                 oos.reason_code_id equals reasoncode.id into spotexceptionoutofspec
                                                                 where oos.recommended_plan_id == planId && oos.program_air_time >= weekStartDate
                                                                 && oos.program_air_time <= weekEndDate
                                                                 from reasoncode in spotexceptionoutofspec
                                                                 group spotexceptionoutofspec by new { reasoncode.id, reasoncode.reason, reasoncode.reason_code, reasoncode.label } into grouped
                                                                 select new
                                                                 {
                                                                     Id = grouped.Key.id,
                                                                     ReasonCode = grouped.Key.reason_code,
                                                                     Reason = grouped.Key.reason,
                                                                     Label = grouped.Key.label,
                                                                     Count = grouped.Count()
                                                                 };
               

                var spotExceptionsOutOfSpecReasonCodes = spotExceptionsOutOfSpecReasonCodesEntities.Select(spotExceptionsOutOfSpecReasonCodesEntity => new OutOfSpecSpotReasonCodesDto
                {
                    Id = spotExceptionsOutOfSpecReasonCodesEntity.Id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCodesEntity.ReasonCode,
                    Reason = spotExceptionsOutOfSpecReasonCodesEntity.Reason,
                    Label = spotExceptionsOutOfSpecReasonCodesEntity.Label,
                    Count = spotExceptionsOutOfSpecReasonCodesEntity.Count
                }).ToList();

                return Task.FromResult(spotExceptionsOutOfSpecReasonCodes);
            });
        }

        /// <inheritdoc />
        public async Task<List<OutOfSpecSpotReasonCodesDto>> GetOutOfSpecSpotDoneReasonCodesAsync(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return await _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecReasonCodesEntities = from oos in context.spot_exceptions_out_of_specs_done
                                                                 join reasoncode in context.spot_exceptions_out_of_spec_reason_codes on
                                                                 oos.reason_code_id equals reasoncode.id into spotexceptionoutofspec
                                                                 where oos.recommended_plan_id == planId && oos.program_air_time >= weekStartDate
                                                                 && oos.program_air_time <= weekEndDate
                                                                 from reasoncode in spotexceptionoutofspec
                                                                 group spotexceptionoutofspec by new { reasoncode.id, reasoncode.reason, reasoncode.reason_code, reasoncode.label } into grouped
                                                                 select new
                                                                 {
                                                                     Id = grouped.Key.id,
                                                                     ReasonCode = grouped.Key.reason_code,
                                                                     Reason = grouped.Key.reason,
                                                                     Label = grouped.Key.label,
                                                                     Count = grouped.Count()
                                                                 };


                var spotExceptionsOutOfSpecReasonCodes = spotExceptionsOutOfSpecReasonCodesEntities.Select(spotExceptionsOutOfSpecReasonCodesEntity => new OutOfSpecSpotReasonCodesDto
                {
                    Id = spotExceptionsOutOfSpecReasonCodesEntity.Id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCodesEntity.ReasonCode,
                    Reason = spotExceptionsOutOfSpecReasonCodesEntity.Reason,
                    Label = spotExceptionsOutOfSpecReasonCodesEntity.Label,
                    Count = spotExceptionsOutOfSpecReasonCodesEntity.Count
                }).ToList();

                return Task.FromResult(spotExceptionsOutOfSpecReasonCodes);
            });
        }

        /// <inheritdoc />
        public async Task<List<ProgramNameDto>> FindProgramFromProgramsAsync(string programSearchString)
        {
            return await _InReadUncommitedTransaction(
                 context =>
                 {
                     return Task.FromResult(context.programs
                      .Where(p => p.name.ToLower().Contains(programSearchString.ToLower()))
                      .OrderBy(p => p.name)
                      .Select(
                          p => new ProgramNameDto
                          {
                              OfficialProgramName = p.name,
                              GenreId = p.genre_id
                          }).ToList());

                 });
        }

        /// <inheritdoc />
        public async Task<List<ProgramNameDto>> FindProgramFromSpotExceptionDecisionsAsync(string programSearchString)
        {
            return await _InReadUncommitedTransaction(
                context =>
                {
                    return Task.FromResult(context.spot_exceptions_out_of_spec_done_decisions
                        .Where(p => p.program_name.ToLower().Contains(programSearchString.ToLower()))
                        .OrderBy(p => p.program_name)
                        .Distinct()
                        .Select(
                            p => new ProgramNameDto
                            {
                                OfficialProgramName = p.program_name,
                                GenreId = p.genre_name == null ? context.genres.FirstOrDefault(x => x.name == p.genre_name).id : (int?)null
                            }).ToList());
                });
        }

        /// <inheritdoc />
        public int GetOutOfSpecDecisionQueuedCountAsync()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var OutOfSpecDecisionCount = context.spot_exceptions_out_of_spec_done_decisions
                  .Where(x => x.synced_at == null)
                  .Count();

                return OutOfSpecDecisionCount;
            });
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
        
        private OutOfSpecSpotsToDoDto _MapOutOfSpecSpotsToDoToDto(spot_exceptions_out_of_specs outOfSpecsEntity, station stationEntity, spot_exceptions_out_of_spec_comments commentsEntity)
        {
            var planVersion = outOfSpecsEntity.plan?.plan_versions.First(v => v.id == outOfSpecsEntity.plan.latest_version_id);

            var outOfSpecSpotsToDo = new OutOfSpecSpotsToDoDto
            {
                Id = outOfSpecsEntity.id,
                SpotUniqueHashExternal = outOfSpecsEntity.spot_unique_hash_external,
                ExecutionIdExternal = outOfSpecsEntity.execution_id_external,
                ReasonCodeMessage = outOfSpecsEntity.reason_code_message,
                EstimateId = outOfSpecsEntity.estimate_id,
                IsciName = outOfSpecsEntity.isci_name,
                HouseIsci = outOfSpecsEntity.house_isci,
                RecommendedPlanId = outOfSpecsEntity.recommended_plan_id,
                RecommendedPlanName = outOfSpecsEntity.plan?.name,
                ProgramName = outOfSpecsEntity.program_name,
                StationLegacyCallLetters = outOfSpecsEntity.station_legacy_call_letters,
                DaypartCode = outOfSpecsEntity.daypart_code,
                GenreName = outOfSpecsEntity.genre_name,
                Affiliate = stationEntity?.affiliation,
                Market = stationEntity?.market?.geography_name,
                SpotLength = _MapSpotLengthToDto(outOfSpecsEntity.spot_lengths),
                Audience = _MapAudienceToDto(outOfSpecsEntity.audience),
                ProgramAirTime = outOfSpecsEntity.program_air_time,
                ProgramNetwork = outOfSpecsEntity.program_network,
                IngestedBy = outOfSpecsEntity.ingested_by,
                IngestedAt = outOfSpecsEntity.ingested_at,
                IngestedMediaWeekId = outOfSpecsEntity.ingested_media_week_id,
                Impressions = outOfSpecsEntity.impressions,
                PlanId = outOfSpecsEntity.recommended_plan_id ?? 0,
                FlightStartDate = planVersion?.flight_start_date,
                FlightEndDate = planVersion?.flight_end_date,
                AdvertiserMasterId = outOfSpecsEntity.plan?.campaign.advertiser_master_id,
                Product = null,
                SpotExceptionsOutOfSpecReasonCode = _MapSpotExceptionsOutOfSpecReasonCodeToDto(outOfSpecsEntity.spot_exceptions_out_of_spec_reason_codes),
                MarketCode = outOfSpecsEntity.market_code,
                MarketRank = outOfSpecsEntity.market_rank,
                Comments = commentsEntity != null ? commentsEntity.comment : null,
                InventorySourceName = outOfSpecsEntity.inventory_source_name
            };

            return outOfSpecSpotsToDo;
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
        /// <summary>
        /// Saves the spot exceptions out of spec done decisions .
        /// </summary>
        public bool SaveSpotExceptionsOutOfSpecDoneDecisions(List<SpotExceptionsOutOfSpecDoneDecisionsDto> spotExceptionsOutOfSpecDoneDecisions, string userName, DateTime decidedAt)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Saving Out Of Spec Decisions to Done");
            return _InReadUncommitedTransaction(context =>
            {
                var decisionIds = spotExceptionsOutOfSpecDoneDecisions.Select(x => x.Id).ToList();
                var foundDecisions = context.spot_exceptions_out_of_spec_done_decisions.Where(x => decisionIds.Contains(x.spot_exceptions_out_of_spec_done_id)).ToList();
                foundDecisions.ForEach(decision =>
                {
                    var request = spotExceptionsOutOfSpecDoneDecisions.Where(x => x.Id == decision.spot_exceptions_out_of_spec_done_id).Single();
                    decision.accepted_as_in_spec = request.AcceptedAsInSpec;
                    decision.decision_notes = request.AcceptedAsInSpec ? "In" : "Out";
                    decision.decided_by = userName;
                    decision.decided_at = decidedAt;
                    decision.program_name = request.ProgramName;
                    decision.genre_name = request.GenreName;
                    decision.daypart_code = request.DaypartCode;
                });

                isSaved = context.SaveChanges() > 1;
                _LogInfo($"Finished: Saving Out Of Spec Decisions to Done");

                return isSaved;
            });
        }

        /// <inheritdoc />
        public  List<SpotExceptionsOutOfSpecsDoneDto> GetOutOfSpecSpotsDoneByIds(List<int> spotId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecsEntities = context.spot_exceptions_out_of_specs_done
                    .Where(spotExceptionsOutOfSpecDoneDb => spotId.Contains(spotExceptionsOutOfSpecDoneDb.id))
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
                var outOfSpecComment = context.spot_exceptions_out_of_spec_comments.ToList();
                var spotExceptionsoutOfSpecDonePosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecDoneToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station, outOfSpecComment)).ToList();
                return spotExceptionsoutOfSpecDonePosts;

            });
        }
        private SpotExceptionsOutOfSpecsDoneDto _MapOutOfSpecDoneToDto(spot_exceptions_out_of_specs_done spotExceptionsOutOfSpecDoneEntity, station stationEntity, List<spot_exceptions_out_of_spec_comments> lstComments)
        {
            var planVersion = spotExceptionsOutOfSpecDoneEntity.plan?.plan_versions.First(v => v.id == spotExceptionsOutOfSpecDoneEntity.plan.latest_version_id);
            var outOfSpecComment = lstComments.FirstOrDefault(x => x.spot_unique_hash_external == spotExceptionsOutOfSpecDoneEntity.execution_id_external
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
        /// <summary>
        /// Saving out of spec comments 
        /// </summary>
        public bool SaveOutOfSpecComments(List<SpotExceptionOutOfSpecCommentsDto> OutOfSpecsCommentsToAdd, string userName, DateTime decidedAt)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Saving Out Of Spec Comments");
            return (_InReadUncommitedTransaction(context =>
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
    }
}
