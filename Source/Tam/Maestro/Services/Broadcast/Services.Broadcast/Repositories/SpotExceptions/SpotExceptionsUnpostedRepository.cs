using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.SpotExceptions.Unposted;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    public interface ISpotExceptionsUnpostedRepository : IDataRepository
    {
        Task<List<SpotExceptionsUnpostedNoPlanDto>> GetSpotExceptionUnpostedNoPlanAsync(DateTime weekStartDate, DateTime weekEndDate);

        Task<List<SpotExceptionsUnpostedNoReelRosterDto>> GetSpotExceptionUnpostedNoReelRosterAsync(DateTime weekStartDate, DateTime weekEndDate);
    }

    /// <inheritdoc />
    public class SpotExceptionsUnpostedRepository : BroadcastRepositoryBase, ISpotExceptionsUnpostedRepository
    {
        public SpotExceptionsUnpostedRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        public async Task<List<SpotExceptionsUnpostedNoPlanDto>> GetSpotExceptionUnpostedNoPlanAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsUnpostedNoPlanEntities = context.spot_exceptions_unposted_no_plan
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Select(x => new SpotExceptionsUnpostedNoPlanDto
                    {
                        HouseIsci = x.house_isci,
                        ClientIsci = x.client_isci,
                        ClientSpotLengthId = x.client_spot_length_id,
                        Count = x.count,
                        ProgramAirTime = x.program_air_time,
                        EstimateID = x.estimate_id.Value,
                        IngestedBy = x.ingested_by,
                        IngestedAt = x.ingested_at,
                        IngestMediaWeekId = x.ingested_media_week_id
                    }).ToList();
                return spotExceptionsUnpostedNoPlanEntities;
            });
        }

        public async Task<List<SpotExceptionsUnpostedNoReelRosterDto>> GetSpotExceptionUnpostedNoReelRosterAsync(DateTime weekStartDate, DateTime weekEndDate)
        {
            weekStartDate = weekStartDate.Date;
            weekEndDate = weekEndDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsUnpostedNoReelRosterEntities = context.spot_exceptions_unposted_no_reel_roster
                    .Where(spotExceptionsRecommendedPlanDb => spotExceptionsRecommendedPlanDb.program_air_time >= weekStartDate && spotExceptionsRecommendedPlanDb.program_air_time <= weekEndDate)
                    .Select(x => new SpotExceptionsUnpostedNoReelRosterDto
                    {
                        HouseIsci = x.house_isci,
                        Count = x.count,
                        ProgramAirTime = x.program_air_time,
                        EstimateId = x.estimate_id.Value,
                        IngestedBy = x.ingested_by,
                        IngestedAt = x.ingested_at,
                        IngestedMediaWeekId = x.ingested_media_week_id
                    }).ToList();
                return spotExceptionsUnpostedNoReelRosterEntities;
            });
        }
    }
}
