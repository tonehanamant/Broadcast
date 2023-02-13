﻿using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
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
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsServiceV2 : IApplicationService
    {
        /// <summary>
        /// Triggers the decision synchronize.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <returns></returns>
        Task<bool> TriggerDecisionSyncAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest);

        /// <summary>
        /// Clears the spot exception all data.
        /// </summary>
        /// <returns></returns>
        bool ResetSpotExceptionResultsIndicator();
    }

    public class SpotExceptionsServiceV2 : BroadcastBaseClass, ISpotExceptionsServiceV2
    {
        private readonly ISpotExceptionsRepositoryV2 _SpotExceptionsRepositoryV2;
        private readonly ISpotExceptionsApiClient _SpotExceptionsApiClient;
        private readonly ISpotExceptionsValidator _SpotExceptionValidator;

        private readonly IDateTimeEngine _DateTimeEngine;

        private readonly Lazy<bool> _IsNotifyDataReadyEnabled;

        public SpotExceptionsServiceV2(
            IDataRepositoryFactory dataRepositoryFactory,
            ISpotExceptionsApiClient spotExceptionsApiClient,
            ISpotExceptionsValidator spotExceptionValidator,
            IDateTimeEngine dateTimeEngine,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsRepositoryV2 = dataRepositoryFactory.GetDataRepository<ISpotExceptionsRepositoryV2>();
            _SpotExceptionsApiClient = spotExceptionsApiClient;
            _SpotExceptionValidator = spotExceptionValidator;
            _DateTimeEngine = dateTimeEngine;
            _IsNotifyDataReadyEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SPOT_EXCEPTION_NOTIFY_SYNC));
        }

        /// <inheritdoc />
        public async Task<bool> TriggerDecisionSyncAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            _LogInfo($"Beginning results sync. Requested by '{triggerDecisionSyncRequest.UserName}';");

            bool result;
            var currentDate = _DateTimeEngine.GetCurrentMoment();

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
                                        
                    if (runningSyncRunId != 0)
                    {
                        _LogInfo($"Attempting to verify the state of last running job with a runId '{runningSyncRunId}'.");
                        var jobState = await _SpotExceptionValidator.ValidateSyncAlreadyRunning(runningSyncRunId);

                        if(jobState.Result.State.State != "RUNNING")
                        {
                            _SpotExceptionsRepositoryV2.SetCurrentJobToComplete(jobState);
                        }
                    }
                    else
                    {
                        _SpotExceptionsRepositoryV2.SaveRunningSyncJob(syncRequest, currentDate);
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

        /// <inheritdoc />
        public bool ResetSpotExceptionResultsIndicator()
        {
            var result = _SpotExceptionsRepositoryV2.ResetSpotExceptionResultsIndicator();

            _LogInfo($"The results sync indicator has been manually reset by Maintenance.");

            return result;
        }
    }
}
