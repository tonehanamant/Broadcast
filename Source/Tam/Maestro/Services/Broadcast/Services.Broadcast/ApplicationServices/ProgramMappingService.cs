using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.ReportGenerators.ProgramMapping;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using System.Collections.Concurrent;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProgramMappingService : IApplicationService
    {
        /// <summary>
        /// Loads the program mappings, and hands it off to background job.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>The background jobs Id</returns>
        string LoadProgramMappings(Stream fileStream, string fileName, string userName, DateTime createdDate);

        /// <summary>
        /// Runs the program mappings processing job.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        [Queue("programmappings")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        void RunProgramMappingsProcessingJob(Guid fileId, string userName, DateTime createdDate);

        /// <summary>
        /// Exports a file containing all the program mappings.
        /// </summary>
        /// <returns></returns>
        ReportOutput ExportProgramMappingsFile(string username);
        /// <summary>
        /// Generate Excel for UnMapped Program Names
        /// </summary>
        /// <returns></returns>
        ReportOutput GenerateUnmappedProgramNameReport();
    }

    public class ProgramMappingService : BroadcastBaseClass, IProgramMappingService
    {
        private readonly IBackgroundJobClient _BackgroundJobClient;
        private readonly IProgramMappingRepository _ProgramMappingRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IShowTypeRepository _ShowTypeRepository;
        private readonly IGenreRepository _GenreRepository;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IProgramNameMappingsExportEngine _ProgramNameMappingsExportEngine;
        private const string UnmappedProgramReportFileName = "UnmappedProgramReport.xlsx";
        public ProgramMappingService(IBackgroundJobClient backgroundJobClient,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISharedFolderService sharedFolderService,
            IProgramNameMappingsExportEngine programNameMappingsExportEngine)
        {
            _BackgroundJobClient = backgroundJobClient;
            _ProgramMappingRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ShowTypeRepository = broadcastDataRepositoryFactory.GetDataRepository<IShowTypeRepository>();
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _SharedFolderService = sharedFolderService;
            _ProgramNameMappingsExportEngine = programNameMappingsExportEngine;
        }
        
        /// <inheritdoc />
        public string LoadProgramMappings(Stream fileStream, string fileName, string userName, DateTime createdDate)
        {
            var fileId = _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = _GetProgramMappingsDirectoryPath(),
                FileNameWithExtension = fileName,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.CampaignExport,
                CreatedDate = createdDate,
                CreatedBy = userName,
                FileContent = fileStream
            });

            // Hand off to a background job
            var hangfireJobId = _BackgroundJobClient.Enqueue<IProgramMappingService>(x => x.RunProgramMappingsProcessingJob(fileId, userName, createdDate));
            return hangfireJobId;
        }

        public void RunProgramMappingsProcessingJob(Guid fileId, string userName, DateTime createdDate)
        {
            var durationSw = new Stopwatch();
            durationSw.Start();
            var file = _SharedFolderService.GetFile(fileId);
            var ingestedRecordsCount = 0;
            var updatedInventoryCount = 0;

            _LogInfo($"Started processing the program mapping file {file.FileNameWithExtension}");

            var programMappings = _ReadProgramMappingsFile(file.FileContent);
            _LogInfo($"The selected program mapping file has {programMappings.Count} rows");

            WebUtilityHelper.HtmlDecodeProgramNames(programMappings);
            _ProcessProgramMappings(programMappings, createdDate, userName, ref updatedInventoryCount, ref ingestedRecordsCount);

            _SharedFolderService.RemoveFile(fileId);

            durationSw.Stop();
            _LogInfo($"Processing of the program mapping file {file.FileNameWithExtension}, finished successfully in {durationSw.ElapsedMilliseconds} ms. Ingested {ingestedRecordsCount} records, updated {updatedInventoryCount} inventory.");
                
        }

        /// <inheritdoc />
        public ReportOutput ExportProgramMappingsFile(string username)
        {
            const string fileName = "BroadcastMappedPrograms.xlsx";

            _LogInfo($"Export beginning." , username);
            var durationSw = new Stopwatch();
            durationSw.Start();

            var mappings = _ProgramMappingRepository.GetProgramMappings();

            _LogInfo($"Exporting {mappings.Count} records.", username);

            var excelPackage = _ProgramNameMappingsExportEngine.GenerateExportFile(mappings);
            var saveStream = new MemoryStream();
            excelPackage.SaveAs(saveStream);
            saveStream.Position = 0;
            
            var output = new ReportOutput(fileName)
            {
                Stream = saveStream
            };

            durationSw.Stop();

            _LogInfo($"Export complete for {mappings.Count} mappings to file '{fileName}'. Duration {durationSw.ElapsedMilliseconds} ms", username);

            return output;
        }

        protected void _ProcessProgramMappings(
            List<ProgramMappingsFileRequestDto> programMappings,
            DateTime createdDate, string username,
            ref int updatedInventoryCount, ref int ingestedRecordsCount)
        {
            const ProgramSourceEnum programSource = ProgramSourceEnum.Mapped;
            var results = new ConcurrentBag<Tuple<int, int>>();
            Parallel.ForEach(programMappings, 
                new ParallelOptions { MaxDegreeOfParallelism = 4 }, 
                (mapping) => {
                    var result = _ProcessIndividualProgramMapping(mapping, programSource, username, createdDate);
                    results.Add(result);
                });
            ingestedRecordsCount = results.Sum(r => r.Item1);
            updatedInventoryCount = results.Sum(r => r.Item2);

        }

        private Tuple<int, int> _ProcessIndividualProgramMapping(ProgramMappingsFileRequestDto mapping, ProgramSourceEnum programSource, string username, DateTime createdDate)
        {
            using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(30)))
            {
                var updatedRecords = 0;
                var existingMapping = _ProgramMappingRepository.GetProgramMappingOrDefaultByOriginalProgramName(mapping.OriginalProgramName);
                if (existingMapping != null) 
                {
                    if (existingMapping.OfficialProgramName != mapping.OfficialProgramName ||
                        existingMapping.OfficialGenre.Name != mapping.OfficialGenre ||
                        existingMapping.OfficialShowType.Name != mapping.OfficialShowType)
                    {
                        // There are changes for an existing mapping
                        existingMapping.OfficialProgramName = mapping.OfficialProgramName;
                        existingMapping.OfficialGenre = _GenreRepository.GetGenreByName(mapping.OfficialGenre, programSource);
                        existingMapping.OfficialShowType = _ShowTypeRepository.GetShowTypeByName(mapping.OfficialShowType);
                        _ProgramMappingRepository.UpdateProgramMapping(existingMapping, username, createdDate);
                        updatedRecords = _UpdateInventoryWithEnrichedProgramName(existingMapping, mapping, createdDate, programSource);
                    }
                }
                else
                {
                    var newProgramMapping = new ProgramMappingsDto
                    {
                        OriginalProgramName = mapping.OriginalProgramName,
                        OfficialProgramName = mapping.OfficialProgramName,
                        OfficialGenre = _GenreRepository.GetGenreByName(mapping.OfficialGenre, programSource),
                        OfficialShowType = _ShowTypeRepository.GetShowTypeByName(mapping.OfficialShowType)
                    };
                    _ProgramMappingRepository.CreateProgramMapping(newProgramMapping, username, createdDate);
                    updatedRecords = _UpdateInventoryWithEnrichedProgramName(newProgramMapping, mapping, createdDate, programSource);
                }
                transaction.Complete();
                var ingestedRecords = 1;
                return Tuple.Create(ingestedRecords, updatedRecords);
            }
        }

        private int _UpdateInventoryWithEnrichedProgramName(
            ProgramMappingsDto programMapping, 
            ProgramMappingsFileRequestDto mapping, 
            DateTime createdDate,
            ProgramSourceEnum programSource
            )
        {
            var durationSw = new Stopwatch();
            durationSw.Start();

            var updatedManifestDaypartIds = new List<int>();

            // Get all StationInventoryManifestDaypart's with ProgramName
            var manifestDayparts = _InventoryRepository.GetManifestDaypartsForProgramName(programMapping.OriginalProgramName);

            var updatedInventoryCount = 0;
            foreach (var daypart in manifestDayparts)
            {
                // Get all StationInventoryManifestDaypartProgram for these
                var manifestDaypartPrograms = _InventoryRepository.GetDaypartProgramsForInventoryDayparts(new List<int> { daypart.Id.Value });

                // Remove the old programs
                var manifestDaypartIds = manifestDaypartPrograms.Select(x => x.StationInventoryManifestDaypartId).Distinct().ToList();
                if (!manifestDaypartIds.IsEmpty())
                {
                    _InventoryRepository.DeleteInventoryPrograms(manifestDaypartIds);
                }

                // Create the new StationInventoryManifestDaypartProgram
                var newManifestDaypartPrograms = new List<StationInventoryManifestDaypartProgram>
                    {
                        new StationInventoryManifestDaypartProgram
                        {
                            StationInventoryManifestDaypartId = daypart.Id.Value,
                            ProgramName = programMapping.OfficialProgramName,
                            ProgramSourceId = (int)programSource,
                            MaestroGenreId = programMapping.OfficialGenre.Id,
                            SourceGenreId = programMapping.OfficialGenre.Id,
                            ShowType = mapping.OfficialShowType,
                            StartTime = daypart.Daypart.StartTime,
                            EndTime = daypart.Daypart.EndTime,
                            CreatedDate = createdDate
                        }
                    };

                _InventoryRepository.CreateInventoryPrograms(newManifestDaypartPrograms, createdDate);

                updatedManifestDaypartIds.Add(daypart.Id.Value);
                updatedInventoryCount++;

            };

            _ResetPrimaryPrograms(updatedManifestDaypartIds);

            durationSw.Stop();
            _LogInfo($"Updating inventory for program with {programMapping.OriginalProgramName}, finished successfully in {durationSw.ElapsedMilliseconds} ms.");
            return updatedInventoryCount;
        }

        private void _ResetPrimaryPrograms(List<int> manifestDaypartIds)
        {
            var manifestDaypartProgramsByManifestDaypart = _InventoryRepository
                .GetDaypartProgramsForInventoryDayparts(manifestDaypartIds)
                .ToDictionary(x => x.StationInventoryManifestDaypartId, x => x.Id);

            var manifestDayparts = manifestDaypartIds
                .Where(x => manifestDaypartProgramsByManifestDaypart.ContainsKey(x))
                .Select(manifestDaypartId => new StationInventoryManifestDaypart 
                { 
                    Id = manifestDaypartId, 
                    PrimaryProgramId = manifestDaypartProgramsByManifestDaypart[manifestDaypartId] 
                })
                .ToList();

            _InventoryRepository.UpdatePrimaryProgramsForManifestDayparts(manifestDayparts);
        }

        private List<ProgramMappingsFileRequestDto> _ReadProgramMappingsFile(Stream stream)
        {
            var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[1];
            var programMappings = worksheet.ConvertSheetToObjects<ProgramMappingsFileRequestDto>();

            return programMappings
                // Ignore blank rows
                .Where(mapping => !string.IsNullOrEmpty(mapping.OriginalProgramName) && !string.IsNullOrEmpty(mapping.OfficialProgramName))
                .ToList();
        }

        protected string _GetProgramMappingsDirectoryPath()
        {
            const string dirName = "ProgramMappings";
            var appFolderPath = _GetBroadcastAppFolder();
            return Path.Combine(appFolderPath, dirName);
        }

        public ReportOutput GenerateUnmappedProgramNameReport()
        {
	        _LogInfo("Started process to generate unmapped program names.");
	        var durationSw = new Stopwatch();
	        durationSw.Start();

            var programNames = _InventoryRepository.GetUnmappedPrograms();

	        _LogInfo($"Total count of Distinct unmapped program names: {programNames.Count}");
	        var reportData = new UnMappedProgramNameReportData
	        {
		        ProgramNames = programNames,
		        ExportFileName = UnmappedProgramReportFileName
	        };
	        var reportGenerator = new UnMappedProgramNameReportGenerator();

	        _LogInfo("Process of generating excel sheet from report data has been started.");
	        var report = reportGenerator.Generate(reportData);
	        durationSw.Stop();
	        _LogInfo($"Processing of the program mapping file {reportData.ExportFileName}, finished successfully with count {programNames.Count} in {durationSw.ElapsedMilliseconds} ms.");


	        return report;
        }

    }
}
