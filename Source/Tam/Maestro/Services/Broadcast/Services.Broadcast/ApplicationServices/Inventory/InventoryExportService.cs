using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

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
        int GenerateExportForOpenMarket(InventoryExportRequestDto inventoryExportDto, string userName);

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

        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly IInventoryExportEngine _InventoryExportEngine;
        private readonly IFileService _FileService;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IDaypartCache _DaypartCache;
        private readonly IMarketService _MarketService;

        public InventoryExportService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IInventoryExportEngine inventoryExportEngine,
            IFileService fileService,
            ISpotLengthEngine spotLengthEngine,
            IDaypartCache daypartCache,
            IMarketService marketService)
        {
            _InventoryExportRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryExportRepository>();
            _InventoryExportJobRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryExportJobRepository>();
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();

            _QuarterCalculationEngine = quarterCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _InventoryExportEngine = inventoryExportEngine;
            _FileService = fileService;
            _SpotLengthEngine = spotLengthEngine;
            _DaypartCache = daypartCache;
            _MarketService = marketService;
        }

        /// <inheritdoc />
        public List<LookupDto> GetOpenMarketExportGenreTypes()
        {
            return EnumExtensions.ToLookupDtoList<InventoryExportGenreTypeEnum>()
                .OrderBy(i => i.Display).ToList();
        }

        /// <inheritdoc />
        public InventoryQuartersDto GetOpenMarketExportInventoryQuarters(int inventorySourceId)
        {
            var quarters = _InventoryExportRepository.GetInventoryQuartersForSource(inventorySourceId);
            var quarterDetails = quarters.Select(i => _QuarterCalculationEngine.GetQuarterDetail(i.Quarter, i.Year))
                .ToList();

            return new InventoryQuartersDto
            { Quarters = quarterDetails, DefaultQuarter = quarterDetails.FirstOrDefault() };
        }

        /// <inheritdoc />
        public int GenerateExportForOpenMarket(InventoryExportRequestDto request, string userName)
        {
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
                var exportGenreIds = _GetExportGenreIds(request.Genre);
                var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksByFlight(request.Quarter.StartDate, request.Quarter.EndDate);
                var mediaWeekIds = mediaWeeks.Select(w => w.Id).ToList();

                var gatherInventorySw = new Stopwatch();
                gatherInventorySw.Start();
                var inventory = _InventoryRepository.GetInventoryForExportOpenMarket(spotLengthIds, exportGenreIds, mediaWeekIds);
                gatherInventorySw.Stop();

                if (inventory.Any() == false)
                {
                    throw new InvalidOperationException($"No inventory found to export for job {job.Id}.");
                }
                _LogInfo($"Export job {job.Id} found {inventory.Count} inventory records to export in {gatherInventorySw.ElapsedMilliseconds} ms.", userName);

                var processingSw = new Stopwatch();
                processingSw.Start();

                // Perform the calculations
                var inScopeInventory = inventory.Where(i => mediaWeekIds.Contains(i.MediaWeekId)).ToList();
                var calculated = _InventoryExportEngine.Calculate(inScopeInventory);

                // generate the file
                var relevantStationIds = calculated.Select(s => s.StationId).Distinct();
                var allStations = _StationRepository.GetBroadcastStations();
                var stations = allStations.Where(s => relevantStationIds.Contains(s.Id)).ToList();

                var markets = _MarketService.GetMarketsWithLatestCoverage();

                var relevantDaypartIds = calculated.Select(s => s.DaypartId).Distinct();
                var dayparts = _DaypartCache.GetDisplayDayparts(relevantDaypartIds);

                var mediaWeekStartDates = mediaWeeks.Select(w => w.StartDate).ToList();

                var generatedExportResult = _InventoryExportEngine.GenerateExportFile(calculated, mediaWeekIds, stations, markets, dayparts, mediaWeekStartDates);

                processingSw.Stop();

                var exportFileLineCount = generatedExportResult.InventoryTabLineCount;
                var generatedExportFile = generatedExportResult.ExportExcelPackage;

                _LogInfo($"Export job {job.Id} processed {inventory.Count} records to {exportFileLineCount} file lines in {processingSw.ElapsedMilliseconds} ms", userName);

                // save the file
                var saveDirectory = _GetExportFileSaveDirectory();
                var fileName = _GetInventoryFileName(request.Quarter);
                var filePath = Path.Combine(saveDirectory, fileName);
                _LogInfo($"Export job {job.Id} beginning file save to path '{filePath}'.", userName);

                var memoryStream = new MemoryStream();
                generatedExportFile.SaveAs(memoryStream);
                generatedExportFile.Dispose();

                _FileService.CreateDirectory(saveDirectory);
                _FileService.Create(saveDirectory, fileName, memoryStream);

                // update the jobs object to completed.
                job.FileName = fileName;
                job.Status = BackgroundJobProcessingStatus.Succeeded;
                job.CompletedAt = _GetDateTimeNow();

                _InventoryExportJobRepository.UpdateJob(job);

                // return that job Id
                return job.Id.Value;
            }
            catch (Exception ex)
            {
                _LogError($"Error caught in job {job.Id.Value} for user '{userName}'.", ex);

                job.StatusMessage = ex.Message;
                job.Status = BackgroundJobProcessingStatus.Failed;
                job.CompletedAt = _GetDateTimeNow();
                _InventoryExportJobRepository.UpdateJob(job);

                throw;
            }
        }

        /// <inheritdoc />
        public Tuple<string, Stream, string> DownloadOpenMarketExportFile(int jobId)
        {
            var job = _InventoryExportJobRepository.GetJob(jobId);

            var saveDirectory = _GetExportFileSaveDirectory();
            var fileName = job.FileName;

            var fileMimeType = MimeMapping.GetMimeMapping(fileName);
            Stream fileStream = _FileService.GetFileStream(saveDirectory, fileName);

            var response = new Tuple<string, Stream, string>(fileName, fileStream, fileMimeType);
            return response;
        }

        private List<int> _GetExportGenreIds(InventoryExportGenreTypeEnum genreType)
        {
            const string newsGenreName = "NEWS";
            var allGenres = _GenreRepository.GetAllMaestroGenres();
            var result = genreType == InventoryExportGenreTypeEnum.News
                ? allGenres.Where(g => g.Display.ToUpper().Equals(newsGenreName)).Select(g => g.Id).ToList()
                : allGenres.Where(g => g.Display.ToUpper().Equals(newsGenreName) == false).Select(g => g.Id).ToList();
            return result;
        }

        protected string _GetInventoryFileName(QuarterDetailDto quarter)
        {
            var fileName = $"Open Market inventory {quarter.Year} Q{quarter.Quarter}.xlsx";
            return fileName;
        }

        private string _GetExportFileSaveDirectory()
        {
            const string exportDirectory = "InventoryExports";
            var path = Path.Combine(_GetBroadcastSharedFolders(), exportDirectory);
            return path;
        }

        protected virtual string _GetBroadcastSharedFolders()
        {
            return BroadcastServiceSystemParameter.BroadcastSharedFolder;
        }

        protected virtual DateTime _GetDateTimeNow()
        {
            return DateTime.Now;
        }
    }
}