using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.BarterInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IBarterInventoryService : IApplicationService
    {
        /// <summary>
        /// Saves a barter inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a barter inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveBarterInventoryFile(InventoryFileSaveRequest request, string userName, DateTime now);
    }

    public class BarterInventoryService : IBarterInventoryService
    {
        private const string INVENTORY_SOURCE_CELL = "B3";

        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IBarterRepository _BarterRepository;
        private readonly IBarterFileImporterFactory _BarterFileImporterFactory;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IStationRepository _StationRepository;
        private readonly ILockingEngine _LockingEngine;
        private readonly IInventoryDaypartParsingEngine _DaypartParsingEngine;
        private readonly IStationInventoryGroupService _StationInventoryGroupService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IDataLakeFileService _DataLakeFileService;
        private readonly IInventoryRatingsProcessingService _InventoryRatingsService;

        public BarterInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IBarterFileImporterFactory barterFileImporterFactory
            , IStationProcessingEngine stationProcessingEngine
            , ILockingEngine lockingEngine
            , IInventoryDaypartParsingEngine inventoryDaypartParsingEngine
            , IStationInventoryGroupService stationInventoryGroupService
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IDataLakeFileService dataLakeFileService
            , IInventoryRatingsProcessingService inventoryRatingsService)
        {
            _BarterRepository = broadcastDataRepositoryFactory.GetDataRepository<IBarterRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _BarterFileImporterFactory = barterFileImporterFactory;
            _StationProcessingEngine = stationProcessingEngine;
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _LockingEngine = lockingEngine;
            _DaypartParsingEngine = inventoryDaypartParsingEngine;
            _StationInventoryGroupService = stationInventoryGroupService;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _DataLakeFileService = dataLakeFileService;
            _InventoryRatingsService = inventoryRatingsService;
        }

        /// <summary>
        /// Saves a barter inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a barter inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        public InventoryFileSaveResult SaveBarterInventoryFile(InventoryFileSaveRequest request, string userName, DateTime now)
        {
            if (!request.FileName.EndsWith(".xlsx"))
            {
                return new InventoryFileSaveResult
                {
                    Status = FileStatusEnum.Failed,
                    ValidationProblems = new List<string> { "Invalid file format. Please, provide a .xlsx File" }
                };
            }

            var stationLocks = new List<IDisposable>();
            var lockedStationIds = new List<int>();
            var inventorySource = _ReadInventorySourceFromFile(request.StreamData);
            var fileImporter = _BarterFileImporterFactory.GetFileImporterInstance(inventorySource);

            fileImporter.LoadFromSaveRequest(request);
            fileImporter.CheckFileHash();

            BarterInventoryFile barterFile = fileImporter.GetPendingBarterInventoryFile(userName, inventorySource);

            _CheckValidationProblems(barterFile);

            barterFile.Id = _InventoryFileRepository.CreateInventoryFile(barterFile, userName);
            try
            {
                fileImporter.ExtractData(barterFile);
                barterFile.FileStatus = barterFile.ValidationProblems.Any() ? FileStatusEnum.Failed : FileStatusEnum.Loaded;

                if (barterFile.ValidationProblems.Any())
                {
                    _BarterRepository.AddValidationProblems(barterFile);
                }
                else
                {
                    using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                    {
                        var header = barterFile.Header;
                        var stations = _GetFileStationsOrCreate(barterFile, userName);
                        var stationsDict = stations.ToDictionary(x => x.Id, x => x.LegacyCallLetters);

                        fileImporter.PopulateManifests(barterFile, stations);

                        _LockingEngine.LockStations(stationsDict, lockedStationIds, stationLocks);

                        var manifests = barterFile.InventoryGroups.SelectMany(x => x.Manifests);

                        //The following two steps are processed as a separate background job. See below.
                        //_ImpressionsService.GetProjectedStationImpressions(manifests, header.PlaybackType, header.ShareBookId, header.HutBookId);
                        //_ProprietarySpotCostCalculationEngine.CalculateSpotCost(manifests);

                        _StationInventoryGroupService.AddNewStationInventory(barterFile, header.EffectiveDate, header.EndDate, header.ContractedDaypartId);

                        _StationRepository.UpdateStationList(stationsDict.Keys.ToList(), userName, now, barterFile.InventorySource.Id);

                        _BarterRepository.SaveBarterInventoryFile(barterFile);
                        transaction.Complete();

                        _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                    }
                }
            }
            catch (Exception ex)
            {
                _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                barterFile.ValidationProblems.Add(ex.Message);
                barterFile.FileStatus = FileStatusEnum.Failed;
                _BarterRepository.AddValidationProblems(barterFile);
            }

            _CheckValidationProblems(barterFile);

            _InventoryRatingsService.QueueInventoryFileRatingsJob(barterFile.Id);

            try
            {
                _DataLakeFileService.Save(request);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to send file to Data Lake shared folder and e-mail reporting the error:" + ex);
            }

            return new InventoryFileSaveResult
            {
                FileId = barterFile.Id,
                ValidationProblems = barterFile.ValidationProblems,
                Status = barterFile.FileStatus
            };
        }

        private InventorySource _ReadInventorySourceFromFile(Stream streamData)
        {
            const int searchInventorySourceHeaderCellRowIndexStart = 2;
            const int searchInventorySourceHeaderCellRowIndexEnd = 3;
            const int inventorySourceHeaderCellColumnIndex = 1;
            var inventorySourceHeaderCellTexts = new List<string> { "Inventory Source", "Inv Source" };

            InventorySource inventorySource = null;

            using (var package = new ExcelPackage(streamData))
            {
                var worksheet = package.Workbook.Worksheets.First();

                for (var i = searchInventorySourceHeaderCellRowIndexStart; i <= searchInventorySourceHeaderCellRowIndexEnd; i++)
                {
                    var inventorySourceHeaderCell = worksheet.Cells[i, inventorySourceHeaderCellColumnIndex].GetStringValue();
                    var isInventorySourceHeaderCell = !string.IsNullOrWhiteSpace(inventorySourceHeaderCell) &&
                        inventorySourceHeaderCellTexts.Any(x => inventorySourceHeaderCell.Equals(x, StringComparison.OrdinalIgnoreCase));

                    if (isInventorySourceHeaderCell)
                    {
                        // inventory source value should be next after the header cell
                        var inventorySourceString = worksheet.Cells[i, inventorySourceHeaderCellColumnIndex + 1].GetStringValue();
                        inventorySource = _InventoryRepository.GetInventorySourceByName(inventorySourceString);
                        break;
                    }
                }   
            }

            if (inventorySource == null)
            {
                throw new FileUploadException<InventoryFileProblem>(new List<InventoryFileProblem>
                {
                    new InventoryFileProblem("Could not find inventory source")
                });
            }

            return inventorySource;
        }

        private static void _CheckValidationProblems(BarterInventoryFile barterFile)
        {
            if (barterFile.ValidationProblems.Any())
            {
                var fileProblems = barterFile.ValidationProblems.Select(x => new InventoryFileProblem(x)).ToList();
                throw new FileUploadException<InventoryFileProblem>(fileProblems);
            }
        }
        
        private List<DisplayBroadcastStation> _GetFileStationsOrCreate(BarterInventoryFile barterFile, string userName)
        {
            var now = DateTime.Now;
            var allStationNames = barterFile.DataLines.Select(x => x.Station).Distinct();
            var allLegacyStationNames = allStationNames.Select(_StationProcessingEngine.StripStationSuffix).Distinct().ToList();
            var existingStations = _StationRepository.GetBroadcastStationListByLegacyCallLetters(allLegacyStationNames);
            var existingStationNames = existingStations.Select(x => x.LegacyCallLetters);
            var notExistingStations = allLegacyStationNames.Except(existingStationNames, StringComparer.OrdinalIgnoreCase).ToList();
            var stationsToCreate = new List<DisplayBroadcastStation>();

            foreach (var stationName in allStationNames)
            {
                var legacyStationName = _StationProcessingEngine.StripStationSuffix(stationName);

                if (notExistingStations.Contains(legacyStationName))
                {
                    stationsToCreate.Add(new DisplayBroadcastStation
                    {
                        CallLetters = stationName,
                        LegacyCallLetters = legacyStationName,
                        ModifiedDate = now
                    });
                }
            }

            var newStations = _StationRepository.CreateStations(stationsToCreate, userName);
            existingStations.AddRange(newStations);

            return existingStations;
        }
    }
}
