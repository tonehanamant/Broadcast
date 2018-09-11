using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using OfficeOpenXml;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Services.Broadcast.ApplicationServices
{
    public interface IMarketService : IApplicationService
    {
        /// <summary>
        /// Loads market coverages from an xlsx file to the broadcast database
        /// </summary>
        /// <param name="path">Server path to an xlsx file</param>
        void LoadCoverages(string path);
    }

    public class MarketService : IMarketService
    {
        private readonly IMarketCoverageRepository _MarketCoverageRepository;
        private readonly IMarketRepository _MarketRepository;

        public MarketService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _MarketCoverageRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();
            _MarketRepository = broadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>();
        }

        public void LoadCoverages(string path)
        {
            var marketCovereges = _ReadMarketCoveragesFile(path).ToList();
            var marketNamesFromFile = marketCovereges.Select(x => x.Market);
            var marketsFromDb = _MarketRepository.GetMarketsByGeographyNames(marketNamesFromFile);

            _CheckForNotExistingMarkets(marketsFromDb, marketNamesFromFile);
            _SetMarketCodes(marketCovereges, marketsFromDb);
            _MarketCoverageRepository.ResetMarketCoverages(marketCovereges);
        }

        private IEnumerable<MarketCoverage> _ReadMarketCoveragesFile(string path)
        {
            var file = new FileInfo(path);
            var package = new ExcelPackage(file, true);
            var worksheet = package.Workbook.Worksheets[1];
            var marketCoverages = worksheet.ConvertSheetToObjects<MarketCoverage>();

            // The last row in the worksheet is summary row which doesn`t have rank. 
            // It shouldn`t be loaded. This is how we skip it
            marketCoverages = marketCoverages.Where(x => x.Rank.HasValue);

            return marketCoverages;
        }

        private void _CheckForNotExistingMarkets(IEnumerable<market> marketsFromDb, IEnumerable<string> marketNamesFromFile)
        {
            var marketNamesFromDb = marketsFromDb.Select(x => x.geography_name);
            var notExistingMarkets = marketNamesFromFile.Except(marketNamesFromDb, StringComparer.OrdinalIgnoreCase).ToList();

            if (notExistingMarkets.Any())
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Markets which were not found: ");

                for (var i = 0; i < notExistingMarkets.Count() - 1; i++)
                {
                    stringBuilder.Append($"{notExistingMarkets[i]}, ");
                }

                stringBuilder.Append($"{notExistingMarkets[notExistingMarkets.Count() - 1]}.");

                throw new Exception(stringBuilder.ToString());
            }
        }

        private void _SetMarketCodes(List<MarketCoverage> marketCoverages, IEnumerable<market> marketsFromDb)
        {
            foreach (var marketCoverage in marketCoverages)
            {
                var market = marketsFromDb.Single(x => x.geography_name.Equals(marketCoverage.Market, StringComparison.InvariantCultureIgnoreCase));
                marketCoverage.MarketCode = market.market_code;
            }
        }
    }
}
