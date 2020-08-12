using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventoryProprietary;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

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
		List<InventoryProprietaryQuarterSummaryDto> GetDataForInventoryProprietarySummary(
			InventorySource inventorySource,
			DateTime startDate, DateTime endDate);

		/// <summary>
		///     Get Data to Load [inventory_proprietary_summary_audiences]
		/// </summary>
		/// <param name="inventorySource"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		List<InventoryProprietarySummaryAudiencesDto> GetDataForInventoryProprietarySummaryAudiences(
			InventorySource inventorySource, DateTime startDate, DateTime endDate);

		/// <summary>
		///     Get Data to Load [inventory_proprietary_summary_markets]
		/// </summary>
		/// <param name="inventorySource"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		List<short?> GetMarketCodesForInventoryProprietarySummary(
			InventorySource inventorySource,
			DateTime startDate, DateTime endDate);

		/// <summary>
		///     Load inventory_proprietary_summary and inventory_proprietary_summary_audiences
		/// </summary>
		/// <param name="inventoryProprietaryQuarterSummaryDto"></param>
		void SaveInventoryProprietarySummary(
			InventoryProprietaryQuarterSummaryDto inventoryProprietaryQuarterSummaryDto);
	}

	public class InventoryProprietarySummaryRepository : BroadcastRepositoryBase, IInventoryProprietarySummaryRepository
	{
		public InventoryProprietarySummaryRepository(
			IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
			ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
			: base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
		{
		}

		private const string Aggregate_Inventory_Proprietary_Summary = "AggregateInventoryProprietarySummary";

		/// <inheritdoc />
		public List<InventoryProprietaryQuarterSummaryDto> GetDataForInventoryProprietarySummary(
			InventorySource inventorySource,
			DateTime startDate, DateTime endDate)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					const int fixedSlotNumber = 1;

					var query = from week in context.station_inventory_manifest_weeks
						join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals
							manifest.id
						join manifestGroup in context.station_inventory_group on manifest.station_inventory_group_id
							equals manifestGroup.id
						join file in context.inventory_files on manifest.file_id equals file.id
						join header in context.inventory_file_proprietary_header on file.id equals header
							.inventory_file_id
						join daypartDefaults in context.daypart_defaults on header.daypart_default_id equals
							daypartDefaults.id
						where week.start_date <= endDate && week.end_date >= startDate &&
						      manifest.inventory_source_id == inventorySource.Id &&
						      manifestGroup.slot_number == fixedSlotNumber
						group header by new {header.cpm, header.daypart_default_id}
						into g
						select new InventoryProprietaryQuarterSummaryDto
						{
							InventorySourceId = inventorySource.Id,
							Cpm = g.Key.cpm,
							DefaultDaypartId = g.Key.daypart_default_id,
							SlotNumber = fixedSlotNumber
						};

					return query.ToList();
				});
		}

		public List<short?> GetMarketCodesForInventoryProprietarySummary(
			InventorySource inventorySource,
			DateTime startDate, DateTime endDate)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var query = from week in context.station_inventory_manifest_weeks
						join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals
							manifest.id
						join station in context.stations on manifest.station_id equals station.id
						join manifestGroup in context.station_inventory_group on manifest.station_inventory_group_id
							equals manifestGroup.id
						join file in context.inventory_files on manifest.file_id equals file.id
						join header in context.inventory_file_proprietary_header on file.id equals header
							.inventory_file_id
						join daypartDefaults in context.daypart_defaults on header.daypart_default_id equals
							daypartDefaults.id
						join audiences in context.station_inventory_manifest_audiences on manifest.id equals audiences
							.station_inventory_manifest_id
						where week.start_date <= endDate && week.end_date >= startDate &&
						      manifest.inventory_source_id == inventorySource.Id &&
						      manifestGroup.slot_number == 1
						group station by station.market_code
						into g
						select g.Key;

					return query.ToList();
				}
			);
		}

		public List<InventoryProprietarySummaryAudiencesDto> GetDataForInventoryProprietarySummaryAudiences(
			InventorySource inventorySource, DateTime startDate, DateTime endDate)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var query = from week in context.station_inventory_manifest_weeks
						join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals
							manifest.id
						join manifestGroup in context.station_inventory_group on manifest.station_inventory_group_id
							equals manifestGroup.id
						join file in context.inventory_files on manifest.file_id equals file.id
						join header in context.inventory_file_proprietary_header on file.id equals header
							.inventory_file_id
						join daypartDefaults in context.daypart_defaults on header.daypart_default_id equals
							daypartDefaults.id
						join audiences in context.station_inventory_manifest_audiences on manifest.id equals audiences
							.station_inventory_manifest_id
						where week.start_date <= endDate && week.end_date >= startDate &&
						      manifest.inventory_source_id == inventorySource.Id &&
						      manifestGroup.slot_number == 1 && !audiences.is_reference
								group new { audiences, header } by new
						{
							audiences.audience_id,
							header.daypart_default_id
						}
						into g
						select new InventoryProprietarySummaryAudiencesDto
						{
							AudienceId = g.Key.audience_id,
							Impressions = g.Sum(a => a.audiences.impressions)
						};

					return query.ToList();
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
						daypart_default_id = inventoryProprietaryQuarterSummaryDto.DefaultDaypartId ?? 0,
						quarter_number = inventoryProprietaryQuarterSummaryDto.Quarter.Quarter,
						quarter_year = inventoryProprietaryQuarterSummaryDto.Quarter.Year,
						unit = inventoryProprietaryQuarterSummaryDto.SlotNumber,
						cpm = inventoryProprietaryQuarterSummaryDto.Cpm,
						created_at = DateTime.Now,
						created_by = Aggregate_Inventory_Proprietary_Summary,
						modified_at = DateTime.Now,
						modified_by = Aggregate_Inventory_Proprietary_Summary,
						inventory_proprietary_summary_markets = inventoryProprietaryQuarterSummaryDto.Markets?.Select(
							x => new inventory_proprietary_summary_markets
							{
								market_code = x.MarketCode,
								market_coverage = x.MarketCoverage,
								created_at = DateTime.Now,
								created_by = Aggregate_Inventory_Proprietary_Summary,
								modified_at = DateTime.Now,
								modified_by = Aggregate_Inventory_Proprietary_Summary
							}).ToList(),
						inventory_proprietary_summary_audiences = inventoryProprietaryQuarterSummaryDto.Audiences
							?.Select(
								x => new inventory_proprietary_summary_audiences
								{
									audience_id = x.AudienceId,
									impressions = x.Impressions,
									created_at = DateTime.Now,
									created_by = Aggregate_Inventory_Proprietary_Summary,
									modified_at = DateTime.Now,
									modified_by = Aggregate_Inventory_Proprietary_Summary
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
				            s.quarter_year.Equals(dto.Quarter.Year) && s.quarter_number.Equals(dto.Quarter.Quarter))
				.Select(s => s.id).ToList();
			var audianceToBeRemoved = context.inventory_proprietary_summary_audiences
				.Where(a => idList.Contains(a.inventory_proprietary_summary_id)).ToList();

			context.inventory_proprietary_summary_audiences.RemoveRange(audianceToBeRemoved);

			var marketsToBeRemoved = context.inventory_proprietary_summary_markets
				.Where(m => idList.Contains(m.inventory_proprietary_summary_id)).ToList();
			context.inventory_proprietary_summary_markets.RemoveRange(marketsToBeRemoved);
			context.inventory_proprietary_summary.RemoveRange(context.inventory_proprietary_summary
				.Where(s => idList.Contains(s.id)));
		}
	}
}