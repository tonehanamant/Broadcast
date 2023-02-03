using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
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

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecServiceV2 : IApplicationService
    {
        /// <summary>
        /// Get the list of reason codes
        /// </summary>
        Task<List<SpotExceptionsOutOfSpecReasonCodeResultDtoV2>> GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(
SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        /// <summary>
        /// Gets the inventory Sources and the Count
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">plan id , start date ,end date</param>
        /// <returns>count and inventory source List</returns>
        Task<List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2>> GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        /// <summary>
        /// Generats out of spec report.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="currentDate">The current date.</param>
        /// <param name="templatesFilePath">The templates file path.</param>
        /// <returns></returns>
        Guid GenerateOutOfSpecExportReport(OutOfSpecExportRequestDto request, string userName, DateTime currentDate, string templatesFilePath);


        }
    public class SpotExceptionsOutOfSpecServiceV2 : BroadcastBaseClass, ISpotExceptionsOutOfSpecServiceV2
    {
        private readonly ISpotExceptionsOutOfSpecRepositoryV2 _SpotExceptionsOutOfSpecRepositoryV2;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IFileService _FileService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly Lazy<bool> _EnableSharedFileServiceConsolidation;
        const string fileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        const string outOfSpecBuyerExportFileName= "Template - Out of Spec Report Buying Team.xlsx";
        public SpotExceptionsOutOfSpecServiceV2(
          IDataRepositoryFactory dataRepositoryFactory,
          IFeatureToggleHelper featureToggleHelper,
          IDateTimeEngine dateTime,
          IFileService fileService,
          ISharedFolderService sharedFolderService,
          IConfigurationSettingsHelper configurationSettingsHelper)
          : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsOutOfSpecRepositoryV2 = dataRepositoryFactory.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>();
            _DateTimeEngine = dateTime;
            _FileService = fileService;
            _SharedFolderService = sharedFolderService;
            _EnableSharedFileServiceConsolidation = new Lazy<bool>(_GetEnableSharedFileServiceConsolidation);
        }
        /// <inheritdoc />
        public async Task<List<SpotExceptionsOutOfSpecReasonCodeResultDtoV2>> GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(
SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var spotExceptionsOutOfSpecReasonCodeResults = new List<SpotExceptionsOutOfSpecReasonCodeResultDtoV2>();

            _LogInfo($"Starting: Retrieving Spot Exception Out Of Spec Spot Reason Codes V2");
            try
            {
                var spotExceptionsOutOfSpecToDoReasonCodes = await _SpotExceptionsOutOfSpecRepositoryV2.GetSpotExceptionsOutOfSpecToDoReasonCodesV2(spotExceptionsOutOfSpecSpotsRequest.PlanId,
                    spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
                var spotExceptionsOutOfSpecDoneReasonCodes = await _SpotExceptionsOutOfSpecRepositoryV2.GetSpotExceptionsOutOfSpecDoneReasonCodesV2(spotExceptionsOutOfSpecSpotsRequest.PlanId,
                   spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                var combinedReasonCodeList = new List<SpotExceptionsOutOfSpecReasonCodeDtoV2>();

                combinedReasonCodeList.AddRange(spotExceptionsOutOfSpecToDoReasonCodes);
                combinedReasonCodeList.AddRange(spotExceptionsOutOfSpecDoneReasonCodes);

                var distinctReasonCodes = combinedReasonCodeList.Select(x=> x.Reason).Distinct().ToList();


                foreach (var reasonCode in distinctReasonCodes)
                {
                    var reasonEntity = combinedReasonCodeList.Where(x => x.Reason == reasonCode).ToList();
                    int count = reasonEntity.Sum(x=> x.Count);
                    var resonCodeEntity = reasonEntity.FirstOrDefault();
                    var result = new SpotExceptionsOutOfSpecReasonCodeResultDtoV2
                    {
                        Id = resonCodeEntity.Id,
                        ReasonCode = resonCodeEntity.ReasonCode,
                        Description = resonCodeEntity.Reason,
                        Label = resonCodeEntity.Label,
                        Count = count
                    };
                    spotExceptionsOutOfSpecReasonCodeResults.Add(result);
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Spot Reason Codes V2");
                return spotExceptionsOutOfSpecReasonCodeResults;
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Spot Reason Codes V2";
                throw new CadentException(msg, ex);
            }

            return spotExceptionsOutOfSpecReasonCodeResults;
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2>> GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();
            List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2> inventorySources = new List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Spot Inventory Sources V2");
            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsToDoInventorySourcesV2Async(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsDoneInventorySourcesV2Async(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                var concatTodoAndDoneStringList = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).OrderBy(y => y).ToList();
                var groupedInventorySources = concatTodoAndDoneStringList.GroupBy(x => x).ToList();
                foreach (var inventorySource in groupedInventorySources)
                {
                    int count = inventorySource.Count();
                    string name = inventorySource.Select(x => x).FirstOrDefault();
                    var result = new SpotExceptionOutOfSpecSpotInventorySourcesDtoV2
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
        private string _GetExportFileSaveDirectory()
        {
            var path = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.OUT_OF_SPEC_EXPORT_REPORT);
            return path;
        }
        private bool _GetEnableSharedFileServiceConsolidation()
        {
            var result = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION);
            return result;
        }
    }
}
