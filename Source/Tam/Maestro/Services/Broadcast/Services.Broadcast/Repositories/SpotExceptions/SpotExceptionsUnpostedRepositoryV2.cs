using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities.SpotExceptions.Unposted;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    /// <summary></summary>
    public interface ISpotExceptionsUnpostedRepositoryV2 : IDataRepository
    {
        /// <summary>
        /// Gets the spot exception unposted no plan asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<OutOfSpecUnpostedNoPlanDto> GetSpotExceptionUnpostedNoPlan(DateTime weekStartDate, DateTime weekEndDate);

        /// <summary>
        /// Gets the spot exception unposted no reel roster asynchronous.
        /// </summary>
        /// <param name="weekStartDate">The week start date.</param>
        /// <param name="weekEndDate">The week end date.</param>
        /// <returns></returns>
        List<OutOfSpecUnpostedNoReelRosterDto> GetSpotExceptionUnpostedNoReelRoster(DateTime weekStartDate, DateTime weekEndDate);
    }

    /// <inheritdoc />
    public class SpotExceptionsUnpostedRepositoryV2 : BroadcastRepositoryBase, ISpotExceptionsUnpostedRepositoryV2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpotExceptionsUnpostedRepositoryV2" /> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="configurationSettingsHelper">The configuration settings helper.</param>
        public SpotExceptionsUnpostedRepositoryV2(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        /// <inheritdoc />
        public List<OutOfSpecUnpostedNoPlanDto> GetSpotExceptionUnpostedNoPlan(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var unpostedNoPlanEntities = context.spot_exceptions_unposted_no_plan
                    .Where(unpostedNoPlanDb => unpostedNoPlanDb.program_air_time >= weekStartDate && unpostedNoPlanDb.program_air_time <= weekEndDate)
                    .Select(x => new OutOfSpecUnpostedNoPlanDto
                    {
                        HouseIsci = x.house_isci,
                        ClientIsci = x.client_isci,
                        ClientSpotLength = x.spot_lengths.length,
                        Count = x.count,
                        ProgramAirTime = x.program_air_time,
                        EstimateID = x.estimate_id,
                        IngestedBy = x.ingested_by,
                        IngestedAt = x.ingested_at,
                        IngestMediaWeekId = x.ingested_media_week_id
                    }).ToList();

                return unpostedNoPlanEntities;
            });
        }

        /// <inheritdoc />
        public List<OutOfSpecUnpostedNoReelRosterDto> GetSpotExceptionUnpostedNoReelRoster(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var unpostedNoReelRosterEntities = context.spot_exceptions_unposted_no_reel_roster
                    .Where(unpostedNoReelRosterDb => unpostedNoReelRosterDb.program_air_time >= weekStartDate && unpostedNoReelRosterDb.program_air_time <= weekEndDate)
                    .Select(x => new OutOfSpecUnpostedNoReelRosterDto
                    {
                        HouseIsci = x.house_isci,
                        Count = x.count,
                        ProgramAirTime = x.program_air_time,
                        EstimateId = x.estimate_id,
                        IngestedBy = x.ingested_by,
                        IngestedAt = x.ingested_at,
                        IngestedMediaWeekId = x.ingested_media_week_id
                    }).ToList();

                return unpostedNoReelRosterEntities;
            });
        }
    }
}
