using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryMarkets;
using Services.Broadcast.Entities.InventoryMarketsAffiliates;
using Services.Broadcast.Helpers;
using Services.Broadcast.ReportGenerators.InventoryExport;
using System;
using System.IO;

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
        Guid GenerateMarketAffiliatesReport(InventoryMarketAffiliatesReportRequest request, string userName, DateTime currentDate, string templatesFilePath);

        /// <summary>
        /// Gets the market affiliates report data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        InventoryMarketAffiliatesReportData GetMarketAffiliatesReportData(InventoryMarketAffiliatesReportRequest request);
    };

    public class InventoryMarketAffiliatesExportService : BroadcastBaseClass, IInventoryMarketAffiliatesExportService
    {
        private readonly ISharedFolderService _SharedFolderService;
        private readonly IDateTimeEngine _DateTimeEngine;

        public InventoryMarketAffiliatesExportService(
            ISharedFolderService sharedFolderService,
            IDateTimeEngine dateTimeEngine,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SharedFolderService = sharedFolderService;
            _DateTimeEngine = dateTimeEngine;
        }

        /// <inheritdoc />
        public Guid GenerateMarketAffiliatesReport(InventoryMarketAffiliatesReportRequest request, string userName, DateTime currentDate, string templatesFilePath)
        {
            var marketAffiliateReportData = GetMarketAffiliatesReportData(request);
            var reportGenerator = new InventoryMarketAffiliatesReportGenerator(templatesFilePath);
            var report = reportGenerator.Generate(marketAffiliateReportData);
            var folderPath = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.INVENTORY_MARKET_AFFILIATES_REPORT);

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
        public InventoryMarketAffiliatesReportData GetMarketAffiliatesReportData(InventoryMarketAffiliatesReportRequest request)
        {
            var marketAffiliatesData = new InventoryMarketAffiliatesReportData()
            {
                // todo add getting data
            };
            return marketAffiliatesData;
        }
    }
}
