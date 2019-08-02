using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using ConfigurationService.Client;
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
        /// <param name="marketCoverageFile">MarketCoverageFile object</param>
        void SaveMarketCoverageFile(MarketCoverageFile marketCoverageFile);
        
        /// <summary>
        /// Returns all the market coverages
        /// </summary>
        List<MarketCoverage> GetAll();

        bool HasFile(string fileHash);
        
        /// <summary>
        /// Returns a dictionary of market code and percentage coverage based on the market ids sent.
        /// </summary>
        /// <param name="marketCodes">Market codes list.</param>
        /// <returns>Dictionary of market code and percentage coverage</returns>
        MarketCoverageDto GetLatestMarketCoverages(IEnumerable<int> marketCodes = null);

        MarketCoverageDto GetMarketCoveragesForFile(IEnumerable<int> marketIds, int marketCoverageFileId);
        MarketCoverageByStation GetLatestMarketCoveragesWithStations();
        List<MarketCoverageFile> GetMarketCoverageFiles();
        MarketCoverageByStation GetMarketCoveragesWithStations(int marketCoverageFileId);
    }

    public class MarketCoverageRepository : BroadcastRepositoryBase, IMarketCoverageRepository
    {
        public MarketCoverageRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public MarketCoverageDto GetLatestMarketCoverages(IEnumerable<int> marketCodes = null)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var lastMarketCoverageFile = context.market_coverage_files
                        .Include(x => x.market_coverages)
                        .OrderByDescending(x => x.created_date)
                        .First();

                    var marketCoverages = lastMarketCoverageFile.market_coverages;

                    if (marketCodes != null)
                    {
                        marketCoverages = marketCoverages.Where(x => marketCodes.Contains(x.market_code)).ToList();
                    }

                    var marketCoveragesByMarketCode = marketCoverages.ToDictionary(m => Convert.ToInt32(m.market_code), m => m.percentage_of_us);

                    return new MarketCoverageDto
                    {
                        MarketCoverageFileId = lastMarketCoverageFile.id,
                        MarketCoveragesByMarketCode = marketCoveragesByMarketCode
                    };
                 });
        }

        public MarketCoverageDto GetMarketCoveragesForFile(IEnumerable<int> marketIds, int marketCoverageFileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var marketCoverageFile = context.market_coverage_files.Find(marketCoverageFileId);

                    var marketCoveragesByMarketCode = (from m in marketCoverageFile.market_coverages
                                                       where marketIds.Contains(m.market_code)
                                                       select m).ToDictionary(m => Convert.ToInt32(m.market_code), m => m.percentage_of_us);

                    return new MarketCoverageDto
                    {
                        MarketCoverageFileId = marketCoverageFile.id,
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
                            Coverage = x.percentage_of_us,
                            Stations = x.market.stations.Select(s => new MarketCoverageByStation.Market.Station
                            {
                                LegacyCallLetters = s.legacy_call_letters
                            }).ToList()
                        }).ToList()
                    };
                });
        }

        public MarketCoverageByStation GetMarketCoveragesWithStations(int marketCoverageFileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var lastMarketCoverageFile = context.market_coverage_files
                            .Include(x => x.market_coverages)
                            .Include(x => x.market_coverages.Select(mc => mc.market.stations))
                            .Single(x => x.id == marketCoverageFileId);

                    return new MarketCoverageByStation
                    {
                        MarketCoverageFileId = lastMarketCoverageFile.id,
                        Markets = lastMarketCoverageFile.market_coverages.Select(x => new MarketCoverageByStation.Market
                        {
                            MarketCode = x.market_code,
                            Rank = x.rank,
                            Coverage = x.percentage_of_us,
                            Stations = x.market.stations.Select(s => new MarketCoverageByStation.Market.Station
                            {
                                LegacyCallLetters = s.legacy_call_letters
                            }).ToList()
                        }).ToList()
                    };
                });
        }

        public List<MarketCoverageFile> GetMarketCoverageFiles()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.market_coverage_files.Select(x => new MarketCoverageFile
                    {
                        Id = x.id,
                        CreatedDate = x.created_date
                    }).ToList();                    
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
