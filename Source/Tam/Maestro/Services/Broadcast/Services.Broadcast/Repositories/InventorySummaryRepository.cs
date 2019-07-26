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

        /// <summary>
        /// Gets the aggregated summary data for the source and selected quarter
        /// </summary>
        /// <param name="inventorySource">Source to filter by</param>
        /// <param name="quarter">Quarter number to filter by</param>
        /// <param name="year">Quarter year to filter by</param>
        /// <returns>InventorySummaryAggregation object</returns>
        InventorySummaryAggregation GetInventorySummaryDataForSources(InventorySource inventorySource, int quarter, int year);
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
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, inventorySummaryDto.Quarter.Quarter, inventorySummaryDto.Quarter.Year, context);

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
                        share_book_id = inventorySummaryDto.ShareBook != null ? inventorySummaryDto.ShareBook.Id : default(int?),
                        hut_book_id = inventorySummaryDto.HutBook != null ? inventorySummaryDto.HutBook.Id : default(int?),
                        inventory_summary_quarter_details = inventorySummaryDto.Details.Select(x => new inventory_summary_quarter_details
                        {
                            daypart = x.Daypart,
                            cpm = x.CPM,
                            total_coverage = x.TotalCoverage,
                            total_markets = x.TotalMarkets,
                            total_units = x.TotalUnits,
                            total_projected_impressions = x.HouseholdImpressions
                        }).ToList()
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
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

        /// <summary>
        /// Saves the open market summary data
        /// </summary>
        /// <param name="inventorySummaryDto">OpenMarketInventorySummaryDto object to be saved</param>
        public void SaveInventorySummaryForOpenMarketSources(OpenMarketInventorySummaryDto inventorySummaryDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, inventorySummaryDto.Quarter.Quarter, inventorySummaryDto.Quarter.Year, context);

                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_programs = inventorySummaryDto.TotalPrograms,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        share_book_id = inventorySummaryDto.ShareBook != null ? inventorySummaryDto.ShareBook.Id : default(int?),
                        hut_book_id = inventorySummaryDto.HutBook != null ? inventorySummaryDto.HutBook.Id : default(int?)
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                    context.SaveChanges();
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
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, inventorySummaryDto.Quarter.Quarter, inventorySummaryDto.Quarter.Year, context);

                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_daypart_codes = inventorySummaryDto.TotalDaypartCodes,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_programs = inventorySummaryDto.TotalPrograms,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        share_book_id = inventorySummaryDto.ShareBook != null ? inventorySummaryDto.ShareBook.Id : default(int?),
                        hut_book_id = inventorySummaryDto.HutBook != null ? inventorySummaryDto.HutBook.Id : default(int?),
                        inventory_summary_quarter_details = inventorySummaryDto.Details.Select(x => new inventory_summary_quarter_details
                        {
                            daypart = x.Daypart,
                            cpm = x.CPM,
                            total_coverage = x.TotalCoverage,
                            total_markets = x.TotalMarkets,
                            total_projected_impressions = x.HouseholdImpressions,
                            total_programs = x.TotalPrograms,
                            min_spots_per_week = x.MinSpotsPerWeek,
                            max_spots_per_week = x.MaxSpotsPerWeek
                        }).ToList()
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                    context.SaveChanges();
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
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, inventorySummaryDto.Quarter.Quarter, inventorySummaryDto.Quarter.Year, context);

                    context.inventory_summary.Add(_MapInventorySummaryData(inventorySummaryDto));
                    context.inventory_summary_quarters.Add(new inventory_summary_quarters
                    {
                        inventory_source_id = inventorySummaryDto.InventorySourceId,
                        quarter_number = inventorySummaryDto.Quarter.Quarter,
                        quarter_year = inventorySummaryDto.Quarter.Year,
                        total_markets = inventorySummaryDto.TotalMarkets,
                        total_programs = inventorySummaryDto.TotalPrograms,
                        total_stations = inventorySummaryDto.TotalStations,
                        total_projected_impressions = inventorySummaryDto.HouseholdImpressions,
                        share_book_id = inventorySummaryDto.ShareBook != null ? inventorySummaryDto.ShareBook.Id : default(int?),
                        hut_book_id = inventorySummaryDto.HutBook != null ? inventorySummaryDto.HutBook.Id : default(int?),
                        total_daypart_codes = 1, //syndication has only 1 daypartcode: "SYN"                        
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                    context.SaveChanges();
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
                    _RemoveExistingAggregationData(inventorySummaryDto.InventorySourceId, inventorySummaryDto.Quarter.Quarter, inventorySummaryDto.Quarter.Year, context);

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
                        share_book_id = inventorySummaryDto.ShareBook != null ? inventorySummaryDto.ShareBook.Id : default(int?),
                        hut_book_id = inventorySummaryDto.HutBook != null ? inventorySummaryDto.HutBook.Id : default(int?),
                        cpm = inventorySummaryDto.CPM,
                        inventory_summary_quarter_details = inventorySummaryDto.Details.Select(x => new inventory_summary_quarter_details
                        {
                            daypart = x.Daypart,
                            cpm = x.CPM,
                            total_projected_impressions = x.HouseholdImpressions
                        }).ToList()
                    });
                    context.inventory_summary_gaps.AddRange(_MapInventorySummaryGapsData(inventorySummaryDto));
                    context.SaveChanges();
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

        private static void _RemoveExistingAggregationData(int inventorySourceId, int quarterNumber, int year, QueryHintBroadcastContext context)
        {
            context.inventory_summary.RemoveRange(context.inventory_summary.Where(x => x.inventory_source_id == inventorySourceId).ToList());
            context.inventory_summary_quarters.RemoveRange(
                context.inventory_summary_quarters
                .Where(x => x.inventory_source_id == inventorySourceId
                        && x.quarter_number == quarterNumber
                        && x.quarter_year == year).ToList());
        }

        private List<inventory_summary_gaps> _MapInventorySummaryGapsData(InventorySummaryDto inventorySummaryDto)
        {
            return inventorySummaryDto.InventoryGaps.Select(x => new inventory_summary_gaps
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
        public InventorySummaryAggregation GetInventorySummaryDataForSources(InventorySource inventorySource, int quarter, int year)
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

                var summaryAggregationData = new InventorySummaryAggregation
                {
                    LastUpdatedDate = summaryData.last_update_date,
                    RatesAvailableFromQuarter = new InventorySummaryQuarter { Quarter = summaryData.first_quarter_number, Year = summaryData.first_quarter_year },
                    RatesAvailableToQuarter = new InventorySummaryQuarter { Quarter = summaryData.last_quarter_number, Year = summaryData.last_quarter_year },
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
                    Quarter = new InventorySummaryQuarter { Quarter = quarterData.quarter_number, Year = quarterData.quarter_year },
                    InventoryGaps = _MapInventorySummaryGapsData(quarterData.inventory_summary_gaps),
                    IsUpdating = (from file in context.inventory_files
                                  from job in file.inventory_file_ratings_jobs
                                  where file.inventory_source_id == inventorySource.Id
                                  && (job.status == (int)BackgroundJobProcessingStatus.Queued || job.status == (int)BackgroundJobProcessingStatus.Processing)
                                  select job).Any(),
                    Details = quarterData.inventory_summary_quarter_details.Select((Func<inventory_summary_quarter_details, InventorySummaryAggregation.Detail>)(x => new InventorySummaryAggregation.Detail
                    {
                        CPM = x.cpm,
                        Daypart = x.daypart,
                        TotalCoverage = x.total_coverage,
                        TotalMarkets = x.total_markets,
                        TotalProjectedHouseholdImpressions = x.total_projected_impressions,
                        TotalUnits = x.total_units,
                        MaxSpotsPerWeek = x.max_spots_per_week,
                        MinSpotsPerWeek = x.min_spots_per_week,
                        TotalPrograms = x.total_programs
                    })).ToList()
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
