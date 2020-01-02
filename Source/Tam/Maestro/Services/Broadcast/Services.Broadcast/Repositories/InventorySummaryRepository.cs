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
using System.Data.Entity;

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
        Dictionary<int, DateTime?> GetLatestSummaryUpdatesBySource();
        /// <summary>
        /// Saves the inventory summary aggregated data for the source
        /// </summary>
        /// <param name="inventorySummaryAggregation">InventorySummaryAggregation object to be saved</param>
        void SaveInventoryAggregatedData(InventoryQuarterSummary inventorySummaryAggregation);

        /// <summary>
        /// Gets the aggregated summary data for the source and selected quarter
        /// </summary>
        /// <param name="inventorySource">Source to filter by</param>
        /// <param name="quarter">Quarter number to filter by</param>
        /// <param name="year">Quarter year to filter by</param>
        /// <returns>InventorySummaryAggregation object</returns>
        InventoryQuarterSummary GetInventorySummaryDataForSources(InventorySource inventorySource, int quarter, int year);
    }

    public class InventorySummaryRepository : BroadcastRepositoryBase, IInventorySummaryRepository
    {
        public InventorySummaryRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

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
                            join daypartDefaults in context.daypart_defaults on header.daypart_default_id equals daypartDefaults.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                StationId = station.id,
                                MarketCode = station.market_code,
                                DaypartDefaultIds = new List<int> { daypartDefaults.id },
                                UnitName = manifestGroup.name,
                                FileId = file.id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => x.FirstOrDefault())
                            .ToList();
                });
        }

        /// <inheritdoc/>
        public void SaveInventoryAggregatedData(InventoryQuarterSummary inventorySummaryAggregation)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySummaryAggregation.InventorySourceId, inventorySummaryAggregation.Quarter.Quarter, inventorySummaryAggregation.Quarter.Year, context);

                    //add new data
                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryAggregation));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryAggregation.InventorySourceId,
                        quarter_number = inventorySummaryAggregation.Quarter.Quarter,
                        quarter_year = inventorySummaryAggregation.Quarter.Year,
                        total_daypart_codes = inventorySummaryAggregation.TotalDaypartCodes,
                        total_markets = inventorySummaryAggregation.TotalMarkets,
                        total_stations = inventorySummaryAggregation.TotalStations,
                        total_projected_impressions = inventorySummaryAggregation.TotalProjectedHouseholdImpressions,
                        total_units = inventorySummaryAggregation.TotalUnits,
                        share_book_id = inventorySummaryAggregation.ShareBookId,
                        hut_book_id = inventorySummaryAggregation.HutBookId,
                        total_programs = inventorySummaryAggregation.TotalPrograms,
                        cpm = inventorySummaryAggregation.CPM,
                        inventory_summary_quarter_details = inventorySummaryAggregation.Details?.Select(x => new inventory_summary_quarter_details
                        {
                            daypart_default_id = x.DaypartDefaultId,
                            cpm = x.CPM,
                            total_coverage = x.TotalCoverage,
                            total_markets = x.TotalMarkets,
                            total_units = x.TotalUnits,
                            total_projected_impressions = x.TotalProjectedHouseholdImpressions,
                            total_programs = x.TotalPrograms,
                            min_spots_per_week = x.MinSpotsPerWeek,
                            max_spots_per_week = x.MaxSpotsPerWeek
                        }).ToList()
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryAggregation));
                    context.SaveChanges();
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
                            join daypartDefaults in context.daypart_defaults on header.daypart_default_id equals daypartDefaults.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                StationId = station.id,
                                MarketCode = station.market_code,
                                DaypartDefaultIds = new List<int> { daypartDefaults.id },
                                FileId = file.id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => x.FirstOrDefault())
                            .ToList();
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
                            join daypartDefaults in context.daypart_defaults on header.daypart_default_id equals daypartDefaults.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                DaypartDefaultIds = new List<int> { daypartDefaults.id },
                                FileId = file.id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => x.FirstOrDefault())
                            .ToList();
                });
        }

        public List<InventorySummaryManifestDto> GetInventorySummaryManifestsForDiginetSources(InventorySource inventorySource,
            DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from week in context.station_inventory_manifest_weeks
                            join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                            join manifestDaypart in context.station_inventory_manifest_dayparts on manifest.id equals manifestDaypart.station_inventory_manifest_id
                            join manifestDaypartDefault in context.daypart_defaults on manifestDaypart.daypart_default_id equals manifestDaypartDefault.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new
                            {
                                ManifestId = manifest.id,
                                DaypartCode = manifestDaypartDefault.id,
                                FileId = manifest.file_id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => new InventorySummaryManifestDto
                            {
                                ManifestId = x.Key,
                                FileId = x.FirstOrDefault().FileId,
                                DaypartDefaultIds = x.Select(d => d.DaypartCode).Distinct().ToList()
                            })
                            .ToList();
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

        /// <summary>
        /// Returns source id and timestamp for the latest inventory summary completed by source.
        /// Used for caching inventory data.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, DateTime?> GetLatestSummaryUpdatesBySource()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from source in context.inventory_summary
                            select source)
                            .ToDictionary(s => s.inventory_source_id, s => s.last_update_date);
                            
                });
        }

        private static inventory_summary _MapInventorySummaryData(InventoryQuarterSummary inventorySummaryAggregation)
        {
            return new inventory_summary
            {
                first_quarter_number = inventorySummaryAggregation.RatesAvailableFromQuarter.Quarter,
                first_quarter_year = inventorySummaryAggregation.RatesAvailableFromQuarter.Year,
                inventory_source_id = inventorySummaryAggregation.InventorySourceId,
                last_quarter_number = inventorySummaryAggregation.RatesAvailableToQuarter.Quarter,
                last_quarter_year = inventorySummaryAggregation.RatesAvailableToQuarter.Year,
                last_update_date = inventorySummaryAggregation.LastUpdatedDate,
            };
        }

        private static void _RemoveExistingAggregationData(int inventorySourceId, int quarterNumber, int year, QueryHintBroadcastContext context)
        {
            context.inventory_summary.RemoveRange(context.inventory_summary.Where(x => x.inventory_source_id == inventorySourceId).ToList());
            context.inventory_summary_quarters.RemoveRange(
                context.inventory_summary_quarters
                .Where(x => x.inventory_source_id == inventorySourceId
                        && x.quarter_number == quarterNumber
                        && x.quarter_year == year).ToList());
        }

        private List<inventory_summary_gaps> _MapInventorySummaryGapsData(InventoryQuarterSummary inventorySummaryAggregation)
        {
            return inventorySummaryAggregation.InventoryGaps.Select(x => new inventory_summary_gaps
            {
                all_quarter_missing = x.AllQuarterMissing,
                quarter_number = x.Quarter.Quarter,
                quarter_year = x.Quarter.Year,
                inventory_summary_gap_ranges = x.DateGaps?.Select(y => new inventory_summary_gap_ranges
                {
                    start_date = y.Start.Value, //these dates cannot be empty
                    end_date = y.End.Value
                }).ToList()
            }).ToList();
        }

        /// <summary>
        /// Gets the aggregated summary data for the source and selected quarter
        /// </summary>
        /// <param name="inventorySource">Source to filter by</param>
        /// <param name="quarter">Quarter number to filter by</param>
        /// <param name="year">Quarter year to filter by</param>
        /// <returns>InventorySummaryAggregation object</returns>
        public InventoryQuarterSummary GetInventorySummaryDataForSources(InventorySource inventorySource, int quarter, int year)
        {
            return _InReadUncommitedTransaction(
            context =>
            {
                var summaryData = (from summary in context.inventory_summary
                                   where summary.inventory_source_id == inventorySource.Id
                                   select new
                                   {
                                       summary.inventory_source_id,
                                       summary.first_quarter_number,
                                       summary.last_quarter_number,
                                       summary.first_quarter_year,
                                       summary.last_quarter_year,
                                       summary.last_update_date
                                   }).SingleOrDefault();

                //summaryData will be null when there is no inventory for a specific source
                if (summaryData == null)
                {
                    return null;
                }

                var quarterData = (from summaryQuarter in context.inventory_summary_quarters
                                   .Include(x => x.inventory_summary_quarter_details)
                                   .Include(x => x.inventory_summary_gaps)
                                   .Include(x => x.inventory_summary_gaps.Select(y => y.inventory_summary_gap_ranges))
                                   where summaryQuarter.inventory_source_id == inventorySource.Id && summaryQuarter.quarter_number == quarter && summaryQuarter.quarter_year == year
                                   select summaryQuarter).SingleOrDefault();
                //quarterData will be null when there is no inventory for a specific quarter
                if (quarterData == null) return null;

                var summaryAggregationData = new InventoryQuarterSummary
                {
                    LastUpdatedDate = summaryData.last_update_date,
                    RatesAvailableFromQuarter = new QuarterDto { Quarter = summaryData.first_quarter_number, Year = summaryData.first_quarter_year },
                    RatesAvailableToQuarter = new QuarterDto { Quarter = summaryData.last_quarter_number, Year = summaryData.last_quarter_year },
                    InventorySourceId = inventorySource.Id,
                    InventorySourceName = inventorySource.Name,
                    TotalProjectedHouseholdImpressions = quarterData.total_projected_impressions,
                    HutBookId = quarterData.hut_book_id,
                    ShareBookId = quarterData.share_book_id,
                    TotalDaypartCodes = quarterData.total_daypart_codes,
                    TotalMarkets = quarterData.total_markets,
                    TotalStations = quarterData.total_stations,
                    TotalPrograms = quarterData.total_programs,
                    TotalUnits = quarterData.total_units,
                    CPM = quarterData.cpm,
                    Quarter = new QuarterDto { Quarter = quarterData.quarter_number, Year = quarterData.quarter_year },
                    InventoryGaps = _MapInventorySummaryGapsData(quarterData.inventory_summary_gaps),
                    IsUpdating = (from file in context.inventory_files
                                  from job in file.inventory_file_ratings_jobs
                                  where file.inventory_source_id == inventorySource.Id
                                  && (job.status == (int)BackgroundJobProcessingStatus.Queued || job.status == (int)BackgroundJobProcessingStatus.Processing)
                                  select job).Any(),
                    Details = quarterData.inventory_summary_quarter_details.Select(x => new InventoryQuarterSummary.Detail
                    {
                        CPM = x.cpm,
                        DaypartDefaultId = x.daypart_default_id,
                        DaypartCode = x.daypart_defaults?.daypart.code,
                        TotalCoverage = x.total_coverage,
                        TotalMarkets = x.total_markets,
                        TotalProjectedHouseholdImpressions = x.total_projected_impressions,
                        TotalUnits = x.total_units,
                        MaxSpotsPerWeek = x.max_spots_per_week,
                        MinSpotsPerWeek = x.min_spots_per_week,
                        TotalPrograms = x.total_programs
                    }).OrderBy(x=>x.DaypartDefaultId).ToList()
                };
                return summaryAggregationData;
            });
        }

        private List<InventoryGapDetail> _MapInventorySummaryGapsData(ICollection<inventory_summary_gaps> inventory_summary_gaps)
        {
            return inventory_summary_gaps.Select(x => new InventoryGapDetail
            {
                AllQuarterMissing = x.all_quarter_missing,
                Quarter = new QuarterDetailDto { Quarter = x.quarter_number, Year = x.quarter_year },
                DateGaps = x.inventory_summary_gap_ranges.Select(y => new DateRange(y.start_date, y.end_date)).ToList()
            }).ToList();
        }
    }
}
