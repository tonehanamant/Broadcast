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
using Tam.Maestro.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IMarketService : IApplicationService
    {
        /// <summary>
        /// Loads market coverages from an xlsx file to the broadcast database
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="userName">The name of the current user</param>
        /// <param name="createdDate">Created date</param>
        void LoadCoverages(Stream fileStream, string fileName, string userName, DateTime createdDate);
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

        /// <summary>
        /// Loads market coverages from an xlsx file to the broadcast database
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="userName">The name of the current user</param>
        /// <param name="createdDate">Created date</param>
        public void LoadCoverages(Stream fileStream, string fileName, string userName, DateTime createdDate)
        {
            var marketCoverageFile = new MarketCoverageFile
            {
                FileName = fileName,
                CreatedBy = userName,
                CreatedDate = createdDate,
                FileHash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(fileStream))
            };

            _CheckIfFileAlreadyUploaded(marketCoverageFile.FileHash);

            var marketCoverages = _ReadMarketCoveragesFile(fileStream);
            var marketNamesFromFile = marketCoverages.Select(x => x.Market);
            var marketsFromDb = _MarketRepository.GetMarketsByGeographyNames(marketNamesFromFile);

            _CheckForMissingMarkets(marketsFromDb, marketNamesFromFile);

            marketCoverages.ForEach(x =>
            {
                var market = marketsFromDb.Single(y => y.geography_name.Equals(x.Market, StringComparison.InvariantCultureIgnoreCase));
                x.MarketCode = market.market_code;
            });

            marketCoverageFile.MarketCoverages.AddRange(marketCoverages);

            _MarketCoverageRepository.SaveMarketCoverageFile(marketCoverageFile);
        }

        private void _CheckIfFileAlreadyUploaded(string fileHash)
        {
            if (_MarketCoverageRepository.HasFile(fileHash))
                throw new Exception("Market coverage file already uploaded to the system");
        }

        private List<MarketCoverage> _ReadMarketCoveragesFile(Stream stream)
        {
            var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[1];
            var marketCoverages = worksheet.ConvertSheetToObjects<MarketCoverage>();

            // The last row in the worksheet is summary row which doesn't have rank. 
            // It shouldn't be loaded. This is how we skip it
            marketCoverages = marketCoverages.Where(x => x.Rank.HasValue);

            return marketCoverages.ToList();
        }

        private void _CheckForMissingMarkets(IEnumerable<market> marketsFromDb, IEnumerable<string> marketNamesFromFile)
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
    }
}
