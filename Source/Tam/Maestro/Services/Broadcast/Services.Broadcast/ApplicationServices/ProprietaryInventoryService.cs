using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.ProprietaryInventory;
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
        /// <param name="now"></param>
        /// <returns>InventoryFileSaveResult object</returns>
        InventoryFileSaveResult SaveProprietaryInventoryFile(FileRequest request, string userName, DateTime now);

        /// <summary>
        /// Generates one SCX archive for the current quarter
        /// </summary>
        /// <returns>Returns a zip archive as stream and the zip name</returns>
        Tuple<string, Stream> GenerateScxFileArchive(InventoryScxDownloadRequest request);

        /// <summary>
        /// Generate a list of SCX files for a request
        /// </summary>
        /// <param name="request">The generation request</param>
        /// <returns></returns>
        List<InventoryScxFile> GenerateScxFiles(InventoryScxDownloadRequest request);
        /// <summary>
        /// Generate a list of open market SCX files for a request
        /// </summary>
        /// <param name="request">The open market file generation request</param>
        /// <returns></returns>
        List<OpenMarketInventoryScxFile> GenerateScxOpenMarketFiles(InventoryScxOpenMarketsDownloadRequest request);
    }

    public class ProprietaryInventoryService : BroadcastBaseClass, IProprietaryInventoryService
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
        private readonly IInventoryRatingsProcessingService _InventoryRatingsService;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IInventoryWeekEngine _InventoryWeekEngine;        
        private readonly IInventoryScxDataPrepFactory _InventoryScxDataPrepFactory;
        private readonly IInventoryProgramsProcessingService _InventoryProgramsProcessingService;
        private readonly IStationMappingService _IStationMappingService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IGenreRepository _GenreRepository;
        private readonly IInventoryManagementApiClient _InventoryApiClient;
        private readonly Lazy<bool> _IsInventoryServiceMigrationEnabled;
        /// <summary>
        /// Spot lengths dictionary where key is the id and value is the duration
        /// </summary>
        private readonly Lazy<Dictionary<int, int>> _SpotLengthDurationsById;

        public ProprietaryInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IProprietaryFileImporterFactory proprietaryFileImporterFactory
            , IStationProcessingEngine stationProcessingEngine
            , ILockingEngine lockingEngine
            , IInventoryDaypartParsingEngine inventoryDaypartParsingEngine
            , IProprietarySpotCostCalculationEngine proprietarySpotCostCalculationEngine
            , IStationInventoryGroupService stationInventoryGroupService
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IInventoryScxDataConverter inventoryScxDataConverter
            , IInventoryRatingsProcessingService inventoryRatingsService
            , IQuarterCalculationEngine quarterCalculationEngine
            , IInventoryWeekEngine inventoryWeekEngine
            , IInventoryScxDataPrepFactory inventoryScxDataPrepFactory
            , IInventoryProgramsProcessingService inventoryProgramsProcessingService
            , ISharedFolderService sharedFolderService
            , IStationMappingService stationMappingService
            ,IInventoryManagementApiClient inventoryApiClient
            , IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) 
            : base(featureToggleHelper, configurationSettingsHelper)
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
            _InventoryRatingsService = inventoryRatingsService;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _InventoryWeekEngine = inventoryWeekEngine;
            _InventoryScxDataPrepFactory = inventoryScxDataPrepFactory;
            _InventoryProgramsProcessingService = inventoryProgramsProcessingService;
            _IStationMappingService = stationMappingService;
            _SharedFolderService = sharedFolderService;
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _SpotLengthDurationsById = new Lazy<Dictionary<int, int>>(() => broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthDurationsById());
            _InventoryApiClient = inventoryApiClient;
            _IsInventoryServiceMigrationEnabled = new Lazy<bool>(() =>
              _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION));

        }

        ///<inheritdoc/>
        public InventoryFileSaveResult SaveProprietaryInventoryFile(FileRequest request, string userName, DateTime nowDate)
        {
            InventoryFileSaveResult result = new InventoryFileSaveResult();
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                InventoryFileSaveRequestDto fileRequest = new InventoryFileSaveRequestDto
                {
                    FileName = request.FileName,
                    RawData = FileStreamExtensions.ConvertToBase64String(request.StreamData),
                    UserName = userName
                };
                result = _InventoryApiClient.SaveInventoryFile(fileRequest);
                if (result.Status== FileStatusEnum.Loaded)
                {
                    _InventoryRatingsService.QueueInventoryFileRatingsJob(result.FileId);
                    _InventoryProgramsProcessingService.QueueProcessInventoryProgramsByFileJob(result.FileId, userName);
                }
            }
            else
            {
                if (!request.FileName.EndsWith(".xlsx"))
                {
                    result = new InventoryFileSaveResult
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

                proprietaryFile.Id = _InventoryFileRepository.CreateInventoryFile(proprietaryFile, userName, nowDate);
                try
                {
                    var fileStreamWithErrors = fileImporter.ExtractData(proprietaryFile);
                    proprietaryFile.FileStatus = proprietaryFile.ValidationProblems.Any() ? FileStatusEnum.Failed : FileStatusEnum.Loaded;

                    if (proprietaryFile.ValidationProblems.Any())
                    {
                        _InventoryRepository.AddValidationProblems(proprietaryFile);
                        WriteErrorFileToDisk(fileStreamWithErrors, proprietaryFile.Id, request.FileName, userName, nowDate);
                    }
                    else
                    {
                        using (var transaction = TransactionScopeHelper.CreateTransactionScopeWrapper(TimeSpan.FromMinutes(20)))
                        {
                            var header = proprietaryFile.Header;
                            var stations = fileImporter.GetFileStations(proprietaryFile);
                            var stationsDict = stations.ToDictionary(x => x.Id, x => x.LegacyCallLetters);

                            fileImporter.PopulateManifests(proprietaryFile, stations);

                            var spotLEngthDurationsById = _SpotLengthDurationsById.Value;
                            fileImporter.HandleManifestDuplicates(proprietaryFile, spotLEngthDurationsById);

                            fileImporter.PopulateInventoryFileDateRange(proprietaryFile);
                            _SetStartAndEndDatesForManifestWeeks(proprietaryFile, header.EffectiveDate, header.EndDate);
                            WebUtilityHelper.HtmlDecodeProgramNames(proprietaryFile);

                            _LockingEngine.LockStations(stationsDict, lockedStationIds, stationLocks);

                            _StationInventoryGroupService.AddNewStationInventory(proprietaryFile, header.ContractedDaypartId);

                            _StationRepository.UpdateStationList(stationsDict.Keys.ToList(), userName, nowDate, proprietaryFile.InventorySource.Id);

                            proprietaryFile.RowsProcessed = proprietaryFile.DataLines.Count();

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
                    _InventoryRepository.AddValidationProblems(proprietaryFile);
                }

                if (!proprietaryFile.ValidationProblems.Any())
                {
                    _InventoryRatingsService.QueueInventoryFileRatingsJob(proprietaryFile.Id);
                    _InventoryProgramsProcessingService.QueueProcessInventoryProgramsByFileJob(proprietaryFile.Id, userName);
                }

                result = new InventoryFileSaveResult
                {
                    FileId = proprietaryFile.Id,
                    ValidationProblems = proprietaryFile.ValidationProblems,
                    Status = proprietaryFile.FileStatus
                };
            }
            return result;
        }

        private void _SetStartAndEndDatesForManifestWeeks(InventoryFileBase inventoryFile, DateTime effectiveDate, DateTime endDate)
        {
            var allManifestWeeks = inventoryFile.GetAllManifests().SelectMany(x => x.ManifestWeeks);

            foreach (var manifestWeek in allManifestWeeks)
            {
                var dateRange = _InventoryWeekEngine.GetDateRangeInventoryIsAvailableForForWeek(manifestWeek.MediaWeek, effectiveDate, endDate);
                manifestWeek.StartDate = dateRange.Start.Value;
                manifestWeek.EndDate = dateRange.End.Value;
            }
        }

        ///<inheritdoc/>
        public Tuple<string, Stream> GenerateScxFileArchive(InventoryScxDownloadRequest request)
        {
            const string fileNameTemplate = "{0}{1}.scx";
            var archiveFileName = $"InventoryUnits_{DateTime.Now.ToString("yyyyMMddhhmmss")}.zip";
            var inventorySource = _InventoryRepository.GetInventorySource(request.InventorySourceId);
            var inventoryDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(inventorySource.InventoryType);
            var inventoryData = inventoryDataPrep.GetInventoryScxData(request.InventorySourceId, request.StandardDaypartId, request.StartDate, request.EndDate, request.UnitNames);
            var scxFiles = _InventoryScxDataConverter.ConvertInventoryData(inventoryData);
            var archiveFile = new MemoryStream();

            using (var archive = new ZipArchive(archiveFile, ZipArchiveMode.Create, true))
            {
                foreach (var scxFile in scxFiles)
                {
                    var scxFileName = string.Format(fileNameTemplate, scxFile.InventorySource.Name, scxFile.UnitName);
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

        ///<inheritdoc/>
        public List<InventoryScxFile> GenerateScxFiles(InventoryScxDownloadRequest request)
        {
            var inventorySource = _InventoryRepository.GetInventorySource(request.InventorySourceId);
            var inventoryDataPrep = _InventoryScxDataPrepFactory.GetInventoryDataPrep(inventorySource.InventoryType);
            var inventoryData = inventoryDataPrep.GetInventoryScxData(request.InventorySourceId, request.StandardDaypartId, request.StartDate, request.EndDate, request.UnitNames);

            return _InventoryScxDataConverter.ConvertInventoryData(inventoryData);
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

        private void WriteErrorFileToDisk(Stream stream, int fileId, string fileName, string userName, DateTime nowDate)
        {
            const string fileMediaType = "text/plain";
            var saveDirectory = _GetInventoryUploadErrorDirectory();
	        var fullFileName = $@"{fileId}_{fileName}";
	        stream.Position = 0;

            var sharedFolderFile = new SharedFolderFile
            {
                FolderPath = saveDirectory,
                FileNameWithExtension = fullFileName,
                FileMediaType = fileMediaType,
                FileUsage = SharedFolderFileUsage.InventoryErrorFile,
                CreatedDate = nowDate,
                CreatedBy = userName,
                FileContent = stream
            };

            _LogInfo($"Saving error file '{fullFileName}' for inventory file '{fileName}'...");
            var sharedFolderFileId = _SharedFolderService.SaveFile(sharedFolderFile);
            _LogInfo($"Saved error file '{fullFileName}' for inventory file '{fileName}' as ID '{sharedFolderFileId}'");

            _InventoryFileRepository.SaveErrorFileId(fileId, sharedFolderFileId);
        }
        private string _GetInventoryUploadErrorDirectory()
        {
			var path = Path.Combine(_GetInventoryUploadFolder()
                , BroadcastConstants.FolderNames.INVENTORY_UPLOAD_ERRORS);
            return path;
		}

        protected virtual string _GetInventoryUploadFolder()
        {
            var path = Path.Combine(_GetBroadcastAppFolder()
                , BroadcastConstants.FolderNames.INVENTORY_UPLOAD);
            return path;
        }
        public List<OpenMarketInventoryScxFile> GenerateScxOpenMarketFiles(InventoryScxOpenMarketsDownloadRequest request)
        {
            const int inventorySourceIdOpenMarket = 1;
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceIdOpenMarket);
            var inventoryDataPrep = _InventoryScxDataPrepFactory.GetOpenMarketInventoryDataPrep(inventorySource.InventoryType);
            var genres = _GenreRepository.GetAllMaestroGenres();
            var exportGenreIds = GenreHelper.GetGenreIdsForOpenMarket(request.GenreType, genres);
            var inventoryData = inventoryDataPrep.GetInventoryScxOpenMarketData(inventorySourceIdOpenMarket, request.DaypartIds, request.StartDate, request.EndDate,request.MarketRanks, exportGenreIds, request.Affiliates);
            return _InventoryScxDataConverter.ConvertOpenMrketInventoryData(inventoryData);
        }
    }
}
