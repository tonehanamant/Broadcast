using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Inventory;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories.Inventory
{
    public interface IInventoryExportRepository : IDataRepository
	{
		List<QuarterDto> GetInventoryQuartersForSource(int inventorySourceId);

        /// <summary>
        /// Gets the inventory for export.  Only for Open Market.
        /// </summary>
        List<InventoryExportDto> GetInventoryForExportOpenMarket(List<int> spotLengthIds, List<int> genreIds, List<int> mediaWeekIds);

        /// <summary>
        /// Gets the inventory that is not enriched for export.  Only for Open Market.
        /// </summary>
        List<InventoryExportDto> GetInventoryForExportOpenMarketNotEnriched(List<int> spotLengthIds, List<int> mediaWeekIds);
	}

	public class InventoryExportRepository : BroadcastRepositoryBase, IInventoryExportRepository
	{
		public InventoryExportRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
			ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
			: base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper)
		{
		}

		public List<QuarterDto> GetInventoryQuartersForSource(int inventorySourceId)
		{
			return _InReadUncommitedTransaction(
				context =>
				{
					var queryData = (from summaryQuarters in context.inventory_summary_quarters
						where summaryQuarters.inventory_source_id == inventorySourceId
						select new QuarterDto
						{
							Quarter = summaryQuarters.quarter_number,
							Year = summaryQuarters.quarter_year
						}).OrderByDescending(q => q.Year).ThenByDescending(q => q.Quarter).ToList();

					return queryData;
				});
		}

        ///<inheritdoc/>
        public List<InventoryExportDto> GetInventoryForExportOpenMarket(List<int> spotLengthIds, List<int> genreIds, List<int> mediaWeekIds)
        {
            const int openMarketInventorySourceId = 1;
            const int hhAudienceId = 31;

            return _InReadUncommitedTransaction(
                context =>
                {
                        var query = (from week in context.station_inventory_manifest_weeks
                                    join manifest in context.station_inventory_manifest
                                        on week.station_inventory_manifest_id equals manifest.id
                                    join daypart in context.station_inventory_manifest_dayparts
                                        on manifest.id equals daypart.station_inventory_manifest_id
                                    join ratingsJob in context.inventory_file_ratings_jobs
                                        on manifest.file_id equals ratingsJob.inventory_file_id
                                    join projectedHh in context.station_inventory_manifest_audiences
                                        on new { id = manifest.id, audienceId = hhAudienceId, IsReference = false }
                                        equals new
                                        {
                                            id = projectedHh.station_inventory_manifest_id,
                                            audienceId = projectedHh.audience_id,
                                            IsReference = projectedHh.is_reference
                                        }
                                    join rates in context.station_inventory_manifest_rates
                                        on new { id = manifest.id, spot_length_id = manifest.spot_length_id }
                                        equals new { id = rates.station_inventory_manifest_id, spot_length_id = rates.spot_length_id }
                                    join programs in context.station_inventory_manifest_daypart_programs
                                        // the primary program
                                        on daypart.primary_program_id equals programs.id
                                    where manifest.inventory_source_id == openMarketInventorySourceId
                                          && spotLengthIds.Contains(manifest.spot_length_id)
                                          && mediaWeekIds.Contains(week.media_week_id)
                                          && ratingsJob.status == (int)BackgroundJobProcessingStatus.Succeeded
                                          && genreIds.Contains(programs.maestro_genre_id)
                                    select new InventoryExportDto
                                    {
                                        InventoryId = manifest.id,
                                        StationId = manifest.station_id,
                                        MediaWeekId = week.media_week_id,
                                        DaypartId = daypart.daypart_id,
                                        SpotCost = rates.spot_cost,
                                        InventoryProgramName = daypart.program_name,
                                        ProgramName = programs.name,
                                        MaestroGenreId = programs.maestro_genre_id,
                                        ProgramSource = (ProgramSourceEnum)programs.program_source_id,
                                        HhImpressionsProjected = projectedHh.impressions,
                                        ProvidedAudiences = manifest.station_inventory_manifest_audiences
                                            .Where(a => a.is_reference)
                                            .Select(a => new InventoryExportAudienceDto
                                            {
                                                AudienceId = a.audience_id,
                                                Impressions = a.impressions
                                            }).ToList()
                                    }
                        );
                        var entities = query.ToList();
                    return entities;
                });
        }

        ///<inheritdoc/>
        public List<InventoryExportDto> GetInventoryForExportOpenMarketNotEnriched(List<int> spotLengthIds, List<int> mediaWeekIds)
        {
            const int openMarketInventorySourceId = 1;
            const int hhAudienceId = 31;

            return _InReadUncommitedTransaction(
                context =>
                {
                    var entities = (from week in context.station_inventory_manifest_weeks
                                    join manifest in context.station_inventory_manifest
                                        on week.station_inventory_manifest_id equals manifest.id
                                    join daypart in context.station_inventory_manifest_dayparts
                                        on manifest.id equals daypart.station_inventory_manifest_id
                                    join ratingsJob in context.inventory_file_ratings_jobs
                                        on manifest.file_id equals ratingsJob.inventory_file_id
                                    join projectedHh in context.station_inventory_manifest_audiences
                                        on new { id = manifest.id, audienceId = hhAudienceId, IsReference = false }
                                        equals new
                                        {
                                            id = projectedHh.station_inventory_manifest_id,
                                            audienceId = projectedHh.audience_id,
                                            IsReference = projectedHh.is_reference
                                        }
                                    join rates in context.station_inventory_manifest_rates
                                        on new { id = manifest.id, spot_length_id = manifest.spot_length_id }
                                        equals new { id = rates.station_inventory_manifest_id, spot_length_id = rates.spot_length_id }
                                    where manifest.inventory_source_id == openMarketInventorySourceId
                                          && spotLengthIds.Contains(manifest.spot_length_id)
                                          && mediaWeekIds.Contains(week.media_week_id)
                                          && ratingsJob.status == (int)BackgroundJobProcessingStatus.Succeeded
                                          && !daypart.station_inventory_manifest_daypart_programs.Any()
                                    select new InventoryExportDto
                                    {
                                        InventoryId = manifest.id,
                                        StationId = manifest.station_id,
                                        MediaWeekId = week.media_week_id,
                                        DaypartId = daypart.daypart_id,
                                        SpotCost = rates.spot_cost,
                                        InventoryProgramName = daypart.program_name,
                                        HhImpressionsProjected = projectedHh.impressions,
                                        ProvidedAudiences = manifest.station_inventory_manifest_audiences
                                            .Where(a => a.is_reference)
                                            .Select(a => new InventoryExportAudienceDto
                                            {
                                                AudienceId = a.audience_id,
                                                Impressions = a.impressions
                                            }).ToList()
                                    }
                        )
                        .ToList();
                    return entities;
                });
        }
    }
}