using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Exceptions;
using System.Threading.Tasks;
using System;
using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
using Common.Services.Repositories;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using Services.Broadcast.Validators;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsServiceV2 : IApplicationService
    {
        /// <summary>
        /// Triggers the decision synchronize.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <returns></returns>
        Task<bool> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest);
    }

    public class SpotExceptionsServiceV2 : BroadcastBaseClass, ISpotExceptionsServiceV2
    {
        private readonly ISpotExceptionsRepositoryV2 _SpotExceptionsRepositoryV2;
        private readonly ISpotExceptionsApiClient _SpotExceptionsApiClient;
        private readonly ISpotExceptionsValidator _SpotExceptionValidator;

        private readonly Lazy<bool> _IsNotifyDataReadyEnabled;

        public SpotExceptionsServiceV2(
            IDataRepositoryFactory dataRepositoryFactory,
            ISpotExceptionsApiClient spotExceptionsApiClient,
            ISpotExceptionsValidator spotExceptionValidator,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsRepositoryV2 = dataRepositoryFactory.GetDataRepository<ISpotExceptionsRepositoryV2>();
            _SpotExceptionsApiClient = spotExceptionsApiClient;
            _SpotExceptionValidator = spotExceptionValidator;
            _IsNotifyDataReadyEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SPOT_EXCEPTION_NOTIFY_SYNC));
        }

        /// <inheritdoc />
        public async Task<bool> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            _LogInfo($"Beginning results sync. Requested by '{triggerDecisionSyncRequest.UserName}';");

            bool result;

            if (_IsNotifyDataReadyEnabled.Value)
            {

                _LogInfo($"Attempting to notify consumers that results data is ready.  Requested by '{triggerDecisionSyncRequest.UserName}'.");

                try
                {
                    var syncRequest = new ResultsSyncRequest
                    {
                        RequestedBy = triggerDecisionSyncRequest.UserName
                    };

                    _SpotExceptionValidator.ValidateDataAvailableForSync();

                    var runningSyncRunId = await _SpotExceptionsRepositoryV2.GetRunningSyncRunId();

                    _LogInfo($"Attempting to verify the state of last running job with a runId '{runningSyncRunId}'.");
                    if (runningSyncRunId != 0)
                    {
                        var jobState = await _SpotExceptionValidator.ValidateSyncAlreadyRunning(runningSyncRunId);

                        if(jobState.Result.State.State != "RUNNING")
                        {
                            _SpotExceptionsRepositoryV2.SetCurrentJobToComplete(jobState);
                        }
                    }

                    result = await _SpotExceptionsApiClient.PublishSyncRequestAsync(syncRequest);
                }
                catch (SpotExceptionsException ex)
                {
                    var msg = $"Sync can not be initiated";
                    throw new SpotExceptionsException(msg, ex);
                }
                                
                _LogInfo($"Successfully notified consumers that results data is ready.  Requested by '{triggerDecisionSyncRequest.UserName}'.");
            }
            else
            {
                var dateTime = DateTime.Now;

                _LogWarning($"Mocking the sync and just marking synced without notifying the DataLake.  Requested by '{triggerDecisionSyncRequest.UserName}';");
                // this is the mock.
                try
                {
                    var isSyncedOutOfSpecDecision = await _SpotExceptionsRepositoryV2.SyncOutOfSpecDecisionsAsync(triggerDecisionSyncRequest, dateTime);
                    var isSyncedRecommandedPlanDecision = await _SpotExceptionsRepositoryV2.SyncRecommendedPlanDecisionsAsync(triggerDecisionSyncRequest, dateTime);

                    if (isSyncedOutOfSpecDecision == false && isSyncedRecommandedPlanDecision == false)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
                catch (SpotExceptionsException ex)
                {
                    var msg = $"Sync can not be initiated";
                    throw new SpotExceptionsException(msg, ex);
                }

                _LogInfo($"Completed results sync. Requested by '{triggerDecisionSyncRequest.UserName}';");
            }

            _LogInfo($"Completed results sync. Requested by '{triggerDecisionSyncRequest.UserName}';");

            return result;
        }
    }
}
