using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.SpotExceptionsApi;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Operations for running \ managing Spot Exception jobs.
    /// </summary>
    /// <seealso cref="Common.Services.ApplicationServices.IApplicationService" />
    public interface ISpotExceptionsSyncService : IApplicationService
    {
        Task<SpotExceptionsIngestTriggerResponse> TriggerIngestForWeekAsync(SpotExceptionsIngestTriggerRequest request);

        Task<SpotExceptionsIngestTriggerResponse> TriggerIngestForDateRangeAsync(SpotExceptionsIngestTriggerRequest request);

        [Queue("spotexceptionssyncingest")]
        Task<SpotExceptionsIngestTriggerResponse> PerformIngest(SpotExceptionsIngestTriggerRequest request);

        string SetJobToError(string username, int jobId);
    }

    public class SpotExceptionsSyncService : BroadcastBaseClass, ISpotExceptionsSyncService
    {
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly ISpotExceptionsIngestApiClient _ApiClient;

        private readonly ISpotExceptionsIngestJobRepository _SpotExceptionsIngestJobRepository;

        public SpotExceptionsSyncService(IDataRepositoryFactory dataRepositoryFactory, IBackgroundJobClient backgroundJobClient,
            IDateTimeEngine dateTimeEngine, ISpotExceptionsIngestApiClient apiClient,
            IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) 
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _BackgroundJobClient = backgroundJobClient;
            _SpotExceptionsIngestJobRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionsIngestJobRepository>();

            _DateTimeEngine = dateTimeEngine;
            _ApiClient = apiClient;
        }
        
        public async Task<SpotExceptionsIngestTriggerResponse> TriggerIngestForWeekAsync(SpotExceptionsIngestTriggerRequest request)
        {
            if (!request.RequestId.HasValue)
            {
                request.RequestId = Guid.NewGuid();
            }

            var currentDate = _DateTimeEngine.GetCurrentMoment();
            var currentBroadcastWeek = BroadcastWeeksHelper.GetContainingWeek(currentDate);

            request.StartDate = currentBroadcastWeek.WeekStartDate;
            request.EndDate = currentBroadcastWeek.WeekEndDate;
            
            var response = await _ExecuteOrEnqueueIngest(request);
            return response;
        }

        public async Task<SpotExceptionsIngestTriggerResponse> TriggerIngestForDateRangeAsync(SpotExceptionsIngestTriggerRequest request)
        {
            if (!request.RequestId.HasValue)
            {
                request.RequestId = Guid.NewGuid();
            }

            var response = await _ExecuteOrEnqueueIngest(request);
            return response;
        }

        public string SetJobToError(string username, int jobId)
        {
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _SpotExceptionsIngestJobRepository.SetJobToError(jobId, username, currentDate);

            return $"Successfully set JobId '{jobId}' to error state.";
        }

        public async Task<SpotExceptionsIngestTriggerResponse> PerformIngest(SpotExceptionsIngestTriggerRequest request)
        {
            _LogInfo($"Performing ingest for request. RequestId : '{request.RequestId}';");

            try
            {
                var apiRequest = new IngestApiRequest
                {
                    Username = request.Username,
                    StartDate = request.StartDate.Value,
                    EndDate = request.EndDate.Value
                };

                var apiResponse = await _ApiClient.IngestAsync(apiRequest);

                _LogInfo($"Completed ingest for request. RequestId : '{request.RequestId}';");

                var response = new SpotExceptionsIngestTriggerResponse
                {
                    Request = request,
                    Response = apiResponse,
                    Success = true,
                    Message = $"Request Id : '{request.RequestId}' completed."
                };

                return response;
            }
            catch (Exception ex)
            {
                var message = $"Error caught handling ingest request. RequestId : '{request.RequestId}';";
                _LogError(message, ex);

                throw new CadentException(message, ex);
            }
        }

        private async Task<SpotExceptionsIngestTriggerResponse> _ExecuteOrEnqueueIngest(SpotExceptionsIngestTriggerRequest request)
        {
            _LogInfo($"Registering request : " +
                $"RequestId : '{request.RequestId}'; " +
                $"Username : '{request.Username}'; " +
                $"StartDate : '{request.StartDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}'; " +
                $"EndDate : '{request.EndDate.Value.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}'; " +
                $"RunInBackground : '{request.RunInBackground}';");

            if (request.RunInBackground)
            {
                _BackgroundJobClient.Enqueue<ISpotExceptionsSyncService>(x => x.PerformIngest(request));

                var queueResponse = new SpotExceptionsIngestTriggerResponse
                {
                    Request = request,
                    Success = true,
                    Message = $"Job has been queued for request '{request.RequestId}'."
                };
                return queueResponse;
            }

            var ingestResponse = await PerformIngest(request);
            return ingestResponse;
        }
    }
}
