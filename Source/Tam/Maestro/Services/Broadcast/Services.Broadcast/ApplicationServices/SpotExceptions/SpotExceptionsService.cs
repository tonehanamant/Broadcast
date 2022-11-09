using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsService : IApplicationService
    {
        /// <summary>
        /// Clear all data from spot exceptions tables.
        /// </summary>   
        bool ClearSpotExceptionAllData();

        /// <summary>
        /// Triggers the decision sync operation.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <returns></returns>
        Task<bool> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest);

        /// <summary>
        /// Gets the queued decision count.
        /// </summary>
        Task<int> GetQueuedDecisionCount();
    }

    public class SpotExceptionsService : BroadcastBaseClass, ISpotExceptionsService
    {
        private readonly ISpotExceptionsRepository _SpotExceptionsRepository;
        private readonly ISpotExceptionsApiClient _SpotExceptionsApiClient;

        private readonly Lazy<bool> _IsNotifyDataReadyEnabled;

        public SpotExceptionsService(
            IDataRepositoryFactory dataRepositoryFactory,
            ISpotExceptionsApiClient spotExceptionsApiClient,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionsRepository>();
            _SpotExceptionsApiClient = spotExceptionsApiClient;

            _IsNotifyDataReadyEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SPOT_EXCEPTION_NOTIFY_SYNC));
        }

        /// <inheritdoc />
        public bool ClearSpotExceptionAllData()
        {
            var result = _SpotExceptionsRepository.ClearSpotExceptionAllData();
            return result;
        }

        /// <inheritdoc />
        public async Task<bool> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            _LogInfo($"Beginning results sync. Requested by '{triggerDecisionSyncRequest.UserName}';");

            bool result;

            if (_IsNotifyDataReadyEnabled.Value)
            {
                
                _LogInfo($"Attempting to notify consumers that results data is ready.  Requested by '{triggerDecisionSyncRequest.UserName}'.");
                var syncRequest = new ResultsSyncRequest
                {
                    RequestedBy = triggerDecisionSyncRequest.UserName
                };
                result = await _SpotExceptionsApiClient.PublishSyncRequestAsync(syncRequest);
                _LogInfo($"Successfully notified consumers that results data is ready.  Requested by '{triggerDecisionSyncRequest.UserName}'.");
            }
            else
            {
                var dateTime = DateTime.Now;

                _LogWarning($"Mocking the sync and just marking synced without notifying the DataLake.  Requested by '{triggerDecisionSyncRequest.UserName}';");
                // this is the mock.
                try
                {
                    var isSyncedOutOfSpecDecision = await _SpotExceptionsRepository.SyncOutOfSpecDecisionsAsync(triggerDecisionSyncRequest, dateTime);
                    var isSyncedRecommandedPlanDecision = await _SpotExceptionsRepository.SyncRecommendedPlanDecisionsAsync(triggerDecisionSyncRequest, dateTime);

                    if (isSyncedOutOfSpecDecision == false && isSyncedRecommandedPlanDecision == false)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    var msg = $"Could not retrieve the data from the Database";
                    throw new CadentException(msg, ex);
                }

                _LogInfo($"Completed results sync. Requested by '{triggerDecisionSyncRequest.UserName}';");
            }

            _LogInfo($"Completed results sync. Requested by '{triggerDecisionSyncRequest.UserName}';");

            return result;
        }

        /// <inheritdoc />
        public async Task<int> GetQueuedDecisionCount()
        {
            int totalDecisionCount;
            try
            {
                var outOfSpecDecisonQueuedCount = await _SpotExceptionsRepository.GetOutOfSpecDecisionQueuedCountAsync();
                var recommandedPlanDecisonQueuedCount = await _SpotExceptionsRepository.GetRecommendedPlanDecisionQueuedCountAsync();
                totalDecisionCount = outOfSpecDecisonQueuedCount + recommandedPlanDecisonQueuedCount;
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return totalDecisionCount;
        }
    }
}
