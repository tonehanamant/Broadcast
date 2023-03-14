using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.InventoryExport;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices.Inventory
{
    public interface IInventoryExportService : IApplicationService
    {
        /// <summary>
        ///     Get OpenMarket Export Inventory Quarters
        /// </summary>
        InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId);

        /// <summary>
        /// Gets the open market export genre types.
        /// </summary>
        /// <returns></returns>
        List<LookupDto> GetOpenMarketExportGenreTypes();

        /// <summary>
        /// Generates the inventory export file for the Open Market inventory source.
        /// </summary>
        int GenerateExportForOpenMarket(InventoryExportRequestDto inventoryExportDto, string userName, string templatesFilePath);

        /// <summary>
        /// Downloads the generated export file.
        /// </summary>
        Tuple<string, Stream, string> DownloadOpenMarketExportFile(int jobId);
    }

    public class InventoryExportService : BroadcastBaseClass, IInventoryExportService
    {
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IInventoryExportRepository _InventoryExportRepository;
        private readonly IInventoryExportJobRepository _InventoryExportJobRepository;
        private readonly IGenreRepository _GenreRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IBroadcastAudienceRepository _AudienceRepository;

        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IInventoryExportEngine _InventoryExportEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IDaypartCache _DaypartCache;
        private readonly IMarketService _MarketService;
        private readonly INsiPostingBookService _NsiPostingBookService;

        private readonly IFileService _FileService;
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IDateTimeEngine _DateTimeEngine;

        private readonly Lazy<bool> _EnableSharedFileServiceConsolidation;
        private readonly IInventoryManagementApiClient _InventoryManagementApiClient;
        protected Lazy<bool> _IsInventoryServiceMigrationEnabled;

        public InventoryExportService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IInventoryExportEngine inventoryExportEngine,
            IFileService fileService,
            ISharedFolderService sharedFolderService,
            ISpotLengthEngine spotLengthEngine,
            IDaypartCache daypartCache,
            IMarketService marketService,
            INsiPostingBookService nsiPostingBookService,
            IDateTimeEngine dateTimeEngine,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper,
            IInventoryManagementApiClient inventoryManagementApiClient)
                : base(featureToggleHelper, configurationSettingsHelper)
        {
            _InventoryExportRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryExportRepository>();
            _InventoryExportJobRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryExportJobRepository>();
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _AudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();

            _QuarterCalculationEngine = quarterCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _InventoryExportEngine = inventoryExportEngine;
            _FileService = fileService;
            _SharedFolderService = sharedFolderService;
            _SpotLengthEngine = spotLengthEngine;
            _DaypartCache = daypartCache;
            _MarketService = marketService;
            _NsiPostingBookService = nsiPostingBookService;
            _DateTimeEngine = dateTimeEngine;

            _EnableSharedFileServiceConsolidation = new Lazy<bool>(_GetEnableSharedFileServiceConsolidation);
            _InventoryManagementApiClient = inventoryManagementApiClient;
            _IsInventoryServiceMigrationEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION));
        }

        /// <inheritdoc />
        public List<LookupDto> GetOpenMarketExportGenreTypes()
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                return _InventoryManagementApiClient.GetInventoryGenreTypes();
            }
            else
            {
                return EnumExtensions.ToLookupDtoList<InventoryExportGenreTypeEnum>()
                    .OrderBy(i => i.Display).ToList();
            }
        }

        /// <inheritdoc />
        public InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                return _InventoryManagementApiClient.GetOpenMarketExportInventoryQuarters(inventorySourceId);
            }
            else
            {
                var quarters = _InventoryExportRepository.GetInventoryQuartersForSource(inventorySourceId);
                var quarterDetails = quarters.Select(i => _QuarterCalculationEngine.GetQuarterDetail(i.Quarter, i.Year))
                    .ToList();

                return new InventoryQuartersDto
                { Quarters = quarterDetails, DefaultQuarter = quarterDetails.FirstOrDefault() };
            }
        }

        /// <inheritdoc />
        public int GenerateExportForOpenMarket(InventoryExportRequestDto request, string userName, string templatesFilePath)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                request.UserName = userName;
                return _InventoryManagementApiClient.GenerateExportForOpenMarket(request);
            }
            const int spotLengthMinutes = 30;
            const int inventorySourceIdOpenMarket = 1;
            var inventorySource = _InventoryRepository.GetInventorySource(inventorySourceIdOpenMarket);

            var job = new InventoryExportJobDto
            {
                InventorySourceId = inventorySource.Id,
                Quarter = request.Quarter,
                ExportGenreType = request.Genre,
                Status = BackgroundJobProcessingStatus.Processing
            };

            // create the job record as in progress
            job.Id = _InventoryExportJobRepository.CreateJob(job, userName);
            try
            {
                _LogInfo($"Starting export job {job.Id}. Inventory Source = '{inventorySource.Name}'; ExportGenreType = {request.Genre}; Quarter = Q{request.Quarter.Quarter}-{request.Quarter.Year};", userName);

                // Get the raw data from the repo
                var spotLengthIds = new List<int> { _SpotLengthEngine.GetSpotLengthIdByValue(spotLengthMinutes) };
                var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksByFlight(request.Quarter.StartDate, request.Quarter.EndDate);
                var mediaWeekIds = mediaWeeks.Select(w => w.Id).ToList();

                var genres = _GenreRepository.GetAllMaestroGenres();

                var gatherInventorySw = new Stopwatch();
                gatherInventorySw.Start();

                List<InventoryExportDto> inventory;
                if (request.Genre == InventoryExportGenreTypeEnum.NotEnriched)
                {
                    inventory = _InventoryExportRepository.GetInventoryForExportOpenMarketNotEnriched(spotLengthIds, mediaWeekIds);
                }
                else
                {
                    var exportGenreIds = GenreHelper.GetGenreIds(request.Genre, genres);
                    inventory = _InventoryExportRepository.GetInventoryForExportOpenMarket(spotLengthIds, exportGenreIds, mediaWeekIds);
                }

                gatherInventorySw.Stop();

                if (inventory.Any() == false)
                {
                    throw new InvalidOperationException($"No '{request.Genre.GetDescriptionAttribute()}' inventory found to export for Q{request.Quarter.Quarter} {request.Quarter.Year}.");
                }
                _LogInfo($"Export job {job.Id} found {inventory.Count} inventory records to export in {gatherInventorySw.ElapsedMilliseconds} ms.", userName);

                var processingSw = new Stopwatch();
                processingSw.Start();
                // Perform the calculations
                var inScopeInventory = inventory.Where(i => mediaWeekIds.Contains(i.MediaWeekId)).ToList();
                var calculated = _InventoryExportEngine.Calculate(inScopeInventory);

                // generate the file
                var generatedTimeStampValue = _InventoryExportEngine.GetExportGeneratedTimestamp(_DateTimeEngine.GetCurrentMoment());
                var fileName = _InventoryExportEngine.GetInventoryExportFileName(request.Genre, request.Quarter);

                var relevantAudienceIds = inScopeInventory.SelectMany(i => i.ProvidedAudiences.Select(s => s.AudienceId))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
                var audiences = _AudienceRepository.GetAudienceDtosById(relevantAudienceIds);
                var audienceColumnHeaders = _InventoryExportEngine.GetInventoryTableAudienceColumnHeaders(audiences);

                var mediaWeekStartDates = mediaWeeks.Select(w => w.StartDate).ToList();
                var weeklyColumnHeaders = _InventoryExportEngine.GetInventoryTableWeeklyColumnHeaders(mediaWeekStartDates);

                var relevantStationIds = calculated.Select(s => s.StationId).Distinct();
                var allStations = _StationRepository.GetBroadcastStations();
                var stations = allStations.Where(s => relevantStationIds.Contains(s.Id)).ToList();

                var relevantDaypartIds = calculated.Select(s => s.DaypartId).Distinct();
                var dayparts = _DaypartCache.GetDisplayDayparts(relevantDaypartIds);

                var markets = _MarketService.GetMarketsWithLatestCoverage();

                var tableData = _InventoryExportEngine.GetInventoryTableData(calculated, stations, markets, mediaWeekIds, dayparts, audiences, genres);

                var shareBookId = _NsiPostingBookService.GetLatestNsiPostingBookForMonthContainingDate(request.Quarter.StartDate);
                var shareBookMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(shareBookId);

                var reportData = new InventoryExportReportData
                {
                    ExportFileName = fileName,
                    GeneratedTimestampValue = generatedTimeStampValue,
                    ProvidedAudienceHeaders = audienceColumnHeaders,
                    WeeklyColumnHeaders = weeklyColumnHeaders,
                    InventoryTableData = tableData,
                    ShareBookValue = $"Share Book : {shareBookMonth.LongMonthNameAndYear}"
                };

                var reportGenerator = new InventoryExportGenerator(templatesFilePath);
                var reportOutput = reportGenerator.Generate(reportData);

                processingSw.Stop();

                _LogInfo($"Export job {job.Id} processed {inventory.Count} records to {tableData.Length} file lines in {processingSw.ElapsedMilliseconds} ms", userName);

                // save the file

                _LogInfo($"Export job {job.Id} beginning file '{reportData.ExportFileName}'.", userName);
                var savedFileId = _SaveFile(reportData.ExportFileName, reportOutput.Stream, userName);

                // update the jobs object to completed.
                job.StatusMessage = templatesFilePath;
                job.FileName = reportData.ExportFileName;
                job.SharedFolderFileId = savedFileId;
                job.Status = BackgroundJobProcessingStatus.Succeeded;
                job.CompletedAt = _DateTimeEngine.GetCurrentMoment();

                _InventoryExportJobRepository.UpdateJob(job);

                // return that job Id
                return job.Id.Value;
            }
            catch (Exception ex)
            {
                _LogError($"Error caught in job {job.Id.Value} for user '{userName}'.", ex);

                job.StatusMessage = ex.Message;
                job.Status = BackgroundJobProcessingStatus.Failed;
                job.CompletedAt = _DateTimeEngine.GetCurrentMoment();
                _InventoryExportJobRepository.UpdateJob(job);

                throw;
            }
        }

        private Guid? _SaveFile(string fileName, Stream fileStream, string userName)
        {
            var folderPath = _GetExportFileSaveDirectory();
            const string fileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

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

        /// <inheritdoc />
        public Tuple<string, Stream, string> DownloadOpenMarketExportFile(int jobId)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                //This call to inventory microservice is disabled When FE call inventory microservice api directly 
                // at that time this API will be directly called by FE from inventory microservice

               // return _InventoryManagementApiClient.DownloadInventoeyForOpenMarket(jobId);
            }
            //else
            //{
                Tuple<string, Stream, string> result;
                var job = _InventoryExportJobRepository.GetJob(jobId);

                if (_EnableSharedFileServiceConsolidation.Value && job.SharedFolderFileId.HasValue)
                {
                    _LogInfo($"Translated jobId '{job.Id}' as sharedFolderFileId '{job.SharedFolderFileId.Value}'");
                    var file = _SharedFolderService.GetFile(job.SharedFolderFileId.Value);
                    result = _BuildPackageReturn(file.FileContent, file.FileNameWithExtension);
                    _SharedFolderService.RemoveFileFromFileShare(job.SharedFolderFileId.Value);
                    return result;
                }

                result = _GetFileFromFileService(job);
                return result;
           // }
        }

        private Tuple<string, Stream, string> _BuildPackageReturn(Stream fileStream, string fileName)
        {
            var fileMimeType = MimeMapping.GetMimeMapping(fileName);
            var result = new Tuple<string, Stream, string>(fileName, fileStream, fileMimeType);
            return result;
        }

        private Tuple<string, Stream, string> _GetFileFromFileService(InventoryExportJobDto job)
        {
            var saveDirectory = _GetExportFileSaveDirectory();
            var fileName = job.FileName;
            var fileStream = _FileService.GetFileStream(saveDirectory, fileName);

            var result = _BuildPackageReturn(fileStream, fileName);
            return result;
        }

        private string _GetExportFileSaveDirectory()
        {
            var path = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.INVENTORY_EXPORTS);
            return path;
        }

        private bool _GetEnableSharedFileServiceConsolidation()
        {
            var result = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION);
            return result;
        }
    }
}