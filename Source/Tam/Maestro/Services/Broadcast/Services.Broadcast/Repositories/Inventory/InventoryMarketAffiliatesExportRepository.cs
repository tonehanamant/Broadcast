using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventoryMarketAffiliates;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.Inventory
{
    public interface IInventoryMarketAffiliatesExportRepository : IDataRepository
    {
        /// <summary>
        /// Gets the market invenory list.
        /// </summary>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        /// <param name="mediaWeekIds">The media week ids.</param>
        /// <param name="affiliates">The affiliates.</param>
        /// <param name="genreIds">The genre ids.</param>
        /// <returns></returns>
        List<MarketInventoryDto> GetMarketInvenoryList(int inventorySourceId, List<int> mediaWeekIds, List<string> affiliates, List<int> genreIds);

        /// <summary>
        /// Gets the market affiliate list.
        /// </summary>
        /// <param name="affiliates">The affiliates.</param>
        /// <param name="MarketCoverageFileId">The market coverage identifier.</param>
        /// <returns></returns>
        List<MarketAffiliatesDto> GetMarketAffiliateList(List<string> affiliates, int MarketCoverageFileId);
    }

    public class InventoryMarketAffiliatesExportRepository : BroadcastRepositoryBase, IInventoryMarketAffiliatesExportRepository
    {
        public InventoryMarketAffiliatesExportRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        /// <inheritdoc />
        public List<MarketInventoryDto> GetMarketInvenoryList(int inventorySourceId, List<int> mediaWeekIds, List<string> affiliates, List<int> genreIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var marketInventoryList = (from stationInventory in context.station_inventory_manifest
                                               join weeks in context.station_inventory_manifest_weeks
                                                       on stationInventory.id equals weeks.station_inventory_manifest_id
                                               join daypart in context.station_inventory_manifest_dayparts
                                                       on stationInventory.id equals daypart.station_inventory_manifest_id
                                               join station in context.stations
                                                       on stationInventory.station_id equals station.id
                                               join programs in context.station_inventory_manifest_daypart_programs
                                                       on daypart.primary_program_id equals programs.id
                                               where stationInventory.inventory_source_id == inventorySourceId
                                                       && mediaWeekIds.Contains(weeks.media_week_id)
                                                       && daypart.primary_program_id != null
                                                       && affiliates.Contains(station.affiliation)
                                                       && genreIds.Contains(programs.maestro_genre_id)
                                               group station by new
                                               {
                                                   station.market_code,
                                                   station.affiliation
                                               } into grouping
                                               select new MarketInventoryDto
                                               {
                                                   marketCode = grouping.Key.market_code,
                                                   affiliation = grouping.Key.affiliation,
                                                   isInventory = grouping.Count() > 0 ? "Yes" : "No"
                                               }).ToList();
                    return marketInventoryList;
                });
        }

        /// <inheritdoc />
        public List<MarketAffiliatesDto> GetMarketAffiliateList(List<string> affiliates, int MarketCoverageFileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var marketAffiliatesList = (from market in context.markets
                                        join marketCoverage in context.market_coverages
                                            on market.market_code equals marketCoverage.market_code
                                        from affiliate in context.affiliates
                                        where marketCoverage.market_coverage_file_id == MarketCoverageFileId
                                            && affiliates.Contains(affiliate.name)
                                        orderby marketCoverage.rank, affiliate.name
                                        select new MarketAffiliatesDto
                                        {
                                            marketName = market.geography_name,
                                            marketCode = market.market_code,
                                            rank = marketCoverage.rank,
                                            affiliation = affiliate.name
                                        }).ToList();
                return marketAffiliatesList;
                });
        }
    }
}
