using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

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
        /// Gets the market coverages most recently loaded into the database.
        /// </summary>
        /// <returns>List of <see cref="MarketCoverage"/></returns>
        List<MarketCoverage> GetMarketsWithLatestCoverage();

        /// <summary>
        /// Returns a dictionary of market code and percentage coverage based on the market ids sent.
        /// </summary>
        /// <param name="marketCodes">Market codes list.</param>
        /// <returns>Dictionary of market code and percentage coverage</returns>
        MarketCoverageDto GetLatestMarketCoverages(IEnumerable<int> marketCodes = null);

        MarketCoverageDto GetMarketCoveragesForFile(IEnumerable<int> marketIds, int marketCoverageFileId);
        MarketCoverageByStation GetLatestMarketCoveragesWithStations();
        List<MarketCoverageFile> GetMarketCoverageFiles();

        /// <summary>
        /// Gets the latest market coverage file.
        /// </summary>
        /// <returns></returns>
        MarketCoverageFile GetLatestMarketCoverageFile();
        MarketCoverageByStation GetMarketCoveragesWithStations(int marketCoverageFileId);

        MarketCoverageDto GetLatestTop100MarketCoverages();

        MarketCoverageDto GetLatestTop50MarketCoverages();

        MarketCoverageDto GetLatestTop25MarketCoverages();
    }

    public class MarketCoverageRepository : BroadcastRepositoryBase, IMarketCoverageRepository
    {
        public MarketCoverageRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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

        public MarketCoverageDto GetLatestTop100MarketCoverages()
        {
            return GetLatestTopMarketCoverages(100);
        }

        public MarketCoverageDto GetLatestTop50MarketCoverages()
        {
            return GetLatestTopMarketCoverages(50);
        }

        public MarketCoverageDto GetLatestTop25MarketCoverages()
        {
            return GetLatestTopMarketCoverages(25);
        }

        public MarketCoverageDto GetLatestTopMarketCoverages(int marketCount)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var lastMarketCoverageFile = context.market_coverage_files
                        .Include(x => x.market_coverages)
                        .OrderByDescending(x => x.created_date)
                        .First();

                    var marketCoveragesOrdered = lastMarketCoverageFile.market_coverages.OrderBy(x => x.rank).Take(marketCount);

                    var marketCoveragesByMarketCode = marketCoveragesOrdered.ToDictionary(m => Convert.ToInt32(m.market_code), m => m.percentage_of_us);

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
                            MarketName = x.market.geography_name,
                            Rank = x.rank,
                            Coverage = x.percentage_of_us,
                            Stations = x.market.stations.Select(s => new MarketCoverageByStation.Station
                            {
                                Id = s.id,
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
                            Stations = x.market.stations.Select(s => new MarketCoverageByStation.Station
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

        public MarketCoverageFile GetLatestMarketCoverageFile()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.market_coverage_files.OrderBy(x => x.created_date).Select(x => new MarketCoverageFile
                    {
                        Id = x.id,
                        CreatedDate = x.created_date
                    }).First();
                });
        }

        /// <inheritdoc />
        public List<MarketCoverage> GetMarketsWithLatestCoverage()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var latestFileId = context.market_coverage_files.OrderByDescending(f => f.created_date)
                        .Select(f => f.id)
                        .FirstOrDefault();
                    
                    var coverages = context.market_coverages
                        .Include(c => c.market)
                        .Where(c => c.market_coverage_file_id == latestFileId)
                        .Select(s => new MarketCoverage
                        {
                            MarketCoverageFileId = s.market_coverage_file_id,
                            Rank = s.rank,
                            Market = s.market.geography_name,
                            TVHomes = s.tv_homes,
                            PercentageOfUS = s.percentage_of_us,
                            MarketCode = s.market_code
                        })
                        .ToList();

                    return coverages;
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
