using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.InventoryMarketAffiliates;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.InventoryExport;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ApplicationServices.Inventory
{
    public interface IInventoryMarketAffiliatesExportService : IApplicationService
    {
        /// <summary>
        /// Generates the market affiliates report.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="currentDate">The current date.</param>
        /// <param name="templatesFilePath">The templates file path.</param>
        /// <returns></returns>
        Guid GenerateMarketAffiliatesReport(InventoryMarketAffiliatesRequest request, string userName, DateTime currentDate, string templatesFilePath);

        /// <summary>
        /// Gets the market affiliates report data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        InventoryMarketAffiliatesData GetMarketAffiliatesReportData(InventoryMarketAffiliatesRequest request);
    };

    public class InventoryMarketAffiliatesExportService : BroadcastBaseClass, IInventoryMarketAffiliatesExportService
    {
        private static readonly List<string> affiliates = new List<string> { "ABC", "NBC", "FOX", "CBS", "CW" };

        private readonly ISharedFolderService _SharedFolderService;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IInventoryMarketAffiliatesExportRepository _InventoryMarketAffiliatesExportRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IGenreRepository _GenreRepository;
        protected readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly IInventoryManagementApiClient _InventoryManagementApiClient;
        protected Lazy<bool> _IsInventoryServiceMigrationEnabled;

        public InventoryMarketAffiliatesExportService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            ISharedFolderService sharedFolderService,
            IDateTimeEngine dateTimeEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper,
            IInventoryManagementApiClient inventoryManagementApiClient) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SharedFolderService = sharedFolderService;
            _DateTimeEngine = dateTimeEngine;
            _InventoryMarketAffiliatesExportRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryMarketAffiliatesExportRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _InventoryManagementApiClient = inventoryManagementApiClient;
            _IsInventoryServiceMigrationEnabled = new Lazy<bool>(() =>
               _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_INVENTORY_SERVICE_MIGRATION));
        }

        /// <inheritdoc />
        public Guid GenerateMarketAffiliatesReport(InventoryMarketAffiliatesRequest request, string userName, DateTime currentDate, string templatesFilePath)
        {
            if (_IsInventoryServiceMigrationEnabled.Value)
            {
                _LogInfo("Calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                try
                {
                    request.UserName = userName;
                    var result = _InventoryManagementApiClient.GenerateOpenMarketAffiliates(request);
                    _LogInfo("Completed calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.");
                    return result;
                }
                catch (Exception ex)
                {
                    _LogError("Exception calling the Inventory Management Service for this operation per the toggle 'enable-inventory-service-migration'.", ex);
                    throw;
                }
            }
            _LogInfo($"Gathering the report data...");
            var marketAffiliateReportData = GetMarketAffiliatesReportData(request);
            var reportGenerator = new InventoryMarketAffiliatesReportGenerator(templatesFilePath);
            _LogInfo($"Preparing to generate the file.  templatesFilePath='{templatesFilePath}'");
            var report = reportGenerator.Generate(marketAffiliateReportData);
            var folderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.INVENTORY_MARKET_AFFILIATES_REPORT);

            _LogInfo($"Saving generated file '{report.Filename}' to folder '{folderPath}'");

            return _SharedFolderService.SaveFile(new SharedFolderFile
            {
                FolderPath = folderPath,
                FileNameWithExtension = report.Filename,
                FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileUsage = SharedFolderFileUsage.InventoryMarketAffiliatesReport,
                CreatedDate = _DateTimeEngine.GetCurrentMoment(),
                CreatedBy = userName,
                FileContent = report.Stream
            });
        }

        /// <inheritdoc />
        public InventoryMarketAffiliatesData GetMarketAffiliatesReportData(InventoryMarketAffiliatesRequest request)
        {         
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksByFlight(request.Quarter.StartDate, request.Quarter.EndDate);
            var mediaWeekIds = mediaWeeks.Select(w => w.Id).ToList();

            var allGenres = _GenreRepository.GetAllMaestroGenres();
            var newsGenreIds = GenreHelper.GetGenreIds(InventoryExportGenreTypeEnum.News, allGenres);
            var nonNewsGenreIds = GenreHelper.GetGenreIds(InventoryExportGenreTypeEnum.NonNews, allGenres);

            var marketCoverage = _MarketCoverageRepository.GetLatestMarketCoverageFile();

            var newsMarkets = _GetMarketAffiliates(request.InventorySourceId, mediaWeekIds, affiliates, newsGenreIds, marketCoverage.Id);
            var nonNewsMarket = _GetMarketAffiliates(request.InventorySourceId, mediaWeekIds, affiliates, nonNewsGenreIds, marketCoverage.Id);

            var marketAffiliatesData = new InventoryMarketAffiliatesData()
            {
                NewsMarketAffiliates = newsMarkets,
                NonNewsMarketAffiliates = nonNewsMarket
            };
            return marketAffiliatesData;
        }


        /// <summary>
        /// Gets the market affiliates.
        /// </summary>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        /// <param name="mediaWeekIds">The media week ids.</param>
        /// <param name="affiliates">The affiliates.</param>
        /// <param name="genreIds">The genre ids.</param>
        /// <param name="MarketCoverageFileId">The market coverage.</param>
        /// <returns></returns>
        internal List<InventoryMarketAffiliates> _GetMarketAffiliates(int inventorySourceId, List<int> mediaWeekIds, List<string> affiliates, List<int> genreIds, int MarketCoverageFileId)
        {
            var rowData = new List<InventoryMarketAffiliates>();
                        
            var marketInventoryList = _InventoryMarketAffiliatesExportRepository.GetMarketInvenoryList(inventorySourceId, mediaWeekIds, affiliates, genreIds);
            var marketAffiliatesList = _InventoryMarketAffiliatesExportRepository.GetMarketAffiliateList(affiliates, MarketCoverageFileId);

            var isAggregateList = _GetIsAggregateList(marketInventoryList);

            foreach (var test in marketAffiliatesList)
            {
                var isInventory = marketInventoryList
                    .Where(x => x.marketCode == test.marketCode && x.affiliation == test.affiliation)
                    .Select(x => x.isInventory).SingleOrDefault();

                var isAggregate = isAggregateList
                    .Where(x => x.marketCode == test.marketCode)
                    .Select(x => x.isAggregate).SingleOrDefault();

                rowData.Add(
                    new InventoryMarketAffiliates
                    {
                        marketName = test.marketName,
                        marketRank = test.rank,
                        affiliates = test.affiliation,
                        inventory = isInventory == null ? "No" : isInventory,
                        aggregate = isAggregate
                    }
                );
            }
            return rowData;
        }

        /// <summary>
        /// Gets the is aggregate list.
        /// </summary>
        /// <param name="marketInventoryList">The market inventory list.</param>
        /// <returns></returns>
        internal List<IsAggregateDto> _GetIsAggregateList(List<MarketInventoryDto> marketInventoryList)
        {
            var isAggregateList = (from market in marketInventoryList
                                   group market by new
                                   {
                                       market.marketCode
                                   } into markets
                                   select new IsAggregateDto
                                   {
                                       marketCode = markets.Key.marketCode,
                                       isAggregate = markets.Where(x => x.isInventory == "Yes").Count() == 5 ? "Yes" : "No"
                                   }).ToList();
            return isAggregateList;
        }
    }
}
