using Common.Services;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramGuide;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public interface IInventoryProgramsProcessingEngine
    {
        InventoryProgramsProcessingJobDiagnostics ProcessInventoryJob(int jobId);

        string ImportInventoryProgramResults(Stream fileStream, string fileName);
    }

    public abstract class InventoryProgramsProcessingEngineBase : IInventoryProgramsProcessingEngine
    {
        protected const string EXPORT_FILE_NAME_SEED = "ProgramGuideExport";
        protected const string EXPORT_FILE_SUFFIX_TIMESTAMP_FORMAT = "yyyyMMdd_HHmmss";
        protected const string EXPORT_FILE_NAME_DATE_FORMAT = "yyyyMMdd";

        protected readonly IInventoryRepository _InventoryRepository;
        private readonly IProgramGuideApiClient _ProgramGuideApiClient;
        private readonly IStationMappingService _StationMappingService;
        private readonly IGenreCache _GenreCache;
        private readonly IFileService _FileService;
        private readonly IEmailerService _EmailerService;

        protected InventoryProgramsProcessingEngineBase(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProgramGuideApiClient programGuideApiClient,
            IStationMappingService stationMappingService,
            IGenreCache genreCache,
            IFileService fileService,
            IEmailerService emailerService)
        {
            _ProgramGuideApiClient = programGuideApiClient;
            _StationMappingService = stationMappingService;
            _GenreCache = genreCache;
            _FileService = fileService;
            _EmailerService = emailerService;

            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        protected abstract IInventoryProgramsJobsRepository _GetJobsRepository();

        protected abstract InventoryProgramsProcessingJobDiagnostics _GetNewDiagnostics();

        protected abstract InventorySource _GetInventorySource(int jobId);

        protected abstract List<StationInventoryManifest> _GatherInventory(int jobId, InventoryProgramsProcessingJobDiagnostics processDiagnostics);

        protected abstract InventoryProgramsRequestPackage _BuildRequestPackage(List<StationInventoryManifest> inventoryManifests,
            InventorySource inventorySource, InventoryProgramsProcessingJobDiagnostics processDiagnostics, int jobId);

        protected abstract List<StationInventoryManifestDaypartProgram> _GetProgramsFromResponse(GuideResponseElementDto currentResponse,
            GuideRequestResponseMapping currentMapping, InventoryProgramsRequestPackage requestPackage);

        protected abstract string _GetExportedFileReadyNotificationEmailBody(int jobId, string filePath);

        protected abstract string _GetExportedFileFailedNotificationEmailBody(int jobId);

        protected abstract string _GetNoInventoryToProcessNotificationEmailBody(int jobId);


        protected abstract string _GetExportFileName(int jobId);

        internal void OnDiagnosticMessageUpdate(int jobId, string message)
        {
            _GetJobsRepository().UpdateJobNotes(jobId, message);
        }

        private List<List<GuideInterfaceExportElement>> _GetImportFileProcessingChunks(List<GuideInterfaceExportElement> lineItems)
        {
            var batchSize = _GetSaveBatchSize();
            var processingBatches = lineItems.OrderBy(x => x.inventory_id)
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / batchSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            // make sure the inventory ids don't span multiple batches
            // don't need to check the last batch
            var maxBatchIndex = processingBatches.Count - 1;
            for (var i = 0; i < maxBatchIndex; i++)
            {
                // below we move them around, so perhaps we don't have any!
                if (processingBatches[i].Any() == false)
                {
                    continue;
                }

                var lastId = processingBatches[i].Last().inventory_id;
                // since we're not checking the last batch, [i+1] won't go 'out of index'.
                var spansNextBatch = processingBatches[i + 1].Where(s => s.inventory_id == lastId).ToList();
                if (spansNextBatch.Any())
                {
                    processingBatches[i].AddRange(spansNextBatch);
                    processingBatches[i + 1].RemoveRange(0, spansNextBatch.Count);
                }
            }
            return processingBatches;
        }

        public string ImportInventoryProgramResults(Stream fileStream, string fileName)
        {
            const GenreSourceEnum GENRE_SOURCE = GenreSourceEnum.Dativa;
            var success = false;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var processingLog = new StringBuilder();
            processingLog.AppendLine($"Processing {fileName}");

            try
            {
                var deleteBatchSize = _GetDeleteBatchSize();
                var saveBatchSize = _GetSaveBatchSize();

                _CreateDirectoriesIfNotExist();
                _WriteImportFileToInProgressDirectory(fileName, fileStream, processingLog);

                var fileParseSw = new Stopwatch();
                fileParseSw.Start();
                var parseResult = _ParseLinesFromFile(fileStream);
                fileParseSw.Stop();
                processingLog.AppendLine($"File parse took {fileParseSw.Elapsed.TotalSeconds} seconds.");

                if (parseResult.Success == false)
                {
                    success = false;
                    processingLog.AppendLine($"Parsing attempt failed on {parseResult.Messages.Count} lines.");
                    // Don't log them... it clouds the waters and takes a long time.
                    // maybe aggregate them and report that.
                    //parseResult.Messages.ForEach(m => processingLog.AppendLine(m));

                    _MoveImportFileFromInProgressToFailedDirectory(fileName, processingLog);

                    stopWatch.Stop();
                    processingLog.AppendLine($"Processing took {stopWatch.Elapsed.TotalSeconds} seconds.");

                    return processingLog.ToString();
                }

                if (parseResult.Warning)
                {
                    processingLog.AppendLine($"Parsing attempt yielded {parseResult.Messages.Count} warnings.");
                    // Don't log them... it clouds the waters and takes a long time.
                    // maybe aggregate them and report that.
                    //parseResult.Messages.ForEach(m => processingLog.AppendLine(m));
                }

                var lineItems = parseResult.LineItems;

                processingLog.AppendLine($"Exported {lineItems.Count} lines from the file.");

                var processingBatches = _GetImportFileProcessingChunks(lineItems);
                var totalProgramsExtracted = 0;

                processingLog.AppendLine($"Transformed to {processingBatches.Count} processing batches.");

                var batchindex = 0;
                foreach (var batch in processingBatches)
                {
                    batchindex++;
                    processingLog.AppendLine($"Starting batch {batchindex} of {processingBatches.Count}.");

                    processingLog.AppendLine($"Compressing dates for batch of {batch.Count}");
                    var programGroups = batch.GroupBy(b => new
                        {
                            b.inventory_daypart_id,
                            b.program_name,
                            b.show_type,
                            b.genre,
                            b.program_start_time,
                            b.program_end_time
                        })
                        .ToList();

                    processingLog.AppendLine($"Grouped batch of {batch.Count} to {programGroups.Count} program groups.");
                    processingLog.AppendLine($"Transforming to programs for {programGroups.Count} groups.");
                    
                    var programs = new List<StationInventoryManifestDaypartProgram>();
                    foreach (var programGroup in programGroups)
                    {
                        var exported = programGroup.Key;
                        var startDate = programGroup.Min(p => p.program_start_date.Value);
                        var endDate = programGroup.Max(p => p.program_end_date.Value);

                        try
                        {
                            var sourceGenre = _GenreCache.GetSourceGenreByName(exported.genre, GENRE_SOURCE);
                            var maestroGenre = _GenreCache.GetMaestroGenreBySourceGenre(sourceGenre, GENRE_SOURCE);

                            var program = new StationInventoryManifestDaypartProgram
                            {
                                StationInventoryManifestDaypartId = exported.inventory_daypart_id,
                                ProgramName = exported.program_name,
                                ShowType = exported.show_type,
                                GenreSourceId = (int)GENRE_SOURCE,
                                SourceGenreId = sourceGenre.Id,
                                MaestroGenreId = maestroGenre.Id,
                                StartDate = startDate,
                                EndDate = endDate,
                                StartTime = exported.program_start_time.Value,
                                EndTime = exported.program_end_time.Value
                            };

                            programs.Add(program);
                        }
                        catch (Exception e)
                        {
                            processingLog.AppendLine($"Error transforming from batch item to program : {e.Message}");
                        }
                    }
                    
                    totalProgramsExtracted += programs.Count;

                    processingLog.AppendLine($"Transforming to {programs.Count} programs making {totalProgramsExtracted} total programs.");

                    // setup first to minimize the transaction scope.

                    // prep the saves
                    var programSaveChunks = programs.Select((x, i) => new {Index = i, Value = x})
                        .GroupBy(x => x.Index / saveBatchSize)
                        .Select(x => x.Select(v => v.Value).ToList())
                        .ToList();

                    processingLog.AppendLine($"Transformed to {programSaveChunks.Count} save chunks.");

                    // prep the deletes.
                    var dateRangeManifests = batch.Select(d => new
                        {
                            StartDate = d.start_date,
                            EndDate = d.end_date
                        })
                        .Distinct()
                        .Select(dr =>
                            new
                            {
                                DateRange = dr,
                                ManifestIds = batch.Where(i => i.start_date == dr.StartDate && i.end_date == dr.EndDate)
                                    .Select((i => i.inventory_id))
                                    .Distinct()
                                    .ToList()
                            })
                        .ToList();

                    processingLog.AppendLine($"Transformed deletes to {dateRangeManifests.Count} dateRangeManifests.");

                    const int dbOperationTimeoutMinutes = 20;
                    // take action
                    using (var scope =
                        TransactionScopeHelper.CreateTransactionScope(TimeSpan.FromMinutes(dbOperationTimeoutMinutes)))
                    {
                        processingLog.AppendLine($"Performing deletes...");
                        // always delete
                        foreach (var group in dateRangeManifests)
                        {
                            var toDeleteChunks = group.ManifestIds.Select((x, i) => new { Index = i, Value = x })
                                .GroupBy(x => x.Index / deleteBatchSize)
                                .Select(x => x.Select(v => v.Value).ToList())
                                .ToList();

                            foreach (var manifestChunk in toDeleteChunks)
                            {
                                _InventoryRepository.DeleteInventoryPrograms(manifestChunk, group.DateRange.StartDate,
                                    group.DateRange.EndDate);
                            }
                        }

                        processingLog.AppendLine($"Performing saves...");
                        if (programs.Any())
                        {
                            programSaveChunks.ForEach(chunk =>
                                _InventoryRepository.UpdateInventoryPrograms(chunk, _GetCurrentDateTime()));
                        }

                        scope.Complete();
                    }
                }

                // adjust the primary program for the dayparts.
                var manifestIds = lineItems.Select(s => s.inventory_id).Distinct().ToList();
                var manifests = _InventoryRepository.GetStationInventoryManifestsByIds(manifestIds);
                _SetProgramData(manifests, (message) => processingLog.AppendLine(message));
                
                processingLog.AppendLine($"Extracted and saved {totalProgramsExtracted} program records.");
                success = true;
                _MoveImportFileFromInProgressToCompletedDirectory(fileName, processingLog);

                stopWatch.Stop();
                processingLog.AppendLine($"Processing took {stopWatch.Elapsed.TotalSeconds} seconds.");
            }
            catch (Exception ex)
            {
                success = false;

                LogHelper.Logger.Error($"Error caught ingesting results file '{fileName}'.", ex);
                processingLog.AppendLine($"Error caught : {ex.Message}");
                _MoveImportFileFromInProgressToFailedDirectory(fileName, processingLog);

                stopWatch.Stop();
                processingLog.AppendLine($"Processing took {stopWatch.Elapsed.TotalSeconds} seconds.");
            }

            return processingLog.ToString();
        }

        private void _WriteImportFileToInProgressDirectory(string fileName, Stream fileStream, StringBuilder processingLog)
        {
            try
            {
                var directoryName = _GetProgramGuideInterfaceInProgressDirectoryPath();
                var filePath = Path.Combine(directoryName, fileName);
                _FileService.Create(filePath, fileStream);

                processingLog.AppendLine($"File '{fileName}' saved to '{directoryName}'.");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error attempting to save file '{fileName}' in failed folder. Message : {e.Message}", e);
            }
        }

        private void _MoveImportFileFromInProgressToCompletedDirectory(string fileName, StringBuilder processingLog)
        {
            var fromDirectory = _GetProgramGuideInterfaceInProgressDirectoryPath();
            var fromFilePath = Path.Combine(fromDirectory, fileName);
            var toDirectory = _GetProgramGuideInterfaceCompletedDirectoryPath();

            try
            {
                _FileService.Move(fromFilePath, toDirectory);

                processingLog.AppendLine($"File '{fileName}' moved from to '{toDirectory}'.");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error attempting to move file '{fileName}' from to '{fromDirectory}' to '{toDirectory}'. Message : {e.Message}");
            }
        }

        private void _MoveImportFileFromInProgressToFailedDirectory(string fileName, StringBuilder processingLog)
        {
            var fromDirectory = _GetProgramGuideInterfaceInProgressDirectoryPath();
            var fromFilePath = Path.Combine(fromDirectory, fileName);
            var toDirectory = _GetProgramGuideInterfaceFailedDirectoryPath();

            try
            {
                _FileService.Move(fromFilePath, toDirectory);

                processingLog.AppendLine($"File '{fileName}' moved from to '{toDirectory}'.");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error attempting to move file '{fileName}' from to '{fromDirectory}' to '{toDirectory}'. Message : {e.Message}");
            }
        }

        public InventoryProgramsProcessingJobDiagnostics ProcessInventoryJob(int jobId)
        {
            var processDiagnostics = _GetNewDiagnostics();
            processDiagnostics.JobId = jobId;
            processDiagnostics.SaveChunkSize = _GetSaveBatchSize();
            processDiagnostics.RequestChunkSize = _GetRequestElementMaxCount();

            var jobsRepo = _GetJobsRepository();

            try
            {
                var inventorySource = _GetInventorySource(jobId);
                processDiagnostics.RecordInventorySource(inventorySource);

                processDiagnostics.RecordStart();

                processDiagnostics.RecordGatherInventoryStart();
                jobsRepo.UpdateJobStatus(jobId, InventoryProgramsJobStatus.GatherInventory, "Beginning step ");

                var manifests = _GatherInventory(jobId, processDiagnostics);

                processDiagnostics.RecordGatherInventoryStop();

                if (manifests.Any() == false)
                {
                    var message = "Job ending because no manifest records found to process.";
                    jobsRepo.SetJobCompleteWarning(jobId, message, message);
                    _ReportNoInventoryToProcess(jobId);
                    return processDiagnostics;
                }

                // Disabling calling ProgramGuide with PRI-23390
                //var requestsPackage = _BuildRequestPackage(manifests, inventorySource, processDiagnostics, jobId);
                //_ProcessInventory(jobId, requestsPackage, processDiagnostics);
                _ExportInventoryForProgramGuide(jobId, manifests, inventorySource, processDiagnostics);
            }
            catch (Exception ex)
            {
                LogHelper.Logger.Error(
                    $"Error caught processing for program names.  JobId = '{jobId}'", ex);
                jobsRepo.SetJobCompleteError(jobId, ex.Message, $"Error caught : {ex.Message}");

                _ReportExportFileFailed(jobId);

                throw;
            }
            finally
            {
                processDiagnostics.RecordStop();
            }
            return processDiagnostics;
        }

        private void _ExportInventoryForProgramGuide(int jobId, List<StationInventoryManifest> manifests, InventorySource inventorySource, 
            InventoryProgramsProcessingJobDiagnostics processingDiags)
        {
            var jobsRepository = _GetJobsRepository();

            processingDiags.RecordExportManifestsStart(manifests.Count);

            // transform
            var transformWarnings = new List<string>();
            var exportFileLines = manifests.SelectMany(m => _MapToExport(m, inventorySource, transformWarnings)).ToList();
            // consolidate the warnings
            transformWarnings.Distinct().ToList().ForEach(w => jobsRepository.UpdateJobNotes(jobId, w));

            // export to file
            var fileName = _GetExportFileName(jobId);

            var filePath = Path.Combine(_GetProgramGuideInterfaceExportDirectoryPath(), fileName);
            var headerLine = _GetProgramsExportFileHeaderLine();

            processingDiags.RecordExportManifestsDetails(exportFileLines.Count, fileName);

            var fileLines = new List<string>();
            fileLines.Add(headerLine);
            foreach (var lineItem in exportFileLines)
            {
                fileLines.Add(_GetProgramsExportFileItemLine(lineItem));
            }

            _CreateDirectoriesIfNotExist();
            _FileService.CreateTextFile(filePath, fileLines);

            processingDiags.RecordExportManifestStop();

            // notify complete
            jobsRepository.UpdateJobNotes(jobId, $"Notifying export completed.");
            _ReportExportFileCompleted(jobId, filePath);

            if (transformWarnings.Any())
            {
                jobsRepository.SetJobCompleteWarning(jobId, null, null);
            }
            else
            {
                jobsRepository.SetJobCompleteSuccess(jobId, null, null);
            }
        }

        private void _CreateDirectoriesIfNotExist()
        {
            _FileService.CreateDirectory(_GetProgramGuideInterfaceExportDirectoryPath());
            _FileService.CreateDirectory(_GetProgramGuideInterfaceInProgressDirectoryPath());
            _FileService.CreateDirectory(_GetProgramGuideInterfaceCompletedDirectoryPath());
            _FileService.CreateDirectory(_GetProgramGuideInterfaceFailedDirectoryPath());
        }

        private void _ReportExportFileCompleted(int jobId, string filePath)
        {
            var priority = MailPriority.Normal;
            var subject = "Broadcast Inventory Programs - ProgramGuide Interface Export file available";
            var body = _GetExportedFileReadyNotificationEmailBody(jobId, filePath);

            var toEmails = _GetProcessingBySourceResultReportToEmails();
            if (toEmails?.Any() != true)
            {
                throw new InvalidOperationException($"Failed to send notification email.  Email addresses are not configured correctly.");
            }

            _EmailerService.QuickSend(false, body, subject, priority, toEmails);
        }

        private void _ReportNoInventoryToProcess(int jobId)
        {
            var priority = MailPriority.Normal;
            var subject = "Broadcast Inventory Programs - No inventory to process.";
            var body = _GetNoInventoryToProcessNotificationEmailBody(jobId);

            var toEmails = _GetProcessingBySourceResultReportToEmails();
            if (toEmails?.Any() != true)
            {
                throw new InvalidOperationException($"Failed to send notification email.  Email addresses are not configured correctly.");
            }

            _EmailerService.QuickSend(false, body, subject, priority, toEmails);
        }

        private void _ReportExportFileFailed(int jobId)
        {
            var priority = MailPriority.High;
            var subject = "Broadcast Inventory Programs - ProgramGuide Interface Export file failed";
            var body = _GetExportedFileFailedNotificationEmailBody(jobId);

            var toEmails = _GetProcessingBySourceResultReportToEmails();
            if (toEmails?.Any() != true)
            {
                throw new InvalidOperationException($"Failed to send notification email.  Email addresses are not configured correctly.");
            }

            _EmailerService.QuickSend(false, body, subject, priority, toEmails);
        }

        private List<GuideInterfaceExportElement> _MapToExport(StationInventoryManifest manifest, InventorySource inventorySource, List<string> transformWarnings)
        {
            var toExport = new List<GuideInterfaceExportElement>();

            if (string.IsNullOrWhiteSpace(manifest.Station.Affiliation))
            {
                return toExport;
            }

            string mappedStationCallLetters;

            try
            {
                mappedStationCallLetters = _GetManifestStationCallLetters(manifest, inventorySource);
            }
            catch (Exception ex)
            {
                transformWarnings.Add(ex.Message);
                return toExport;
            }

            foreach (var week in manifest.ManifestWeeks)
            {
                foreach (var daypart in manifest.ManifestDayparts)
                {
                    var exportItem = new GuideInterfaceExportElement
                    {
                        inventory_id = manifest.Id.Value,
                        inventory_week_id = week.Id,
                        inventory_daypart_id = daypart.Id.Value,
                        station_call_letters = mappedStationCallLetters,
                        affiliation = manifest.Station.Affiliation,
                        start_date = week.StartDate,
                        end_date = week.EndDate,
                        daypart_text = daypart.Daypart.Preview,
                        mon = daypart.Daypart.Monday,
                        tue = daypart.Daypart.Tuesday,
                        wed = daypart.Daypart.Wednesday,
                        thu = daypart.Daypart.Thursday,
                        fri = daypart.Daypart.Friday,
                        sat = daypart.Daypart.Saturday,
                        sun = daypart.Daypart.Sunday,
                        daypart_start_time = daypart.Daypart.StartTime,
                        daypart_end_time = daypart.Daypart.EndTime
                    };
                    toExport.Add(exportItem);
                }
            }
            return toExport;
        }

        private void _SetProgramData(List<StationInventoryManifest> manifests, Action<string> logger)
        {
            logger($"Aggregating program data has started, manifests count: {manifests.Count}");

            _SetPrimaryProgramForManifestDayparts(manifests, logger);

            logger($"Aggregating program data has finished, manifests count: {manifests.Count}");
        }

        private void _SetPrimaryProgramForManifestDayparts(List<StationInventoryManifest> manifests, Action<string> logger)
        {
            var batchSize = _GetSaveBatchSize();
            var manifestDayparts = manifests.SelectMany(x => x.ManifestDayparts);
            var manifestDaypartsCount = manifestDayparts.Count();
            var chunks = manifestDayparts.Select((item, index) => new { Index = index, Value = item })
                            .GroupBy(x => x.Index / batchSize)
                            .Select(x => x.Select(v => v.Value).ToList())
                            .ToList();

            logger($"Setting primary programs. Total manifest dayparts: {manifestDaypartsCount}");

            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];

                logger($"Setting primary programs for chunk #{i + 1} / {chunks.Count}, items: {chunk.Count}");

                _SetPrimaryProgramForManifestDayparts(chunk);
            }

            logger($"Finished setting primary programs. Total manifest dayparts: {manifestDaypartsCount}");
        }

        private void _SetPrimaryProgramForManifestDayparts(List<StationInventoryManifestDaypart> manifestDayparts)
        {
            var manifestDaypartIds = manifestDayparts.Select(x => x.Id.Value).ToList();
            var allManifestDaypartPrograms = _InventoryRepository.GetDaypartProgramsForInventoryDayparts(manifestDaypartIds);

            foreach (var manifestDaypart in manifestDayparts)
            {
                var manifestDaypartPrograms = allManifestDaypartPrograms.Where(x => x.StationInventoryManifestDaypartId == manifestDaypart.Id);

                if (!manifestDaypartPrograms.Any())
                    continue;

                manifestDaypart.PrimaryProgramId = manifestDaypartPrograms
                    .GroupBy(x => x.ProgramName)
                    .Select(x =>
                    {
                        var programs = x.ToList();
                        var totalTime = _CalculateTotalTime(programs, manifestDaypart.Daypart);

                        return new
                        {
                            // programs are grouped by name
                            // so that we can just choose any of them and treat as primary for a manifest daypart
                            programId = programs.First().Id,
                            totalTime
                        };
                    })
                    .OrderByDescending(x => x.totalTime)
                    .First()
                    .programId;
            }

            _InventoryRepository.UpdatePrimaryProgramsForManifestDayparts(manifestDayparts);
        }

        private int _CalculateTotalTime(
            List<StationInventoryManifestDaypartProgram> programs,
            DisplayDaypart daypart)
        {
            var totalTime = 0;
            var timeRange = new TimeRange
            {
                StartTime = daypart.StartTime,
                EndTime = daypart.EndTime
            };

            foreach (var program in programs)
            {
                var programTimeRange = new TimeRange
                {
                    StartTime = program.StartTime,
                    EndTime = program.EndTime
                };
                var programTimePerDay = DaypartTimeHelper.GetIntersectingTotalTime(timeRange, programTimeRange);

                // programs are stored weekly
                // this is a total time for a week
                var programTotalTime = daypart.ActiveDays * programTimePerDay;

                // add up time from each week
                totalTime += programTotalTime;
            }

            return totalTime;
        }

        private void _ProcessInventory(int jobId, InventoryProgramsRequestPackage requestPackage,
            InventoryProgramsProcessingJobDiagnostics processDiagnostics)
        {
            var result = InventoryProgramsJobStatus.Completed;
            var jobsRepository = _GetJobsRepository();

            var requestElementMaxCount = _GetRequestElementMaxCount();
            var parallelEnabled = _GetParallelApiCallsEnabled();
            var maxDop = _GetMaxDegreesOfParallelism();

            var requestChunks = requestPackage.RequestElements.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / requestElementMaxCount)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            var saveBatchSize = _GetSaveBatchSize();

            var currentRequestChunkIndex = 0;
            foreach (var requestChunk in requestChunks)
            {
                var programs = new List<StationInventoryManifestDaypartProgram>();

                /*** Call Api ***/
                jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.CallApi);

                currentRequestChunkIndex++;
                processDiagnostics.RecordIterationStartCallToApi(currentRequestChunkIndex, requestChunks.Count);


                // All of this is just "Import now...


                // the api call
                var programGuideResponse = parallelEnabled
                    ? _MakeParallelCallsToApi(requestChunk, maxDop)
                    : _ProgramGuideApiClient.GetProgramsForGuide(requestChunk);

                processDiagnostics.RecordIterationStopCallToApi(programGuideResponse.Count);

                /*** Apply Api Response ***/
                processDiagnostics.RecordIterationStartApplyApiResponse();
                jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.ApplyProgramData);

                if (programGuideResponse.Any() == false)
                {
                    result = InventoryProgramsJobStatus.Warning;
                    jobsRepository.UpdateJobNotes(jobId, $"Request set {currentRequestChunkIndex} returned no responses.");
                }
                else
                {
                    foreach (var mapping in requestPackage.RequestMappings)
                    {
                        // if a requestMapping doesn't have a response then log it
                        var mappedResponses = programGuideResponse.Where(e => e.RequestDaypartId.Equals(mapping.RequestEntryId)).ToList();

                        if (mappedResponses.Any() == false)
                        {
                            result = InventoryProgramsJobStatus.Warning;
                            jobsRepository.UpdateJobNotes(jobId, $"A request received no response : {mapping}");
                            continue;
                        }

                        foreach (var responseEntry in mappedResponses)
                        {
                            var entryPrograms = _GetProgramsFromResponse(responseEntry, mapping, requestPackage);
                            programs.AddRange(entryPrograms);
                        }
                    }
                }
                processDiagnostics.RecordIterationStopApplyApiResponse(programs.Count);

                /*** Save the programs ***/

                jobsRepository.UpdateJobStatus(jobId, InventoryProgramsJobStatus.SavePrograms);
                processDiagnostics.RecordIterationStartSavePrograms();
                var programSaveChunksCount = 0;

                // clear out the old programs first.
                
                var deletePrograms = requestPackage.RequestMappings.Select(m => m.ManifestId).Distinct().ToList();
                var deleteProgramsChunks = deletePrograms.Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / saveBatchSize)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                jobsRepository.UpdateJobNotes(jobId, $"Removing programs from {requestPackage.RequestMappings.Count} manifests split into {deleteProgramsChunks.Count} chunks.");
                deleteProgramsChunks.ForEach(chunk =>
                    _InventoryRepository.DeleteInventoryPrograms(chunk.Select(c => c).ToList(),
                        requestPackage.StartDateRange, requestPackage.EndDateRange));

                if (programs.Any())
                {
                    var programSaveChunks = programs.Select((x, i) => new { Index = i, Value = x })
                        .GroupBy(x => x.Index / saveBatchSize)
                        .Select(x => x.Select(v => v.Value).ToList())
                        .ToList();
                    programSaveChunksCount = programSaveChunks.Count;

                    programSaveChunks.ForEach(chunk => _InventoryRepository.UpdateInventoryPrograms(chunk, DateTime.Now));
                }
                else
                {
                    result = InventoryProgramsJobStatus.Warning;
                    jobsRepository.UpdateJobNotes(jobId, $"Ending iteration without saving programs.");
                }

                processDiagnostics.RecordIterationStopSavePrograms(programSaveChunksCount);
            }

            if (result == InventoryProgramsJobStatus.Warning)
            {
                jobsRepository.SetJobCompleteWarning(jobId, null, null);
            }
            else
            {
                jobsRepository.SetJobCompleteSuccess(jobId, null, null);
            }
        }

        protected string _GetManifestStationCallLetters(StationInventoryManifest manifest, InventorySource source)
        {
            const StationMapSetNamesEnum programGuideMapSet = StationMapSetNamesEnum.Extended;
            // get the Cadent Callsign for the inventory station
            var cadentCallsign = manifest.Station.LegacyCallLetters;
            // call the mappings service to get all stations mapped to that guy
            var mappings = _StationMappingService.GetStationMappingsByCadentCallLetter(cadentCallsign);
            // find the station callsign for my map_set

            var mappedStations = mappings.Where(m => m.MapSet == programGuideMapSet).Select(s => s.MapValue).ToList();

            // not using single to provide informative messages.
            if (mappedStations.Count == 0)
            {
                throw new Exception($"Mapping for CadentCallsign '{cadentCallsign}' and Map Set '{programGuideMapSet}' not found.");
            }
            if (mappedStations.Count > 1)
            {
                throw new Exception($"Mapping for CadentCallsign '{cadentCallsign}' and Map Set '{programGuideMapSet}' has {mappedStations.Count} mappings when only one expected.");
            }

            var mappedStationInfo = mappedStations.Single();
            return mappedStationInfo;
        }

        protected StationInventoryManifestDaypartProgram _MapProgramDto(GuideResponseProgramDto guideProgram, int manifestDaypartId,
            InventoryProgramsRequestPackage requestPackage)
        {
            const GenreSourceEnum GENRE_SOURCE = GenreSourceEnum.Dativa;

            var sourceGenre = _GenreCache.GetSourceGenreByName(guideProgram.SourceGenre, GENRE_SOURCE);
            var maestroGenre = _GenreCache.GetMaestroGenreBySourceGenre(sourceGenre, GENRE_SOURCE);

            var program = new StationInventoryManifestDaypartProgram
            {
                StationInventoryManifestDaypartId = manifestDaypartId,
                ProgramName = guideProgram.ProgramName,
                ShowType = guideProgram.ShowType,
                SourceGenreId = sourceGenre.Id,
                GenreSourceId = (int)GENRE_SOURCE,
                MaestroGenreId = maestroGenre.Id,
                StartDate = guideProgram.StartDate,
                EndDate = guideProgram.EndDate,
                StartTime = guideProgram.StartTime,
                EndTime = guideProgram.EndTime
            };
            return program;
        }

        private List<GuideResponseElementDto> _MakeParallelCallsToApi(List<GuideRequestElementDto> requests,
            int maxDop)
        {
            var responses = new ConcurrentBag<GuideResponseElementDto>();
            var errors = new ConcurrentBag<Exception>();

            var parallelRequestsBatchSize = _GetParallelApiCallsBatchSize();
            var requestBatches = requests.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / parallelRequestsBatchSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            // Send calls in parallel
            // Send one request per call
            Parallel.ForEach(requestBatches, new ParallelOptions { MaxDegreeOfParallelism = maxDop }, (requestBatch) =>
            {
                try
                {
                    var response = _ProgramGuideApiClient.GetProgramsForGuide(requestBatch);
                    response.ForEach(r => responses.Add(r));
                }
                catch (Exception e)
                {
                    errors.Add(e);
                }
            });

            if (errors.Any())
            {
                var msg = $"{errors.Count} errors caught while calling Program Guide in parallel.  MaxDop = {maxDop}; " +
                          $"ParallelRequestsBatchSize = {parallelRequestsBatchSize}; RequestCount = {requests.Count}. ";
                throw new AggregateException(msg, errors);
            }

            return responses.ToList();
        }

        private class ProgramsFileParseResult
        {
            public bool Success { get; set; }
            public bool Warning { get; set; }
            public List<string> Messages { get; } = new List<string>();
            public List<GuideInterfaceExportElement> LineItems { get; set; } = new List<GuideInterfaceExportElement>();
        }

        private ProgramsFileParseResult _ParseLinesFromFile(Stream fileStream)
        {
            var result = new ProgramsFileParseResult();

            // reset the file for parsing.
            fileStream.Position = 0;
            var headerFields = GetProgramsImportFileHeaderFields();

            var reader = new CsvFileReader(headerFields);

            var currentRowNumber = 1; // header is row 1 so start at one and increment below.

            try
            {
                using (reader.Initialize(fileStream))
                {
                    while (reader.IsEOF() == false)
                    {
                        reader.NextRow();
                        currentRowNumber++;

                        if (reader.IsEmptyRow())
                        {
                            break;
                        }

                        try
                        {
                            // this bloat makes it easier when debugging the parsing.
                            // And the exception stack trace calls out the failing line number.
                            var inventory_id = Convert.ToInt32(reader.GetCellValue(nameof(GuideInterfaceExportElement.inventory_id)));
                            var inventory_week_id = Convert.ToInt32(reader.GetCellValue(nameof(GuideInterfaceExportElement.inventory_week_id)));
                            var inventory_daypart_id = Convert.ToInt32(reader.GetCellValue(nameof(GuideInterfaceExportElement.inventory_daypart_id)));
                            var station_call_letters = reader.GetCellValue(nameof(GuideInterfaceExportElement.station_call_letters));
                            var affiliation = reader.GetCellValue(nameof(GuideInterfaceExportElement.affiliation));
                            var start_date = DateTime.Parse(reader.GetCellValue(nameof(GuideInterfaceExportElement.start_date)));
                            var end_date = DateTime.Parse(reader.GetCellValue(nameof(GuideInterfaceExportElement.end_date)));
                            var daypart_text = reader.GetCellValue(nameof(GuideInterfaceExportElement.daypart_text));
                            var mon = reader.GetCellValue(nameof(GuideInterfaceExportElement.mon)) == "1";
                            var tue = reader.GetCellValue(nameof(GuideInterfaceExportElement.tue)) == "1";
                            var wed = reader.GetCellValue(nameof(GuideInterfaceExportElement.wed)) == "1";
                            var thu = reader.GetCellValue(nameof(GuideInterfaceExportElement.thu)) == "1";
                            var fri = reader.GetCellValue(nameof(GuideInterfaceExportElement.fri)) == "1";
                            var sat = reader.GetCellValue(nameof(GuideInterfaceExportElement.sat)) == "1";
                            var sun = reader.GetCellValue(nameof(GuideInterfaceExportElement.sun)) == "1";
                            var daypart_start_time = Convert.ToInt32(reader.GetCellValue(nameof(GuideInterfaceExportElement.daypart_start_time)));
                            var daypart_end_time = Convert.ToInt32(reader.GetCellValue(nameof(GuideInterfaceExportElement.daypart_end_time)));
                            var program_name = reader.GetCellValue(nameof(GuideInterfaceExportElement.program_name));
                            var show_type = reader.GetCellValue(nameof(GuideInterfaceExportElement.show_type));
                            var genre = reader.GetCellValue(nameof(GuideInterfaceExportElement.genre));
                            var program_start_time = Convert.ToInt32(Convert.ToDouble(reader.GetCellValue(nameof(GuideInterfaceExportElement.program_start_time))));
                            var program_end_time = Convert.ToInt32(Convert.ToDouble(reader.GetCellValue(nameof(GuideInterfaceExportElement.program_end_time))));
                            var program_start_date = DateTime.Parse(reader.GetCellValue(nameof(GuideInterfaceExportElement.program_start_date)));
                            var program_end_date = DateTime.Parse(reader.GetCellValue(nameof(GuideInterfaceExportElement.program_end_date)));

                            var lineItem = new GuideInterfaceExportElement
                            {
                                inventory_id = inventory_id,
                                inventory_week_id = inventory_week_id,
                                inventory_daypart_id = inventory_daypart_id,
                                station_call_letters = station_call_letters,
                                affiliation = affiliation,
                                start_date = start_date,
                                end_date = end_date,
                                daypart_text = daypart_text,
                                mon = mon,
                                tue = tue,
                                wed = wed,
                                thu = thu,
                                fri = fri,
                                sat = sat,
                                sun = sun,
                                daypart_start_time = daypart_start_time,
                                daypart_end_time = daypart_end_time,
                                program_name = program_name,
                                show_type = show_type,
                                genre = genre,
                                program_start_time = program_start_time,
                                program_end_time = program_end_time,
                                program_start_date = program_start_date,
                                program_end_date = program_end_date
                            };
                            result.LineItems.Add(lineItem);
                        }
                        catch (Exception e)
                        {
                            result.Warning = true;
                            result.Messages.Add($"Error caught parsing row {currentRowNumber}. Continuing with rest of file.  Error : {e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Messages.Add($"Error caught parsing row {currentRowNumber} : {e.Message}");
                return result;
            }

            result.Success = true;
            return result;
        }

        public static List<string> GetProgramsImportFileHeaderFields()
        {
            return new List<string>
            {
                nameof(GuideInterfaceExportElement.inventory_id),
                nameof(GuideInterfaceExportElement.inventory_week_id),
                nameof(GuideInterfaceExportElement.inventory_daypart_id),
                nameof(GuideInterfaceExportElement.station_call_letters),
                nameof(GuideInterfaceExportElement.affiliation),
                nameof(GuideInterfaceExportElement.start_date),
                nameof(GuideInterfaceExportElement.end_date),
                nameof(GuideInterfaceExportElement.daypart_text),
                nameof(GuideInterfaceExportElement.mon),
                nameof(GuideInterfaceExportElement.tue),
                nameof(GuideInterfaceExportElement.wed),
                nameof(GuideInterfaceExportElement.thu),
                nameof(GuideInterfaceExportElement.fri),
                nameof(GuideInterfaceExportElement.sat),
                nameof(GuideInterfaceExportElement.sun),
                nameof(GuideInterfaceExportElement.daypart_start_time),
                nameof(GuideInterfaceExportElement.daypart_end_time),
                nameof(GuideInterfaceExportElement.program_name),
                nameof(GuideInterfaceExportElement.show_type),
                nameof(GuideInterfaceExportElement.genre),
                nameof(GuideInterfaceExportElement.program_start_time),
                nameof(GuideInterfaceExportElement.program_end_time),
                nameof(GuideInterfaceExportElement.program_start_date),
                nameof(GuideInterfaceExportElement.program_end_date),
            };
        }

        private static string _GetProgramsExportFileHeaderLine()
        {
            return _GetCsvLine(GetProgramsImportFileHeaderFields());
        }

        private static string _GetProgramsExportFileItemLine(GuideInterfaceExportElement item)
        {
            var itemType = typeof(GuideInterfaceExportElement);
            var lineCsv = new StringBuilder();
            foreach (var fieldName in GetProgramsImportFileHeaderFields())
            {
                if (lineCsv.Length > 0)
                {
                    lineCsv.Append($",");
                }

                var property = itemType.GetProperty(fieldName);
                if (property == null)
                {
                    continue;
                }

                string fieldValue;
                if (property.PropertyType == typeof(DateTime))
                {
                    fieldValue = ((DateTime)property.GetValue(item)).ToString(BroadcastConstants.DATE_FORMAT_STANDARD);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    fieldValue = ((bool)property.GetValue(item)) ? "1" : "0";
                }
                else
                {
                    fieldValue = property.GetValue(item)?.ToString() ?? "";
                }

                if (fieldValue.Contains(","))
                {
                    fieldValue = $"\"{fieldValue}\"";
                }
                lineCsv.Append($"{fieldValue}");
            }

            return lineCsv.ToString();
        }

        private static string _GetCsvLine(List<string> lineFields)
        {
            var line = new StringBuilder();
            foreach (var field in lineFields)
            {
                if (line.Length > 0)
                {
                    line.Append(",");
                }
                line.Append(field);
            }
            return line.ToString();
        }

        protected virtual int _GetSaveBatchSize()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineSaveBatchSize;
        }

        protected virtual int _GetDeleteBatchSize()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineDeleteBatchSize;
        }

        protected virtual int _GetRequestElementMaxCount()
        {
            return ProgramGuideApiClient.RequestElementMaxCount;
        }

        protected virtual bool _GetParallelApiCallsEnabled()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineParallelEnabled;
        }

        protected virtual int _GetParallelApiCallsBatchSize()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineParallelBatchSize;
        }

        protected virtual int _GetMaxDegreesOfParallelism()
        {
            return BroadcastServiceSystemParameter.InventoryProgramsEngineMaxDop;
        }

        protected string _GetProgramGuideInterfaceExportDirectoryPath()
        {
            const string dirName = "Export";
            return Path.Combine(_GetProgramGuideInterfacePath(), dirName);
        }

        protected string _GetProgramGuideInterfaceInProgressDirectoryPath()
        {
            const string dirName = "InProgress";
            return Path.Combine(_GetProgramGuideInterfacePath(), dirName);
        }

        protected string _GetProgramGuideInterfaceCompletedDirectoryPath()
        {
            const string dirName = "Completed";
            return Path.Combine(_GetProgramGuideInterfacePath(), dirName);
        }

        protected string _GetProgramGuideInterfaceFailedDirectoryPath()
        {
            const string dirName = "Failed";
            return Path.Combine(_GetProgramGuideInterfacePath(), dirName);
        }

        protected string _GetProgramGuideInterfacePath()
        {
            const string dirName = "ProgramGuideInterfaceDirectory";
            return Path.Combine(_GetBroadcastSharedDirectoryPath(), dirName);
        }

        protected virtual string _GetBroadcastSharedDirectoryPath()
        {
            return BroadcastServiceSystemParameter.BroadcastSharedFolder;
        }

        protected virtual string[] _GetProcessingBySourceResultReportToEmails()
        {
            var raw = BroadcastServiceSystemParameter.InventoryProcessingNotificationEmails;
            var split = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return split;
        }

        protected virtual DateTime _GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}