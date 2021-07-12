using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

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
        /// Saves the inventory aggregated data.
        /// </summary>
        /// <param name="inventorySummaryAggregation">The inventory summary aggregation.</param>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        /// <param name="affectedQuarters">The affected quarters. Null means all.</param>
        void SaveInventoryAggregatedData(List<InventoryQuarterSummary> inventorySummaryAggregation, int inventorySourceId, List<QuarterDetailDto> affectedQuarters);

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
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

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
                            join standardDayparts in context.standard_dayparts on header.standard_daypart_id equals standardDayparts.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                StationId = station.id,
                                MarketCode = station.market_code,
                                StandardDaypartIds = new List<int> { standardDayparts.id },
                                UnitName = manifestGroup.name,
                                FileId = file.id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => x.FirstOrDefault())
                            .ToList();
                });
        }
        
        /// <inheritdoc/>
        public void SaveInventoryAggregatedData(List<InventoryQuarterSummary> inventorySummaryAggregation, int inventorySourceId, 
            List<QuarterDetailDto> affectedQuarters)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySourceId, context, affectedQuarters);

                    var first = inventorySummaryAggregation.FirstOrDefault();

                    if (first == null)
                        return;

                    //add new data
                    context.inventory_summary.Add(_MapInventorySummaryData(first));

                    foreach (var summaryForQuarter in inventorySummaryAggregation)
                    {
                        context.inventory_summary_quarters.Add(new inventory_summary_quarters
                        {
                            inventory_source_id = summaryForQuarter.InventorySourceId,
                            quarter_number = summaryForQuarter.Quarter.Quarter,
                            quarter_year = summaryForQuarter.Quarter.Year,
                            total_daypart_codes = summaryForQuarter.TotalDaypartCodes,
                            total_markets = summaryForQuarter.TotalMarkets,
                            total_stations = summaryForQuarter.TotalStations,
                            total_projected_impressions = summaryForQuarter.TotalProjectedHouseholdImpressions,
                            total_units = summaryForQuarter.TotalUnits,
                            share_book_id = summaryForQuarter.ShareBookId,
                            hut_book_id = summaryForQuarter.HutBookId,
                            total_programs = summaryForQuarter.TotalPrograms,
                            cpm = summaryForQuarter.CPM,
                            inventory_summary_quarter_details = summaryForQuarter.Details?.Select(x => new inventory_summary_quarter_details
                            {
                                standard_daypart_id = x.StandardDaypartId,
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
                    }

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
                            join standardDayparts in context.standard_dayparts on header.standard_daypart_id equals standardDayparts.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                StationId = station.id,
                                MarketCode = station.market_code,
                                StandardDaypartIds = new List<int> { standardDayparts.id },
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
                            join standardDayparts in context.standard_dayparts on header.standard_daypart_id equals standardDayparts.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new InventorySummaryManifestDto
                            {
                                ManifestId = manifest.id,
                                StandardDaypartIds = new List<int> { standardDayparts.id },
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
                            join manifestStandardDaypart in context.standard_dayparts on manifestDaypart.standard_daypart_id equals manifestStandardDaypart.id
                            where week.start_date <= endDate && week.end_date >= startDate && manifest.inventory_source_id == inventorySource.Id
                            select new
                            {
                                ManifestId = manifest.id,
                                DaypartCode = manifestStandardDaypart.id,
                                FileId = manifest.file_id
                            })
                            .GroupBy(x => x.ManifestId)
                            .Select(x => new InventorySummaryManifestDto
                            {
                                ManifestId = x.Key,
                                FileId = x.FirstOrDefault().FileId,
                                StandardDaypartIds = x.Select(d => d.DaypartCode).Distinct().ToList()
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
                inventory_summary_gaps = _MapInventorySummaryGapsData(inventorySummaryAggregation)
            };
        }

        private static void _RemoveExistingAggregationData(int inventorySourceId, QueryHintBroadcastContext context, List<QuarterDetailDto> quarters)
        {
            context.inventory_summary.RemoveRange(context.inventory_summary.Where(x => x.inventory_source_id == inventorySourceId).ToList());

            if (quarters?.Any() == true)
            {
                var quarterIdentifiers = quarters.Select(s => $"{s.Year}_Q{s.Quarter}").ToList();
                var allQuarters = context.inventory_summary_quarters.Where(x => x.inventory_source_id == inventorySourceId).ToList();
                var quartersToRemove = allQuarters.Where(s => quarterIdentifiers.Contains($"{s.quarter_year}_Q{s.quarter_number}")).ToList();
                context.inventory_summary_quarters.RemoveRange(quartersToRemove);
            }
            else
            {
                // remove all of them.
                context.inventory_summary_quarters.RemoveRange(context.inventory_summary_quarters.Where(x => x.inventory_source_id == inventorySourceId).ToList());
            }
        }

        private static List<inventory_summary_gaps> _MapInventorySummaryGapsData(InventoryQuarterSummary inventorySummaryAggregation)
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
                                   select summary).SingleOrDefault();

                var summaryAggregationData = new InventoryQuarterSummary
                {
                    InventorySourceId = inventorySource.Id,
                    InventorySourceName = inventorySource.Name,
                    IsUpdating = (from file in context.inventory_files
                                  from job in file.inventory_file_ratings_jobs
                                  where file.inventory_source_id == inventorySource.Id
                                  && (job.status == (int)BackgroundJobProcessingStatus.Queued || job.status == (int)BackgroundJobProcessingStatus.Processing)
                                  select job).Any()
                };
                
                if (summaryData != null)
                {
                    summaryAggregationData.HasInventorySourceSummary = true;
                    summaryAggregationData.LastUpdatedDate = summaryData.last_update_date;
                    summaryAggregationData.RatesAvailableFromQuarter = new QuarterDto { Quarter = summaryData.first_quarter_number, Year = summaryData.first_quarter_year };
                    summaryAggregationData.RatesAvailableToQuarter = new QuarterDto { Quarter = summaryData.last_quarter_number, Year = summaryData.last_quarter_year };
                    summaryAggregationData.InventoryGaps = _MapInventorySummaryGapsData(summaryData.inventory_summary_gaps);
                }

                var quarterData = (from summaryQuarter in context.inventory_summary_quarters
                                   .Include(x => x.inventory_summary_quarter_details)
                                   where summaryQuarter.inventory_source_id == inventorySource.Id && summaryQuarter.quarter_number == quarter && summaryQuarter.quarter_year == year
                                   select summaryQuarter).SingleOrDefault();
                
                if (quarterData != null)
                {
                    summaryAggregationData.HasInventorySourceSummaryQuarterDetails = true;
                    summaryAggregationData.TotalProjectedHouseholdImpressions = quarterData.total_projected_impressions;
                    summaryAggregationData.HutBookId = quarterData.hut_book_id;
                    summaryAggregationData.ShareBookId = quarterData.share_book_id;
                    summaryAggregationData.TotalDaypartCodes = quarterData.total_daypart_codes;
                    summaryAggregationData.TotalMarkets = quarterData.total_markets;
                    summaryAggregationData.TotalStations = quarterData.total_stations;
                    summaryAggregationData.TotalPrograms = quarterData.total_programs;
                    summaryAggregationData.TotalUnits = quarterData.total_units;
                    summaryAggregationData.CPM = quarterData.cpm;
                    summaryAggregationData.Quarter = new QuarterDto { Quarter = quarterData.quarter_number, Year = quarterData.quarter_year };
                    summaryAggregationData.Details = quarterData.inventory_summary_quarter_details.Select(x => new InventoryQuarterSummary.Detail
                    {
                        CPM = x.cpm,
                        StandardDaypartId = x.standard_daypart_id,
                        DaypartCode = x.standard_dayparts?.code,
                        TotalCoverage = x.total_coverage,
                        TotalMarkets = x.total_markets,
                        TotalProjectedHouseholdImpressions = x.total_projected_impressions,
                        TotalUnits = x.total_units,
                        MaxSpotsPerWeek = x.max_spots_per_week,
                        MinSpotsPerWeek = x.min_spots_per_week,
                        TotalPrograms = x.total_programs
                    }).OrderBy(x => x.StandardDaypartId).ToList();
                }

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
