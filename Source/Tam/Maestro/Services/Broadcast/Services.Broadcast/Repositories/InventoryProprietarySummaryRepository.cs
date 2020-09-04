using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Common.Services.Extensions;
using Services.Broadcast.Entities.StationInventory;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using Services.Broadcast.Entities.ProgramMapping;

namespace Services.Broadcast.Repositories
{
	public interface IInventoryProprietarySummaryRepository : IDataRepository
	{
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
        ///     Get Data to Load [inventory_proprietary_summary_audiences]
        /// </summary>
        /// <param name="proprietaryInventoryMappingId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        List<InventoryProprietarySummaryAudiencesDto> GetInventoryProprietarySummaryAudiences(
			int proprietaryInventoryMappingId, DateTime startDate, DateTime endDate);

        /// <summary>
        ///     Get Data to Load [inventory_proprietary_summary_markets]
        /// </summary>
        /// <param name="proprietaryInventoryMappingId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        List<short?> GetMarketCodesForInventoryProprietarySummary(
            int proprietaryInventoryMappingId, DateTime startDate, DateTime endDate);

		/// <summary>
		///     Load inventory_proprietary_summary and inventory_proprietary_summary_audiences
		/// </summary>
		/// <param name="inventoryProprietaryQuarterSummaryDto"></param>
		void SaveInventoryProprietarySummary(
			InventoryProprietaryQuarterSummaryDto inventoryProprietaryQuarterSummaryDto);

		/// <summary>
		/// </summary>
		/// <param name="quarterDetailDto"></param>
		/// <param name="daypartIds"></param>
		/// <returns></returns>
		List<InventoryProprietarySummary> GetInventoryProprietarySummary(QuarterDetailDto quarterDetailDto,
			HashSet<int> daypartIds);

        /// <summary>
        /// </summary>
        /// <param name="summaryId"></param>
        /// <returns></returns>
        double GetTotalMarketCoverageBySummaryId(int summaryId);

        /// <summary>
        ///     Get Total Impressions By SummaryId And AudienceId
        /// </summary>
        /// <param name="summaryId"></param>
        /// <param name="audienceIds"></param>
        /// <returns></returns>
        double GetTotalImpressionsBySummaryIdAndAudienceIds(int summaryId, List<int> audienceIds);

		/// <summary>
		///     Get DayPartIds from Proprietary Summary Table
		/// </summary>
		/// <param name="quarterDetailDto"></param>
		/// <returns></returns>
		List<int> GetDaypartIds(QuarterDetailDto quarterDetailDto);

		/// <summary>
		///     Get list of  Market code and market Coverage
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		List<PlanMarketDto> GetMarketDataBySummaryIds(List<int> ids);

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

                    foreach(var proprietarySummary in proprietarySummaries)
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
                                            where manifestIds.Contains(manifest.id) &&
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

        public List<short?> GetMarketCodesForInventoryProprietarySummary(
            int proprietaryInventoryMappingId,
			DateTime startDate, 
            DateTime endDate)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
                    const int fixedSlotNumber = 1;

					var query = from week in context.station_inventory_manifest_weeks
						        join manifest in context.station_inventory_manifest 
                                    on week.station_inventory_manifest_id equals manifest.id
						        join station in context.stations 
                                    on manifest.station_id equals station.id
						        join manifestGroup in context.station_inventory_group 
                                    on manifest.station_inventory_group_id equals manifestGroup.id
						        join file in context.inventory_files 
                                    on manifest.file_id equals file.id
						        join header in context.inventory_file_proprietary_header 
                                    on file.id equals header.inventory_file_id
						        join daypartDefaults in context.daypart_defaults 
                                    on header.daypart_default_id equals daypartDefaults.id
                                join mapping in context.inventory_proprietary_daypart_program_mappings
                                    on new { InventorySourceId = manifest.inventory_source_id, DefaultDaypartId = daypartDefaults.id } equals
                                    new { InventorySourceId = mapping.inventory_source_id, DefaultDaypartId = mapping.daypart_default_id }
                                where week.start_date <= endDate && week.end_date >= startDate &&
                                      mapping.id == proprietaryInventoryMappingId &&
                                      manifestGroup.slot_number == fixedSlotNumber
                                group station by station.market_code into g
						        select g.Key;

					return query.Distinct().ToList();
				}
			);
		}

		public List<InventoryProprietarySummaryAudiencesDto> GetInventoryProprietarySummaryAudiences(
			int proprietaryInventoryMappingId, DateTime startDate, DateTime endDate)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
                    const int fixedSlotNumber = 1;

                    var manifests = from week in context.station_inventory_manifest_weeks
                                    join manifest in context.station_inventory_manifest
                                        on week.station_inventory_manifest_id equals manifest.id
                                    join manifestGroup in context.station_inventory_group on manifest.station_inventory_group_id
                                        equals manifestGroup.id
                                    join file in context.inventory_files
                                        on manifest.file_id equals file.id
                                    join header in context.inventory_file_proprietary_header
                                        on file.id equals header.inventory_file_id
                                    join daypartDefaults in context.daypart_defaults
                                        on header.daypart_default_id equals daypartDefaults.id
                                    join audience in context.station_inventory_manifest_audiences
                                        on manifest.id equals audience.station_inventory_manifest_id
                                    join mapping in context.inventory_proprietary_daypart_program_mappings
                                        on new { InventorySourceId = manifest.inventory_source_id, DefaultDaypartId = daypartDefaults.id } equals
                                        new { InventorySourceId = mapping.inventory_source_id, DefaultDaypartId = mapping.daypart_default_id }
                                    where week.start_date <= endDate && week.end_date >= startDate &&
                                          mapping.id == proprietaryInventoryMappingId &&
                                          manifestGroup.slot_number == fixedSlotNumber &&
                                          !audience.is_reference
                                    group manifest by manifest.id into m
                                    select m.Key;

                    var manifestIds = manifests.ToList();
                    var audienceDictionary = new Dictionary<int, double>();

                    foreach(var manifestId in manifestIds)
                    {
                        var audienceQuery = from manifest in context.station_inventory_manifest
                                            join audience in context.station_inventory_manifest_audiences
                                                on manifest.id equals audience.station_inventory_manifest_id
                                            let week = (from week in context.station_inventory_manifest_weeks
                                                        where week.station_inventory_manifest_id == manifest.id
                                                        select week).FirstOrDefault()
                                            where manifestIds.Contains(manifest.id)
                                            select new { audience, week };

                        var manifestAudiencesAndWeek = audienceQuery.ToList();

                        foreach (var manifestAudienceAndWeek in manifestAudiencesAndWeek)
                        {
                            var impressions = (manifestAudienceAndWeek.audience.impressions ?? 0) *
                                                    manifestAudienceAndWeek.week.spots;

                            audienceDictionary.TryGetValue(manifestAudienceAndWeek.audience.audience_id, out double totalImpressions);

                            audienceDictionary[manifestAudienceAndWeek.audience.audience_id] = totalImpressions + impressions;
                        }
                    }

                    return audienceDictionary.Select(x => new InventoryProprietarySummaryAudiencesDto
                    {
                        AudienceId = x.Key,
                        Impressions = x.Value
                    }).ToList();
				});
		}

		public void SaveInventoryProprietarySummary(
			InventoryProprietaryQuarterSummaryDto inventoryProprietaryQuarterSummaryDto)
		{
			_InReadUncommitedTransaction(
				context =>
				{
					_RemoveExistingInventoryProprietarySummary(inventoryProprietaryQuarterSummaryDto, context);
                    context.inventory_proprietary_summary.Add(new inventory_proprietary_summary
                    {
                        inventory_source_id = inventoryProprietaryQuarterSummaryDto.InventorySourceId,
                        inventory_proprietary_daypart_program_mappings_id = inventoryProprietaryQuarterSummaryDto.ProprietaryDaypartProgramMappingId,
                        quarter_number = inventoryProprietaryQuarterSummaryDto.Quarter.Quarter,
						quarter_year = inventoryProprietaryQuarterSummaryDto.Quarter.Year,
						unit = inventoryProprietaryQuarterSummaryDto.SlotNumber,
						created_at = DateTime.Now,
						created_by = AggregateInventoryProprietarySummary,
						modified_at = DateTime.Now,
						modified_by = AggregateInventoryProprietarySummary,
                        unit_cost = inventoryProprietaryQuarterSummaryDto.UnitCost,
                        inventory_proprietary_summary_markets = inventoryProprietaryQuarterSummaryDto.Markets?.Select(
							x => new inventory_proprietary_summary_markets
							{
								market_code = x.MarketCode,
								market_coverage = x.MarketCoverage,
								created_at = DateTime.Now,
								created_by = AggregateInventoryProprietarySummary,
								modified_at = DateTime.Now,
								modified_by = AggregateInventoryProprietarySummary
							}).ToList(),
						inventory_proprietary_summary_audiences = inventoryProprietaryQuarterSummaryDto.Audiences
							?.Select(
								x => new inventory_proprietary_summary_audiences
								{
									audience_id = x.AudienceId,
									impressions = x.Impressions,
									created_at = DateTime.Now,
									created_by = AggregateInventoryProprietarySummary,
									modified_at = DateTime.Now,
									modified_by = AggregateInventoryProprietarySummary
								}).ToList()
					});
					context.SaveChanges();
				});
		}

		private static void _RemoveExistingInventoryProprietarySummary(InventoryProprietaryQuarterSummaryDto dto,
			QueryHintBroadcastContext context)
		{
			var idList = context.inventory_proprietary_summary
				.Where(s => s.inventory_source_id.Equals(dto.InventorySourceId) &&
				            s.quarter_year.Equals(dto.Quarter.Year) && 
                            s.quarter_number.Equals(dto.Quarter.Quarter) &&
                            s.inventory_proprietary_daypart_program_mappings_id == dto.ProprietaryDaypartProgramMappingId)
				.Select(s => s.id).ToList();

            var audiencesToRemoved = context.inventory_proprietary_summary_audiences
				.Where(a => idList.Contains(a.inventory_proprietary_summary_id)).ToList();

			context.inventory_proprietary_summary_audiences.RemoveRange(audiencesToRemoved);

			var marketsToBeRemoved = context.inventory_proprietary_summary_markets
				.Where(m => idList.Contains(m.inventory_proprietary_summary_id)).ToList();
			context.inventory_proprietary_summary_markets.RemoveRange(marketsToBeRemoved);
			context.inventory_proprietary_summary.RemoveRange(context.inventory_proprietary_summary
				.Where(s => idList.Contains(s.id)));
		}

		public List<InventoryProprietarySummary> GetInventoryProprietarySummary(
            QuarterDetailDto quarterDetailDto, 
            HashSet<int> defaultDaypartIds)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var query = from summary in context.inventory_proprietary_summary
						        join source in context.inventory_sources 
                                    on summary.inventory_source_id equals source.id
                                join mappings in context.inventory_proprietary_daypart_program_mappings 
                                    on summary.inventory_proprietary_daypart_program_mappings_id equals mappings.id
                                join daypartDefault in context.daypart_defaults 
                                    on mappings.daypart_default_id equals daypartDefault.id
						        join daypart in context.dayparts 
                                    on daypartDefault.daypart_id equals daypart.id
						        join programName in context.inventory_proprietary_daypart_programs 
                                    on mappings.inventory_proprietary_daypart_programs_id equals programName.id
                                where defaultDaypartIds.Contains(mappings.daypart_default_id) && 
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

		public double GetTotalImpressionsBySummaryIdAndAudienceIds(int id, List<int> audienceIds)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var query = context.inventory_proprietary_summary.Where(x => x.id == id)
						.Select(x => x.inventory_proprietary_summary_audiences
							.Where(a => audienceIds.Contains(a.audience_id))
							.GroupBy(y => y.inventory_proprietary_summary_id)
							.Select(y => y.Sum(z => z.impressions ?? 0))).FirstOrDefault();
					var entities = query.ToList().FirstOrDefault();

					return entities;
				});
		}

		public double GetTotalMarketCoverageBySummaryId(int id)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var query = context.inventory_proprietary_summary.Where(x =>
							x.id == id)
						.Select(x => x.inventory_proprietary_summary_markets
							.GroupBy(y => y.inventory_proprietary_summary_id)
							.Select(y => y.Sum(z => z.market_coverage ?? 0))).FirstOrDefault();
					var entities = query.ToList().FirstOrDefault();

					return entities;
				});
		}

		public List<int> GetDaypartIds(QuarterDetailDto quarterDetailDto)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var daypartIds = context.inventory_proprietary_summary
                            .Include(x => x.inventory_proprietary_daypart_program_mappings)
                            .Where(x => x.quarter_number == quarterDetailDto.Quarter && 
                                        x.quarter_year == quarterDetailDto.Year)
						    .Select(x => x.inventory_proprietary_daypart_program_mappings.daypart_default_id)
						    .ToList();

					return daypartIds;
				});
		}

		public List<PlanMarketDto> GetMarketDataBySummaryIds(List<int> ids)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var planMarketDtos = context.inventory_proprietary_summary_markets
						.Where(x =>
							ids.Contains(x.inventory_proprietary_summary_id)).ToList()
						.Select(i => new PlanMarketDto
						{
							MarketCode = i.market_code,
							PercentageOfUS = i.market_coverage ?? 0
						}).ToList();

					return planMarketDtos;
				});
		}

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