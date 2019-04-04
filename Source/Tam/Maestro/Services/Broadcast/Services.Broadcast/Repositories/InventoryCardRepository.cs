using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using System.Data.Entity;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryCardRepository : IDataRepository
    {
        List<InventorySource> GetAllInventorySources();
        List<InventoryCardManifestDto> GetInventoryCardManifests(InventorySource inventorySource, Tuple<DateTime, DateTime> quarterDateRangeTuple);
        double? GetInventoryCardHouseholdImpressions(List<int> manifestIds, int householdAudienceId);
        List<InventoryCardManifestFileDto> GetInventoryCardManifestFileDtos(List<int> inventoryFileIds);
        InventorySource GetInventorySource(int inventorySourceId);
        Tuple<DateTime?, DateTime?> GetInventorySourceDateRange(int inventorySourceId);
        Tuple<DateTime?, DateTime?> GetAllInventoriesDateRange();
    }

    public class InventoryCardRepository : BroadcastRepositoryBase, IInventoryCardRepository
    {
        public InventoryCardRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper)
           : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<InventorySource> GetAllInventorySources()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.inventory_sources.
                            Where(x => x.inventory_source_type == (byte)InventorySourceTypeEnum.Barter).
                            Select(x => new InventorySource
                            {
                                Id = x.id,
                                Name = x.name,
                                InventoryType = (InventorySourceTypeEnum)x.inventory_source_type,
                                IsActive = x.is_active
                            }).ToList();
            });
        }

        public List<InventoryCardManifestDto> GetInventoryCardManifests(InventorySource inventorySource, Tuple<DateTime, DateTime> quarterDateRangeTuple)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from manifest in context.station_inventory_manifest.
                                                                    Include(x => x.station).
                                                                    Include(x => x.station_inventory_group).
                                                                    Include(x => x.inventory_files).
                                                                    Include(x => x.inventory_files.inventory_file_barter_header)
                            where manifest.inventory_source_id == inventorySource.Id &&
                                  manifest.effective_date <= quarterDateRangeTuple.Item2 &&
                                  manifest.end_date >= quarterDateRangeTuple.Item1
                            select new
                            {
                                manifest.id,
                                manifest.station_id,
                                manifest.station.market_code,
                                manifest.station_inventory_group.daypart_code,
                                manifest.effective_date,
                                manifest.end_date,
                                manifest.file_id,
                            }).Select(m => new InventoryCardManifestDto
                            {
                                ManifestId = m.id,
                                StationId = m.station_id,
                                MarketCode = m.market_code,
                                DaypartCode = m.daypart_code,
                                EffectiveDate = m.effective_date,
                                EndDate = m.end_date,
                                FileId = m.file_id
                            }).ToList();
                });
        }

        public double? GetInventoryCardHouseholdImpressions(List<int> manifestIds, int householdAudienceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from manifest in context.station_inventory_manifest
                            from audience in manifest.station_inventory_manifest_audiences
                            where manifestIds.Contains(manifest.id) &&
                                  audience.audience_id == householdAudienceId
                            select audience.impressions).Sum();
                });
        }

        public List<InventoryCardManifestFileDto> GetInventoryCardManifestFileDtos(List<int> inventoryFileIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from file in context.inventory_files
                            from header in file.inventory_file_barter_header
                            where inventoryFileIds.Contains(file.id)
                            select new
                            {
                                file.created_date,
                                file.status,
                                header.hut_projection_book_id,
                                header.share_projection_book_id
                            }).Select(f => new InventoryCardManifestFileDto
                            {
                                CreatedDate = f.created_date,
                                Status = (FileStatusEnum)f.status,
                                HutProjectionBookId = f.hut_projection_book_id,
                                ShareProjectionBookId = f.share_projection_book_id
                            }).ToList();
                });
        }

        public InventorySource GetInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.inventory_sources.
                            Where(x => x.id == inventorySourceId).
                            Select(x => new InventorySource
                            {
                                Id = x.id,
                                Name = x.name,
                                InventoryType = (InventorySourceTypeEnum)x.inventory_source_type,
                                IsActive = x.is_active
                            }).Single();
            });
        }

        public Tuple<DateTime?, DateTime?> GetInventorySourceDateRange(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var manifestDates = (from manifest in context.station_inventory_manifest.
                                                                   Include(x => x.station).
                                                                   Include(x => x.station_inventory_group)
                                        where manifest.inventory_source_id == inventorySourceId
                                        select new
                                        {
                                            manifest.effective_date,
                                            manifest.end_date,
                                        });

                   var minDate = manifestDates.Min(x => (DateTime?)x.effective_date);
                   var maxDate = manifestDates.Max(x => x.end_date);

                   return new Tuple<DateTime?, DateTime?>(minDate, maxDate);
               });
        }

        public Tuple<DateTime?, DateTime?> GetAllInventoriesDateRange()
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var manifestDates = (from manifest in context.station_inventory_manifest.
                                                                   Include(x => x.station).
                                                                   Include(x => x.station_inventory_group)
                                        select new
                                        {
                                            manifest.effective_date,
                                            manifest.end_date,
                                        });

                   var minDate = manifestDates.Min(x => (DateTime?)x.effective_date);
                   var maxDate = manifestDates.Max(x => x.end_date);

                   return new Tuple<DateTime?, DateTime?>(minDate, maxDate);
               });
        }
    }
}
