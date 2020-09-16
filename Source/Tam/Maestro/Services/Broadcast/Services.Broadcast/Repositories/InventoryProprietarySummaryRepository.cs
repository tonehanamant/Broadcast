using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventoryProprietary;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Common.Services.Extensions;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryProprietarySummaryRepository : IDataRepository
    {
        /// <summary>
        /// Gets the inventory proprietary summary by station by audience.
        /// </summary>
        /// <param name="proprietaryInventoryMappingId">The proprietary inventory mapping identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>List of InventoryProprietarySummaryByStationByAudience objects</returns>
        List<InventoryProprietarySummaryByStationByAudience> GetInventoryProprietarySummaryByStationByAudience(
            int proprietaryInventoryMappingId,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        ///     Get Data to Load [inventory_proprietary_summary]
        /// </summary>
        /// <param name="inventorySource"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        List<InventoryProprietaryQuarterSummaryDto> GetInventoryProprietaryQuarterSummaries(
            InventorySource inventorySource, DateTime startDate, DateTime endDate);

        /// <summary>
        ///     Load inventory_proprietary_summary and inventory_proprietary_summary_audiences
        /// </summary>
        /// <param name="inventoryProprietaryQuarterSummaryDto"></param>
        void SaveInventoryProprietarySummary(
            InventoryProprietaryQuarterSummaryDto inventoryProprietaryQuarterSummaryDto);

        /// <summary>
        /// Gets the inventory proprietary summary.
        /// </summary>
        /// <param name="quarterDetailDto">The quarter detail dto.</param>
        /// <param name="daypartIds">The daypart ids.</param>
        /// <returns>List of InventoryProprietarySummary objects</returns>
        List<InventoryProprietarySummary> GetInventoryProprietarySummary(QuarterDetailDto quarterDetailDto,
            List<int> daypartIds);

        /// <summary>
        ///     Get Total Impressions By SummaryId And AudienceId
        /// </summary>
        /// <param name="summaryId"></param>
        /// <param name="audienceIds"></param>
        /// <returns></returns>
        double GetTotalImpressionsBySummaryIdAndAudienceIds(int summaryId, List<int> audienceIds);

        /// <summary>
        ///     Get DefaultDayPartIds from Proprietary Summary Table
        /// </summary>
        /// <param name="quarterDetailDto"></param>
        /// <returns></returns>
        List<int> GetDayPartDefaultIds(QuarterDetailDto quarterDetailDto);

        /// <summary>
        /// Gets the inventory proprietary summaries by ids.
        /// </summary>
        /// <param name="summaryIds">The summary ids.</param>
        /// <returns>List of InventoryProprietaryQuarterSummaryDto objects</returns>
        List<InventoryProprietaryQuarterSummaryDto> GetInventoryProprietarySummariesByIds(IEnumerable<int> summaryIds);

        /// <summary>
        /// Gets the market codes by summary ids.
        /// </summary>
        /// <param name="summaryIds">The summary ids.</param>
        /// <returns>List of market codes</returns>
        List<short> GetMarketCodesBySummaryIds(IEnumerable<int> summaryIds);

        /// <summary>
        /// Gets the proprietary summary unit cost.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Unit cost</returns>
        decimal? GetProprietarySummaryUnitCost(int id);
    }

    public class InventoryProprietarySummaryRepository : BroadcastRepositoryBase, IInventoryProprietarySummaryRepository
    {
        public InventoryProprietarySummaryRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
        {
        }

        private const string AggregateInventoryProprietarySummary = "AggregateInventoryProprietarySummary";

        /// <inheritdoc/>
        public List<InventoryProprietaryQuarterSummaryDto> GetInventoryProprietarySummariesByIds(IEnumerable<int> summaryIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.inventory_proprietary_summary
                        .Where(x => summaryIds.Contains(x.id))
                        .Select(x => new InventoryProprietaryQuarterSummaryDto
                        {
                            Id = x.id,
                            UnitCost = x.unit_cost,
                            SummaryByStationByAudience = x.inventory_proprietary_summary_station_audiences
                                .Select(y => new InventoryProprietarySummaryByStationByAudience
                                {
                                    AudienceId = y.audience_id,
                                    MarketCode = y.market_code,
                                    Impressions = y.impressions,
                                    StationId = y.station_id,
                                    SpotsPerWeek = y.spots_per_week,
                                    CostPerWeek = y.cost_per_week
                                })
                                .ToList()
                        })
                        .ToList();
                });
        }

        /// <inheritdoc />
        public List<InventoryProprietaryQuarterSummaryDto> GetInventoryProprietaryQuarterSummaries(
            InventorySource inventorySource,
            DateTime startDate,
            DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    const int fixedSlotNumber = 1;

                    var proprietarySummariesQuery = from week in context.station_inventory_manifest_weeks
                                                    join manifest in context.station_inventory_manifest
                                                        on week.station_inventory_manifest_id equals manifest.id
                                                    join manifestGroup in context.station_inventory_group
                                                        on manifest.station_inventory_group_id equals manifestGroup.id
                                                    join file in context.inventory_files
                                                        on manifest.file_id equals file.id
                                                    join header in context.inventory_file_proprietary_header
                                                        on file.id equals header.inventory_file_id
                                                    join daypartDefaults in context.daypart_defaults
                                                        on header.daypart_default_id equals daypartDefaults.id
                                                    join mappings in context.inventory_proprietary_daypart_program_mappings
                                                        on new { InventorySourceId = inventorySource.Id, DefaultDaypartId = daypartDefaults.id } equals
                                                        new { InventorySourceId = mappings.inventory_source_id, DefaultDaypartId = mappings.daypart_default_id }
                                                    where week.start_date <= endDate && week.end_date >= startDate &&
                                                          manifest.inventory_source_id == inventorySource.Id &&
                                                          manifestGroup.slot_number == fixedSlotNumber
                                                    group manifest by mappings.id
                                                     into g
                                                    select new InventoryProprietaryQuarterSummaryDto
                                                    {
                                                        InventorySourceId = inventorySource.Id,
                                                        SlotNumber = fixedSlotNumber,
                                                        ProprietaryDaypartProgramMappingId = g.Key,
                                                    };

                    var proprietarySummaries = proprietarySummariesQuery.ToList();

                    foreach (var proprietarySummary in proprietarySummaries)
                    {
                        var manifestIdsQuery = from week in context.station_inventory_manifest_weeks
                                               join manifest in context.station_inventory_manifest
                                                   on week.station_inventory_manifest_id equals manifest.id
                                               join manifestGroup in context.station_inventory_group
                                                   on manifest.station_inventory_group_id equals manifestGroup.id
                                               join file in context.inventory_files
                                                   on manifest.file_id equals file.id
                                               join header in context.inventory_file_proprietary_header
                                                   on file.id equals header.inventory_file_id
                                               join daypartDefaults in context.daypart_defaults
                                                   on header.daypart_default_id equals daypartDefaults.id
                                               join mappings in context.inventory_proprietary_daypart_program_mappings
                                                   on new { InventorySourceId = inventorySource.Id, DefaultDaypartId = daypartDefaults.id } equals
                                                   new { InventorySourceId = mappings.inventory_source_id, DefaultDaypartId = mappings.daypart_default_id }
                                               where week.start_date <= endDate && week.end_date >= startDate &&
                                                     mappings.id == proprietarySummary.ProprietaryDaypartProgramMappingId &&
                                                     manifestGroup.slot_number == fixedSlotNumber
                                               group manifest by manifest.id into m
                                               select m.Key;

                        var manifestIds = manifestIdsQuery.ToList();

                        foreach (var manifestId in manifestIds)
                        {
                            var manifestCostQuery = from manifest in context.station_inventory_manifest
                                                    join rates in context.station_inventory_manifest_rates
                                                        on manifest.id equals rates.station_inventory_manifest_id
                                                    let week = (from week in context.station_inventory_manifest_weeks
                                                                where week.station_inventory_manifest_id == manifest.id
                                                                select week).FirstOrDefault()
                                                    where manifest.id == manifestId &&
                                                          rates.spot_length_id == manifest.spot_length_id
                                                    select new { rates, week };

                            var manifestCost = manifestCostQuery.FirstOrDefault();

                            if (manifestCost != null)
                            {
                                var totalManifestCost = manifestCost.rates.spot_cost * manifestCost.week.spots;
                                proprietarySummary.UnitCost += totalManifestCost;
                            }
                        }
                    }

                    return proprietarySummaries.ToList();
                });
        }

        /// <inheritdoc/>
        public List<short> GetMarketCodesBySummaryIds(IEnumerable<int> summaryIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.inventory_proprietary_summary_station_audiences
                        .Where(x => summaryIds.Contains(x.inventory_proprietary_summary_id))
                        .Select(x => x.market_code)
                        .Distinct()
                        .ToList();
                });
        }

        /// <inheritdoc/>
        public List<InventoryProprietarySummaryByStationByAudience> GetInventoryProprietarySummaryByStationByAudience(
            int proprietaryInventoryMappingId,
            DateTime startDate,
            DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    const int fixedSlotNumber = 1;

                    var manifests = from week in context.station_inventory_manifest_weeks
                                    join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                                    join manifestGroup in context.station_inventory_group on manifest.station_inventory_group_id equals manifestGroup.id
                                    join file in context.inventory_files on manifest.file_id equals file.id
                                    join header in context.inventory_file_proprietary_header on file.id equals header.inventory_file_id
                                    join daypartDefaults in context.daypart_defaults on header.daypart_default_id equals daypartDefaults.id
                                    join audience in context.station_inventory_manifest_audiences on manifest.id equals audience.station_inventory_manifest_id
                                    join mapping in context.inventory_proprietary_daypart_program_mappings
                                        on new { InventorySourceId = manifest.inventory_source_id, DefaultDaypartId = daypartDefaults.id } equals
                                        new { InventorySourceId = mapping.inventory_source_id, DefaultDaypartId = mapping.daypart_default_id }
                                    where week.start_date <= endDate &&
                                          week.end_date >= startDate &&
                                          mapping.id == proprietaryInventoryMappingId &&
                                          manifestGroup.slot_number == fixedSlotNumber &&
                                          !audience.is_reference
                                    group manifest by manifest.id into m
                                    select m.Key;

                    var manifestIds = manifests.ToList();

                    var audienceQuery = from manifest in context.station_inventory_manifest
                                        join audience in context.station_inventory_manifest_audiences on manifest.id equals audience.station_inventory_manifest_id
                                        join station in context.stations on manifest.station_id equals station.id
                                        join market in context.markets on station.market_code equals market.market_code
                                        join rate in context.station_inventory_manifest_rates on manifest.id equals rate.station_inventory_manifest_id
                                        let week = (from week in context.station_inventory_manifest_weeks
                                                    where week.station_inventory_manifest_id == manifest.id
                                                    select week).FirstOrDefault()
                                        where manifestIds.Contains(manifest.id) &&
                                              rate.spot_length_id == manifest.spot_length_id
                                        select new { audience, week, station, market, rate };

                    var manifestAudiencesAndWeek = audienceQuery.ToList();

                    var result = manifestAudiencesAndWeek
                        .GroupBy(x => new { x.audience.audience_id, station_id = x.station.id })
                        .Select(x =>
                        {
                            var first = x.First();
                            return new InventoryProprietarySummaryByStationByAudience
                            {
                                AudienceId = x.Key.audience_id,
                                MarketCode = first.market.market_code,
                                StationId = x.Key.station_id,
                                Impressions = x.Sum(y => (y.audience.impressions ?? 0) * y.week.spots),
                                SpotsPerWeek = x.Sum(y => y.week.spots),
                                CostPerWeek = x.Sum(y => y.rate.spot_cost * y.week.spots)
                            };
                        })
                        .ToList();

                    return result;
                });
        }

        /// <inheritdoc/>
        public void SaveInventoryProprietarySummary(
            InventoryProprietaryQuarterSummaryDto inventoryProprietaryQuarterSummaryDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var oldSummaries = context.inventory_proprietary_summary
                        .Where(s => s.is_active &&
                                    s.inventory_source_id == inventoryProprietaryQuarterSummaryDto.InventorySourceId &&
                                    s.quarter_year == inventoryProprietaryQuarterSummaryDto.Quarter.Year &&
                                    s.quarter_number == inventoryProprietaryQuarterSummaryDto.Quarter.Quarter &&
                                    s.inventory_proprietary_daypart_program_mappings_id == inventoryProprietaryQuarterSummaryDto.ProprietaryDaypartProgramMappingId)
                        .ToList();

                    oldSummaries.ForEach(x => x.is_active = false);

                    context.inventory_proprietary_summary.Add(new inventory_proprietary_summary
                    {
                        is_active = true,
                        inventory_source_id = inventoryProprietaryQuarterSummaryDto.InventorySourceId,
                        inventory_proprietary_daypart_program_mappings_id = inventoryProprietaryQuarterSummaryDto.ProprietaryDaypartProgramMappingId,
                        quarter_number = inventoryProprietaryQuarterSummaryDto.Quarter.Quarter,
                        quarter_year = inventoryProprietaryQuarterSummaryDto.Quarter.Year,
                        unit = inventoryProprietaryQuarterSummaryDto.SlotNumber,
                        created_at = DateTime.Now,
                        created_by = AggregateInventoryProprietarySummary,
                        unit_cost = inventoryProprietaryQuarterSummaryDto.UnitCost,
                        inventory_proprietary_summary_station_audiences = inventoryProprietaryQuarterSummaryDto.SummaryByStationByAudience
                            .Select(x => new inventory_proprietary_summary_station_audiences
                            {
                                audience_id = x.AudienceId,
                                market_code = x.MarketCode,
                                station_id = x.StationId,
                                impressions = x.Impressions,
                                spots_per_week = x.SpotsPerWeek,
                                cost_per_week = x.CostPerWeek
                            })
                            .ToList()
                    });

                    context.SaveChanges();
                });
        }

        /// <inheritdoc/>
        public List<InventoryProprietarySummary> GetInventoryProprietarySummary(
            QuarterDetailDto quarterDetailDto,
            List<int> defaultDaypartIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query =
                        from summary in context.inventory_proprietary_summary
                        join source in context.inventory_sources on summary.inventory_source_id equals source.id
                        join mappings in context.inventory_proprietary_daypart_program_mappings on summary.inventory_proprietary_daypart_program_mappings_id equals mappings.id
                        join daypartDefault in context.daypart_defaults on mappings.daypart_default_id equals daypartDefault.id
                        join daypart in context.dayparts on daypartDefault.daypart_id equals daypart.id
                        join programName in context.inventory_proprietary_daypart_programs on mappings.inventory_proprietary_daypart_programs_id equals programName.id
                        where summary.is_active &&
                              defaultDaypartIds.Contains(mappings.daypart_default_id) &&
                              summary.quarter_number == quarterDetailDto.Quarter &&
                              summary.quarter_year == quarterDetailDto.Year
                        select new InventoryProprietarySummary
                        {
                            Id = summary.id,
                            DaypartName = daypart.daypart_text,
                            InventorySourceName = source.name,
                            UnitType = programName.unit_type,
                            UnitCost = summary.unit_cost
                        };

                    return query.Distinct().ToList();
                });
        }

        /// <inheritdoc/>
        public double GetTotalImpressionsBySummaryIdAndAudienceIds(int id, List<int> audienceIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from summary in context.inventory_proprietary_summary
                            from station_summary in summary.inventory_proprietary_summary_station_audiences
                            where summary.id == id &&
                                   audienceIds.Contains(station_summary.audience_id)
                            select station_summary.impressions).DefaultIfEmpty().Sum();
                });
        }

        /// <inheritdoc/>
        public List<int> GetDayPartDefaultIds(QuarterDetailDto quarterDetailDto)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var dayPartDefaultIds = context.inventory_proprietary_summary
                        .Include(x => x.inventory_proprietary_daypart_program_mappings)
                        .Where(x => x.is_active &&
                                    x.quarter_number == quarterDetailDto.Quarter &&
                                    x.quarter_year == quarterDetailDto.Year)
                        .Select(x => x.inventory_proprietary_daypart_program_mappings.daypart_default_id)
                        .ToList();

                    return dayPartDefaultIds;
                });
        }

        /// <inheritdoc/>
        public decimal? GetProprietarySummaryUnitCost(int proprietarySummaryId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var proprietarySummary = context.inventory_proprietary_summary
                        .SingleOrDefault(x => x.id == proprietarySummaryId);

                    return proprietarySummary?.unit_cost;
                });
        }
    }
}