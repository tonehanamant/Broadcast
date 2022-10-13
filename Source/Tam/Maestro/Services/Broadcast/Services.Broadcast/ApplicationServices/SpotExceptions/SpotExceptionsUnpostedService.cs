using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.SpotExceptions.Unposted;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Services.Broadcast.Exceptions;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsUnpostedService : IApplicationService
    {
        Task<SpotExceptionsUnpostedResultDto> GetSpotExceptionsUnposted(SpotExceptionsUnpostedRequestDto spotExceptionUnpostedRequest);
    }

    public class SpotExceptionsUnpostedService : BroadcastBaseClass, ISpotExceptionsUnpostedService
    {
        const string flightStartDateFormat = "MM/dd";
        const string flightEndDateFormat = "MM/dd/yyyy";

        private readonly ISpotExceptionsUnpostedRepository _SpotExceptionsUnpostedRepository;

        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public SpotExceptionsUnpostedService(
            IDataRepositoryFactory dataRepositoryFactory,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsUnpostedRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionsUnpostedRepository>();
            _FeatureToggleHelper = featureToggleHelper;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsUnpostedResultDto> GetSpotExceptionsUnposted(SpotExceptionsUnpostedRequestDto spotExceptionsUnpostedRequest)
        {
            var spotExceptionOutOfSpecUnpostedResult = new SpotExceptionsUnpostedResultDto();

            try
            {
                var spotExceptionUnpostedNoPlanResult = await _SpotExceptionsUnpostedRepository.GetSpotExceptionUnpostedNoPlanAsync(spotExceptionsUnpostedRequest.WeekStartDate, spotExceptionsUnpostedRequest.WeekEndDate);
                var spotExceptionUnpostedNoReelRosterResult = await _SpotExceptionsUnpostedRepository.GetSpotExceptionUnpostedNoReelRosterAsync(spotExceptionsUnpostedRequest.WeekStartDate, spotExceptionsUnpostedRequest.WeekEndDate);

                spotExceptionOutOfSpecUnpostedResult.NoPlan = spotExceptionUnpostedNoPlanResult.Select(x => new SpotExceptionOutOfSpecNoPlanDto
                {
                    HouseIsci = x.HouseIsci,
                    ClientIsci = x.ClientIsci,
                    ClientSpotLength = ":" + x.ClientSpotLengthId.ToString(),
                    AffectedSpotsCount = x.Count,
                    ProgramAirDate = x.ProgramAirTime.ToShortDateString(),
                    EstimateId = x.EstimateID
                }).ToList();

                spotExceptionOutOfSpecUnpostedResult.NoReelRoster = spotExceptionUnpostedNoReelRosterResult.Select(x => new SpotExceptionOutOfSpecNoReelRosterDto
                {
                    HouseIsci = x.HouseIsci,
                    AffectedSpotsCount = x.Count,
                    ProgramAirDate = x.ProgramAirTime.ToShortDateString(),
                    EstimateId = x.EstimateId
                }).ToList();
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return spotExceptionOutOfSpecUnpostedResult;
        }
    }
}
