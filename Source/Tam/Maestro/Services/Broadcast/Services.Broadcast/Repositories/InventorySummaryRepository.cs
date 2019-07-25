using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using System;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Repositories
{
    public interface IInventorySummaryRepository : IDataRepository
    {
        List<InventorySummaryManifestDto> GetInventorySummaryManifestsForBarterSources(InventorySource inventorySource, DateTime startDate, DateTime endDate);
        List<InventorySummaryManifestDto> GetInventorySummaryManifestsForOpenMarketSources(InventorySource inventorySource, DateTime startDate, DateTime endDate);
        List<InventorySummaryManifestDto> GetInventorySummaryManifestsForProprietaryOAndOSources(InventorySource inventorySource, DateTime startDate, DateTime endDate);
        List<InventorySummaryManifestDto> GetInventorySummaryManifestsForSyndicationSources(InventorySource inventorySource, DateTime startDate, DateTime endDate);
        List<InventorySummaryManifestDto> GetInventorySummaryManifestsForDiginetSources(InventorySource inventorySource, DateTime startDate, DateTime endDate);
        List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFileDtos(List<int> inventoryFileIds);

        /// <summary>
        /// Saves the barter summary data
        /// </summary>
        /// <param name="inventorySummaryDto">BarterInventorySummaryDto object to be saved</param>
        void SaveInventorySummaryForBarterSources(BarterInventorySummaryDto inventorySummaryDto);

        /// <summary>
        /// Saves the open market summary data
        /// </summary>
        /// <param name="inventorySummaryDto">BarterInventorySummaryDto object to be saved</param>
        void SaveInventorySummaryForOpenMarketSources(OpenMarketInventorySummaryDto inventorySummaryDto);

        /// <summary>
        /// Saves the Proprietary O&O summary data
        /// </summary>
        /// <param name="inventorySummaryDto">ProprietaryOAndOInventorySummaryDto object to be saved</param>
        void SaveInventorySummaryForProprietaryOAndOSources(ProprietaryOAndOInventorySummaryDto inventorySummaryDto);

        /// <summary>
        /// Saves the Syndication summary data
        /// </summary>
        /// <param name="inventorySummaryDto">SyndicationInventorySummaryDto object to be saved</param>
        void SaveInventorySummaryForSyndicationSources(SyndicationInventorySummaryDto inventorySummaryDto);

        /// <summary>
        /// Saves the diginet summary data
        /// </summary>
        /// <param name="inventorySummaryDto">DiginetInventorySummaryDto object to be saved</param>
        void SaveInventorySummaryForDiginetSources(DiginetInventorySummaryDto inventorySummaryDto);
    }

    public class InventorySummaryRepository : BroadcastRepositoryBase, IInventorySummaryRepository
    {
        public InventorySummaryRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<InventorySummaryManifestDto> GetInventorySummaryManifestsForBarterSources(InventorySource inventorySource, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from week in context.station_inventory_manifest_weeks
                            join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                            join manifestGroup in context.station_inventory_group on manifest.station_inventory_group_id equals manifestGroup.id
                            join station in context.stations on manifest.station_id equals station.id
                            join file in context.inventory_files on manifest.file_id equals file.id
                            join header in context.inventory_file_proprietary_header on file.id equals header.inventory_file_id
                            join daypartCode in context.daypart_codes on header.daypart_code_id equals daypartCode.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                StationId = station.id,
                                MarketCode = station.market_code,
                                DaypartCodes = new List<string> { daypartCode.code },
                                UnitName = manifestGroup.name,
                                FileId = file.id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => x.FirstOrDefault())
                            .ToList();
                });
        }

        /// <summary>
        /// Saves the barter summary data
        /// </summary>
        /// <param name="inventorySummaryDto">BarterInventorySummaryDto object to be saved</param>
        public void SaveInventorySummaryForBarterSources(BarterInventorySummaryDto inventorySummaryDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, context);

                    //add new data
                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_daypart_codes = inventorySummaryDto.TotalDaypartCodes,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        total_units = inventorySummaryDto.TotalUnits,
                        share_book_id = inventorySummaryDto.ShareBook.Id,
                        hut_book_id = inventorySummaryDto.HutBook.Id,
                        inventory_summary_quarter_details = inventorySummaryDto.Details.Select(x => new inventory_summary_quarter_details
                        {
                            daypart = x.Daypart,
                            cpm = x.CPM,
                            total_coverage = x.TotalCoverage,
                            total_markets = x.TotalMarkets,
                            total_units = x.TotalUnits,
                            total_projected_impressions = x.HouseholdImpressions
                        }).ToList(),
                        
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                });
        }

        public List<InventorySummaryManifestDto> GetInventorySummaryManifestsForOpenMarketSources(InventorySource inventorySource, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.station_inventory_manifest_weeks
                        .Where(w => w.start_date <= endDate && w.end_date >= startDate)
                        .GroupBy(x => x.station_inventory_manifest_id)
                        .Select(x => x.FirstOrDefault().station_inventory_manifest)
                        .Where(x => x.inventory_source_id == inventorySource.Id)
                        .Select(x => new InventorySummaryManifestDto
                        {
                            ManifestId = x.id,
                            StationId = x.station_id,
                            MarketCode = x.station.market_code,
                            FileId = x.file_id
                        })
                        .ToList();
                });
        }

        /// <summary>
        /// Saves the open market summary data
        /// </summary>
        /// <param name="inventorySummaryDto">OpenMarketInventorySummaryDto object to be saved</param>
        public void SaveInventorySummaryForOpenMarketSources(OpenMarketInventorySummaryDto inventorySummaryDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, context);

                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        total_programs = inventorySummaryDto.TotalPrograms,
                        share_book_id = inventorySummaryDto.ShareBook.Id,
                        hut_book_id = inventorySummaryDto.HutBook.Id
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                });
        }
        
        public List<InventorySummaryManifestDto> GetInventorySummaryManifestsForProprietaryOAndOSources(InventorySource inventorySource, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from week in context.station_inventory_manifest_weeks
                            join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                            join station in context.stations on manifest.station_id equals station.id
                            join file in context.inventory_files on manifest.file_id equals file.id
                            join header in context.inventory_file_proprietary_header on file.id equals header.inventory_file_id
                            join daypartCode in context.daypart_codes on header.daypart_code_id equals daypartCode.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                StationId = station.id,
                                MarketCode = station.market_code,
                                DaypartCodes = new List<string> { daypartCode.code },
                                FileId = file.id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => x.FirstOrDefault())
                            .ToList();
                });
        }

        /// <summary>
        /// Saves the Proprietary O&O summary data
        /// </summary>
        /// <param name="inventorySummaryDto">ProprietaryOAndOInventorySummaryDto object to be saved</param>
        public void SaveInventorySummaryForProprietaryOAndOSources(ProprietaryOAndOInventorySummaryDto inventorySummaryDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, context);

                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_daypart_codes = inventorySummaryDto.TotalDaypartCodes,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        total_programs = inventorySummaryDto.TotalPrograms,
                        share_book_id = inventorySummaryDto.ShareBook.Id,
                        hut_book_id = inventorySummaryDto.HutBook.Id,
                        inventory_summary_quarter_details = inventorySummaryDto.Details.Select(x => new inventory_summary_quarter_details
                        {
                            daypart = x.Daypart,
                            cpm = x.CPM,
                            total_coverage = x.TotalCoverage,
                            total_markets = x.TotalMarkets,
                            total_projected_impressions = x.HouseholdImpressions,
                            total_programs = x.TotalPrograms,
                            min_spots_per_week = x.MinSpotsPerWeek,
                            max_spots_per_week= x.MaxSpotsPerWeek
                        }).ToList()
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                });
        }

        public List<InventorySummaryManifestDto> GetInventorySummaryManifestsForSyndicationSources(InventorySource inventorySource, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from week in context.station_inventory_manifest_weeks
                            join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                            join file in context.inventory_files on manifest.file_id equals file.id
                            join header in context.inventory_file_proprietary_header on file.id equals header.inventory_file_id
                            join daypartCode in context.daypart_codes on header.daypart_code_id equals daypartCode.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                DaypartCodes = new List<string> { daypartCode.code },
                                FileId = file.id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => x.FirstOrDefault())
                            .ToList();
                });
        }

        /// <summary>
        /// Saves the Syndication summary data
        /// </summary>
        /// <param name="inventorySummaryDto">SyndicationInventorySummaryDto object to be saved</param>
        public void SaveInventorySummaryForSyndicationSources(SyndicationInventorySummaryDto inventorySummaryDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, context);

                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        total_programs = inventorySummaryDto.TotalPrograms,
                        total_daypart_codes = 1, //syndication has only 1 daypartcode: "SYN"                        
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                });
        }

        public List<InventorySummaryManifestDto> GetInventorySummaryManifestsForDiginetSources(InventorySource inventorySource, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from week in context.station_inventory_manifest_weeks
                            join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                            join manifestDaypart in context.station_inventory_manifest_dayparts on manifest.id equals manifestDaypart.station_inventory_manifest_id
                            join manifestDaypartCode in context.daypart_codes on manifestDaypart.daypart_code_id equals manifestDaypartCode.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new
                            {
                                ManifestId = manifest.id,
                                DaypartCode = manifestDaypartCode.code,
                                FileId = manifest.file_id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => new InventorySummaryManifestDto
                            {
                                ManifestId = x.Key,
                                FileId = x.FirstOrDefault().FileId,
                                DaypartCodes = x.Select(d => d.DaypartCode).Distinct().ToList()
                            })
                            .ToList();
                });
        }

        /// <summary>
        /// Saves the diginet summary data
        /// </summary>
        /// <param name="inventorySummaryDto">DiginetInventorySummaryDto object to be saved</param>
        public void SaveInventorySummaryForDiginetSources(DiginetInventorySummaryDto inventorySummaryDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, context);

                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_daypart_codes = inventorySummaryDto.TotalDaypartCodes,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        cpm = inventorySummaryDto.CPM,
                        inventory_summary_quarter_details = inventorySummaryDto.Details.Select(x => new inventory_summary_quarter_details
                        {
                            daypart = x.Daypart,
                            cpm = x.CPM,
                            total_projected_impressions = x.HouseholdImpressions
                        }).ToList()
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                });
        }

        public List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFileDtos(List<int> inventoryFileIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from file in context.inventory_files
                            from header in file.inventory_file_proprietary_header.DefaultIfEmpty()
                            from job in file.inventory_file_ratings_jobs.DefaultIfEmpty()
                            where inventoryFileIds.Contains(file.id)
                            select new
                            {
                                file_id = file.id,
                                job.completed_at,
                                job.status,
                                header.hut_projection_book_id,
                                header.share_projection_book_id,
                                file.created_date,
                            }).Select(f => new InventorySummaryManifestFileDto
                            {
                                FileId = f.file_id,
                                JobCompletedDate = f.completed_at,
                                JobStatus = (BackgroundJobProcessingStatus?)f.status,
                                HutProjectionBookId = f.hut_projection_book_id,
                                ShareProjectionBookId = f.share_projection_book_id,
                                CreatedDate = f.created_date
                            }).ToList();
                });
        }

        private static inventory_summary _MapInventorySummaryData<T>(T inventorySummaryDto) where T : InventorySummaryDto
        {
            return new inventory_summary
            {
                first_quarter_number = inventorySummaryDto.RatesAvailableFromQuarter.Quarter,
                first_quarter_year = inventorySummaryDto.RatesAvailableFromQuarter.Year,
                inventory_source_id = inventorySummaryDto.InventorySourceId,
                last_quarter_number = inventorySummaryDto.RatesAvailableToQuarter.Quarter,
                last_quarter_year = inventorySummaryDto.RatesAvailableToQuarter.Year,
                last_update_date = inventorySummaryDto.LastUpdatedDate,
            };
        }

        private static void _RemoveExistingAggregationData(int inventorySourceId, QueryHintBroadcastContext context)
        {
            context.inventory_summary.RemoveRange(context.inventory_summary.Where(x => x.inventory_source_id == inventorySourceId).ToList());
            context.inventory_summary_quarters.RemoveRange(context.inventory_summary_quarters.Where(x => x.inventory_source_id == inventorySourceId).ToList());
        }
        
        private List<inventory_summary_gaps> _MapInventorySummaryGapsData(InventorySummaryDto inventorySummaryDto)
        {
            return inventorySummaryDto.InventoryGaps.Select(x => new inventory_summary_gaps
            {
                all_quarter_missing = x.AllQuarterMissing,
                inventory_summary_gap_ranges = x.DateGaps.Select(y => new inventory_summary_gap_ranges
                {
                    start_date = y.Start.Value, //these dates cannot be empty
                    end_date = y.End.Value
                }).ToList()
            }).ToList();
        }
    }
}
