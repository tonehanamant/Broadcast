﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IInventorySummaryRepository : IDataRepository
    {
        List<InventorySummaryManifestDto> GetInventorySummaryManifests(InventorySource inventorySource, DateTime startDate, DateTime endDate);
        double? GetInventorySummaryHouseholdImpressions(List<int> manifestIds, int householdAudienceId);
        List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFileDtos(List<int> inventoryFileIds);
    }

    public class InventorySummaryRepository : BroadcastRepositoryBase, IInventorySummaryRepository
    {
        public InventorySummaryRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper)
           : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<InventorySummaryManifestDto> GetInventorySummaryManifests(InventorySource inventorySource, DateTime startDate, DateTime endDate)
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
                            DaypartCode = x.station_inventory_group.daypart_code,
                            FileId = x.file_id
                        })
                        .ToList();
                });
        }

        public double? GetInventorySummaryHouseholdImpressions(List<int> manifestIds, int householdAudienceId)
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

        public List<InventorySummaryManifestFileDto> GetInventorySummaryManifestFileDtos(List<int> inventoryFileIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from file in context.inventory_files
                            from header in file.inventory_file_proprietary_header
                            from job in file.inventory_file_ratings_jobs
                            where inventoryFileIds.Contains(file.id)
                            select new
                            {
                                job.completed_at,
                                job.status,
                                header.hut_projection_book_id,
                                header.share_projection_book_id
                            }).Select(f => new InventorySummaryManifestFileDto
                            {
                                LastCompletedDate = f.completed_at,
                                JobStatus = (InventoryFileRatingsProcessingStatus)f.status,
                                HutProjectionBookId = f.hut_projection_book_id,
                                ShareProjectionBookId = f.share_projection_book_id
                            }).ToList();
                });
        }
    }
}
