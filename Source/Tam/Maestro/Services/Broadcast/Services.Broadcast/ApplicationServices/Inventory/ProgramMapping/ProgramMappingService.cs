using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using F23.StringSimilarity;
using Hangfire;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping.Entities;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.ProgramMapping;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Tam.Maestro.Data.Entities.DataTransferObjects;
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

        /// <summary>
        /// Returns a list of programs that don't have an existing mapping in the database.
        /// </summary>
        /// <returns></returns>
        List<UnmappedProgram> GetUnmappedPrograms();

        /// <summary>
        /// Given a list of program names, it cleans the program names according to the rules and returns a clean list with the original values.
        /// </summary>
        /// <param name="programList"></param>
        /// <returns></returns>
        List<UnmappedProgram> GetCleanPrograms(List<string> programList);


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
        private readonly IProgramMappingCleanupEngine _ProgramCleanupEngine;
        private readonly IProgramNameMappingKeywordRepository _ProgramNameMappingKeywordRepository;
        private readonly IMasterProgramListImporter _MasterProgramListImporter;
        private const string MISC_SHOW_TYPE = "Miscellaneous";
        private const string UnmappedProgramReportFileName = "UnmappedProgramReport.xlsx";
        private const float MATCH_EXACT = 1;
        private const float MATCH_NOT_FOUND = 0;

        public ProgramMappingService(
            IBackgroundJobClient backgroundJobClient,
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISharedFolderService sharedFolderService,
            IProgramNameMappingsExportEngine programNameMappingsExportEngine,
            IGenreCache genreCache,
            IShowTypeCache showTypeCache,
            IProgramsSearchApiClient programsSearchApiClient,
            IProgramMappingCleanupEngine programMappingCleanupEngine,
            IMasterProgramListImporter masterListImporter)
        {
            _BackgroundJobClient = backgroundJobClient;
            _ProgramMappingRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramMappingRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ProgramNameExceptionsRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramNameExceptionsRepository>();
            _ProgramNameMappingKeywordRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramNameMappingKeywordRepository>();
            _SharedFolderService = sharedFolderService;
            _ProgramNameMappingsExportEngine = programNameMappingsExportEngine;
            _GenreCache = genreCache;
            _ShowTypeCache = showTypeCache;
            _ProgramsSearchApiClient = programsSearchApiClient;
            _ProgramCleanupEngine = programMappingCleanupEngine;
            _MasterProgramListImporter = masterListImporter;
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

            var programMappingErrors = _ValidateProgramMappings(uniqueProgramMappings);

            if (programMappingErrors.Any())
            {
                _SharedFolderService.RemoveFile(fileId);
                throw new Exception(_GetErrorMessage(programMappingErrors));
            }

            _ProcessProgramMappings(uniqueProgramMappings, createdDate, userName);

            _SharedFolderService.RemoveFile(fileId);

            durationSw.Stop();

            _LogInfo($@"Processing of the program mapping file {file.FileNameWithExtension}, finished successfully in {durationSw.ElapsedMilliseconds} ms");

            var hangfireId = _BackgroundJobClient.Enqueue<IInventoryProgramsProcessingService>(x => x.PerformRepairInventoryPrograms(CancellationToken.None));
            _LogInfo($"RepairInventoryPrograms job has been queued, hangfire id: {hangfireId}");
        }

        private string _GetErrorMessage(List<ProgramMappingValidationErrorDto> programMappingErrors)
        {
            var fullErrorMessage = new StringBuilder();

            foreach(var programMappingError in programMappingErrors)
            {
                fullErrorMessage.AppendLine(
                    $"Error parsing program {programMappingError.OfficialProgramName}: {programMappingError.ErrorMessage}");
            }

            return fullErrorMessage.ToString();
        }

        private List<ProgramMappingValidationErrorDto> _ValidateProgramMappings(List<ProgramMappingsFileRequestDto> uniqueProgramMappings)
        {
            var masterListPrograms = GetMasterProgramList();
            var programNameExceptions = _ProgramNameExceptionsRepository.GetProgramExceptions();
            var programMappingValidationErrors = new List<ProgramMappingValidationErrorDto>();

            foreach (var programMapping in uniqueProgramMappings)
            {
                var masterListProgram = masterListPrograms.FirstOrDefault(p => 
                            p.OfficialProgramName.Equals(programMapping.OfficialProgramName, 
                                                         StringComparison.OrdinalIgnoreCase));

                if (masterListProgram == null)
                {
                    var programNameException = programNameExceptions.FirstOrDefault(p =>
                            p.CustomProgramName.Equals(programMapping.OfficialProgramName,
                                                         StringComparison.OrdinalIgnoreCase));

                    if (programNameException == null)
                    {
                        programMappingValidationErrors.Add(new ProgramMappingValidationErrorDto
                        {
                            OfficialProgramName = programMapping.OfficialProgramName,
                            ErrorMessage = "Program not found in master list or exception list"
                        });
                    }
                }

                try
                {
                    var genre = _GenreCache.GetMaestroGenreByName(programMapping.OfficialGenre);
                }
                catch
                {
                    programMappingValidationErrors.Add(new ProgramMappingValidationErrorDto
                    {
                        OfficialProgramName = programMapping.OfficialProgramName,
                        ErrorMessage = $"Genre not found: {programMapping.OfficialGenre}"
                    });
                }
            }

            return programMappingValidationErrors;
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
                    var showType = _ShowTypeCache.GetMaestroShowTypeByName(mapping.OfficialShowType);

                    // if there are changes for an existing mapping
                    if (existingMapping.OfficialProgramName != mapping.OfficialProgramName ||
                        existingMapping.OfficialGenre.Name != genre.Name ||
                        existingMapping.OfficialShowType.Name != showType.Name)
                    {
                        existingMapping.OfficialProgramName = mapping.OfficialProgramName;
                        existingMapping.OfficialGenre = genre;
                        existingMapping.OfficialShowType = showType;

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
                        OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName(mapping.OfficialShowType)
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

        public string _GetProgramMappingsDirectoryPath()
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

        public List<UnmappedProgram> GetUnmappedPrograms()
        {
            _LogInfo("Started process to get unmapped programs.");
            var durationSw = new Stopwatch();
            durationSw.Start();

            var programNames = _InventoryRepository.GetUnmappedPrograms().Where(p => p != null).ToList();

            durationSw.Stop();
            _LogInfo($"Obtained unmapped programs with count {programNames.Count} in {durationSw.ElapsedMilliseconds} ms.");

            var cleanPrograms = GetCleanPrograms(programNames);

            var programsMatchedWithMapping = _MatchExistingMappings(cleanPrograms).OrderByDescending(p => p.MatchConfidence).ToList();

            if (programsMatchedWithMapping.All(p => p.MatchConfidence == MATCH_EXACT))
                return programsMatchedWithMapping;

            var masterProgramList = GetMasterProgramList();

            var result = _MatchAgainstMasterList(programsMatchedWithMapping, masterProgramList);

            _MatchProgramByKeyword(result);

            _MatchBySimilarity(result, masterProgramList);

            return result;
        }

        private void _MatchBySimilarity(List<UnmappedProgram> programs, List<ProgramMappingsDto> masterProgramList)
        {
            var unmatchedPrograms = programs.Where(p => p.MatchConfidence != MATCH_EXACT);
            if (!unmatchedPrograms.Any())
                return;

            var jaroWinkler = new JaroWinkler();
            foreach (var masterProgramListItem in masterProgramList)
            {
                foreach (var unmatchedProgram in unmatchedPrograms)
                {
                    var similarity = jaroWinkler.Similarity(unmatchedProgram.ProgramName, masterProgramListItem.OfficialProgramName);
                    if (similarity > unmatchedProgram.MatchConfidence)
                    {
                        unmatchedProgram.MatchConfidence = (float)similarity;
                        unmatchedProgram.Genre = masterProgramListItem.OfficialGenre.Name;
                        unmatchedProgram.MatchedName = masterProgramListItem.OfficialProgramName;
                        unmatchedProgram.ShowType = masterProgramListItem.OfficialShowType.Name;
                    }
                }
            }
        }

        private List<UnmappedProgram> _MatchAgainstMasterList(List<UnmappedProgram> programs, List<ProgramMappingsDto> masterProgramList)
        {
            foreach (var program in programs.Where(p => p.MatchConfidence == MATCH_NOT_FOUND))
            {
                var foundMapping = masterProgramList
                    .Where(m => m.OfficialProgramName.Equals(program.ProgramName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                if (foundMapping != null)
                {
                    program.MatchedName = foundMapping.OfficialProgramName;
                    program.Genre = foundMapping.OfficialGenre.Name;
                    program.ShowType = foundMapping.OfficialShowType.Name;
                    program.MatchConfidence = MATCH_EXACT;
                }
            }

            return programs;
        }

        private List<ProgramMappingsDto> GetMasterProgramList()
        {
            var appFolder = _GetBroadcastAppFolder();
            const string masterListFolder = "ProgramMappingMasterList";
            const string masterListFile = "MasterListWithWwtvTitles.txt";

            var masterListPath = Path.Combine(
                appFolder,
                masterListFolder,
                masterListFile);

            var fileStream = File.OpenText(masterListPath);

            var masterList = _MasterProgramListImporter.ImportMasterProgramList(fileStream.BaseStream);

            return masterList;
        }

        private void _MatchProgramByKeyword(List<UnmappedProgram> programs)
        {
            var keywords = _ProgramNameMappingKeywordRepository.GetProgramNameMappingKeywords();

            foreach (var program in programs.Where(p => p.MatchConfidence == MATCH_NOT_FOUND))
            {
                var keywordMatches = keywords.Where(k => program.ProgramName.Contains(k.Keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                if (keywordMatches.Any())
                {
                    var keyword = keywordMatches.FirstOrDefault();
                    program.MatchedName = keyword.ProgramName;
                    program.Genre = keyword.Genre.Display;
                    program.ShowType = keyword.ShowType.Display;
                    program.MatchConfidence = MATCH_EXACT;
                }
            }
        }

        private List<UnmappedProgram> _MatchExistingMappings(List<UnmappedProgram> cleanPrograms)
        {
            var result = cleanPrograms;

            var programMappings = _ProgramMappingRepository.GetProgramMappings();

            foreach (var program in cleanPrograms)
            {
                var foundMapping = programMappings
                    .Where(m => m.OriginalProgramName.Equals(program.ProgramName, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
                if (foundMapping != null)
                {
                    program.MatchedName = foundMapping.OfficialProgramName;
                    program.Genre = foundMapping.OfficialGenre.Name;
                    program.ShowType = foundMapping.OfficialShowType.Name;
                    program.MatchConfidence = MATCH_EXACT; //Exact match found
                }
            }

            return result;
        }

        public List<UnmappedProgram> GetCleanPrograms(List<string> programList)
        {
            var result = new List<UnmappedProgram>();

            foreach (var programName in programList)
            {
                var program = new UnmappedProgram();
                program.OriginalName = programName;
                program.ProgramName = _ProgramCleanupEngine.GetCleanProgram(programName);
                result.Add(program);
            }

            return result;
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
                                    mapping.OfficialShowType = _ShowTypeCache.GetMaestroShowTypeLookupDtoByName(matchedProgram.ShowType)
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
                                            _ShowTypeCache.GetMaestroShowTypeLookupDtoByName(showType).Display;
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
