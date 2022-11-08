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
        /// Saves the out of spec comments to do asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpec">The spot exceptions out of spec.</param>
        /// <returns></returns>
        Task<bool> SaveOutOfSpecCommentsToDoAsync(List<SpotExceptionsOutOfSpecsToDoDto> spotExceptionsOutOfSpec);

        /// <summary>
        /// Gets the out of spec spot.
        /// </summary>
        /// <param name="todoId">The todo identifier.</param>
        /// <returns>
        /// </returns>
        Task<SpotExceptionsOutOfSpecsToDoDto> GetOutOfSpecSpot(int? todoId);

        /// <summary>
        /// Adds the out of spec to done.
        /// </summary>
        /// <param name="doneOutOfSpecToAdd">The done out of spec to add.</param>
        /// <param name="outOfSpecRequest">The out of spec request.</param>
        /// <param name="decidedBy">The decided by.</param>
        /// <param name="decidedAt">The decided at.</param>
        void AddOutOfSpecToDone(SpotExceptionsOutOfSpecsDoneDto doneOutOfSpecToAdd, SpotExceptionsOutOfSpecDecisionsToSaveRequestDto outOfSpecRequest, string decidedBy, DateTime decidedAt);

        /// <summary>
        /// Deletes the recommended plan from to do.
        /// </summary>
        /// <param name="todoOutOfSpecPlan">The todo out of spec plan.</param>
        void DeleteOutOfSpecFromToDo(int todoOutOfSpecPlan);

        /// <summary>
        /// Saves the out of spec comments done asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpec">The spot exceptions out of spec.</param>
        /// <returns>
        /// </returns>
        Task<bool> SaveOutOfSpecCommentsDoneAsync(List<SpotExceptionsOutOfSpecsDoneDto> spotExceptionsOutOfSpec);

        /// <summary>
        /// Saves the spot exceptions out of spec done decisions asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecDoneDecisions">The spot exceptions out of spec done decisions.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="decidedAt">The decided at.</param>
        /// <returns></returns>
        Task<bool> SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(List<SpotExceptionsOutOfSpecDoneDecisionsDto> spotExceptionsOutOfSpecDoneDecisions, string userName, DateTime decidedAt);
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
                    && spotExceptionsOutOfSpecDoneDb.program_air_time <= weekEndDate).ToList();

                var outOfSpecGroupingDone = outOfSpecDetailsDone.GroupBy(x => new { x.recommended_plan_id })
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

                var spotExceptionsOutOfSpecPosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecToDoToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station)).ToList();
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
                        (spotExceptionsOutOfSpecDoneDb, stationDb) => new { SpotExceptionsoutOfSpec = spotExceptionsOutOfSpecDoneDb, Station = stationDb.FirstOrDefault() })
                    .ToList();

                var spotExceptionsoutOfSpecDonePosts = spotExceptionsOutOfSpecsEntities.Select(spotExceptionsOutOfSpecEntity => _MapOutOfSpecDoneToDto(spotExceptionsOutOfSpecEntity.SpotExceptionsoutOfSpec, spotExceptionsOutOfSpecEntity.Station)).ToList();
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
                        .Select(
                            p => new ProgramNameDto
                            {
                                OfficialProgramName = p.program_name,
                                GenreId = context.genres.FirstOrDefault(x => x.name == p.genre_name).id
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
        public Task<bool> SaveOutOfSpecCommentsToDoAsync(List<SpotExceptionsOutOfSpecsToDoDto> spotExceptionsOutOfSpec)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Saving Out Of Spec Comments to ToDo");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                spotExceptionsOutOfSpec.ForEach(outOfSpec =>
                {
                    var foundOutOfSpec = context.spot_exceptions_out_of_specs.Single(x => x.id == outOfSpec.Id);

                    foundOutOfSpec.comment = outOfSpec.Comments;
                });

                isSaved = context.SaveChanges() > 0;
                _LogInfo($"Finished: Saving Out Of Spec Comments to ToDo");

                return isSaved;
            }));
        }

        /// <inheritdoc />
        public Task<SpotExceptionsOutOfSpecsToDoDto> GetOutOfSpecSpot(int? todoId)
        {
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                var foundOutOfSpecTodo = context.spot_exceptions_out_of_specs
                    .Where(x => x.id == todoId)
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
                    .Single();

                var OutOfSpecToDo = _MapOutOfSpecToDoToDto(foundOutOfSpecTodo.SpotExceptionsoutOfSpec, foundOutOfSpecTodo.Station);

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details ToDo");
                return OutOfSpecToDo;
            }));
        }

        /// <inheritdoc />
        public void AddOutOfSpecToDone(SpotExceptionsOutOfSpecsDoneDto doneOutOfSpecToAdd, SpotExceptionsOutOfSpecDecisionsToSaveRequestDto outOfSpecRequest, string decidedBy, DateTime decidedAt)
        {
            _InReadUncommitedTransaction(context =>
            {
                var outOfSpecDoneEntity = new spot_exceptions_out_of_specs_done()
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
                    spot_length_id = doneOutOfSpecToAdd.SpotLengthId,
                    audience_id = doneOutOfSpecToAdd.AudienceId,
                    program_air_time = doneOutOfSpecToAdd.ProgramAirTime,
                    reason_code_id = doneOutOfSpecToAdd.SpotExceptionsOutOfSpecReasonCode.Id,
                    ingested_by = doneOutOfSpecToAdd.IngestedBy,
                    ingested_at = doneOutOfSpecToAdd.IngestedAt,
                    ingested_media_week_id = doneOutOfSpecToAdd.IngestedMediaWeekId,
                    impressions = doneOutOfSpecToAdd.Impressions,
                    market_code = doneOutOfSpecToAdd.MarketCode,
                    market_rank = doneOutOfSpecToAdd.MarketRank,
                    comment = doneOutOfSpecToAdd.Comments,
                    inventory_source_name = doneOutOfSpecToAdd.InventorySourceName
                };

                if (!(string.IsNullOrEmpty(outOfSpecRequest.ProgramName) && string.IsNullOrEmpty(outOfSpecRequest.GenreName) && string.IsNullOrEmpty(outOfSpecRequest.DaypartCode)))
                {
                    outOfSpecDoneEntity.spot_exceptions_out_of_spec_done_decisions = new List<spot_exceptions_out_of_spec_done_decisions>
                    {
                        new spot_exceptions_out_of_spec_done_decisions
                        {
                            spot_exceptions_out_of_spec_done_id = doneOutOfSpecToAdd.Id,
                            accepted_as_in_spec = outOfSpecRequest.AcceptAsInSpec,
                            decision_notes  = outOfSpecRequest.AcceptAsInSpec ? "In" : "Out",
                            program_name = outOfSpecRequest.ProgramName,
                            genre_name = outOfSpecRequest.GenreName,
                            daypart_code = outOfSpecRequest.DaypartCode,
                            decided_by = decidedBy,
                            decided_at = decidedAt
                        }
                    };
                }
                else
                {
                    outOfSpecDoneEntity.spot_exceptions_out_of_spec_done_decisions = new List<spot_exceptions_out_of_spec_done_decisions>
                    {
                        new spot_exceptions_out_of_spec_done_decisions
                        {
                            spot_exceptions_out_of_spec_done_id = doneOutOfSpecToAdd.Id,
                            accepted_as_in_spec = outOfSpecRequest.AcceptAsInSpec,
                            decided_by = decidedBy,
                            decided_at = decidedAt
                        }
                    };
                }

                context.spot_exceptions_out_of_specs_done.Add(outOfSpecDoneEntity);
                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void DeleteOutOfSpecFromToDo(int todoOutOfSpecPlan)
        {
            _InReadUncommitedTransaction(context =>
            {
                var foundTodoOutOfSpecPlan = context.spot_exceptions_out_of_specs
                    .Where(x => x.id == todoOutOfSpecPlan)
                    .Single();

                context.spot_exceptions_out_of_specs.Remove(foundTodoOutOfSpecPlan);

                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public Task<bool> SaveOutOfSpecCommentsDoneAsync(List<SpotExceptionsOutOfSpecsDoneDto> spotExceptionsOutOfSpec)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Saving Out Of Spec Comments to Done");
            return Task.FromResult(_InReadUncommitedTransaction(context =>
            {
                spotExceptionsOutOfSpec.ForEach(outOfSpec =>
                {
                    var foundOutOfSpec = context.spot_exceptions_out_of_specs_done.Single(x => x.id == outOfSpec.Id);

                    foundOutOfSpec.comment = outOfSpec.Comments;
                });

                isSaved = context.SaveChanges() > 1;
                _LogInfo($"Finished: Saving Out Of Spec Comments to Done");

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
                spotExceptionsOutOfSpecDoneDecisions.ForEach(decision =>
                {
                    var foundDecision = context.spot_exceptions_out_of_spec_done_decisions.Single(x => x.spot_exceptions_out_of_spec_done_id == decision.Id);

                    if (!(string.IsNullOrEmpty(decision.ProgramName) && string.IsNullOrEmpty(decision.GenreName) && string.IsNullOrEmpty(decision.DaypartCode)))
                    {
                        foundDecision.accepted_as_in_spec = decision.AcceptedAsInSpec;
                        foundDecision.decision_notes = decision.AcceptedAsInSpec ? "In" : "Out";
                        foundDecision.decided_by = userName;
                        foundDecision.decided_at = decidedAt;
                        foundDecision.program_name = decision.ProgramName;
                        foundDecision.genre_name = decision.GenreName;
                        foundDecision.daypart_code = decision.DaypartCode;
                    }
                    else
                    {
                        foundDecision.accepted_as_in_spec = decision.AcceptedAsInSpec;
                        foundDecision.decision_notes = decision.AcceptedAsInSpec ? "In" : "Out";
                        foundDecision.decided_by = userName;
                        foundDecision.decided_at = decidedAt;
                    }
                });

                isSaved = context.SaveChanges() > 1;
                _LogInfo($"Finished: Saving Out Of Spec Decisions to Done");

                return isSaved;
            }));
        }

        private SpotExceptionsOutOfSpecsToDoDto _MapOutOfSpecToDoToDto(spot_exceptions_out_of_specs spotExceptionsOutOfSpecToDoEntity, station stationEntity)
        {
            var planVersion = spotExceptionsOutOfSpecToDoEntity.plan?.plan_versions.First(v => v.id == spotExceptionsOutOfSpecToDoEntity.plan.latest_version_id);

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
                Comments = spotExceptionsOutOfSpecDoneEntity.comment,
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
