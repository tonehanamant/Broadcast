using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using F23.StringSimilarity;
using Hangfire;
using Microsoft.EntityFrameworkCore.Internal;
using OfficeOpenXml;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping.Entities;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Tam.Maestro.Data.Entities.DataTransferObjects;

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

        /// <summary>
        /// Export the unmapped programs
        /// </summary>
        ReportOutput ExportUnmappedPrograms();
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
        private readonly IProgramMappingCleanupEngine _ProgramCleanupEngine;
        private readonly IProgramNameMappingKeywordRepository _ProgramNameMappingKeywordRepository;
        private readonly IMasterProgramListImporter _MasterProgramListImporter;
        private readonly IDateTimeEngine _DateTimeEngine;
        private const string MISC_SHOW_TYPE = "Miscellaneous";
        private const string SERIES_SHOW_TYPE = "Series";
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
            IProgramMappingCleanupEngine programMappingCleanupEngine,
            IMasterProgramListImporter masterListImporter,
            IDateTimeEngine dateTimeEngine,
            IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
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
            _ProgramCleanupEngine = programMappingCleanupEngine;
            _MasterProgramListImporter = masterListImporter;
            _DateTimeEngine = dateTimeEngine;
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
            // clean before de-dupping in case a clean produces a dup.
            _CleanProgramMappings(programMappings);
            var uniqueProgramMappings = _RemoveDuplicateMappings(programMappings);
            _LogInfo($"The selected program mapping file has {programMappings.Count} rows, unique mappings: {uniqueProgramMappings.Count}");

            WebUtilityHelper.HtmlDecodeProgramNames(uniqueProgramMappings);

            var masterListPrograms = _MasterProgramListImporter.ImportMasterProgramList();
            var programNameExceptions = _ProgramNameExceptionsRepository.GetProgramExceptions();

            var programMappingErrors = _ValidateProgramMappings(uniqueProgramMappings, masterListPrograms, programNameExceptions);

            if (programMappingErrors.Any())
            {
                _SharedFolderService.RemoveFile(fileId);
                var flatErrorMessages = _GetErrorMessage(programMappingErrors);
                throw new InvalidOperationException(flatErrorMessages);
            }

            _PopulateShowTypes(uniqueProgramMappings, masterListPrograms, programNameExceptions);

            _ProcessProgramMappings(uniqueProgramMappings, createdDate, userName);

            _SharedFolderService.RemoveFile(fileId);

            durationSw.Stop();

            _LogInfo($@"Processing of the program mapping file {file.FileNameWithExtension}, finished successfully in {durationSw.ElapsedMilliseconds} ms");

            var hangfireId = _BackgroundJobClient.Enqueue<IInventoryProgramsProcessingService>(x => x.PerformRepairInventoryPrograms(CancellationToken.None));
            _LogInfo($"RepairInventoryPrograms job has been queued, hangfire id: {hangfireId}");
        }

        private void _PopulateShowTypes(List<ProgramMappingsFileRequestDto> uniqueProgramMappings,
            List<ProgramMappingsDto> masterListPrograms, List<ProgramNameExceptionDto> programNameExceptions)
        {
            foreach (var programMapping in uniqueProgramMappings)
            {
                var foundExceptions = programNameExceptions.Where(p =>
                           p.CustomProgramName.Equals(programMapping.OfficialProgramName, StringComparison.OrdinalIgnoreCase) &&
                           p.GenreName.Equals(programMapping.OfficialGenre, StringComparison.OrdinalIgnoreCase)).ToList();
                if (foundExceptions.Any())
                {
                    if (foundExceptions.Any(f => f.ShowTypeName.Equals(SERIES_SHOW_TYPE, StringComparison.OrdinalIgnoreCase)))
                        programMapping.OfficialShowType = SERIES_SHOW_TYPE;
                    else
                        programMapping.OfficialShowType = foundExceptions.FirstOrDefault().ShowTypeName;
                }
                else
                {
                    var foundMasterList = masterListPrograms.Where(p =>
                            p.OfficialProgramName.Equals(programMapping.OfficialProgramName, StringComparison.OrdinalIgnoreCase) &&
                            p.OfficialGenre.Name.Equals(programMapping.OfficialGenre, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (foundMasterList.Any())
                    {
                        if (foundMasterList.Any(m => m.OfficialShowType.Name.Equals(SERIES_SHOW_TYPE, StringComparison.OrdinalIgnoreCase)))
                        {
                            programMapping.OfficialShowType = SERIES_SHOW_TYPE;
                        }
                        else
                        {
                            programMapping.OfficialShowType = foundMasterList.FirstOrDefault().OfficialShowType.Name;
                        }
                    }
                    else
                    {
                        programMapping.OfficialShowType = MISC_SHOW_TYPE;
                    }
                }
            }
        }

        private void _CleanProgramMappings(List<ProgramMappingsFileRequestDto> programMappings)
        {
            programMappings.ForEach(pm => pm.OfficialProgramName = _ProgramCleanupEngine.InvertPrepositions(pm.OfficialProgramName).Trim());
        }

        private string _GetErrorMessage(List<ProgramMappingValidationErrorDto> programMappingErrors)
        {
            var fullErrorMessage = new StringBuilder();

            foreach (var programMappingError in programMappingErrors)
            {
                fullErrorMessage.AppendLine(
                    $"Error parsing program '{programMappingError.MappingProgramName}': {programMappingError.ErrorMessage}; " +
                    $"MetaData={programMappingError.RateCardName}|{programMappingError.MappingProgramName}|{programMappingError.MappingGenreName};");
            }

            return fullErrorMessage.ToString();
        }

        private List<ProgramMappingsDto> _GetCadentPrograms(string programName,
            List<ProgramMappingsDto> masterListPrograms,
            List<ProgramNameExceptionDto> programNameExceptions)
        {
            var result = new List<ProgramMappingsDto>();

            // check the master list
            var foundFromMasterList = masterListPrograms.FindAll(p => p.OfficialProgramName.Equals(programName, StringComparison.OrdinalIgnoreCase));
            if (foundFromMasterList.Any())
            {
                result.AddRange(foundFromMasterList);
            }

            // check exceptions
            var foundFromExceptions = programNameExceptions.FindAll(p => p.CustomProgramName.Equals(programName, StringComparison.OrdinalIgnoreCase));
            if (foundFromExceptions.Any())
            {
                // transform for downstream usage.
                var transformed = foundFromExceptions.Select(s => new ProgramMappingsDto
                {
                    OfficialProgramName = s.CustomProgramName,
                    OfficialGenre = new Genre { Name = s.GenreName, ProgramSourceId = (int)ProgramSourceEnum.Maestro }
                }).ToList();
                result.AddRange(transformed);
            }
            return result;
        }

        private Genre _GetCadentGenre(string genreName)
        {
            try
            {
                var foundGenre = _GenreCache.GetMaestroGenreByName(genreName);
                return foundGenre;
            }
            catch
            {
                return null;
            }
        }

        private List<ProgramMappingValidationErrorDto> _ValidateProgramMappings(List<ProgramMappingsFileRequestDto> uniqueProgramMappings,
            List<ProgramMappingsDto> masterListPrograms, List<ProgramNameExceptionDto> programNameExceptions)
        {
            var programMappingValidationErrors = new List<ProgramMappingValidationErrorDto>();

            foreach (var programMapping in uniqueProgramMappings)
            {
                // Validate the given Mapping Program Name
                var cadentPrograms = _GetCadentPrograms(programMapping.OfficialProgramName, masterListPrograms, programNameExceptions);
                if (!cadentPrograms.Any())
                {
                    programMappingValidationErrors.Add(new ProgramMappingValidationErrorDto
                    {
                        RateCardName = programMapping.OriginalProgramName,
                        MappingProgramName = programMapping.OfficialProgramName,
                        MappingGenreName = programMapping.OfficialGenre,
                        ErrorMessage = "Mapping Program not found in master list or exception list."
                    });
                    continue;
                }

                // Validate given Mapping Genres
                // does the given genre exist?
                var cadentGenre = _GetCadentGenre(programMapping.OfficialGenre);
                if (cadentGenre == null)
                {
                    programMappingValidationErrors.Add(new ProgramMappingValidationErrorDto
                    {
                        RateCardName = programMapping.OriginalProgramName,
                        MappingProgramName = programMapping.OfficialProgramName,
                        MappingGenreName = programMapping.OfficialGenre,
                        ErrorMessage = $"Mapping Genre not found: {programMapping.OfficialGenre}"
                    });
                    continue;
                }
                
                // does the given genre match CadentProgram.Genre
                var masterGenreList = cadentPrograms.Select(g => g.OfficialGenre).Select(g => g.Name).Distinct().ToList();
                var genresMatch = masterGenreList.Contains(cadentGenre.Name);
                if (!genresMatch)
                {
                    programMappingValidationErrors.Add(new ProgramMappingValidationErrorDto
                    {
                        RateCardName = programMapping.OriginalProgramName,
                        MappingProgramName = programMapping.OfficialProgramName,
                        MappingGenreName = programMapping.OfficialGenre,
                        ErrorMessage = $"Mapping Program name '{programMapping.OfficialProgramName}' found, but mistmatched on Genre. " +
                        $"Found expected genres : '{string.Join(", ", masterGenreList)}'."
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

        internal void _ProcessProgramMappings(
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
                var genre = _GetGenre(mapping.OfficialGenre);

                if (existingProgramMappingByOriginalProgramName.TryGetValue(mapping.OriginalProgramName, out var existingMapping))
                {
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
                        OfficialGenre = genre,
                        OfficialShowType = _ShowTypeCache.GetMaestroShowTypeByName(mapping.OfficialShowType)
                    };

                    newProgramMappings.Add(newProgramMapping);
                }
            }
        }

        private Genre _GetGenre(string genreName)
        {
            Genre genre;

            try
            {
                genre = _GenreCache.GetMaestroGenreByName(genreName);
            }
            catch
            {
                var genreDto = _GenreCache.GetSourceGenreLookupDtoByName(genreName, ProgramSourceEnum.Master);

                genre = _GenreCache.GetMaestroGenreBySourceGenre(genreDto, ProgramSourceEnum.Master);
            }

            return genre;
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

            _LogInfo("Cleaning the programs...");
            var cleanPrograms = GetCleanPrograms(programNames);

            _LogInfo("Start compiling mapping suggestions.");

            _LogInfo("Looking for exact match suggestions...");
            var programsMatchedWithMapping = _MatchExistingMappings(cleanPrograms).OrderByDescending(p => p.MatchConfidence).ToList();

            if (programsMatchedWithMapping.All(p => p.MatchConfidence == MATCH_EXACT))
            {
                _LogInfo("Returning with all exact matches.");
                return programsMatchedWithMapping;
            }

            _LogInfo("Importing the master program list.");
            var masterProgramList = _MasterProgramListImporter.ImportMasterProgramList();
            _LogInfo($"Imported the master program list : {masterProgramList.Count} programs");

            _LogInfo("Attempting matching suggestions programs against the master program list.");
            var result = _MatchAgainstMasterList(programsMatchedWithMapping, masterProgramList);

            _LogInfo("Attempting matching suggestions programs by keyword.");
            _MatchProgramByKeyword(result);

            _LogInfo("Attempting matching suggestions programs by similarity.");
            _MatchBySimilarity(result, masterProgramList);

            _LogInfo("Finished compiling mapping suggestions.");

            return result;
        }

        private void _MatchBySimilarity(List<UnmappedProgram> programs, List<ProgramMappingsDto> masterProgramList)
        {
            var unmatchedPrograms = programs.Where(p => p.MatchConfidence != MATCH_EXACT);

            _LogInfo($"Attempting similarity suggestions.  Checking {programs.Count} programs against a master list of {masterProgramList.Count} programs.; ");

            if (!unmatchedPrograms.Any())
                return;

            var jaroWinkler = new JaroWinkler();
            foreach (var masterProgramListItem in masterProgramList)
            {
                foreach (var unmatchedProgram in unmatchedPrograms)
                {
                    // The algorithm is case sensitive, so it's necessary both string have the same case
                    var similarity = jaroWinkler.Similarity(unmatchedProgram.ProgramName.ToLower(), masterProgramListItem.OfficialProgramName.ToLower());
                    if (similarity > unmatchedProgram.MatchConfidence)
                    {
                        unmatchedProgram.MatchConfidence = (float)similarity;
                        unmatchedProgram.Genre = masterProgramListItem.OfficialGenre.Name;
                        unmatchedProgram.MatchedName = masterProgramListItem.OfficialProgramName;
                        unmatchedProgram.ShowType = masterProgramListItem.OfficialShowType.Name;
                        unmatchedProgram.MatchType = ProgramMappingMatchTypeEnum.BySimilarity;
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
                    program.MatchType = ProgramMappingMatchTypeEnum.ByMasterList;
                }
            }

            return programs;
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
                    program.MatchType = ProgramMappingMatchTypeEnum.ByKeyword;
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
                    program.MatchType = ProgramMappingMatchTypeEnum.ByExistingMapping;
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

        public ReportOutput ExportUnmappedPrograms()
        {
            _LogInfo("Start get unmapped programs.");
            var programs = GetUnmappedPrograms();

            _LogInfo("Got data. Start excel generation.");
            var exactMatches = programs.Where(p => p.MatchType == ProgramMappingMatchTypeEnum.ByMasterList || p.MatchType == ProgramMappingMatchTypeEnum.ByExistingMapping).ToList();
            var exactReport = _GenerateExcelFile(exactMatches, $"Unmapped_Exact_{_GetCurrentDateOnReportFormat()}.xlsx");

            var keywordMatches = programs.Where(p => p.MatchType == ProgramMappingMatchTypeEnum.ByKeyword).ToList();
            var keywordReport = _GenerateExcelFile(keywordMatches, $"Unmapped_Keyword_{_GetCurrentDateOnReportFormat()}.xlsx");

            const string SPORTS = "Sports";
            var sportsMatches = programs.Where(p => p.MatchType == ProgramMappingMatchTypeEnum.BySimilarity && p.Genre.Contains(SPORTS, StringComparison.OrdinalIgnoreCase)).ToList();
            var sportsReport = _GenerateExcelFile(sportsMatches, $"Unmapped_Sports_{_GetCurrentDateOnReportFormat()}.xlsx");

            const string NEWS = "News";
            var newsMatches = programs.Where(p => p.MatchType == ProgramMappingMatchTypeEnum.BySimilarity && p.MatchConfidence != MATCH_EXACT && p.Genre.Contains(NEWS, StringComparison.OrdinalIgnoreCase)).ToList();
            var newsReport = _GenerateExcelFile(newsMatches, $"Unmapped_News_{_GetCurrentDateOnReportFormat()}.xlsx");

            var otherMatches = programs.Where(p => p.MatchType == ProgramMappingMatchTypeEnum.BySimilarity && !p.Genre.Contains(SPORTS, StringComparison.OrdinalIgnoreCase) && !p.Genre.Contains(NEWS, StringComparison.OrdinalIgnoreCase)).ToList();
            var otherReport = _GenerateExcelFile(otherMatches, $"Unmapped_Other_{_GetCurrentDateOnReportFormat()}.xlsx");

            _LogInfo("Generate and return zips");
            return _ZipReports(exactReport, keywordReport, sportsReport, newsReport, otherReport);
        }

        private string _GetCurrentDateOnReportFormat() =>
            _DateTimeEngine.GetCurrentMoment().ToString("MMddyyy_HHmmss");

        private ReportOutput _ZipReports(params ReportOutput[] reports)
        {
            var reportOutput = new ReportOutput($"Unmapped_{_GetCurrentDateOnReportFormat()}.zip");

            using (var archive = new ZipArchive(reportOutput.Stream, ZipArchiveMode.Create, true))
            {
                foreach (var report in reports)
                {
                    var excelFile = archive.CreateEntry(report.Filename);
                    using (var entryStream = excelFile.Open())
                    using (var fileToCompressStream = report.Stream)
                    {
                        fileToCompressStream.CopyTo(entryStream);
                    }
                }
            }

            reportOutput.Stream.Position = 0;

            return reportOutput;
        }

        private ReportOutput _GenerateExcelFile(List<UnmappedProgram> programs, string fileName)
        {
            var reportaData = new UnmappedProgramReportData(fileName, programs.OrderByDescending(p => p.MatchConfidence).ThenBy(p => p.OriginalName).ToList());
            return new UnmappedProgramsReportGenerator().Generate(reportaData);
        }

        //BP1-402 is pushed to next release so leaving this code for later use. Additional testing and coordination to point to the Dativa Search would be required.
        internal void _LoadShowTypes(List<ProgramMappingsFileRequestDto> mappings)
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
                        mapping.OfficialShowType = !string.IsNullOrWhiteSpace(mapping.OfficialShowType)
                            ? mapping.OfficialShowType
                            : MISC_SHOW_TYPE;
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
