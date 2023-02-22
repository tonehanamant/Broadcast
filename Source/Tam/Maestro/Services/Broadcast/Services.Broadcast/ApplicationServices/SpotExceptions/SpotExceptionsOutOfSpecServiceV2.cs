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
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecServiceV2 : IApplicationService
    {
        /// <summary>
        /// Gets the out of spec plans to do.
        /// </summary>
        /// <param name="outOfSpecPlansRequest">The out of spec plans request request.</param>
        /// <returns>List of to do plans</returns>
        List<OutOfSpecPlansResultDto> GetOutOfSpecPlansToDo(OutOfSpecPlansRequestDto outOfSpecPlansRequest);

        /// <summary>
        /// Gets the out of spec plans done.
        /// </summary>
        /// <param name="outOfSpecPlansRequest">The out of spec plans request request.</param>
        /// <returns>List of done plans</returns>
        List<OutOfSpecPlansResultDto> GetOutOfSpecPlansDone(OutOfSpecPlansRequestDto outOfSpecPlansRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec plan inventory sources.
        /// </summary>
        /// <param name="outOfSpecPlanInventorySourcesRequest">The spot exceptions out of spec plans request.</param>
        /// <returns></returns>
        List<string> GetOutOfSpecPlanInventorySources(OutOfSpecPlanInventorySourcesRequestDto outOfSpecPlanInventorySourcesRequest);

        /// <summary>
        /// Saves the out of spec plan acceptance.
        /// </summary>
        /// <param name="saveOutOfSpecPlanAcceptanceRequest">The save out of spec plan acceptance request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        bool SaveOutOfSpecPlanAcceptance(SaveOutOfSpecPlanAcceptanceRequestDto saveOutOfSpecPlanAcceptanceRequest, string userName);

        /// <summary>
        /// Gets the out of spec spot inventory sources.
        /// </summary>
        /// <param name="outOfSpecSpotsRequest">The out of spec spots request.</param>
        /// <returns></returns>
        List<OutOfSpecSpotInventorySourcesDto> GetOutOfSpecSpotInventorySources(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec spot reason codes.
        /// </summary>
        /// <param name="outOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        List<OutOfSpecSpotReasonCodeResultsDto> GetOutOfSpecSpotReasonCodes(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec programs.
        /// </summary>
        /// <param name="programNameQuery">The program name query.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        List<OutOfSpecSpotProgramsDto> GetOutOfSpecSpotPrograms(string programNameQuery, string userName);

        /// <summary>
        /// Saves the out of spec spot comment to do.
        /// </summary>
        /// <param name="saveOutOfSpecSpotCommentsRequest">The save out of spec spot comments request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        bool SaveOutOfSpecSpotCommentsToDo(SaveOutOfSpecSpotCommentsRequestDto saveOutOfSpecSpotCommentsRequest, string userName);

        /// <summary>
        /// Saves the out of spec spots bulk edit.
        /// </summary>
        /// <param name="saveOutOfSpecSpotBulkEditRequest">The save out of spec spot bulk edit request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        bool SaveOutOfSpecSpotsBulkEditDone(SaveOutOfSpecSpotBulkEditRequestDto saveOutOfSpecSpotBulkEditRequest, string userName);

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
        /// OutOfSpec Spot Bulk Edit.
        /// </summary>
        /// <param name="saveOutOfSpecSpotBulkEditRequest">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        bool SaveOutOfSpecSpotBulkEditToDo(SaveOutOfSpecSpotBulkEditRequestDto saveOutOfSpecSpotBulkEditRequest, string userName);
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
        public List<OutOfSpecPlansResultDto> GetOutOfSpecPlansToDo(OutOfSpecPlansRequestDto outOfSpecPlansRequest)
        {
            List<OutOfSpecPlansResultDto> outOfSpecPlansToDo = new List<OutOfSpecPlansResultDto>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Plans Todo");
            try
            {
                var outOfSpecToDo = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlansToDo(outOfSpecPlansRequest.InventorySourceNames, outOfSpecPlansRequest.WeekStartDate, outOfSpecPlansRequest.WeekEndDate);

                if (outOfSpecToDo?.Any() ?? false)
                {
                    outOfSpecPlansToDo = outOfSpecToDo.Select(x => new OutOfSpecPlansResultDto
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

                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Plans Todo");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Plans Todo";
                throw new CadentException(msg, ex);
            }

            return outOfSpecPlansToDo;
        }

        /// <inheritdoc />
        public List<OutOfSpecPlansResultDto> GetOutOfSpecPlansDone(OutOfSpecPlansRequestDto outOfSpecPlansRequest)
        {
            List<OutOfSpecPlansResultDto> outOfSpecPlansDone = new List<OutOfSpecPlansResultDto>();

            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Plans Done");
                var outOfSpecDone = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlansDone(outOfSpecPlansRequest.WeekStartDate, outOfSpecPlansRequest.WeekEndDate, outOfSpecPlansRequest.InventorySourceNames);

                if (outOfSpecDone?.Any() ?? false)
                {
                    outOfSpecPlansDone = outOfSpecDone.Select(x =>
                    {
                        return new OutOfSpecPlansResultDto
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

            return outOfSpecPlansDone;
        }

        /// <inheritdoc />
        public List<string> GetOutOfSpecPlanInventorySources(OutOfSpecPlanInventorySourcesRequestDto outOfSpecPlanInventorySourcesRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();
            List<string> inventorySources = new List<string>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Plan Inventory Sources");
            try
            {
                outOfSpecSpotsToDo = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlanToDoInventorySources(outOfSpecPlanInventorySourcesRequest.WeekStartDate, outOfSpecPlanInventorySourcesRequest.WeekEndDate);
                outOfSpecSpotsDone = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecPlanDoneInventorySources(outOfSpecPlanInventorySourcesRequest.WeekStartDate, outOfSpecPlanInventorySourcesRequest.WeekEndDate);

                inventorySources = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().OrderBy(inventorySource => inventorySource).ToList();
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Plan Inventory Sources");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Plan Inventory Sources";
                throw new CadentException(msg, ex);
            }

            return inventorySources;
        }

        /// <inheritdoc />
        public bool SaveOutOfSpecPlanAcceptance(SaveOutOfSpecPlanAcceptanceRequestDto saveOutOfSpecPlanAcceptanceRequest, string userName)
        {
            bool isSpotsSaved = false;
            List<OutOfSpecSpotsToDoDto> outOfSpecSpotsToDo = new List<OutOfSpecSpotsToDoDto>();

            _LogInfo($"Starting: Saving The Out Of Spec Plan Through Acceptance");
            try
            {
                saveOutOfSpecPlanAcceptanceRequest.PlanIds.ForEach(planId =>
                {
                    var outOfSpecSpotsToDoPerPlanId = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsToDo(planId, saveOutOfSpecPlanAcceptanceRequest.Filters.WeekStartDate, saveOutOfSpecPlanAcceptanceRequest.Filters.WeekEndDate);

                    outOfSpecSpotsToDo.AddRange(outOfSpecSpotsToDoPerPlanId);
                });

                if (saveOutOfSpecPlanAcceptanceRequest.Filters.InventorySourceNames != null && saveOutOfSpecPlanAcceptanceRequest.Filters.InventorySourceNames.Count > 0)
                {
                    outOfSpecSpotsToDo = outOfSpecSpotsToDo.Where(x => saveOutOfSpecPlanAcceptanceRequest.Filters.InventorySourceNames.Contains(x.InventorySourceName)).ToList();
                }

                isSpotsSaved = _SaveOutOfSpecPlanDecisionsToDo(outOfSpecSpotsToDo, userName);

            }
            catch (Exception ex)
            {
                var msg = $"Could Not Save The Out Of Spec Plan Through Acceptance";
                throw new CadentException(msg, ex);
            }

            _LogInfo($"Finished: Saving The Out Of Spec Plan Through Acceptance");
            return isSpotsSaved;
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotInventorySourcesDto> GetOutOfSpecSpotInventorySources(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();
            List<OutOfSpecSpotInventorySourcesDto> inventorySources = new List<OutOfSpecSpotInventorySourcesDto>();

            _LogInfo($"Starting: Retrieving Out Of Spec Spot Inventory Sources");
            try
            {
                outOfSpecSpotsToDo = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotToDoInventorySources(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotDoneInventorySources(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);

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
                _LogInfo($"Finished: Retrieving Out Of Spec Spot Inventory Sources");
            }
            catch (Exception ex)
            {
                var msg = $"Could Not retrieve Out Of Spec Spot Inventory Sources";
                throw new CadentException(msg, ex);
            }

            return inventorySources;
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotReasonCodeResultsDto> GetOutOfSpecSpotReasonCodes(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest)
        {
            var outOfSpecReasonCodeResults = new List<OutOfSpecSpotReasonCodeResultsDto>();

            _LogInfo($"Starting: Retrieving Spot Exception Out Of Spec Spot Reason Codes");
            try
            {
                var outOfSpecToDoReasonCodes = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotToDoReasonCodes(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);
                var outOfSpecDoneReasonCodes = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotDoneReasonCodes(outOfSpecSpotsRequest.PlanId, outOfSpecSpotsRequest.WeekStartDate, outOfSpecSpotsRequest.WeekEndDate);

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
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Spot Reason Codes";
                throw new CadentException(msg, ex);
            }

            _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Spot Reason Codes");
            return outOfSpecReasonCodeResults;
        }

        /// <inheritdoc />
        public List<OutOfSpecSpotProgramsDto> GetOutOfSpecSpotPrograms(string programNameQuery, string userName)
        {
            SearchRequestProgramDto searchRequest = new SearchRequestProgramDto();
            searchRequest.ProgramName = programNameQuery;
            List<OutOfSpecSpotProgramsDto> programList;

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Spot Programs");
            try
            {
                programList = _LoadProgramFromPrograms(searchRequest);
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
        public bool SaveOutOfSpecSpotCommentsToDo(SaveOutOfSpecSpotCommentsRequestDto saveOutOfSpecSpotCommentsRequest, string userName)
        {
            int commentsSavedCount;
            var addedAt = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Saving The Out Of Spec Spot Comments To Do");
            try
            {
                _LogInfo($"Starting: Retrieving Out Of Spec Spots To Add Comment To Do");
                var outOfSpecSpotsToDo = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsToDoByIds(saveOutOfSpecSpotCommentsRequest.SpotIds);
                _LogInfo($"Finished: Retrieving Out Of Spec Spots To Add Comment To Do. Retrieved '{outOfSpecSpotsToDo.Count}' Out Of Spec Spots");

                var outOfSpecSpotsToDoToAdd = outOfSpecSpotsToDo.Where(x => x.Comment == null).ToList();
                var outOfSpecSpotsToDoToUpdate = outOfSpecSpotsToDo.Where(x => x.Comment != null).ToList();

                int commentsAddedCount = 0;
                int commentsUpdatedCount = 0;

                if (outOfSpecSpotsToDoToAdd.Any())
                {
                    _LogInfo($"Starting: Adding New Out Of Spec Spot Comments To ToDo");
                    var outOfSpecCommentsToDoToAdd = outOfSpecSpotsToDoToAdd.Select(outOfSpecSpotsCommentsToDoToAdd => new OutOfSpecSpotCommentsDto
                    {
                        SpotUniqueHashExternal = outOfSpecSpotsCommentsToDoToAdd.SpotUniqueHashExternal,
                        ExecutionIdExternal = outOfSpecSpotsCommentsToDoToAdd.ExecutionIdExternal,
                        IsciName = outOfSpecSpotsCommentsToDoToAdd.IsciName,
                        ProgramAirTime = outOfSpecSpotsCommentsToDoToAdd.ProgramAirTime,
                        StationLegacyCallLetters = outOfSpecSpotsCommentsToDoToAdd.StationLegacyCallLetters,
                        ReasonCode = outOfSpecSpotsCommentsToDoToAdd.OutOfSpecSpotReasonCodes.Id,
                        RecommendedPlanId = outOfSpecSpotsCommentsToDoToAdd.RecommendedPlanId.Value,
                        Comment = saveOutOfSpecSpotCommentsRequest.Comment,
                        AddedBy = userName,
                        AddedAt = addedAt
                    }).ToList();

                    commentsAddedCount = _SpotExceptionsOutOfSpecRepositoryV2.AddOutOfSpecSpotCommentsToDo(outOfSpecCommentsToDoToAdd);
                    _LogInfo($"Finished: Adding New Out Of Spec Spot Comments To ToDo. Added '{commentsAddedCount}' Comments");
                }

                if (outOfSpecSpotsToDoToUpdate.Any())
                {
                    _LogInfo($"Starting: Updating New Out Of Spec Spot Comments To ToDo");
                    var outOfSpecCommentsToDoToUpdate = outOfSpecSpotsToDoToUpdate.Select(outOfSpecSpotsCommentsToDoToUpdate => new OutOfSpecSpotCommentsDto
                    {
                        SpotUniqueHashExternal = outOfSpecSpotsCommentsToDoToUpdate.SpotUniqueHashExternal,
                        ExecutionIdExternal = outOfSpecSpotsCommentsToDoToUpdate.ExecutionIdExternal,
                        IsciName = outOfSpecSpotsCommentsToDoToUpdate.IsciName,
                        ProgramAirTime = outOfSpecSpotsCommentsToDoToUpdate.ProgramAirTime,
                        StationLegacyCallLetters = outOfSpecSpotsCommentsToDoToUpdate.StationLegacyCallLetters,
                        ReasonCode = outOfSpecSpotsCommentsToDoToUpdate.OutOfSpecSpotReasonCodes.Id,
                        RecommendedPlanId = outOfSpecSpotsCommentsToDoToUpdate.RecommendedPlanId.Value,
                        Comment = saveOutOfSpecSpotCommentsRequest.Comment,
                        AddedBy = userName,
                        AddedAt = addedAt
                    }).ToList();

                    commentsUpdatedCount = _SpotExceptionsOutOfSpecRepositoryV2.UpdateOutOfSpecCommentsToDo(outOfSpecCommentsToDoToUpdate);
                    _LogInfo($"Finished: Updating New Out Of Spec Spot Comments To ToDo. Added '{commentsAddedCount}' Comments");
                }

                commentsSavedCount = commentsAddedCount + commentsUpdatedCount;
                _LogInfo($"Finished: Saving The Out Of Spec Spot Comments To Do. Saved '{commentsSavedCount}' Comments");
            }
            catch (Exception ex )
            {
                var msg = $"Could Not Save The Out Of Spec Spot Comments To Do";
                throw new CadentException(msg, ex);
            }

            return commentsSavedCount > 0;
        }

        /// <inheritdoc />
        public bool SaveOutOfSpecSpotsBulkEditDone(SaveOutOfSpecSpotBulkEditRequestDto saveOutOfSpecSpotBulkEditRequest, string userName)
        {
            var isSaved = false;

            _LogInfo($"Starting: Saving Decisions to Out of Spec");
            try
            {
                isSaved = _SaveOutOfSpecSpotDecisionsDone(saveOutOfSpecSpotBulkEditRequest, userName);

                if (!string.IsNullOrEmpty(saveOutOfSpecSpotBulkEditRequest.Decisions.Comments))
                {
                    isSaved = _SaveOutOfSpecSpotCommentsDone(saveOutOfSpecSpotBulkEditRequest, userName);
                }
                _LogInfo($"Finished: Saving Decisions to Out of Spec");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Decisions to Out of Spec";
                throw new CadentException(msg, ex);
            }

            return isSaved;
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

        /// <summary>
        /// Saves the out of spec Plan Decisions to do.
        /// </summary>
        /// <param name="outOfSpecSpotsToDo">The existing out of specs to do.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        private bool _SaveOutOfSpecPlanDecisionsToDo(List<OutOfSpecSpotsToDoDto> outOfSpecSpotsToDo, string userName)
        {
            bool isSaved;
            var acceptAsInSpec = false;

            _LogInfo($"Starting: Saving the Out Of Spec Plan Decisions to Done");
            try
            {
                var doneOutOfSpecsToAdd = outOfSpecSpotsToDo.Select(OutOfSpecSpotDecisionsToAdd => new OutOfSpecSpotsDoneDto
                {
                    SpotUniqueHashExternal = OutOfSpecSpotDecisionsToAdd.SpotUniqueHashExternal,
                    ExecutionIdExternal = OutOfSpecSpotDecisionsToAdd.ExecutionIdExternal,
                    ReasonCodeMessage = OutOfSpecSpotDecisionsToAdd.ReasonCodeMessage,
                    EstimateId = OutOfSpecSpotDecisionsToAdd.EstimateId,
                    IsciName = OutOfSpecSpotDecisionsToAdd.IsciName,
                    HouseIsci = OutOfSpecSpotDecisionsToAdd.HouseIsci,
                    RecommendedPlanId = OutOfSpecSpotDecisionsToAdd.RecommendedPlanId,
                    RecommendedPlanName = OutOfSpecSpotDecisionsToAdd.RecommendedPlanName,
                    ProgramName = OutOfSpecSpotDecisionsToAdd.ProgramName,
                    StationLegacyCallLetters = OutOfSpecSpotDecisionsToAdd.StationLegacyCallLetters,
                    DaypartCode = OutOfSpecSpotDecisionsToAdd.DaypartCode,
                    GenreName = OutOfSpecSpotDecisionsToAdd.GenreName,
                    Affiliate = OutOfSpecSpotDecisionsToAdd.Affiliate,
                    Market = OutOfSpecSpotDecisionsToAdd.Market,
                    SpotLength = OutOfSpecSpotDecisionsToAdd.SpotLength,
                    Audience = OutOfSpecSpotDecisionsToAdd.Audience,
                    ProgramAirTime = OutOfSpecSpotDecisionsToAdd.ProgramAirTime,
                    ProgramNetwork = OutOfSpecSpotDecisionsToAdd.ProgramNetwork,
                    IngestedBy = OutOfSpecSpotDecisionsToAdd.IngestedBy,
                    IngestedAt = OutOfSpecSpotDecisionsToAdd.IngestedAt,
                    IngestedMediaWeekId = OutOfSpecSpotDecisionsToAdd.IngestedMediaWeekId,
                    Impressions = OutOfSpecSpotDecisionsToAdd.Impressions,
                    PlanId = OutOfSpecSpotDecisionsToAdd.PlanId,
                    FlightStartDate = OutOfSpecSpotDecisionsToAdd.FlightStartDate,
                    FlightEndDate = OutOfSpecSpotDecisionsToAdd.FlightEndDate,
                    AdvertiserMasterId = OutOfSpecSpotDecisionsToAdd.AdvertiserMasterId,
                    Product = OutOfSpecSpotDecisionsToAdd.Product,
                    OutOfSpecSpotReasonCodes = OutOfSpecSpotDecisionsToAdd.OutOfSpecSpotReasonCodes,
                    MarketCode = OutOfSpecSpotDecisionsToAdd.MarketCode,
                    MarketRank = OutOfSpecSpotDecisionsToAdd.MarketRank,
                    InventorySourceName = OutOfSpecSpotDecisionsToAdd.InventorySourceName,
                    OutOfSpecSpotDoneDecisions = new OutOfSpecSpotDoneDecisionsDto()
                    {
                        AcceptedAsInSpec = acceptAsInSpec,
                        DecisionNotes = "Out",
                        ProgramName = OutOfSpecSpotDecisionsToAdd.ProgramName,
                        GenreName = OutOfSpecSpotDecisionsToAdd.GenreName,
                        DaypartCode = OutOfSpecSpotDecisionsToAdd.DaypartCode,
                        DecidedBy = userName,
                        DecidedAt = _DateTimeEngine.GetCurrentMoment()
                    }
                }).ToList();

                using (var transaction = new TransactionScopeWrapper())
                {
                    if (doneOutOfSpecsToAdd.Any())
                    {
                        _SpotExceptionsOutOfSpecRepositoryV2.AddOutOfSpecsToDone(doneOutOfSpecsToAdd);
                    }
                    _SpotExceptionsOutOfSpecRepositoryV2.DeleteOutOfSpecsFromToDo(outOfSpecSpotsToDo.Select(x => x.Id).ToList());
                    transaction.Complete();
                }

                isSaved = true;
                _LogInfo($"Finished: Saving The Out Of Spec Plan Decisions To Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could Not Save The Out Of Spec Plan Decisions To Done";
                throw new CadentException(msg, ex);
            }
            return isSaved;
        }

        /// <summary>
        /// Saves the out of spec Spot Decisions to do.
        /// </summary>
        /// <param name="outOfSpecSpotsToDo">The existing out of specs to do.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="acceptAsInSpecForBulkEdit">bool parameter for Bulk Edit.</param>
        /// <returns></returns>
        private bool _SaveOutOfSpecSpotDecisionsToDo(List<OutOfSpecSpotsToDoDto> outOfSpecSpotsToDo, string userName, bool acceptAsInSpecForBulkEdit)
        {
            bool isSaved;
            var acceptAsInSpec = acceptAsInSpecForBulkEdit;

            _LogInfo($"Starting: Saving the Out Of Spec Spot Decisions to Done");
            try
            {
                var outOfSpecDoComments = outOfSpecSpotsToDo.Select(outOfSpecSpotCommentToAdd => new OutOfSpecSpotCommentsDto
                {
                    SpotUniqueHashExternal = outOfSpecSpotCommentToAdd.SpotUniqueHashExternal,
                    ExecutionIdExternal = outOfSpecSpotCommentToAdd.ExecutionIdExternal,
                    IsciName = outOfSpecSpotCommentToAdd.IsciName,
                    RecommendedPlanId = outOfSpecSpotCommentToAdd.RecommendedPlanId.Value,
                    StationLegacyCallLetters = outOfSpecSpotCommentToAdd.StationLegacyCallLetters,
                    ProgramAirTime = outOfSpecSpotCommentToAdd.ProgramAirTime,
                    ReasonCode = outOfSpecSpotCommentToAdd.OutOfSpecSpotReasonCodes.Id,
                    Comment = outOfSpecSpotCommentToAdd.Comment
                }).ToList();

                var doneOutOfSpecsToAdd = outOfSpecSpotsToDo.Select(OutOfSpecSpotDecisionsToAdd => new OutOfSpecSpotsDoneDto
                {
                    SpotUniqueHashExternal = OutOfSpecSpotDecisionsToAdd.SpotUniqueHashExternal,
                    ExecutionIdExternal = OutOfSpecSpotDecisionsToAdd.ExecutionIdExternal,
                    ReasonCodeMessage = OutOfSpecSpotDecisionsToAdd.ReasonCodeMessage,
                    EstimateId = OutOfSpecSpotDecisionsToAdd.EstimateId,
                    IsciName = OutOfSpecSpotDecisionsToAdd.IsciName,
                    HouseIsci = OutOfSpecSpotDecisionsToAdd.HouseIsci,
                    RecommendedPlanId = OutOfSpecSpotDecisionsToAdd.RecommendedPlanId,
                    RecommendedPlanName = OutOfSpecSpotDecisionsToAdd.RecommendedPlanName,
                    ProgramName = OutOfSpecSpotDecisionsToAdd.ProgramName,
                    StationLegacyCallLetters = OutOfSpecSpotDecisionsToAdd.StationLegacyCallLetters,
                    DaypartCode = OutOfSpecSpotDecisionsToAdd.DaypartCode,
                    GenreName = OutOfSpecSpotDecisionsToAdd.GenreName,
                    Affiliate = OutOfSpecSpotDecisionsToAdd.Affiliate,
                    Market = OutOfSpecSpotDecisionsToAdd.Market,
                    SpotLength = OutOfSpecSpotDecisionsToAdd.SpotLength,
                    Audience = OutOfSpecSpotDecisionsToAdd.Audience,
                    ProgramAirTime = OutOfSpecSpotDecisionsToAdd.ProgramAirTime,
                    ProgramNetwork = OutOfSpecSpotDecisionsToAdd.ProgramNetwork,
                    IngestedBy = OutOfSpecSpotDecisionsToAdd.IngestedBy,
                    IngestedAt = OutOfSpecSpotDecisionsToAdd.IngestedAt,
                    IngestedMediaWeekId = OutOfSpecSpotDecisionsToAdd.IngestedMediaWeekId,
                    Impressions = OutOfSpecSpotDecisionsToAdd.Impressions,
                    PlanId = OutOfSpecSpotDecisionsToAdd.PlanId,
                    FlightStartDate = OutOfSpecSpotDecisionsToAdd.FlightStartDate,
                    FlightEndDate = OutOfSpecSpotDecisionsToAdd.FlightEndDate,
                    AdvertiserMasterId = OutOfSpecSpotDecisionsToAdd.AdvertiserMasterId,
                    Product = OutOfSpecSpotDecisionsToAdd.Product,
                    OutOfSpecSpotReasonCodes = OutOfSpecSpotDecisionsToAdd.OutOfSpecSpotReasonCodes,
                    MarketCode = OutOfSpecSpotDecisionsToAdd.MarketCode,
                    MarketRank = OutOfSpecSpotDecisionsToAdd.MarketRank,
                    InventorySourceName = OutOfSpecSpotDecisionsToAdd.InventorySourceName,
                    OutOfSpecSpotDoneDecisions = new OutOfSpecSpotDoneDecisionsDto()
                    {
                        AcceptedAsInSpec = acceptAsInSpec,
                        DecisionNotes = acceptAsInSpec ? "In" : "Out",
                        ProgramName = OutOfSpecSpotDecisionsToAdd.ProgramName,
                        GenreName = OutOfSpecSpotDecisionsToAdd.GenreName,
                        DaypartCode = OutOfSpecSpotDecisionsToAdd.DaypartCode,
                        DecidedBy = userName,
                        DecidedAt = _DateTimeEngine.GetCurrentMoment()
                    }
                }).ToList();

                using (var transaction = new TransactionScopeWrapper())
                {
                    if (doneOutOfSpecsToAdd.Any())
                    {
                        _SpotExceptionsOutOfSpecRepositoryV2.AddOutOfSpecsToDone(doneOutOfSpecsToAdd);
                    }
                    _SpotExceptionsOutOfSpecRepositoryV2.DeleteOutOfSpecsFromToDo(outOfSpecSpotsToDo.Select(x => x.Id).ToList());
                    _SpotExceptionsOutOfSpecRepositoryV2.SaveOutOfSpecSpotComments(outOfSpecDoComments, userName, DateTime.Now);
                    transaction.Complete();
                }

                isSaved = true;
                _LogInfo($"Finished: Saving The Out Of Spec Spot Decisions To Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could Not Save The Out Of Spec Spot Decisions To Done";
                throw new CadentException(msg, ex);
            }
            return isSaved;
        }

        /// <summary>
        /// Gets the name of the advertiser.
        /// </summary>
        /// <param name="masterId">The master identifier.</param>
        /// <returns></returns>
        private string _GetAdvertiserName(Guid? masterId)
        {
            string advertiserName = null;
            if (masterId.HasValue)
            {
                advertiserName = _AabEngine.GetAdvertiser(masterId.Value)?.Name;
            }
            return advertiserName;
        }

        /// <summary>
        /// Gets the total number of weeks.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">EndDate should be greater than StartDate</exception>
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

        /// <summary>
        /// Loads the program from programs.
        /// </summary>
        /// <param name="searchRequest">The search request.</param>
        /// <returns></returns>
        private List<OutOfSpecSpotProgramsDto> _LoadProgramFromPrograms(SearchRequestProgramDto searchRequest)
        {
            List<string> combinedProgramNames = new List<string>();
            var result = new List<OutOfSpecSpotProgramsDto>();

            try
            {
                var programs = _SpotExceptionsOutOfSpecRepositoryV2.FindProgramFromPrograms(searchRequest.ProgramName);
                var programsSpotExceptionDecisions = _SpotExceptionsOutOfSpecRepositoryV2.FindProgramFromSpotExceptionDecisions(searchRequest.ProgramName);

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

        private bool _SaveOutOfSpecSpotDecisionsDone(SaveOutOfSpecSpotBulkEditRequestDto outOfSpecBulkEditRequest, string userName)
        {
            bool isOutOfSpecSpotDoneDecisionSaved;
            var outOfSpecSpotsDone = new List<OutOfSpecSpotDoneDecisionsDto>();
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Saving decisions for the Done");
            try
            {
                foreach (var outOfSpecBulkEdit in outOfSpecBulkEditRequest.SpotIds)
                {
                    var outOfSpecSpotDoneDecision = new OutOfSpecSpotDoneDecisionsDto
                    {
                        Id = outOfSpecBulkEdit,
                        AcceptedAsInSpec = outOfSpecBulkEditRequest.Decisions.AcceptAsInSpec,
                        ProgramName = outOfSpecBulkEditRequest.Decisions.ProgramName?.ToUpper(),
                        GenreName = outOfSpecBulkEditRequest.Decisions.GenreName?.ToUpper(),
                        DaypartCode = outOfSpecBulkEditRequest.Decisions.DaypartCode
                    };
                    outOfSpecSpotsDone.Add(outOfSpecSpotDoneDecision);
                }

                isOutOfSpecSpotDoneDecisionSaved = _SpotExceptionsOutOfSpecRepositoryV2.SaveOutOfSpecSpotDoneDecisions(outOfSpecSpotsDone, userName, currentDate);
            }
            catch (Exception ex)
            {
                var msg = $"Could not save decisions for the Done";
                throw new CadentException(msg, ex);
            }

            _LogInfo($"Starting: Saving decisions for the Done");
            return isOutOfSpecSpotDoneDecisionSaved;
        }

        private  bool _SaveOutOfSpecSpotCommentsDone(SaveOutOfSpecSpotBulkEditRequestDto saveOutOfSpecSpotBulkEditRequest, string userName)
        {
            bool isCommentSaved;

            _LogInfo($"Starting: Saving Comments to Out of Spec Done");
            try
            {
                var existingOutOfSpecsDone =  _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsDoneByIds(saveOutOfSpecSpotBulkEditRequest.SpotIds);

                var outOfSpecSpotDoneComments = existingOutOfSpecsDone.Select(doneOutOfSpecToAdd => new OutOfSpecSpotCommentsDto
                {
                    SpotUniqueHashExternal = doneOutOfSpecToAdd.SpotUniqueHashExternal,
                    ExecutionIdExternal = doneOutOfSpecToAdd.ExecutionIdExternal,
                    IsciName = doneOutOfSpecToAdd.IsciName,
                    RecommendedPlanId = doneOutOfSpecToAdd.RecommendedPlanId.Value,
                    StationLegacyCallLetters = doneOutOfSpecToAdd.StationLegacyCallLetters,
                    ProgramAirTime = doneOutOfSpecToAdd.ProgramAirTime,
                    ReasonCode = doneOutOfSpecToAdd.OutOfSpecSpotReasonCodes.Id,
                    Comment = saveOutOfSpecSpotBulkEditRequest.Decisions.Comments
                }).ToList();

                isCommentSaved =  _SpotExceptionsOutOfSpecRepositoryV2.SaveOutOfSpecSpotComments(outOfSpecSpotDoneComments, userName, DateTime.Now);
                _LogInfo($"Finished: Saving Comments to Out of Spec Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Comments to Out of Spec Done";
                throw new CadentException(msg, ex);
            }

            return isCommentSaved;
        }
        /// <inheritdoc />
        public bool SaveOutOfSpecSpotBulkEditToDo(SaveOutOfSpecSpotBulkEditRequestDto saveOutOfSpecSpotBulkEditRequest, string userName)
        {
            var isSaved = false;

            _LogInfo($"Starting: Bulk Saving Decisions For Out of Spec Spot ToDo");
            try
            {
                var outOfSpecSpotsToDo = _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsToDoByIds(saveOutOfSpecSpotBulkEditRequest.SpotIds.ToList());
                outOfSpecSpotsToDo.ForEach(x => {
                    x.Comment = string.IsNullOrEmpty(saveOutOfSpecSpotBulkEditRequest.Decisions.Comments) ? x.Comment : saveOutOfSpecSpotBulkEditRequest.Decisions.Comments;
                    x.ProgramName = saveOutOfSpecSpotBulkEditRequest.Decisions.ProgramName;
                    x.DaypartCode = saveOutOfSpecSpotBulkEditRequest.Decisions.DaypartCode;
                    x.GenreName = saveOutOfSpecSpotBulkEditRequest.Decisions.GenreName;
                });
                isSaved = _SaveOutOfSpecSpotDecisionsToDo(outOfSpecSpotsToDo, userName, saveOutOfSpecSpotBulkEditRequest.Decisions.AcceptAsInSpec);

                _LogInfo($"Finished: Bulk Saving Decisions For Out of Spec Spot ToDo");
            }
            catch (Exception ex)
            {
                var msg = $"Could not Bulk Saving Decisions For Out of Spec Spot ToDo";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }
    }
}
