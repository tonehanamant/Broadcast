using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Hangfire;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.ReportGenerators.ProgramMapping;
using Services.Broadcast.Cache;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using System.Threading;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO.Program;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

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
        private readonly IProgramNameExceptionsRepository _ProgramNameExceptionsRepository;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IProgramNameMappingsExportEngine _ProgramNameMappingsExportEngine;
        private readonly IGenreCache _GenreCache;
        private readonly IShowTypeCache _ShowTypeCache;
        private readonly IProgramsSearchApiClient _ProgramsSearchApiClient;
        private const string MISC_SHOW_TYPE = "Miscellaneous";
        private const string UnmappedProgramReportFileName = "UnmappedProgramReport.xlsx";

        public ProgramMappingService(
            IBackgroundJobClient backgroundJobClient,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISharedFolderService sharedFolderService,
            IProgramNameMappingsExportEngine programNameMappingsExportEngine,
            IGenreCache genreCache,
            IShowTypeCache showTypeCache,
            IProgramsSearchApiClient programsSearchApiClient)
        {
            _BackgroundJobClient = backgroundJobClient;
            _ProgramMappingRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ProgramNameExceptionsRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramNameExceptionsRepository>();
            _SharedFolderService = sharedFolderService;
            _ProgramNameMappingsExportEngine = programNameMappingsExportEngine;
            _GenreCache = genreCache;
            _ShowTypeCache = showTypeCache;
            _ProgramsSearchApiClient = programsSearchApiClient;
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

            _LogInfo($"Started processing the program mapping file {file.FileNameWithExtension}");

            var programMappings = _ReadProgramMappingsFile(file.FileContent);
            var uniqueProgramMappings = _RemoveDuplicateMappings(programMappings);
            _LogInfo($"The selected program mapping file has {programMappings.Count} rows, unique mappings: {uniqueProgramMappings.Count}");

            WebUtilityHelper.HtmlDecodeProgramNames(uniqueProgramMappings);
            _ProcessProgramMappings(uniqueProgramMappings, createdDate, userName);
            _SharedFolderService.RemoveFile(fileId);

            durationSw.Stop();

            _LogInfo($@"Processing of the program mapping file {file.FileNameWithExtension}, finished successfully in {durationSw.ElapsedMilliseconds} ms");

            var hangfireId = _BackgroundJobClient.Enqueue<IInventoryProgramsProcessingService>(x => x.PerformRepairInventoryPrograms(CancellationToken.None));
            _LogInfo($"RepairInventoryPrograms job has been queued, hangfire id: {hangfireId}");
        }

        private List<ProgramMappingsFileRequestDto> _RemoveDuplicateMappings(List<ProgramMappingsFileRequestDto> programMappings)
        {
            return programMappings
                .GroupBy(x => x.OriginalProgramName)
                .Select(x => x.First())
                .ToList();
        }

        /// <inheritdoc />
        public ReportOutput ExportProgramMappingsFile(string username)
        {
            const string fileName = "BroadcastMappedPrograms.xlsx";

            _LogInfo($"Export beginning.", username);
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
            DateTime createdDate,
            string username)
        {
      
            var existingProgramMappingByOriginalProgramName = _GetExistingProgramMappings(programMappings);


            _FindNewAndUpdatedProgramMappings(
                programMappings,
                existingProgramMappingByOriginalProgramName,
                out var newProgramMappings,
                out var updatedProgramMappings);

            _LogInfo($"Updating {updatedProgramMappings.Count} existing program mappings");
            _ProgramMappingRepository.UpdateProgramMappings(updatedProgramMappings, username, createdDate);

            _LogInfo($"Inserting {newProgramMappings.Count} new program mappings");
            _ProgramMappingRepository.CreateProgramMappings(newProgramMappings, username, createdDate);
        }

        private Dictionary<string, ProgramMappingsDto> _GetExistingProgramMappings(List<ProgramMappingsFileRequestDto> fileProgramMappings)
        {
            var originalProgramNames = fileProgramMappings.Select(x => x.OriginalProgramName).Distinct();
            var existingProgramMappings = _ProgramMappingRepository.GetProgramMappingsByOriginalProgramNames(originalProgramNames);

            return existingProgramMappings.ToDictionary(x => x.OriginalProgramName, x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        private void _FindNewAndUpdatedProgramMappings(
            List<ProgramMappingsFileRequestDto> mappings,
            Dictionary<string, ProgramMappingsDto> existingProgramMappingByOriginalProgramName,
            out List<ProgramMappingsDto> newProgramMappings,
            out List<ProgramMappingsDto> updatedProgramMappings)
        {
            newProgramMappings = new List<ProgramMappingsDto>();
            updatedProgramMappings = new List<ProgramMappingsDto>();

            foreach (var mapping in mappings)
            {
                if (existingProgramMappingByOriginalProgramName.TryGetValue(mapping.OriginalProgramName, out var existingMapping))
                {
                    var genre = _GenreCache.GetMaestroGenreByName(mapping.OfficialGenre);
                    var showType = _ShowTypeCache.GetShowTypeByName(mapping.OfficialShowType);

                    // if there are changes for an existing mapping
                    if (existingMapping.OfficialProgramName != mapping.OfficialProgramName ||
                        existingMapping.OfficialGenre.Name != genre.Name ||
                        existingMapping.OfficialShowType.Name != showType.Display)
                    {
                        existingMapping.OfficialProgramName = mapping.OfficialProgramName;
                        existingMapping.OfficialGenre = genre;
                        existingMapping.OfficialShowType = _MapToShowTypeDto(showType);

                        updatedProgramMappings.Add(existingMapping);
                    }
                }
                else
                {
                    var newProgramMapping = new ProgramMappingsDto
                    {
                        OriginalProgramName = mapping.OriginalProgramName,
                        OfficialProgramName = mapping.OfficialProgramName,
                        OfficialGenre = _GenreCache.GetMaestroGenreByName(mapping.OfficialGenre),
                        OfficialShowType = _MapToShowTypeDto(_ShowTypeCache.GetShowTypeByName(mapping.OfficialShowType))
                    };

                    newProgramMappings.Add(newProgramMapping);
                }
            }
        }

        private ShowTypeDto _MapToShowTypeDto(LookupDto showTypeLookup)
        {
            return new ShowTypeDto
            {
                Id = showTypeLookup.Id,
                Name = showTypeLookup.Display
            };
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

        protected virtual bool _GetEnableInternalProgramSearch()
        {
            return BroadcastServiceSystemParameter.EnableInternalProgramSearch;
        }

        //BP1-402 is pushed to next release so leaving this code for later use. Additional testing and coordination to point to the Dativa Search would be required.
        protected void _LoadShowTypes(List<ProgramMappingsFileRequestDto> mappings)
        {
            var programNameExceptions = _ProgramNameExceptionsRepository.GetProgramExceptions();

            foreach (var mapping in mappings)
            {
                try
                {
                    var foundException =
                        programNameExceptions.SingleOrDefault(p =>
                            p.CustomProgramName.Equals(mapping.OfficialProgramName) &&
                            p.GenreName.Equals(mapping.OfficialGenre));
                    if (foundException != null)
                    {
                        mapping.OfficialShowType = foundException.ShowTypeName;
                    }
                    else
                    {
                        if (!_GetEnableInternalProgramSearch())
                        {
                            var request = new SearchRequestProgramDto
                            {
                                ProgramName = mapping.OfficialProgramName,
                                Start = 1,
                                Limit = 1000
                            };
                            var programs =
                                _ProgramsSearchApiClient.GetPrograms(request);

                            if (programs != null && programs.Any())
                            {
                                var matchedProgram = programs.SingleOrDefault(p =>
                                    p.ProgramName.Equals(mapping.OfficialProgramName) &&
                                    p.Genre.Equals(mapping.OfficialGenre));
                                if (matchedProgram != null)
                                {
                                    mapping.OfficialShowType = _ShowTypeCache.GetShowTypeByName(matchedProgram.ShowType)
                                        .Display;
                                }
                                else
                                {
                                    var matchingPrograms = programs.Where(p =>
                                        p.ProgramName.Contains(mapping.OfficialProgramName) &&
                                        p.Genre.Equals(mapping.OfficialGenre)).ToList();
                                    if (matchingPrograms.Any())
                                    {
                                        var seriesP = matchingPrograms.SingleOrDefault(p =>
                                            p.ProgramName.ToLower().EndsWith("series"));
                                        var showType = seriesP != null
                                            ? seriesP.ShowType
                                            : matchingPrograms.FirstOrDefault().ShowType;
                                        mapping.OfficialShowType =
                                            _ShowTypeCache.GetShowTypeByName(showType).Display;
                                    }
                                    else
                                    {
                                        mapping.OfficialShowType = MISC_SHOW_TYPE;
                                    }
                                }
                            }
                        }
                        else
                        {
                            mapping.OfficialShowType = !string.IsNullOrWhiteSpace(mapping.OfficialShowType)
                                ? mapping.OfficialShowType
                                : MISC_SHOW_TYPE;
                        }
                    }
                }

                catch (Exception e)
                {
                    throw new InvalidOperationException($"There is error with record OriginalProgramName: {mapping.OriginalProgramName}, OfficialProgramName: {mapping.OfficialProgramName} ", e);
                }
            }
        }
    }
}
