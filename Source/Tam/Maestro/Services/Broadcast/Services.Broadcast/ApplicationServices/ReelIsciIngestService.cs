using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.ReelRosterIscis;
using Services.Broadcast.Helpers;
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
        private readonly IReelIsciRepository _ReelIsciRepository;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IPlanIsciRepository _PlanIsciRepository;

        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private Lazy<int> _IngestNumberOfDays;

        public ReelIsciIngestService(IReelIsciApiClient reelIsciApiClient, IDataRepositoryFactory broadcastDataRepositoryFactory, IDateTimeEngine dateTimeEngine,
            IBackgroundJobClient backgroundJobClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _ReelIsciApiClient = reelIsciApiClient;
            _ReelIsciIngestJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IReelIsciIngestJobsRepository>();
            _ReelIsciRepository = broadcastDataRepositoryFactory.GetDataRepository<IReelIsciRepository>();
            _SpotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _PlanIsciRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

            _DateTimeEngine = dateTimeEngine;            
            _BackgroundJobClient = backgroundJobClient;
            _IngestNumberOfDays = new Lazy<int>(_GetIngestNumberOfDays);
        }

        private int _GetIngestNumberOfDays()
        {
            var numberOfDays = _ConfigurationSettingsHelper.GetConfigValue<int>(ConfigKeys.ReelIsciIngestNumberOfDays);
            return numberOfDays;
        }

        /// <inheritdoc />
        public List<ReelRosterIsciDto> TestReelISciApiClient(DateTime startDate, int numberOfDays)
        {
            _LogInfo($"reel-isci-ingest : Calling RealIsciClient. startDate='{startDate.ToString(ReelIsciApiClient.ReelIsciApiDateFormat)}';numberOfDays='{numberOfDays}'");

            var result = _ReelIsciApiClient.GetReelRosterIscis(startDate, numberOfDays);

            _LogInfo($"reel-isci-ingest : Received a response containing '{result.Count}' records.");
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
            _LogInfo($"reel-isci-ingest : Queued Reel Isci Ingest job.  JobId : {jobId}");

        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void RunQueued(int jobId, string userName)
        {
            _LogInfo($"reel-isci-ingest : De-queueing a Reel Isci Ingest Job.  JobId : {jobId}");

            _Run(jobId, userName);
           
        }

        protected void _Run(int jobId, string userName)
        {
            var numberOfDays = _IngestNumberOfDays.Value;
            var startDate = _GetIngestStartDate(numberOfDays);
            PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, userName);
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void PerformReelIsciIngest(string userName)
        {
            var numberOfDays = _IngestNumberOfDays.Value;
            var startDate = _GetIngestStartDate(numberOfDays);

            _LogInfo($"reel-isci-ingest : Starting the Reel Isci Ingest Job.", userName);

            var jobId = 0;
            var reelIsciIngestJob = new ReelIsciIngestJobDto
            {
                Status = BackgroundJobProcessingStatus.Processing,
                QueuedBy = userName,
                QueuedAt = _DateTimeEngine.GetCurrentMoment()
            };
            jobId = _ReelIsciIngestJobsRepository.AddReelIsciIngestJob(reelIsciIngestJob);

            PerformReelIsciIngestBetweenRange(jobId, startDate, numberOfDays, userName);
           
        }

        private DateTime _GetIngestStartDate(int numberOfDays)
        {
            var startDate = _DateTimeEngine.GetCurrentMoment().AddDays(numberOfDays * -1);
            return startDate;
        }

        internal void PerformReelIsciIngestBetweenRange(int jobId, DateTime startDate, int numberOfDays, string userName)
        {
            // a pseudo-setting for controlling the call strategy per upstream performance
            const bool ingestOverManyCalls = false;

            var isKeepOrphanedIsciMappingEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_KEEP_ORPHANED_ISCI_MAPPING);

            try
            {
                _LogInfo($"reel-isci-ingest : Calling RealIsciClient. startDate='{startDate.ToString(ReelIsciApiClient.ReelIsciApiDateFormat)}';numberOfDays='{numberOfDays}'");
                var reelRosterIscis = _GetReelRosterIscis(startDate, numberOfDays, ingestOverManyCalls);
                _LogInfo($"reel-isci-ingest : Received a response containing '{reelRosterIscis.Count}' records.");

                var endDate = startDate.AddDays(numberOfDays);
                var deletedCount = _DeleteReelIscisBetweenRange(startDate, endDate);
                _LogInfo($"reel-isci-ingest : Deleted {deletedCount} reel iscis.");

                var addedCount = 0;
                if (reelRosterIscis?.Any() ?? false)
                {
                    addedCount = _AddReelIscis(reelRosterIscis);
                }
                _LogInfo($"reel-isci-ingest : Added {addedCount} reel iscis");

                if (!isKeepOrphanedIsciMappingEnabled)
                {
                    DateTime deletedAt = _DateTimeEngine.GetCurrentMoment();
                    var deletedplanIscisCount = _DeletePlanIscisNotExistInReelIsci(deletedAt, userName);
                    _LogInfo($"reel-isci-ingest : Deleted {deletedplanIscisCount} plan iscis.");
                }
                else
                {
                    _LogInfo($"reel_isci_ingest: isKeepOrphanedIscimappingEnable Flag is {isKeepOrphanedIsciMappingEnabled}. No isci products or plan iscis deleted.");
                }

                var reelIsciIngestJobCompleted = new ReelIsciIngestJobDto
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    CompletedAt = _DateTimeEngine.GetCurrentMoment()
                };
                _ReelIsciIngestJobsRepository.UpdateReelIsciIngestJob(reelIsciIngestJobCompleted);

                _LogInfo("reel-isci-ingest : Job completed.");
            }
            catch (Exception ex)
            {
                _LogError("reel-isci-ingest : Error Caught", ex);
                var reelIsciIngestJobFailed = new ReelIsciIngestJobDto
                {
                    Id = jobId,
                    Status = BackgroundJobProcessingStatus.Failed,
                    CompletedAt = _DateTimeEngine.GetCurrentMoment(),
                    ErrorMessage = $"Error Caught : {ex.ToString()}"
                };
                _ReelIsciIngestJobsRepository.UpdateReelIsciIngestJob(reelIsciIngestJobFailed);
                throw;
            }
        }

        internal List<ReelRosterIsciDto> _GetReelRosterIscis(DateTime startDate, int totalNumberOfDays, bool manyCalls)
        {
            if (manyCalls)
            {
                var maxDaysForCall = 100;
                var totalCallsForMaxDays = totalNumberOfDays / maxDaysForCall;
                var totalCallsForRemainingDays = totalNumberOfDays % maxDaysForCall;

                var numberOfDaysList = new List<int>();
                for (int i = 0; i < totalCallsForMaxDays; i++)
                {
                    numberOfDaysList.Add(maxDaysForCall);
                }
                if (totalCallsForRemainingDays > 0)
                {
                    numberOfDaysList.Add(totalCallsForRemainingDays);
                }

                var reelRosterIscis = new List<ReelRosterIsciDto>();
                foreach (var numberOfDays in numberOfDaysList)
                {
                    var result = _ReelIsciApiClient.GetReelRosterIscis(startDate, numberOfDays);
                    reelRosterIscis.AddRange(result);
                    startDate = startDate.AddDays(numberOfDays);
                }
                return reelRosterIscis;
            }
            else
            {
                var reelRosterIscis = _ReelIsciApiClient.GetReelRosterIscis(startDate, totalNumberOfDays);
                return reelRosterIscis;
            }
        }

        private int _DeleteReelIscisBetweenRange(DateTime startDate, DateTime endDate)
        {
            var result = _ReelIsciRepository.DeleteReelIscisBetweenRange(startDate, endDate);
            return result;
        }

        private int _AddReelIscis(List<ReelRosterIsciDto> reelRosterIscis)
        {
            var spotLengths = _SpotLengthRepository.GetSpotLengths();
            var ingestedAt = _DateTimeEngine.GetCurrentMoment();

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
                IngestedAt = ingestedAt
            }).ToList();
            var result = _ReelIsciRepository.AddReelIscis(reelIscis);
            return result;
        }

        private int _DeletePlanIscisNotExistInReelIsci(DateTime deletedAt, string deletedBy)
        {
            var result = _PlanIsciRepository.DeletePlanIscisNotExistInReelIsci(deletedAt, deletedBy);
            return result;
        }
    }
}
