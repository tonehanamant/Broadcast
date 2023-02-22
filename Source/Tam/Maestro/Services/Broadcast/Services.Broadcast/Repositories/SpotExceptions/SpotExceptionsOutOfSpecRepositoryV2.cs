using Common.Services.Repositories;
using Common.Services.Extensions;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Entities.ProgramMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Microsoft.EntityFrameworkCore.Internal;

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
        /// <param name="inventorySources">The inventory source array</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<OutOfSpecPlansDto> GetOutOfSpecPlansToDo(List<string> inventorySources, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the list of out of spec done 
        /// </summary>
        /// <param name="weekStartDate">week start date</param>
        /// <param name="weekEndDate">week end date</param>
        /// <param name="inventorySources">inventory sources</param>
        /// <returns>List of done plans</returns>
        List<OutOfSpecPlansDto> GetOutOfSpecPlansDone(DateTime weekStartDate, DateTime weekEndDate, List<string> inventorySources);

        /// <summary>
        /// Gets the out of spec spots to do inventory sources.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<string> GetOutOfSpecPlanToDoInventorySources(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done inventory sources.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<string> GetOutOfSpecPlanDoneInventorySources(DateTime weekStartDate, DateTime weekEndDate);

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
        void AddOutOfSpecsToDone(List<OutOfSpecSpotsDoneDto> doneOutOfSpecsToAdd);

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
        List<string> GetOutOfSpecSpotToDoInventorySources(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the out of spec spots done inventory sources asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<string> GetOutOfSpecSpotDoneInventorySources(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exceptions out of spec to do reason codes.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<OutOfSpecSpotReasonCodesDto> GetOutOfSpecSpotToDoReasonCodes(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exceptions out of spec done reason codes.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<OutOfSpecSpotReasonCodesDto> GetOutOfSpecSpotDoneReasonCodes(int planId, DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Finds the program from programs asynchronous.
        /// </summary>
        /// <param name="programSearchString">The program search string.</param>
        /// <returns></returns>
        List<ProgramNameDto> FindProgramFromPrograms(string programSearchString);

        /// <summary>
        /// Finds the program from spot exception decisions asynchronous.
        /// </summary>
        /// <param name="programSearchString">The program search string.</param>
        /// <returns></returns>
        List<ProgramNameDto> FindProgramFromSpotExceptionDecisions(string programSearchString);

        /// <summary>
        /// Gets the out of spec spots by ids to do.
        /// </summary>
        /// <param name="spotIds">The plan identifier.</param>
        /// <returns> </returns>
        List<OutOfSpecSpotsToDoDto> GetOutOfSpecSpotsToDoByIds(List<int> spotIds);

        /// <summary>
        /// Gets the out of spec spots done by ids.
        /// </summary>
        /// <param name="spotIds">The spot ids.</param>
        /// <returns></returns>
        List<OutOfSpecSpotsDoneDto> GetOutOfSpecSpotsDoneByIds(List<int> spotIds);

        /// <summary>
        /// Adds the out of spec comments to do.
        /// </summary>
        /// <param name="OutOfSpecSpotCommentsDto">The out of spec spots to do dto.</param>
        /// <returns></returns>
        int AddOutOfSpecSpotCommentsToDo(List<OutOfSpecSpotCommentsDto> OutOfSpecSpotCommentsDto);

        /// <summary>
        /// Updates the out of spec comments to do.
        /// </summary>
        /// <param name="OutOfSpecSpotCommentsDto">The out of spec spots to do dto.</param>
        /// <returns></returns>
        int UpdateOutOfSpecCommentsToDo(List<OutOfSpecSpotCommentsDto> OutOfSpecSpotCommentsDto);

        /// <summary>
        /// Saves the out of spec spot comments.
        /// </summary>
        /// <param name="OutOfSpecsCommentsToAdd">The out of specs comments to add.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="decidedAt">The decided at.</param>
        /// <returns></returns>
        bool SaveOutOfSpecSpotComments(List<OutOfSpecSpotCommentsDto> OutOfSpecsCommentsToAdd, string userName, DateTime decidedAt);

        /// <summary>
        /// Gets the out of spec decision queued count.
        /// </summary>
        /// <returns></returns>
        int GetOutOfSpecDecisionQueuedCountAsync();

        /// <summary>
        /// Saves the spot exceptions out of spec done decisions .
        /// </summary>
        /// <param name="outOfSpecSpotDoneDecisions">The spot exceptions out of spec done decisions.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="decidedAt">The decided at.</param>
        /// <returns></returns>
        bool SaveOutOfSpecSpotDoneDecisions(List<OutOfSpecSpotDoneDecisionsDto> outOfSpecSpotDoneDecisions, string userName, DateTime decidedAt);
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
        public List<OutOfSpecPlansDto> GetOutOfSpecPlansToDo(List<string> inventorySources, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var outOfSpecSpotsToDo = context.spot_exceptions_out_of_specs
                   .Where(outOfSpecToDoDb => outOfSpecToDoDb.program_air_time >= weekStartDate && outOfSpecToDoDb.program_air_time <= weekEndDate);

                if(inventorySources != null && inventorySources.Any())
                {
                    outOfSpecSpotsToDo = outOfSpecSpotsToDo.Where(outOfSpecToDoDb => inventorySources.Contains(outOfSpecToDoDb.inventory_source_name));
                }

                var outOfSpecPlansToDo = outOfSpecSpotsToDo.AsEnumerable().GroupBy(x => new { x.recommended_plan_id })
                    .Select(x =>
                    {
                        var first = x.First();
                        var recommendedPlanVersion = first.plan.plan_versions.Single(planVersion => planVersion.id == first.plan.latest_version_id);
                        var audience = first.audience;
                        return new OutOfSpecPlansDto
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

                return outOfSpecPlansToDo;
            });
        }

        /// <inheritdoc />
        public List<OutOfSpecPlansDto> GetOutOfSpecPlansDone(DateTime weekStartDate, DateTime weekEndDate, List<string> inventorySources)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var outOfSpecSpotsDone = context.spot_exceptions_out_of_specs_done
                   .Where(outOfSpecDoneDb => outOfSpecDoneDb.program_air_time >= weekStartDate && outOfSpecDoneDb.program_air_time <= weekEndDate);

                if (inventorySources != null && inventorySources.Any())
                {
                    outOfSpecSpotsDone = outOfSpecSpotsDone.Where(outOfSpecDoneDb => inventorySources.Contains(outOfSpecDoneDb.inventory_source_name));
                }

                var outOfSpecPlansDone = outOfSpecSpotsDone.AsEnumerable().GroupBy(x => new { x.recommended_plan_id })
                    .Select(x =>
                    {
                        var first = x.First();
                        var recommendedPlanVersion = first.plan.plan_versions.Single(planVersion => planVersion.id == first.plan.latest_version_id);
                        var audience = first.audience;
                        var decisions = x.Select(y => y.spot_exceptions_out_of_spec_done_decisions).ToList();
                        return new OutOfSpecPlansDto
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

                return outOfSpecPlansDone;
            });
        }

        /// <inheritdoc />
        public List<string> GetOutOfSpecPlanToDoInventorySources(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> outOfSpecToDoInventorySources = new List<string>();

            return _InReadUncommitedTransaction(context =>
            {
                outOfSpecToDoInventorySources = context.spot_exceptions_out_of_specs
                   .Where(outOfSpecsToDoDb =>
                           outOfSpecsToDoDb.program_air_time >= weekStartDate && outOfSpecsToDoDb.program_air_time <= weekEndDate)
                   .GroupJoin(
                       context.stations
                       .Include(stationDb => stationDb.market),
                       outOfSpecsToDoDb => outOfSpecsToDoDb.station_legacy_call_letters,
                       stationDb => stationDb.legacy_call_letters,
                       (outOfSpecsToDoDb, stationDb) => new { outOfSpecsToDo = outOfSpecsToDoDb, Station = stationDb.FirstOrDefault() })
                   .Select(r => r.outOfSpecsToDo.inventory_source_name ?? "Unknown")
                   .ToList();

                return outOfSpecToDoInventorySources;

            });
        }

        /// <inheritdoc />
        public List<string> GetOutOfSpecPlanDoneInventorySources(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> outOfSpecDoneInventorySources = new List<string>();

            return _InReadUncommitedTransaction(context =>
            {
                outOfSpecDoneInventorySources = context.spot_exceptions_out_of_specs_done
                    .Where(outOfSpecsDoneDb =>
                            outOfSpecsDoneDb.program_air_time >= weekStartDate && outOfSpecsDoneDb.program_air_time <= weekEndDate)
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        outOfSpecsDoneDb => outOfSpecsDoneDb.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (outOfSpecsDoneDb, stationDb) => new { outOfSpecsDone = outOfSpecsDoneDb, Station = stationDb.FirstOrDefault() })
                    .Select(r => r.outOfSpecsDone.inventory_source_name ?? "Unknown")
                    .ToList();

                return outOfSpecDoneInventorySources;

            });
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotsToDoDto> GetOutOfSpecSpotsToDo(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var outOfSpecsEntities = context.spot_exceptions_out_of_specs
                    .Where(outOfSpecDb => outOfSpecDb.program_air_time >= weekStartDate && outOfSpecDb.program_air_time <= weekEndDate && outOfSpecDb.recommended_plan_id == planId)
                    .Include(outOfSpecDb => outOfSpecDb.plan)
                    .Include(outOfSpecDb => outOfSpecDb.plan.campaign)
                    .Include(outOfSpecDb => outOfSpecDb.plan.plan_versions)
                    .Include(outOfSpecDb => outOfSpecDb.plan.plan_versions.Select(x => x.plan_version_dayparts))
                    .Include(outOfSpecDb => outOfSpecDb.spot_lengths)
                    .Include(outOfSpecDb => outOfSpecDb.audience)
                    .Include(outOfSpecDb => outOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
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
                        outOfSpecDb => outOfSpecDb.OutOfSpecSpotsToDo.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (outOfSpecDb, stationDb) => new { OutOfSpec = outOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var outOfSpecSpotsToDo = outOfSpecsEntities.Select(outOfSpecEntity => _MapOutOfSpecSpotsToDoToDto(outOfSpecEntity.OutOfSpec.OutOfSpecSpotsToDo, outOfSpecEntity.Station, outOfSpecEntity.OutOfSpec.Comments)).ToList();
                
                return outOfSpecSpotsToDo;
            });
        }        

        /// <inheritdoc />
        public void AddOutOfSpecsToDone(List<OutOfSpecSpotsDoneDto> doneOutOfSpecsToAdd)
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
                    reason_code_id = doneOutOfSpecToAdd.OutOfSpecSpotReasonCodes.Id,
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
                            accepted_as_in_spec = doneOutOfSpecToAdd.OutOfSpecSpotDoneDecisions.AcceptedAsInSpec,
                            decision_notes = doneOutOfSpecToAdd.OutOfSpecSpotDoneDecisions.AcceptedAsInSpec ? "In" : "Out",
                            program_name = doneOutOfSpecToAdd.OutOfSpecSpotDoneDecisions.ProgramName,
                            genre_name = doneOutOfSpecToAdd.OutOfSpecSpotDoneDecisions?.GenreName,
                            daypart_code = doneOutOfSpecToAdd.OutOfSpecSpotDoneDecisions.DaypartCode,
                            decided_by = doneOutOfSpecToAdd.OutOfSpecSpotDoneDecisions.DecidedBy,
                            decided_at = doneOutOfSpecToAdd.OutOfSpecSpotDoneDecisions.DecidedAt
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
        public List<string> GetOutOfSpecSpotToDoInventorySources(int planId, DateTime weekStartDate, DateTime weekEndDate)
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
        public List<string> GetOutOfSpecSpotDoneInventorySources(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            List<string> spotExceptionsDoneInventorySources = new List<string>();

            return _InReadUncommitedTransaction(context =>
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

                return spotExceptionsDoneInventorySources;

            });
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotReasonCodesDto> GetOutOfSpecSpotToDoReasonCodes(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);
            return _InReadUncommitedTransaction(context =>
            {
                var outOfSpecSpotReasonCodesEntities = from oos in context.spot_exceptions_out_of_specs
                                                            join reasoncode in context.spot_exceptions_out_of_spec_reason_codes on
                                                            oos.reason_code_id equals reasoncode.id into spotexceptionoutofspec
                                                            where oos.recommended_plan_id == planId && oos.program_air_time >= weekStartDate
                                                            && oos.program_air_time <= weekEndDate
                                                            from reasoncode in spotexceptionoutofspec
                                                            group spotexceptionoutofspec by new 
                                                            { 
                                                                reasoncode.id,
                                                                reasoncode.reason,
                                                                reasoncode.reason_code,
                                                                reasoncode.label 
                                                            }
                                                            into grouped
                                                            select new
                                                            {
                                                                Id = grouped.Key.id,
                                                                ReasonCode = grouped.Key.reason_code,
                                                                Reason = grouped.Key.reason,
                                                                Label = grouped.Key.label,
                                                                Count = grouped.Count()
                                                            };               

                var OutOfSpecSpotReasonCodes = outOfSpecSpotReasonCodesEntities
                .Select(outOfSpecSpotReasonCodesEntity => new OutOfSpecSpotReasonCodesDto
                {
                    Id = outOfSpecSpotReasonCodesEntity.Id,
                    ReasonCode = outOfSpecSpotReasonCodesEntity.ReasonCode,
                    Reason = outOfSpecSpotReasonCodesEntity.Reason,
                    Label = outOfSpecSpotReasonCodesEntity.Label,
                    Count = outOfSpecSpotReasonCodesEntity.Count
                }).ToList();

                return OutOfSpecSpotReasonCodes;
            });
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotReasonCodesDto> GetOutOfSpecSpotDoneReasonCodes(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var outOfSpecSpotReasonCodesEntities = from oos in context.spot_exceptions_out_of_specs_done
                                                                 join reasoncode in context.spot_exceptions_out_of_spec_reason_codes on
                                                                 oos.reason_code_id equals reasoncode.id into spotexceptionoutofspec
                                                                 where oos.recommended_plan_id == planId && oos.program_air_time >= weekStartDate
                                                                 && oos.program_air_time <= weekEndDate
                                                                 from reasoncode in spotexceptionoutofspec
                                                                 group spotexceptionoutofspec by new 
                                                                 { 
                                                                     reasoncode.id,
                                                                     reasoncode.reason,
                                                                     reasoncode.reason_code,
                                                                     reasoncode.label
                                                                 }
                                                                 into grouped
                                                                 select new
                                                                 {
                                                                     Id = grouped.Key.id,
                                                                     ReasonCode = grouped.Key.reason_code,
                                                                     Reason = grouped.Key.reason,
                                                                     Label = grouped.Key.label,
                                                                     Count = grouped.Count()
                                                                 };


                var outOfSpecSpotReasonCodes = outOfSpecSpotReasonCodesEntities.Select(outOfSpecSpotReasonCodesEntity => new OutOfSpecSpotReasonCodesDto
                {
                    Id = outOfSpecSpotReasonCodesEntity.Id,
                    ReasonCode = outOfSpecSpotReasonCodesEntity.ReasonCode,
                    Reason = outOfSpecSpotReasonCodesEntity.Reason,
                    Label = outOfSpecSpotReasonCodesEntity.Label,
                    Count = outOfSpecSpotReasonCodesEntity.Count
                }).ToList();

                return outOfSpecSpotReasonCodes;
            });
        }

        /// <inheritdoc />
        public List<ProgramNameDto> FindProgramFromPrograms(string programSearchString)
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
        public List<ProgramNameDto> FindProgramFromSpotExceptionDecisions(string programSearchString)
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
        public List<OutOfSpecSpotsToDoDto> GetOutOfSpecSpotsToDoByIds(List<int> spotIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var outOfSpecsEntities = context.spot_exceptions_out_of_specs
                    .Join
                    (
                        spotIds,
                        outOfSpecSpotsToDoDb => outOfSpecSpotsToDoDb.id,
                        pIds => pIds,
                        (outOfSpecSpotsToDoDb, pIds) => outOfSpecSpotsToDoDb
                    )
                    .Include(outOfSpecDb => outOfSpecDb.plan)
                    .Include(outOfSpecDb => outOfSpecDb.plan.campaign)
                    .Include(outOfSpecDb => outOfSpecDb.plan.plan_versions)
                    .Include(outOfSpecDb => outOfSpecDb.plan.plan_versions.Select(x => x.plan_version_dayparts))
                    .Include(outOfSpecDb => outOfSpecDb.spot_lengths)
                    .Include(outOfSpecDb => outOfSpecDb.audience)
                    .Include(outOfSpecDb => outOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
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
                        outOfSpecDb => outOfSpecDb.OutOfSpecSpotsToDo.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (outOfSpecDb, stationDb) => new { OutOfSpec = outOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var outOfSpecSpotsToDo = outOfSpecsEntities.Select(outOfSpecEntity => _MapOutOfSpecSpotsToDoToDto(outOfSpecEntity.OutOfSpec.OutOfSpecSpotsToDo, outOfSpecEntity.Station, outOfSpecEntity.OutOfSpec.Comments)).ToList();

                return outOfSpecSpotsToDo;
            });
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotsDoneDto> GetOutOfSpecSpotsDoneByIds(List<int> spotIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var outOfSpecsEntities = context.spot_exceptions_out_of_specs_done
                    .Join
                    (
                        spotIds,
                        outOfSpecSpotsDoneDb => outOfSpecSpotsDoneDb.id,
                        pIds => pIds,
                        (outOfSpecSpotsDoneDb, pIds) => outOfSpecSpotsDoneDb
                    )
                    .Include(outOfSpecDb => outOfSpecDb.plan)
                    .Include(outOfSpecDb => outOfSpecDb.plan.campaign)
                    .Include(outOfSpecDb => outOfSpecDb.plan.plan_versions)
                    .Include(outOfSpecDb => outOfSpecDb.plan.plan_versions.Select(x => x.plan_version_dayparts))
                    .Include(outOfSpecDb => outOfSpecDb.spot_lengths)
                    .Include(outOfSpecDb => outOfSpecDb.audience)
                    .Include(outOfSpecDb => outOfSpecDb.spot_exceptions_out_of_spec_reason_codes)
                    .Include(outOfSpecDb => outOfSpecDb.spot_exceptions_out_of_spec_done_decisions)
                    .GroupJoin
                    (
                        context.spot_exceptions_out_of_spec_comments,
                        x => new { a = x.spot_unique_hash_external, b = x.execution_id_external, c = x.isci_name, d = x.program_air_time, e = x.reason_code_id, f = x.recommended_plan_id.Value },
                        y => new { a = y.spot_unique_hash_external, b = y.execution_id_external, c = y.isci_name, d = y.program_air_time, e = y.reason_code_id, f = y.recommended_plan_id },
                        (x, y) => new { OutOfSpecSpotsDone = x, Comments = y.FirstOrDefault() }
                    )
                    .GroupJoin(
                        context.stations
                        .Include(stationDb => stationDb.market),
                        outOfSpecDb => outOfSpecDb.OutOfSpecSpotsDone.station_legacy_call_letters,
                        stationDb => stationDb.legacy_call_letters,
                        (outOfSpecDb, stationDb) => new { OutOfSpec = outOfSpecDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var outOfSpecSpotsDone = outOfSpecsEntities.Select(outOfSpecEntity => _MapOutOfSpecSpotsDoneToDto(outOfSpecEntity.OutOfSpec.OutOfSpecSpotsDone, outOfSpecEntity.Station, outOfSpecEntity.OutOfSpec.Comments)).ToList();

                return outOfSpecSpotsDone;
            });
        }

        /// <inheritdoc />
        public int AddOutOfSpecSpotCommentsToDo(List<OutOfSpecSpotCommentsDto> OutOfSpecSpotCommentsDto)
        {
            return _InReadUncommitedTransaction(context =>
            {
                OutOfSpecSpotCommentsDto.ForEach(outOfSpecSpotCommentsToDo =>
                {
                    context.spot_exceptions_out_of_spec_comments.Add(new spot_exceptions_out_of_spec_comments()
                    {
                        spot_unique_hash_external = outOfSpecSpotCommentsToDo.SpotUniqueHashExternal,
                        execution_id_external = outOfSpecSpotCommentsToDo.ExecutionIdExternal,
                        isci_name = outOfSpecSpotCommentsToDo.IsciName,
                        program_air_time = outOfSpecSpotCommentsToDo.ProgramAirTime,
                        station_legacy_call_letters = outOfSpecSpotCommentsToDo.StationLegacyCallLetters,
                        reason_code_id = outOfSpecSpotCommentsToDo.ReasonCode,
                        recommended_plan_id = outOfSpecSpotCommentsToDo.RecommendedPlanId.Value,
                        comment = outOfSpecSpotCommentsToDo.Comment,
                        added_by = outOfSpecSpotCommentsToDo.AddedBy,
                        added_at = outOfSpecSpotCommentsToDo.AddedAt
                    });
                });

                var savedCount = context.SaveChanges();

                return savedCount;
            });
        }
            
        /// <inheritdoc />
        public int UpdateOutOfSpecCommentsToDo(List<OutOfSpecSpotCommentsDto> OutOfSpecSpotCommentsDto)
        {
            return _InReadUncommitedTransaction(context =>
            {
                OutOfSpecSpotCommentsDto.ForEach(outOfSpecSpotsToDo =>
                {
                    var foundComment = context.spot_exceptions_out_of_spec_comments
                        .Single(comment => 
                            comment.spot_unique_hash_external == outOfSpecSpotsToDo.SpotUniqueHashExternal &&
                            comment.execution_id_external == outOfSpecSpotsToDo.ExecutionIdExternal &&
                            comment.isci_name == outOfSpecSpotsToDo.IsciName &&
                            comment.program_air_time == outOfSpecSpotsToDo.ProgramAirTime &&
                            comment.station_legacy_call_letters == outOfSpecSpotsToDo.StationLegacyCallLetters &&
                            comment.reason_code_id == outOfSpecSpotsToDo.ReasonCode &&
                            comment.recommended_plan_id == outOfSpecSpotsToDo.RecommendedPlanId
                        );

                    foundComment.comment = outOfSpecSpotsToDo.Comment;
                    foundComment.added_by = outOfSpecSpotsToDo.AddedBy;
                    foundComment.added_at = outOfSpecSpotsToDo.AddedAt;
                });

                var savedCount = context.SaveChanges();

                return savedCount;
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
                OutOfSpecSpotReasonCodes = _MapOutOfSpecSpotReasonCodesToDto(outOfSpecsEntity.spot_exceptions_out_of_spec_reason_codes),
                MarketCode = outOfSpecsEntity.market_code,
                MarketRank = outOfSpecsEntity.market_rank,
                Comment = commentsEntity != null ? commentsEntity.comment : null,
                InventorySourceName = outOfSpecsEntity.inventory_source_name
            };

            return outOfSpecSpotsToDo;
        }

        private OutOfSpecSpotsDoneDto _MapOutOfSpecSpotsDoneToDto(spot_exceptions_out_of_specs_done outOfSpecsEntity, station stationEntity, spot_exceptions_out_of_spec_comments commentsEntity)
        {
            var planVersion = outOfSpecsEntity.plan?.plan_versions.First(v => v.id == outOfSpecsEntity.plan.latest_version_id);

            var outOfSpecSpotsDone = new OutOfSpecSpotsDoneDto
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
                OutOfSpecSpotReasonCodes = _MapOutOfSpecSpotReasonCodesToDto(outOfSpecsEntity.spot_exceptions_out_of_spec_reason_codes),
                OutOfSpecSpotDoneDecisions = outOfSpecsEntity.spot_exceptions_out_of_spec_done_decisions.Select(outOfSpecsDecisionDb => new OutOfSpecSpotDoneDecisionsDto
                {
                    Id = outOfSpecsDecisionDb.id,
                    SpotExceptionsOutOfSpecDoneId = outOfSpecsDecisionDb.spot_exceptions_out_of_spec_done_id,
                    AcceptedAsInSpec = outOfSpecsDecisionDb.accepted_as_in_spec,
                    DecisionNotes = outOfSpecsDecisionDb.decision_notes,
                    DecidedBy = outOfSpecsDecisionDb.decided_by,
                    DecidedAt = outOfSpecsDecisionDb.decided_at,
                    SyncedBy = outOfSpecsDecisionDb.synced_by,
                    SyncedAt = outOfSpecsDecisionDb.synced_at,
                    ProgramName = outOfSpecsDecisionDb.program_name,
                    GenreName = outOfSpecsDecisionDb?.genre_name,
                    DaypartCode = outOfSpecsDecisionDb.daypart_code
                }).SingleOrDefault(),
                MarketCode = outOfSpecsEntity.market_code,
                MarketRank = outOfSpecsEntity.market_rank,
                Comment = commentsEntity != null ? commentsEntity.comment : null,
                InventorySourceName = outOfSpecsEntity.inventory_source_name
            };

            return outOfSpecSpotsDone;
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

        private OutOfSpecSpotReasonCodesDto _MapOutOfSpecSpotReasonCodesToDto(spot_exceptions_out_of_spec_reason_codes spotExceptionsOutOfSpecReasonCodesEntity)
        {
            if (spotExceptionsOutOfSpecReasonCodesEntity == null)
            {
                return null;
            }

            var spotExceptionsOutOfSpecReasonCode = new OutOfSpecSpotReasonCodesDto
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
        public bool SaveOutOfSpecSpotDoneDecisions(List<OutOfSpecSpotDoneDecisionsDto> outOfSpecSpotDoneDecisions, string userName, DateTime decidedAt)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Saving Out Of Spec Decisions to Done");
            return _InReadUncommitedTransaction(context =>
            {
                var decisionIds = outOfSpecSpotDoneDecisions.Select(x => x.Id).ToList();
                var foundDecisions = context.spot_exceptions_out_of_spec_done_decisions.Where(x => decisionIds.Contains(x.spot_exceptions_out_of_spec_done_id)).ToList();
                foundDecisions.ForEach(decision =>
                {
                    var request = outOfSpecSpotDoneDecisions.Single(x => x.Id == decision.spot_exceptions_out_of_spec_done_id);
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
                
        /// <summary>
        /// Saving out of spec comments 
        /// </summary>
        public bool SaveOutOfSpecSpotComments(List<OutOfSpecSpotCommentsDto> OutOfSpecsCommentsToAdd, string userName, DateTime decidedAt)
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
                        outOfSpecComments.comment = y.Comment;
                        outOfSpecComments.added_by = userName;
                        outOfSpecComments.added_at = decidedAt;
                        context.spot_exceptions_out_of_spec_comments.Add(outOfSpecComments);
                    }
                    else
                    {
                        outOfSpecComments.comment = y.Comment;
                    }
                });

                isSaved = context.SaveChanges() > 1;
                _LogInfo($"Finished: Saving Out Of Spec Comments");

                return isSaved;
            }));
        }
    }
}
