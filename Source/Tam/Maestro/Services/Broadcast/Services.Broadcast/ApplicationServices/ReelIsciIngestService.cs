using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.ReelRosterIscis;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IReelIsciIngestService : IApplicationService
    {
        /// <summary>
        /// Allows to test the reel isci client.
        /// </summary>
        List<ReelRosterIsciDto> TestReelISciApiClient(DateTime startDate, int numberOfDays);
        /// <summary>
        /// Perform reel isci ingest.
        /// </summary>
        [Queue("reelisciingest")]
        void PerformReelIsciIngest(string userName);
    }

    public class ReelIsciIngestService : BroadcastBaseClass, IReelIsciIngestService
    {
        private readonly IReelIsciApiClient _ReelIsciApiClient;
        private readonly IReelIsciIngestJobsRepository _ReelIsciIngestJobsRepository;
        private readonly IDateTimeEngine _DateTimeEngine;

        public ReelIsciIngestService(IReelIsciApiClient reelIsciApiClient, IDataRepositoryFactory broadcastDataRepositoryFactory, IDateTimeEngine dateTimeEngine)
        {
            _ReelIsciApiClient = reelIsciApiClient;
            _ReelIsciIngestJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IReelIsciIngestJobsRepository>();
            _DateTimeEngine = dateTimeEngine;
        }

        /// <inheritdoc />
        public List<ReelRosterIsciDto> TestReelISciApiClient(DateTime startDate, int numberOfDays)
        {
            _LogInfo($"Calling RealIsciClient. startDate='{startDate.ToString(ReelIsciApiClient.ReelIsciApiDateFormat)}';numberOfDays='{numberOfDays}'");

            var result = _ReelIsciApiClient.GetReelRosterIscis(startDate, numberOfDays);

            _LogInfo($"Received a response containing '{result.Count}' records.");
            return result;
        }
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void PerformReelIsciIngest(string userName)
        {
            _LogInfo($"executing...... .", userName);
            var jobId = 0;
            try
            {
                _LogInfo("Starting");
                var reelIsciIngestJob = new ReelIsciIngestJobDto
                {
                    Status = BackgroundJobProcessingStatus.Processing,
                    QueuedBy = userName,
                    QueuedAt = _DateTimeEngine.GetCurrentMoment()
                };
                jobId = _ReelIsciIngestJobsRepository.AddReelIsciIngestJob(reelIsciIngestJob);

                var reelIsciIngestJobCompleted = new ReelIsciIngestJobDto
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    QueuedBy = userName,
                    QueuedAt = _DateTimeEngine.GetCurrentMoment(),
                    CompletedAt = _DateTimeEngine.GetCurrentMoment()
                };
                _ReelIsciIngestJobsRepository.UpdateReelIsciIngestJob(reelIsciIngestJobCompleted);
            }
            catch (Exception ex)
            {
                _LogError("Error Caught", ex);
                var reelIsciIngestJobFailed = new ReelIsciIngestJobDto
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Failed,
                    QueuedBy = userName,
                    QueuedAt = _DateTimeEngine.GetCurrentMoment(),
                    ErrorMessage = $"Error Caught : {ex.ToString()}"
                };
                _ReelIsciIngestJobsRepository.UpdateReelIsciIngestJob(reelIsciIngestJobFailed);
            }

            _LogInfo($"Completed.....");

        }
    }
}
