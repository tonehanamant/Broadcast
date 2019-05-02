using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProprietaryInventoryService : IApplicationService
    {
        /// <summary>
        /// Saves a proprietary inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a proprietary inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveProprietaryInventoryFile(FileRequest request, string userName, DateTime now);

        /// <summary>
        /// Generates one SCX archive for the current quarter
        /// </summary>
        /// <returns>Returnsa zip archive as stream and the zip name</returns>
        Tuple<string, Stream> GenerateScxFileArchive(DateTime nowDate);
    }

    public class ProprietaryInventoryService : IProprietaryInventoryService
    {
        private const string INVENTORY_SOURCE_CELL = "B3";

        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryFileRepository _InventoryFileRepository;
        private readonly IProprietaryRepository _ProprietaryRepository;
        private readonly IProprietaryFileImporterFactory _ProprietaryFileImporterFactory;
        private readonly IStationProcessingEngine _StationProcessingEngine;
        private readonly IStationRepository _StationRepository;
        private readonly ILockingEngine _LockingEngine;
        private readonly IInventoryDaypartParsingEngine _DaypartParsingEngine;
        private readonly IProprietarySpotCostCalculationEngine _ProprietarySpotCostCalculationEngine;
        private readonly IStationInventoryGroupService _StationInventoryGroupService;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekCache;
        private readonly IInventoryScxDataConverter _InventoryScxDataConverter;
        private readonly IDataLakeFileService _DataLakeFileService;
        private readonly IInventoryScxDataPrep _InventoryScxDataPrep;
        private readonly IInventoryRatingsProcessingService _InventoryRatingsService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

        public ProprietaryInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IProprietaryFileImporterFactory proprietaryFileImporterFactory
            , IStationProcessingEngine stationProcessingEngine
            , ILockingEngine lockingEngine
            , IInventoryDaypartParsingEngine inventoryDaypartParsingEngine
            , IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine
            , IStationInventoryGroupService stationInventoryGroupService
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IInventoryScxDataConverter inventoryScxDataConverter
            , IDataLakeFileService dataLakeFileService
            , IInventoryRatingsProcessingService inventoryRatingsService
            , IInventoryScxDataPrep inventoryScxDataPrep
            , IQuarterCalculationEngine quarterCalculationEngine)
        {
            _ProprietaryRepository = broadcastDataRepositoryFactory.GetDataRepository<IProprietaryRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _ProprietaryFileImporterFactory = proprietaryFileImporterFactory;
            _StationProcessingEngine = stationProcessingEngine;
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _LockingEngine = lockingEngine;
            _DaypartParsingEngine = inventoryDaypartParsingEngine;
            _ProprietarySpotCostCalculationEngine = proprietarySpotCostCalculationEngine;
            _StationInventoryGroupService = stationInventoryGroupService;
            _MediaMonthAndWeekCache = mediaMonthAndWeekAggregateCache;
            _InventoryScxDataConverter = inventoryScxDataConverter;
            _DataLakeFileService = dataLakeFileService;
            _InventoryScxDataPrep = inventoryScxDataPrep;
            _InventoryRatingsService = inventoryRatingsService;
            _QuarterCalculationEngine = quarterCalculationEngine;
        }

        /// <summary>
        /// Saves a proprietary inventory file
        /// </summary>
        /// <param name="request">InventoryFileSaveRequest object containing a proprietary inventory file</param>
        /// <param name="userName">Username requesting the operation</param>
        /// <returns>InventoryFileSaveResult object</returns>
        public InventoryFileSaveResult SaveProprietaryInventoryFile(FileRequest request, string userName, DateTime now)
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
            var fileImporter = _ProprietaryFileImporterFactory.GetFileImporterInstance(inventorySource);

            fileImporter.LoadFromSaveRequest(request);
            fileImporter.CheckFileHash();

            ProprietaryInventoryFile proprietaryFile = fileImporter.GetPendingProprietaryInventoryFile(userName, inventorySource);

            _CheckValidationProblems(proprietaryFile);

            proprietaryFile.Id = _InventoryFileRepository.CreateInventoryFile(proprietaryFile, userName);
            try
            {
                fileImporter.ExtractData(proprietaryFile);
                proprietaryFile.FileStatus = proprietaryFile.ValidationProblems.Any() ? FileStatusEnum.Failed : FileStatusEnum.Loaded;

                if (proprietaryFile.ValidationProblems.Any())
                {
                    _ProprietaryRepository.AddValidationProblems(proprietaryFile);
                }
                else
                {
                    using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                    {
                        var header = proprietaryFile.Header;
                        var stations = _GetFileStationsOrCreate(proprietaryFile, userName);
                        var stationsDict = stations.ToDictionary(x => x.Id, x => x.LegacyCallLetters);

                        fileImporter.PopulateManifests(proprietaryFile, stations);                     

                        _LockingEngine.LockStations(stationsDict, lockedStationIds, stationLocks);

                        _StationInventoryGroupService.AddNewStationInventory(proprietaryFile, header.EffectiveDate, header.EndDate, header.ContractedDaypartId);

                        _StationRepository.UpdateStationList(stationsDict.Keys.ToList(), userName, now, proprietaryFile.InventorySource.Id);

                        _ProprietaryRepository.SaveProprietaryInventoryFile(proprietaryFile);
                        transaction.Complete();

                        _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                    }
                }
            }
            catch (Exception ex)
            {
                _LockingEngine.UnlockStations(lockedStationIds, stationLocks);
                proprietaryFile.ValidationProblems.Add(ex.Message);
                proprietaryFile.FileStatus = FileStatusEnum.Failed;
                _ProprietaryRepository.AddValidationProblems(proprietaryFile);
            }

            _CheckValidationProblems(proprietaryFile);

            _InventoryRatingsService.QueueInventoryFileRatingsJob(proprietaryFile.Id);

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
                FileId = proprietaryFile.Id,
                ValidationProblems = proprietaryFile.ValidationProblems,
                Status = proprietaryFile.FileStatus
            };
        }

        /// <summary>
        /// Generates one SCX archive for the current quarter
        /// </summary>
        /// <returns>Returnsa zip archive as stream and the zip name</returns>
        public Tuple<string, Stream> GenerateScxFileArchive(DateTime nowDate)
        {
            string fileNameTemplate = "Barter{0}{1}.scx";
            string archiveFileName = $"InventoryUnits_{DateTime.Now.ToString("yyyyMMddhhmmss")}.zip"; //Sebastian add some timestamp to the name

            QuarterDetailDto currentQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(nowDate, 0);
            var inventoryData = _InventoryScxDataPrep.GetInventoryScxData(currentQuarter);

            List<InventoryScxFile> scxFiles = _InventoryScxDataConverter.ConvertInventoryData(inventoryData);

            MemoryStream archiveFile = new MemoryStream();

            using (var archive = new ZipArchive(archiveFile, ZipArchiveMode.Create, true))
            {
                foreach (var scxFile in scxFiles)
                {                    
                    string scxFileName = string.Format(fileNameTemplate, scxFile.InventorySourceName, scxFile.UnitName);

                    var archiveEntry = archive.CreateEntry(scxFileName, System.IO.Compression.CompressionLevel.Fastest);
                    using (var zippedStreamEntry = archiveEntry.Open())
                    {
                        scxFile.ScxStream.CopyTo(zippedStreamEntry);
                    }
                }
            }
            archiveFile.Seek(0, SeekOrigin.Begin);
            return new Tuple<string, Stream>(archiveFileName, archiveFile);
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

        private static void _CheckValidationProblems(ProprietaryInventoryFile proprietaryFile)
        {
            if (proprietaryFile.ValidationProblems.Any())
            {
                var fileProblems = proprietaryFile.ValidationProblems.Select(x => new InventoryFileProblem(x)).ToList();
                throw new FileUploadException<InventoryFileProblem>(fileProblems);
            }
        }
        
        private List<DisplayBroadcastStation> _GetFileStationsOrCreate(ProprietaryInventoryFile proprietaryFile, string userName)
        {
            var now = DateTime.Now;
            var allStationNames = proprietaryFile.DataLines.Select(x => x.Station).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
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
