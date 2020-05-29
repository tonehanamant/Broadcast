using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tam.Maestro.Common;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsBySourceProcessor : InventoryProgramsProcessingEngineBase
    {
        protected readonly IInventoryProgramsBySourceJobsRepository _InventoryProgramsBySourceJobsRepository;
        protected readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public InventoryProgramsBySourceProcessor(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IGenreCache genreCache,
            IFileService fileService,
            IEmailerService emailerService,
            IEnvironmentService environmentService)
            : base(broadcastDataRepositoryFactory,
                programGuideApiClient,
                stationMappingService,
                genreCache,
                fileService,
                emailerService,
                environmentService)
        {
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _InventoryProgramsBySourceJobsRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryProgramsBySourceJobsRepository>();
        }

        protected override IInventoryProgramsJobsRepository _GetJobsRepository()
        {
            return _InventoryProgramsBySourceJobsRepository;
        }

        protected override InventoryProgramsProcessingJobDiagnostics _GetNewDiagnostics()
        {
            return new InventoryProgramsProcessingJobBySourceDiagnostics(OnDiagnosticMessageUpdate);
        }

        protected override InventorySource _GetInventorySource(int jobId)
        {
            var sourceId = _InventoryProgramsBySourceJobsRepository.GetJob(jobId).InventorySourceId;
            var inventorySource = _InventoryRepository.GetInventorySource(sourceId);
            return inventorySource;
        }

        protected override List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            ((InventoryProgramsProcessingJobBySourceDiagnostics)processDiagnostics).RecordRequestParameters(job.InventorySourceId, job.StartDate, job.EndDate);

            var mediaWeekIds = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(job.StartDate, job.EndDate).Select(w => w.Id).ToList();
            ((InventoryProgramsProcessingJobBySourceDiagnostics)processDiagnostics).RecordMediaWeekIds(mediaWeekIds);

            var manifests = _InventoryRepository.GetInventoryBySourceForProgramsProcessing(job.InventorySourceId, mediaWeekIds);
            return manifests;
        }

        protected override InventoryProgramsRequestPackage _BuildRequestPackage(List<StationInventoryManifest> inventoryManifests,
            InventorySource inventorySource, InventoryProgramsProcessingJobDiagnostics processDiagnostics, int jobId)
        {
            var requestElementNumber = 0;

            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            var startDate = job.StartDate;
            var endDate = job.EndDate;

            processDiagnostics.RecordTransformToInputStart(inventoryManifests.Count, startDate, endDate);

            var requestMappings = new List<GuideRequestResponseMapping>();
            var requestElements = new List<GuideRequestElementDto>();

            foreach (var manifest in inventoryManifests)
            {
                if (string.IsNullOrWhiteSpace(manifest.Station.Affiliation))
                {
                    // no affiliate indicates unrated so don't query for.
                    continue;
                }

                foreach (var daypart in manifest.ManifestDayparts.OrderBy(d => d.Daypart.StartTime))
                {
                    var stationCallLetters = _GetManifestStationCallLetters(manifest, inventorySource);

                    var requestElementMapping = new GuideRequestResponseMapping
                    {
                        RequestElementNumber = ++requestElementNumber,
                        ManifestId = manifest.Id ?? 0,
                        ManifestDaypartId = daypart.Id ?? 0,
                        StartDate = startDate,
                        EndDate = endDate,
                        DaypartText = daypart.Daypart.Preview,
                        StationCallLetters = stationCallLetters,
                        NetworkAffiliate = manifest.Station.Affiliation
                    };
                    requestMappings.Add(requestElementMapping);
                    requestElements.Add(
                        new GuideRequestElementDto
                        {
                            Id = requestElementMapping.RequestEntryId,
                            StartDate = startDate,
                            EndDate = endDate,
                            StationCallLetters = stationCallLetters,
                            NetworkAffiliate = manifest.Station.Affiliation,
                            Daypart = new GuideRequestDaypartDto
                            {
                                Id = requestElementMapping.RequestEntryId,
                                Name = daypart.Daypart.Preview,
                                Monday = daypart.Daypart.Monday,
                                Tuesday = daypart.Daypart.Tuesday,
                                Wednesday = daypart.Daypart.Wednesday,
                                Thursday = daypart.Daypart.Thursday,
                                Friday = daypart.Daypart.Friday,
                                Saturday = daypart.Daypart.Saturday,
                                Sunday = daypart.Daypart.Sunday,
                                StartTime = daypart.Daypart.StartTime,
                                EndTime = daypart.Daypart.EndTime
                            }
                        });
                }
            }

            processDiagnostics.RecordTransformToInputStop(requestElements.Count);

            var requestPackage = new InventoryProgramsRequestPackage
            {
                RequestMappings = requestMappings,
                RequestElements = requestElements,
                InventoryManifests = inventoryManifests,
                StartDateRange = startDate,
                EndDateRange = endDate
            };
            return requestPackage;
        }

        protected override List<StationInventoryManifestDaypartProgram> _GetProgramsFromResponse(
            GuideResponseElementDto currentResponse,
            GuideRequestResponseMapping currentMapping,
            InventoryProgramsRequestPackage requestPackage
        )
        {
            var programs = new List<StationInventoryManifestDaypartProgram>();
            currentResponse.Programs.Select(p =>
                _MapProgramDto(p, currentMapping.ManifestDaypartId, requestPackage))
                .ForEach(a => programs.Add(a));

            return programs;
        }

        protected override string _GetExportedFileReadyNotificationEmailBody(int jobId, string filePath)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            var source = _GetInventorySource(jobId);

            var body = new StringBuilder();
            body.AppendLine("Hello,");
            body.AppendLine();
            body.AppendLine($"A ProgramGuide Interface file has been exported.");
            body.AppendLine();
            body.AppendLine($"\tJobGroupID : {job.JobGroupId}");
            body.AppendLine($"\tInventory Source : {source.Name}");
            body.AppendLine($"\tRange Start Date : {job.StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            body.AppendLine($"\tRange End Date : {job.EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            body.AppendLine();
            body.AppendLine($"File Path :");
            body.AppendLine($"\t{filePath}");
            body.AppendLine();
            body.AppendLine($"Have a nice day.");
            return body.ToString();
        }

        protected override string _GetExportedFileFailedNotificationEmailBody(int jobId)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            var source = _GetInventorySource(jobId);

            var body = new StringBuilder();
            body.AppendLine("Hello,");
            body.AppendLine();
            body.AppendLine($"A ProgramGuide Interface file has failed to be exported.");
            body.AppendLine();
            body.AppendLine($"\tJobGroupID : {job.JobGroupId}");
            body.AppendLine($"\tInventory Source : {source.Name}");
            body.AppendLine($"\tRange Start Date : {job.StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            body.AppendLine($"\tRange End Date : {job.EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            body.AppendLine();
            body.AppendLine($"Have a nice day.");
            return body.ToString();
        }

        protected override string _GetNoInventoryToProcessNotificationEmailBody(int jobId)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            var source = _GetInventorySource(jobId);

            var body = new StringBuilder();
            body.AppendLine("Hello,");
            body.AppendLine();
            body.AppendLine($"A ProgramGuide Interface file was not exported because no inventory was found to process.");
            body.AppendLine();
            body.AppendLine($"\tJobGroupID : {job.JobGroupId}");
            body.AppendLine($"\tInventory Source : {source.Name}");
            body.AppendLine($"\tRange Start Date : {job.StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            body.AppendLine($"\tRange End Date : {job.EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            body.AppendLine();
            body.AppendLine($"Have a nice day.");
            return body.ToString();
        }

        protected override string _GetExportFileName(int jobId)
        {
            var job = _InventoryProgramsBySourceJobsRepository.GetJob(jobId);
            var sourceName = _GetShortenedInventorySourceName(_GetInventorySource(jobId).Name);

            return $"{EXPORT_FILE_NAME_SEED}" +
                   $"_SOURCE_{sourceName}" +
                   $"_{job.StartDate.ToString(EXPORT_FILE_NAME_DATE_FORMAT)}" +
                   $"_{job.EndDate.ToString(EXPORT_FILE_NAME_DATE_FORMAT)}" +
                   $"_{_GetCurrentDateTime().ToString(EXPORT_FILE_SUFFIX_TIMESTAMP_FORMAT)}.csv";
        }

        protected string _GetShortenedInventorySourceName(string sourceName)
        {
            const int sourceNameTruncationLimit = 5;
            var withoutSpaces = sourceName.Replace(" ", "");
            return withoutSpaces.Length <= sourceNameTruncationLimit
                ? withoutSpaces
                : withoutSpaces.Substring(0, sourceNameTruncationLimit);
        }

        protected override void SetPrimaryProgramFromProgramMappings(List<StationInventoryManifest> manifests, Action<string> logger)
        {
            // not needed for now
        }
    }
}