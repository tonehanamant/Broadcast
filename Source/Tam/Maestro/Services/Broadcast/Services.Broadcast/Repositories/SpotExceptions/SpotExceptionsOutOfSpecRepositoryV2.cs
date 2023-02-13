using Common.Services.Repositories;
using Common.Services.Extensions;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    /// <summary>
    /// Defines the interface for method defines for the SpotExceptionsOutOfSpecRepositoryV2
    /// </summary>
    public interface ISpotExceptionsOutOfSpecRepositoryV2 : IDataRepository
    {
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
        /// Gets the out of spec decision queued count asynchronous.
        /// </summary>
        /// <returns></returns>
        int GetOutOfSpecDecisionQueuedCountAsync();

        /// <summary>
        /// gets the done plans basis of inventory source filter
        /// </summary>
        /// <param name="weekStartDate">week start date</param>
        /// <param name="weekEndDate">week end date</param>
        /// <param name="InventorySources">inventory sources</param>
        /// <returns>List of done plans</returns>
        Task<List<SpotExceptionsOutOfSpecGroupingDto>> GetOutOfSpecPlansDoneAsync(DateTime weekStartDate, DateTime weekEndDate, List<string> InventorySources);

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
    }
}
