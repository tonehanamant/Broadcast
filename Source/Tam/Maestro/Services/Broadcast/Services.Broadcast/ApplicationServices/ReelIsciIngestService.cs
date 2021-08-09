﻿using Common.Services.ApplicationServices;
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
using System.Linq;

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
        /// <summary>
        /// RunQueued reel isci ingest.
        /// </summary>       
        [Queue("reelisciingest")]
        void RunQueued(int jobId, string userName);
        /// <summary>
        /// Queued reel isci ingest.
        /// </summary>       
       void Queue(string userName);
    }

    public class ReelIsciIngestService : BroadcastBaseClass, IReelIsciIngestService
    {
        private readonly IReelIsciApiClient _ReelIsciApiClient;
        private readonly IReelIsciIngestJobsRepository _ReelIsciIngestJobsRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IBackgroundJobClient _BackgroundJobClient;

        public ReelIsciIngestService(IReelIsciApiClient reelIsciApiClient, IDataRepositoryFactory broadcastDataRepositoryFactory, IDateTimeEngine dateTimeEngine, IBackgroundJobClient backgroundJobClient)
        {
            _ReelIsciApiClient = reelIsciApiClient;
            _ReelIsciIngestJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IReelIsciIngestJobsRepository>();
            _DateTimeEngine = dateTimeEngine;
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _BackgroundJobClient = backgroundJobClient;
        }

        /// <inheritdoc />
        public List<ReelRosterIsciDto> TestReelISciApiClient(DateTime startDate, int numberOfDays)
        {
            _LogInfo($"Calling RealIsciClient. startDate='{startDate.ToString(ReelIsciApiClient.ReelIsciApiDateFormat)}';numberOfDays='{numberOfDays}'");

            var result = _ReelIsciApiClient.GetReelRosterIscis(startDate, numberOfDays);

            _LogInfo($"Received a response containing '{result.Count}' records.");
            return result;
        }

        public void Queue(string userName)
        {
            var reelIsciIngestJob = new ReelIsciIngestJobDto
            {
                Status = BackgroundJobProcessingStatus.Queued,
                QueuedBy = userName,
                QueuedAt = _DateTimeEngine.GetCurrentMoment()
            };
            var jobId = _ReelIsciIngestJobsRepository.AddReelIsciIngestJob(reelIsciIngestJob);
           
            _BackgroundJobClient.Enqueue<IReelIsciIngestService>(x => x.RunQueued(jobId, userName));
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void RunQueued(int jobId, string userName)
        {
            _Run(jobId, userName);
        }

        protected void _Run(int jobId, string userName)
        {
            int numberOfDays = 21;
            DateTime startDate = _DateTimeEngine.GetCurrentMoment().AddDays(numberOfDays * -1);
            PerformReelIsciIngestBetweenRange(startDate, numberOfDays);
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

                int numberOfDays = 21;
                DateTime startDate = _DateTimeEngine.GetCurrentMoment().AddDays(numberOfDays * -1);                
                PerformReelIsciIngestBetweenRange(startDate, numberOfDays);

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

        /// <inheritdoc />
        internal void PerformReelIsciIngestBetweenRange(DateTime startDate, int numberOfDays)
        {
            _LogInfo($"Calling RealIsciClient. startDate='{startDate.ToString(ReelIsciApiClient.ReelIsciApiDateFormat)}';numberOfDays='{numberOfDays}'");
            var reelRosterIscis = _ReelIsciApiClient.GetReelRosterIscis(startDate, numberOfDays);
            _LogInfo($"Received a response containing '{reelRosterIscis.Count}' records.");

            var endDate = startDate.AddDays(numberOfDays);
            var deletedCount = _DeleteReelIscisBetweenRange(startDate, endDate);
            _LogInfo($"Deleted {deletedCount} reel iscis.");

            var addedCount = 0;
            if (reelRosterIscis?.Any() ?? false)
            {
                addedCount = _AddReelIscis(reelRosterIscis);
            }
            _LogInfo($"Added {addedCount} reel iscis");
        }

        private int _DeleteReelIscisBetweenRange(DateTime startDate, DateTime endDate)
        {
            return _ReelIsciIngestJobsRepository.DeleteReelIscisBetweenRange(startDate, endDate);
        }

        private int _AddReelIscis(List<ReelRosterIsciDto> reelRosterIscis)
        {
            var spotLengths = _SpotLengthRepository.GetSpotLengths();

            var reelIscis = reelRosterIscis.Select(reelRosterIsci => new ReelIsciDto()
            {
                Isci = reelRosterIsci.Isci,
                SpotLengthId = spotLengths.Single(x => x.Display.Equals(reelRosterIsci.SpotLengthDuration.ToString())).Id,
                ActiveStartDate = reelRosterIsci.StartDate,
                ActiveEndDate = reelRosterIsci.EndDate,
                ReelIsciAdvertiserNameReferences = reelRosterIsci.AdvertiserNames.Select(x => new ReelIsciAdvertiserNameReferenceDto()
                {
                    AdvertiserNameReference = x
                }).ToList(),
                IngestedAt = _DateTimeEngine.GetCurrentMoment()
            }).ToList();
            return _ReelIsciIngestJobsRepository.AddReelIscis(reelIscis);
        }
    }
}
