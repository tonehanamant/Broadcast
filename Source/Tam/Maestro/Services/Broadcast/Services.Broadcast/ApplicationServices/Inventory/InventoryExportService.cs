﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
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
        private readonly IFileService _FileService;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IDaypartCache _DaypartCache;
        private readonly IMarketService _MarketService;
        private readonly INsiPostingBookService _NsiPostingBookService;

        public InventoryExportService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IInventoryExportEngine inventoryExportEngine,
            IFileService fileService,
            ISpotLengthEngine spotLengthEngine,
            IDaypartCache daypartCache,
            IMarketService marketService,
            INsiPostingBookService nsiPostingBookService, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
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
            _SpotLengthEngine = spotLengthEngine;
            _DaypartCache = daypartCache;
            _MarketService = marketService;
            _NsiPostingBookService = nsiPostingBookService;
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
        public int GenerateExportForOpenMarket(InventoryExportRequestDto request, string userName, string templatesFilePath)
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
                    var exportGenreIds = _GetExportGenreIds(request.Genre, genres);
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
                var generatedTimeStampValue = _InventoryExportEngine.GetExportGeneratedTimestamp(_GetCurrentDateTime());
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
                var saveDirectory = _GetExportFileSaveDirectory();
                var filePath = Path.Combine(saveDirectory, reportData.ExportFileName);
                _LogInfo($"Export job {job.Id} beginning file save to path '{filePath}'.", userName);

                _FileService.CreateDirectory(saveDirectory);
                _FileService.Create(saveDirectory, reportData.ExportFileName, reportOutput.Stream);

                // update the jobs object to completed.
                job.FileName = reportData.ExportFileName;
                job.Status = BackgroundJobProcessingStatus.Succeeded;
                job.CompletedAt = _GetCurrentDateTime();

                _InventoryExportJobRepository.UpdateJob(job);

                // return that job Id
                return job.Id.Value;
            }
            catch (Exception ex)
            {
                _LogError($"Error caught in job {job.Id.Value} for user '{userName}'.", ex);

                job.StatusMessage = ex.Message;
                job.Status = BackgroundJobProcessingStatus.Failed;
                job.CompletedAt = _GetCurrentDateTime();
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

        protected List<int> _GetExportGenreIds(InventoryExportGenreTypeEnum genreType, List<LookupDto> genres)
        {
            const string newsGenreName = "NEWS";
            var result = genreType == InventoryExportGenreTypeEnum.News
                ? genres.Where(g => g.Display.ToUpper().Equals(newsGenreName)).Select(g => g.Id).ToList()
                : genres.Where(g => g.Display.ToUpper().Equals(newsGenreName) == false).Select(g => g.Id).ToList();
            return result;
        }

        private string _GetExportFileSaveDirectory()
        {
            var path = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.INVENTORY_EXPORTS);
            return path;
        }
    }
}