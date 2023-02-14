using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.SpotExceptions;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecServiceV2 : IApplicationService
    {
        /// <summary>
        /// Gets the out of spec plans to do asynchronous.
        /// </summary>
        /// <param name="outOfSpecsPlansIncludingFiltersDoneRequest">The spot exceptions out of spec plans to do request.</param>
        /// <returns></returns>
        Task<List<OutOfSpecPlansResult>> GetOutOfSpecPlansToDoAsync(OutOfSpecPlansIncludingFiltersRequestDto outOfSpecsPlansIncludingFiltersDoneRequest);

        /// <summary>
        /// Gets the done plans using inventory source filter
        /// </summary>
        /// <param name="outOfSpecsPlansIncludingFiltersDoneRequest">week start date, end date and inventory sources</param>
        /// <returns>List of done plans</returns>
        Task<List<OutOfSpecPlansResult>> GetOutOfSpecPlansDoneAsync(OutOfSpecPlansIncludingFiltersRequestDto outOfSpecsPlansIncludingFiltersDoneRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec plan inventory sources asynchronous.
        /// </summary>
        /// <param name="outOfSpecPlansRequest">The spot exceptions out of spec plans request.</param>
        /// <returns></returns>
        Task<List<string>> GetOutOfSpecPlanInventorySourcesAsync(OutOfSpecPlansRequestDto outOfSpecPlansRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec spot inventory sources asynchronous.
        /// </summary>
        /// <param name="outOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        Task<List<OutOfSpecSpotInventorySourcesDto>> GetOutOfSpecSpotInventorySourcesAsync(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec spot reason codes asynchronous v2.
        /// </summary>
        /// <param name="outOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        Task<List<OutOfSpecSpotReasonCodeResultsDto>> GetOutOfSpecSpotReasonCodesAsync(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec programs asynchronous.
        /// </summary>
        /// <param name="programNameQuery">The program name query.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        Task<List<OutOfSpecSpotProgramsDto>> GetOutOfSpecSpotProgramsAsync(string programNameQuery, string userName);

        /// <summary>
        /// Generats out of spec report.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="currentDate">The current date.</param>
        /// <param name="templatesFilePath">The templates file path.</param>
        /// <returns></returns>
        Guid GenerateOutOfSpecExportReport(OutOfSpecExportRequestDto request, string userName, DateTime currentDate, string templatesFilePath);
        /// <summary>
        /// Generats out of spec report.
        /// </summary>
        /// <param name="saveOutOfSpecPlanDecisionsRequest">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        Task<bool> SaveOutOfSpecPlanDecsionsAsync(SaveOutOfSpecPlanDecisionsRequestDto saveOutOfSpecPlanDecisionsRequest, string userName);

    }
    public class SpotExceptionsOutOfSpecServiceV2 : BroadcastBaseClass, ISpotExceptionsOutOfSpecServiceV2
    {
        const string fileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        const string outOfSpecBuyerExportFileName = "Template - Out of Spec Report Buying Team.xlsx";

        private readonly ISpotExceptionsOutOfSpecRepositoryV2 _SpotExceptionsOutOfSpecRepositoryV2;

        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IFileService _FileService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IGenreCache _GenreCache;
        private readonly IAabEngine _AabEngine;

        private readonly Lazy<bool> _EnableSharedFileServiceConsolidation;

        public SpotExceptionsOutOfSpecServiceV2(
          IDataRepositoryFactory dataRepositoryFactory,
          IFeatureToggleHelper featureToggleHelper,
          IDateTimeEngine dateTime,
          IFileService fileService,
          ISharedFolderService sharedFolderService,
          IGenreCache genreCache,
          IAabEngine aabEngine,
          IConfigurationSettingsHelper configurationSettingsHelper)
          : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsOutOfSpecRepositoryV2 = dataRepositoryFactory.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>();
            _DateTimeEngine = dateTime;
            _FileService = fileService;
            _SharedFolderService = sharedFolderService;
            _GenreCache = genreCache;
            _EnableSharedFileServiceConsolidation = new Lazy<bool>(_GetEnableSharedFileServiceConsolidation);
            _AabEngine = aabEngine;
        }

        /// <inheritdoc />
        public async Task<List<OutOfSpecPlansResult>> GetOutOfSpecPlansToDoAsync(OutOfSpecPlansIncludingFiltersRequestDto outOfSpecsPlansIncludingFiltersDoneRequest)
        {
            List<OutOfSpecPlansResult> spotExceptionsOutOfSpecs = new List<OutOfSpecPlansResult>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Plans Todo");
            try
            {
                var outOfSpecToDo = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlansToDoAsync(outOfSpecsPlansIncludingFiltersDoneRequest.InventorySourceNames,
                    outOfSpecsPlansIncludingFiltersDoneRequest.WeekStartDate, outOfSpecsPlansIncludingFiltersDoneRequest.WeekEndDate);

                if (outOfSpecToDo?.Any() ?? false)
                {
                    spotExceptionsOutOfSpecs = outOfSpecToDo.Select(x => new OutOfSpecPlansResult
                    {
                        PlanId = x.PlanId,
                        AdvertiserName = _GetAdvertiserName(x.AdvertiserMasterId),
                        PlanName = x.PlanName,
                        AffectedSpotsCount = x.AffectedSpotsCount,
                        Impressions = Math.Floor(x.Impressions / 1000),
                        SyncedTimestamp = null,
                        SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                        AudienceName = x.AudienceName,
                        FlightString = $"{DateTimeHelper.GetForDisplay(x.FlightStartDate, SpotExceptionsConstants.DateFormat)} - {DateTimeHelper.GetForDisplay(x.FlightEndDate, SpotExceptionsConstants.DateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"

                    }).OrderBy(x => x.AdvertiserName).ThenBy(x => x.PlanName).ToList();
                }

                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Plan Todo");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Plan Todo";
                throw new CadentException(msg, ex);
            }

            return spotExceptionsOutOfSpecs;
        }
        /// <inheritdoc />
        public async Task<List<OutOfSpecPlansResult>> GetOutOfSpecPlansDoneAsync(OutOfSpecPlansIncludingFiltersRequestDto outOfSpecsPlansIncludingFiltersDoneRequest)
        {
            var outOfSpecPlans = new List<OutOfSpecPlansResult>();
            var outOfSpecDone = new List<SpotExceptionsOutOfSpecGroupingDto>();
            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Plans Done");
                outOfSpecDone = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlansDoneAsync(outOfSpecsPlansIncludingFiltersDoneRequest.WeekStartDate, outOfSpecsPlansIncludingFiltersDoneRequest.WeekEndDate, outOfSpecsPlansIncludingFiltersDoneRequest.InventorySourceNames);

                if (outOfSpecDone?.Any() ?? false)
                {
                    outOfSpecPlans = outOfSpecDone.Select(x =>
                    {
                        return new OutOfSpecPlansResult
                        {
                            PlanId = x.PlanId,
                            AdvertiserName = _GetAdvertiserName(x.AdvertiserMasterId),
                            PlanName = x.PlanName,
                            AffectedSpotsCount = x.AffectedSpotsCount,
                            Impressions = Math.Floor(x.Impressions / 1000),
                            SyncedTimestamp = DateTimeHelper.GetForDisplay(x.SyncedTimestamp, SpotExceptionsConstants.DateTimeFormat),
                            SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                            AudienceName = x.AudienceName,
                            FlightString = $"{DateTimeHelper.GetForDisplay(x.FlightStartDate, SpotExceptionsConstants.DateFormat)} - {DateTimeHelper.GetForDisplay(x.FlightEndDate, SpotExceptionsConstants.DateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"
                        };
                    }).OrderBy(x => x.AdvertiserName).ThenBy(x => x.PlanName).ToList();
                }
                _LogInfo($" Finished: Retrieving Spot Exceptions Out Of Spec Plans Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Plans Done";
                throw new CadentException(msg, ex);
            }

            return outOfSpecPlans;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetOutOfSpecPlanInventorySourcesAsync(OutOfSpecPlansRequestDto outOfSpecPlansRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();
            List<string> inventorySources = new List<string>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Plan Inventory Sources V2");
            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlanToDoInventorySourcesAsync(outOfSpecPlansRequest.WeekStartDate, outOfSpecPlansRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlanDoneInventorySourcesAsync(outOfSpecPlansRequest.WeekStartDate, outOfSpecPlansRequest.WeekEndDate);

                inventorySources = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().OrderBy(inventorySource => inventorySource).ToList();
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Plan Inventory Sources V2");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Plan Inventory Sources V2";
                throw new CadentException(msg, ex);
            }

            return inventorySources;
        }

        /// <inheritdoc />
        public async Task<List<OutOfSpecSpotInventorySourcesDto>> GetOutOfSpecSpotInventorySourcesAsync(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();
            List<OutOfSpecSpotInventorySourcesDto> inventorySources = new List<OutOfSpecSpotInventorySourcesDto>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Spot Inventory Sources V2");
            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotToDoInventorySourcesAsync(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotDoneInventorySourcesAsync(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);

                var concatTodoAndDoneStringList = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).OrderBy(y => y).ToList();
                var groupedInventorySources = concatTodoAndDoneStringList.GroupBy(x => x).ToList();
                foreach (var inventorySource in groupedInventorySources)
                {
                    int count = inventorySource.Count();
                    string name = inventorySource.Select(x => x).FirstOrDefault();
                    var result = new OutOfSpecSpotInventorySourcesDto
                    {
                        Name = name,
                        Count = count
                    };
                    inventorySources.Add(result);
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Inventory Sources V2");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Inventory Sources V2";
                throw new CadentException(msg, ex);
            }

            return inventorySources;
        }

        /// <inheritdoc />
        public async Task<List<OutOfSpecSpotReasonCodeResultsDto>> GetOutOfSpecSpotReasonCodesAsync(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest)
        {
            var outOfSpecReasonCodeResults = new List<OutOfSpecSpotReasonCodeResultsDto>();

            _LogInfo($"Starting: Retrieving Spot Exception Out Of Spec Spot Reason Codes V2");
            try
            {
                var outOfSpecToDoReasonCodes = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotToDoReasonCodesAsync(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);
                var outOfSpecDoneReasonCodes = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotDoneReasonCodesAsync(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);

                var combinedReasonCodeList = new List<OutOfSpecSpotReasonCodesDto>();

                combinedReasonCodeList.AddRange(outOfSpecToDoReasonCodes);
                combinedReasonCodeList.AddRange(outOfSpecDoneReasonCodes);

                var distinctReasonCodes = combinedReasonCodeList.Select(x => x.Reason).Distinct().ToList();

                foreach (var reasonCode in distinctReasonCodes)
                {
                    var reasonEntity = combinedReasonCodeList.Where(x => x.Reason == reasonCode).ToList();
                    int count = reasonEntity.Sum(x => x.Count);
                    var resonCodeEntity = reasonEntity.FirstOrDefault();
                    var result = new OutOfSpecSpotReasonCodeResultsDto
                    {
                        Id = resonCodeEntity.Id,
                        ReasonCode = resonCodeEntity.ReasonCode,
                        Description = resonCodeEntity.Reason,
                        Label = resonCodeEntity.Label,
                        Count = count
                    };
                    outOfSpecReasonCodeResults.Add(result);
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Spot Reason Codes V2");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Spot Reason Codes V2";
                throw new CadentException(msg, ex);
            }

            return outOfSpecReasonCodeResults;
        }

        /// <inheritdoc />
        public async Task<List<OutOfSpecSpotProgramsDto>> GetOutOfSpecSpotProgramsAsync(string programNameQuery, string userName)
        {
            SearchRequestProgramDto searchRequest = new SearchRequestProgramDto();
            searchRequest.ProgramName = programNameQuery;
            List<OutOfSpecSpotProgramsDto> programList;

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Spot Programs");
            try
            {
                programList = await _LoadProgramFromProgramsAsync(searchRequest);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Spot Programs";
                throw new CadentException(msg, ex);
            }

            _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Spot Programs");

            return programList;
        }

        /// <inheritdoc />
        public Guid GenerateOutOfSpecExportReport(OutOfSpecExportRequestDto request, string userName, DateTime currentDate, string templatesFilePath)
        {
            OutOfSpecExportReportData outOfSpecExportReportData = new OutOfSpecExportReportData();
            var reportGenerator = new OutOfSpecReportGenerator(templatesFilePath);
            _LogInfo($"Preparing to generate the file.  templatesFilePath='{templatesFilePath}'");
            outOfSpecExportReportData.ExportFileName = outOfSpecBuyerExportFileName;
            var report = reportGenerator.Generate(outOfSpecExportReportData);
            var folderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.OUT_OF_SPEC_EXPORT_REPORT);

            _LogInfo($"Saving generated file '{report.Filename}' to folder '{folderPath}'");
            var fileId = _SaveFile(report.Filename, report.Stream, userName);
            return fileId;

        }

        private string _GetAdvertiserName(Guid? masterId)
        {
            string advertiserName = null;
            if (masterId.HasValue)
            {
                advertiserName = _AabEngine.GetAdvertiser(masterId.Value)?.Name;
            }
            return advertiserName;
        }

        private int _GetTotalNumberOfWeeks(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new Exception("EndDate should be greater than StartDate");
            }
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1);
            var dateDifference = endDate - startDate;
            var totalDays = dateDifference.TotalDays;
            int numberOfWeeks = Convert.ToInt32(totalDays / 7);
            var reminder = totalDays % 7;
            numberOfWeeks = reminder > 0 ? numberOfWeeks + 1 : numberOfWeeks;
            return numberOfWeeks;
        }

        private async Task<List<OutOfSpecSpotProgramsDto>> _LoadProgramFromProgramsAsync(SearchRequestProgramDto searchRequest)
        {
            List<string> combinedProgramNames = new List<string>();
            var result = new List<OutOfSpecSpotProgramsDto>();

            try
            {
                var programs = await _SpotExceptionsOutOfSpecRepositoryV2.FindProgramFromProgramsAsync(searchRequest.ProgramName);
                var programsSpotExceptionDecisions = await _SpotExceptionsOutOfSpecRepositoryV2.FindProgramFromSpotExceptionDecisionsAsync(searchRequest.ProgramName);

                if (programsSpotExceptionDecisions.Any())
                {
                    programs = programs.Union(programsSpotExceptionDecisions).DistinctBy(x => x.OfficialProgramName).ToList();
                }

                _RemoveVariousAndUnmatchedFromPrograms(programs);

                combinedProgramNames = programs.Select(x => x.OfficialProgramName).ToList();
                foreach (var program in combinedProgramNames)
                {
                    var listOfProgramNames = programs.Where(x => x.OfficialProgramName.ToLower() == program.ToLower()).ToList();
                    OutOfSpecSpotProgramsDto spotExceptionsOutOfSpecProgram = new OutOfSpecSpotProgramsDto();
                    spotExceptionsOutOfSpecProgram.ProgramName = program;
                    foreach (var programName in listOfProgramNames)
                    {
                        var genre = programName.GenreId.HasValue ? _GenreCache.GetGenreLookupDtoById(programName.GenreId.Value).Display.ToUpper() : null;
                        var genres = HandleFlexGenres(genre);
                        spotExceptionsOutOfSpecProgram.Genres.AddRange(genres);
                    }
                    result.Add(spotExceptionsOutOfSpecProgram);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }
            return result;
        }

        /// <summary>
        /// Removes the various and unmatched from programs.
        /// </summary>
        /// <param name="result">The result.</param>
        private void _RemoveVariousAndUnmatchedFromPrograms(List<ProgramNameDto> result)
        {
            result.Where(x => x.GenreId.HasValue).ToList().RemoveAll(x => _GenreCache.GetGenreLookupDtoById(x.GenreId.Value).Display.Equals("Various", StringComparison.OrdinalIgnoreCase)
                    || x.OfficialProgramName.Equals("Unmatched", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Handles the flex genres.
        /// </summary>
        /// <param name="genre">The genre.</param>
        /// <returns></returns>
        internal static List<string> HandleFlexGenres(string genre)
        {
            const string flexGenreToken = "/";
            var genres = new List<string> { genre };

            if (genre != null && genre.Contains(flexGenreToken))
            {
                var split = genre.Split(flexGenreToken.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                genres.AddRange(split.Select(s => s.Trim()).ToList());
                genres = genres.OrderBy(s => s).ToList();
            }
            return genres;
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        private Guid _SaveFile(string fileName, Stream fileStream, string userName)
        {
            var folderPath = _GetExportFileSaveDirectory();           

            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = folderPath,
                FileNameWithExtension = fileName,
                FileMediaType = fileMediaType,
                FileUsage = SharedFolderFileUsage.InventoryExport,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = userName,
                FileContent = fileStream
            };
            var fileId = _SharedFolderService.SaveFile(sharedFolderFile);

            // Save to the File Service until the toggle is enabled and then we can remove it.
            if (!_EnableSharedFileServiceConsolidation.Value)
            {
                _FileService.CreateDirectory(folderPath);
                _FileService.Create(folderPath, fileName, fileStream);
            }

            return fileId;
        }

        /// <summary>
        /// Gets the export file save directory.
        /// </summary>
        /// <returns></returns>
        private string _GetExportFileSaveDirectory()
        {
            var path = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.OUT_OF_SPEC_EXPORT_REPORT);
            return path;
        }

        /// <summary>
        /// Gets the enable shared file service consolidation.
        /// </summary>
        /// <returns></returns>
        private bool _GetEnableSharedFileServiceConsolidation()
        {
            var result = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION);
            return result;
        }
        /// <inheritdoc />
        public async Task<bool> SaveOutOfSpecPlanDecsionsAsync(SaveOutOfSpecPlanDecisionsRequestDto saveOutOfSpecPlanDecisionsRequest, string userName)
        {
            bool isSpotsSaved = false;

            _LogInfo($"Starting:  Saving the Spot Exception Plan Decisions");
            try
            {
                var outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsToDoAsync(saveOutOfSpecPlanDecisionsRequest.PlanIds, saveOutOfSpecPlanDecisionsRequest.Filters.WeekStartDate, saveOutOfSpecPlanDecisionsRequest.Filters.WeekEndDate);

                if (saveOutOfSpecPlanDecisionsRequest.Filters.InventorySourceNames.Count > 0)
                {
                    outOfSpecSpotsToDo = outOfSpecSpotsToDo.Where(x => saveOutOfSpecPlanDecisionsRequest.Filters.InventorySourceNames.Contains(x.InventorySourceName)).ToList();
                }

                isSpotsSaved = await _SaveOutOfSpecDecisionsToDoAsync(outOfSpecSpotsToDo, userName);

            }
            catch (Exception ex)
            {
                var msg = $"Could not  Save the Spot Exception Plan Decisions";
                throw new CadentException(msg, ex);
            }

            return isSpotsSaved;
        }
        private async Task<bool> _SaveOutOfSpecDecisionsToDoAsync(List<SpotExceptionsOutOfSpecsToDoDto> existingOutOfSpecsToDo, string userName)
        {
            bool isSaved;
            var acceptAsInSpec = false;
            _LogInfo($"Starting: Moving the Spot Exception Plan by Decision to Done");
            try
            {
                var doneOutOfSpecsToAdd = existingOutOfSpecsToDo.Select(existingOutOfSpecToDo => new SpotExceptionsOutOfSpecsDoneDto
                    {
                        SpotUniqueHashExternal = existingOutOfSpecToDo.SpotUniqueHashExternal,
                        ExecutionIdExternal = existingOutOfSpecToDo.ExecutionIdExternal,
                        ReasonCodeMessage = existingOutOfSpecToDo.ReasonCodeMessage,
                        EstimateId = existingOutOfSpecToDo.EstimateId,
                        IsciName = existingOutOfSpecToDo.IsciName,
                        HouseIsci = existingOutOfSpecToDo.HouseIsci,
                        RecommendedPlanId = existingOutOfSpecToDo.RecommendedPlanId,
                        RecommendedPlanName = existingOutOfSpecToDo.RecommendedPlanName,
                        ProgramName = existingOutOfSpecToDo.ProgramName,
                        StationLegacyCallLetters = existingOutOfSpecToDo.StationLegacyCallLetters,
                        DaypartCode = existingOutOfSpecToDo.DaypartCode,
                        GenreName = existingOutOfSpecToDo.GenreName,
                        Affiliate = existingOutOfSpecToDo.Affiliate,
                        Market = existingOutOfSpecToDo.Market,
                        SpotLength = existingOutOfSpecToDo.SpotLength,
                        Audience = existingOutOfSpecToDo.Audience,
                        ProgramAirTime = existingOutOfSpecToDo.ProgramAirTime,
                        ProgramNetwork = existingOutOfSpecToDo.ProgramNetwork,
                        IngestedBy = existingOutOfSpecToDo.IngestedBy,
                        IngestedAt = existingOutOfSpecToDo.IngestedAt,
                        IngestedMediaWeekId = existingOutOfSpecToDo.IngestedMediaWeekId,
                        Impressions = existingOutOfSpecToDo.Impressions,
                        PlanId = existingOutOfSpecToDo.PlanId,
                        FlightStartDate = existingOutOfSpecToDo.FlightStartDate,
                        FlightEndDate = existingOutOfSpecToDo.FlightEndDate,
                        AdvertiserMasterId = existingOutOfSpecToDo.AdvertiserMasterId,
                        Product = existingOutOfSpecToDo.Product,
                        SpotExceptionsOutOfSpecReasonCode = existingOutOfSpecToDo.SpotExceptionsOutOfSpecReasonCode,
                        MarketCode = existingOutOfSpecToDo.MarketCode,
                        MarketRank = existingOutOfSpecToDo.MarketRank,
                        InventorySourceName = existingOutOfSpecToDo.InventorySourceName,
                        SpotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsDto()
                        {
                            AcceptedAsInSpec = acceptAsInSpec, 
                            DecisionNotes = "Out",
                            ProgramName = existingOutOfSpecToDo.ProgramName,
                            GenreName = existingOutOfSpecToDo.GenreName,
                            DaypartCode = existingOutOfSpecToDo.DaypartCode,
                            DecidedBy = userName,
                            DecidedAt = _DateTimeEngine.GetCurrentMoment()
                        }
                    }).ToList();

                using (var transaction = new TransactionScopeWrapper())
                {
                    if (doneOutOfSpecsToAdd.Any())
                    {
                        _SpotExceptionsOutOfSpecRepositoryV2.AddOutOfSpecToDone(doneOutOfSpecsToAdd);
                    }
                    _SpotExceptionsOutOfSpecRepositoryV2.DeleteOutOfSpecsFromToDo(existingOutOfSpecsToDo.Select(x => x.Id).ToList());
                    transaction.Complete();
                    isSaved = true;
                }

                _LogInfo($"Finished: Moving the Spot Exception Plan by Decision to Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not move Spot Exception Plan by Decision to Done";
                throw new CadentException(msg, ex);
            }
            return isSaved;
        }
    }
}
