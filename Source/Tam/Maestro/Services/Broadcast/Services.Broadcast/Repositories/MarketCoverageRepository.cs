using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IMarketCoverageRepository : IDataRepository
    {
        /// <summary>
        /// Removes all the existing market coverages and inserts new ones
        /// </summary>
        /// <param name="marketCoverages">Market coverages for inserting</param>
        void SaveMarketCoverageFile(MarketCoverageFile marketCoverageFile);
        /// <summary>
        /// Returns all the market coverages
        /// </summary>
        List<MarketCoverage> GetAll();
        bool HasFile(string fileHash);

        /// <summary>
        /// Returns a dictionary of market code and percentage coverage based on the market ids sent.
        /// </summary>
        /// <param name="marketIds">Market id list.</param>
        /// <returns>Dictionary of market code and percentage coverage</returns>
        MarketCoverageDto GetLatestMarketCoverages(IEnumerable<int> marketIds);

        MarketCoverageByStation GetLatestMarketCoveragesWithStations();
    }

    public class MarketCoverageRepository : BroadcastRepositoryBase, IMarketCoverageRepository
    {
        public MarketCoverageRepository(
            ISMSClient pSmsClient, 
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<MarketCoverage> GetAll()
        {
            return _InReadUncommitedTransaction(context => context.market_coverages.Select(x => new MarketCoverage
            {
                MarketCoverageFileId = x.market_coverage_file_id,
                MarketCode = x.market_code,
                PercentageOfUS = x.percentage_of_us,
                TVHomes = x.tv_homes,
                Market = x.market.geography_name,
                Rank = x.rank
            }).ToList());
        }

        public bool HasFile(string fileHash)
        {
            return _InReadUncommitedTransaction(
                context => context.market_coverage_files.Where(x => x.file_hash == fileHash).Count()) > 0;
          
        }

        public void SaveMarketCoverageFile(MarketCoverageFile marketCoverageFile)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var marketCoverageFileDb = _ToDbModel(marketCoverageFile);

                    context.market_coverage_files.Add(marketCoverageFileDb);

                    context.SaveChanges();
                });
        }

        /// <summary>
        /// Returns a dictionary of market code and percentage coverage based on the market ids sent.
        /// </summary>
        /// <param name="marketIds">Market id list.</param>
        /// <returns>Dictionary of market code and percentage coverage</returns>
        public MarketCoverageDto GetLatestMarketCoverages(IEnumerable<int> marketIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var lastMarketCoverageFile = context.market_coverage_files.OrderByDescending(x => x.created_date).First();

                    var marketCoveragesByMarketCode = (from m in lastMarketCoverageFile.market_coverages
                                                       where marketIds.Contains(m.market_code)
                                                       select m).ToDictionary(m => Convert.ToInt32(m.market_code), m => m.percentage_of_us);

                    return new MarketCoverageDto
                    {
                        MarketCoverageFileId = lastMarketCoverageFile.id,
                        MarketCoveragesByMarketCode = marketCoveragesByMarketCode
                    };
                 });
        }

        public MarketCoverageByStation GetLatestMarketCoveragesWithStations()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var lastMarketCoverageFile = context.market_coverage_files
                            .Include(x => x.market_coverages)
                            .Include(x => x.market_coverages.Select(mc => mc.market.stations))
                            .OrderByDescending(x => x.created_date)
                            .First();
                    
                    return new MarketCoverageByStation
                    {
                        MarketCoverageFileId = lastMarketCoverageFile.id,
                        Markets = lastMarketCoverageFile.market_coverages.Select(x => new MarketCoverageByStation.Market
                        {
                            MarketCode = x.market_code,
                            Rank = x.rank,
                            Stations = x.market.stations.Select(s => new MarketCoverageByStation.Market.Station
                            {
                                LegacyCallLetters = s.legacy_call_letters
                            }).ToList()
                        }).ToList()
                    };
                });
        }

        private market_coverage_files _ToDbModel(MarketCoverageFile marketCoverageFile)
        {
            var marketCoverageFileDb = new market_coverage_files
            {              
                file_name = marketCoverageFile.FileName,
                file_hash = marketCoverageFile.FileHash,
                created_date = marketCoverageFile.CreatedDate,
                created_by = marketCoverageFile.CreatedBy
            };

            foreach (var marketCoverage in marketCoverageFile.MarketCoverages)
            {
                marketCoverageFileDb.market_coverages.Add(new market_coverages
                {
                    rank = marketCoverage.Rank.Value,
                    market_code = (short)marketCoverage.MarketCode,
                    tv_homes = marketCoverage.TVHomes,
                    percentage_of_us = marketCoverage.PercentageOfUS
                });
            }

            return marketCoverageFileDb;
        }
    }
}
