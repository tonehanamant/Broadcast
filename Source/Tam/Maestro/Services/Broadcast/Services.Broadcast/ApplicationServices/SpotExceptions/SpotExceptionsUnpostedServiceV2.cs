using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using System.Threading.Tasks;
using System;
using System.Linq;
using Services.Broadcast.Entities.SpotExceptions.Unposted;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using Services.Broadcast.Repositories;
using Services.Broadcast.Exceptions;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    /// <summary></summary>
    public interface ISpotExceptionsUnpostedServiceV2 : IApplicationService
    {
        /// <summary>
        /// Gets the out of spec unposted asynchronous.
        /// </summary>
        /// <param name="outOfSpecUnpostedRequest">The out of spec unposted request.</param>
        /// <returns></returns>
        OutOfSpecUnpostedResultsDto GetOutOfSpecUnposted(OutOfSpecUnpostedRequestDto outOfSpecUnpostedRequest);
    }

    /// <summary></summary>
    public class SpotExceptionsUnpostedServiceV2 : BroadcastBaseClass, ISpotExceptionsUnpostedServiceV2
    {
        private readonly ISpotExceptionsUnpostedRepositoryV2 _SpotExceptionsUnpostedRepositoryV2;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpotExceptionsUnpostedServiceV2" /> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="featureToggleHelper">The feature toggle helper.</param>
        /// <param name="configurationSettingsHelper">The configuration settings helper.</param>
        public SpotExceptionsUnpostedServiceV2(
            IDataRepositoryFactory dataRepositoryFactory,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsUnpostedRepositoryV2 = dataRepositoryFactory.GetDataRepository<ISpotExceptionsUnpostedRepositoryV2>();
        }

        /// <inheritdoc />
        public OutOfSpecUnpostedResultsDto GetOutOfSpecUnposted(OutOfSpecUnpostedRequestDto outOfSpecUnpostedRequest)
        {
            var spotExceptionOutOfSpecUnpostedResult = new OutOfSpecUnpostedResultsDto();

            try
            {
                var unpostedNoPlanResult = _SpotExceptionsUnpostedRepositoryV2.GetSpotExceptionUnpostedNoPlan(outOfSpecUnpostedRequest.WeekStartDate, outOfSpecUnpostedRequest.WeekEndDate);
                var unpostedNoReelRosterResult = _SpotExceptionsUnpostedRepositoryV2.GetSpotExceptionUnpostedNoReelRoster(outOfSpecUnpostedRequest.WeekStartDate, outOfSpecUnpostedRequest.WeekEndDate);

                spotExceptionOutOfSpecUnpostedResult.NoPlan = unpostedNoPlanResult.Select(x => new OutOfSpecNoPlanDto
                {
                    HouseIsci = x.HouseIsci,
                    ClientIsci = x.ClientIsci,
                    ClientSpotLength = x.ClientSpotLength.HasValue ? $":{x.ClientSpotLength.Value}" : null,
                    AffectedSpotsCount = x.Count,
                    ProgramAirDate = DateTimeHelper.GetForDisplay(x.ProgramAirTime, SpotExceptionsConstants.DateFormat),
                    EstimateId = x.EstimateID.Value
                }).ToList();

                spotExceptionOutOfSpecUnpostedResult.NoReelRoster = unpostedNoReelRosterResult.Select(x => new OutOfSpecNoReelRosterDto
                {
                    HouseIsci = x.HouseIsci,
                    AffectedSpotsCount = x.Count,
                    ProgramAirDate = DateTimeHelper.GetForDisplay(x.ProgramAirTime, SpotExceptionsConstants.DateFormat),
                    EstimateId = x.EstimateId.Value
                }).ToList();
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the Out Of Spec Unposted from the Database";
                throw new CadentException(msg, ex);
            }

            return spotExceptionOutOfSpecUnpostedResult;
        }
    }
}
