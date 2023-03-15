using Common.Services.Repositories;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.SpotExceptions.DecisionSync;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories.SpotExceptions;
using System.Threading.Tasks;

namespace Services.Broadcast.Validators
{
    public interface ISpotExceptionsValidator
    {
        /// <summary>
        /// Validates the data available for synchronize.
        /// </summary>
        void ValidateDataAvailableForSync();

        /// <summary>
        /// Validates the synchronize already running.
        /// </summary>
        /// <param name="runningSyncId">The running synchronize identifier.</param>
        /// <returns></returns>
        Task<GetSyncStateResponseDto> ValidateSyncAlreadyRunning(int runningSyncId);
    }

    public class SpotExceptionsValidator : ISpotExceptionsValidator
    {
        private readonly ISpotExceptionsOutOfSpecRepositoryV2 _SpotExceptionsOutOfSpecRepositoryV2;
        private readonly ISpotExceptionsRecommendedPlanRepositoryV2 _SpotExceptionsRecommendedPlanRepositoryV2;

        private readonly ISpotExceptionsApiClient _SpotExceptionsApiClient;

        const string NO_DECISIONS_NEEDING_SYNCED = "There are no decisions available for a sync. Any decisions previously submitted are being processed.";
        const string SYNC_ALREADY_RUNNING = "A Sync process is currently running. Please try again later.";

        public SpotExceptionsValidator(IDataRepositoryFactory broadcastDataRepositoryFactory
            , ISpotExceptionsApiClient spotExceptionsApiClient
            )
        {
            _SpotExceptionsOutOfSpecRepositoryV2 = broadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>();
            _SpotExceptionsRecommendedPlanRepositoryV2 = broadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionsRecommendedPlanRepositoryV2>();
            _SpotExceptionsApiClient = spotExceptionsApiClient;
        }

        /// <inheritdoc />
        public void ValidateDataAvailableForSync()
        {
            int totalDecisionCount;

            var outOfSpecDecisonQueuedCount = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecDecisionQueuedCountAsync();
            var recommandedPlanDecisonQueuedCount = _SpotExceptionsRecommendedPlanRepositoryV2.GetRecommendedPlanDecisionQueuedCountAsync();

            totalDecisionCount = outOfSpecDecisonQueuedCount + recommandedPlanDecisonQueuedCount;

            if (totalDecisionCount == 0)
            {
                throw new SpotExceptionsException(NO_DECISIONS_NEEDING_SYNCED);
            }
        }

        /// <inheritdoc />
        public async Task<GetSyncStateResponseDto> ValidateSyncAlreadyRunning(int runningSyncId)
        {
            if(runningSyncId == -1)
            {
                throw new SpotExceptionsException(SYNC_ALREADY_RUNNING);
            }

            var response = await _SpotExceptionsApiClient.GetSyncStateAsync(runningSyncId);

            if(response.Result.State.State == "RUNNING" || response.Result.State.State == "PENDING")
            {
                throw new SpotExceptionsException(SYNC_ALREADY_RUNNING);
            }
            else
            {
                return response;
            }
        }
    }
}
