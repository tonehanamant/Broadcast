using System.Data.Entity;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryRepository : IDataRepository
    {
        List<InventorySource> GetInventorySources();
        InventorySource GetInventorySourceByName(string sourceName);
        int InventoryExists(string daypartCode, short stationCode, int spotLengthId, int spotsPerWeek, DateTime effectiveDate);
        void SaveStationInventoryGroups(InventoryFile inventoryFile);
        void UpdateInventoryGroups(List<StationInventoryGroup> inventoryGroups);
        List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId);
    }

    public class InventoryRepository : BroadcastRepositoryBase, IInventoryRepository
    {
        public InventoryRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory
            , ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<InventorySource> GetInventorySources()
        {
            return _InReadUncommitedTransaction(
                context => (from a in context.inventory_sources
                    select new InventorySource()
                    {
                        Id = a.id,
                        InventoryType = (InventoryFile.InventoryType)a.inventory_source_type,
                        IsActive = a.is_active,
                        Name = a.name
                    }).ToList());
        }

        public InventorySource GetInventorySourceByName(string sourceName)
        {
            return _InReadUncommitedTransaction(
                context => (from a in context.inventory_sources
                    where a.name.ToLower().Equals(sourceName.ToLower())
                    select new InventorySource()
                    {
                        Id = a.id,
                        InventoryType = (InventoryFile.InventoryType)a.inventory_source_type,
                        IsActive = a.is_active,
                        Name = a.name
                    }).SingleOrDefault());
        }

        public int InventoryExists(string daypartCode, short stationCode, int spotLengthId, int spotsPerWeek,
            DateTime effectiveDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var inventoryId = (from i in context.station_inventory_group
                        join m in context.station_inventory_manifest on i.id equals m.station_inventory_group_id
                        where i.daypart_code == daypartCode &&
                              m.station_code == stationCode &&
                              m.spot_length_id == spotLengthId &&
                              m.spots_per_week == spotsPerWeek &&
                              m.effective_date == effectiveDate
                        select i.id).SingleOrDefault();

                    return inventoryId;
                });
        }

        public void SaveStationInventoryGroups(InventoryFile inventoryFile)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var inventoryGroups = inventoryFile.InventoryGroups;

                    var newGroups = inventoryGroups.Select(inventoryGroup => new station_inventory_group()
                    {
                        daypart_code = inventoryGroup.DaypartCode,
                        inventory_source_id = inventoryFile.InventorySourceId,
                        name = inventoryGroup.Name,
                        slot_number = (byte) inventoryGroup.SlotNumber,
                        start_date = inventoryGroup.StartDate,
                        end_date = inventoryGroup.EndDate,
                        station_inventory_manifest =
                            inventoryGroup.Manifests.Select(manifest => new station_inventory_manifest()
                            {
                                station_code = (short) manifest.Station.Code,
                                spot_length_id = manifest.SpotLengthId,
                                spots_per_day = manifest.SpotsPerDay,
                                spots_per_week = manifest.SpotsPerWeek,
                                effective_date = manifest.EffectiveDate,
                                file_id = inventoryFile.Id,
                                inventory_source_id = inventoryFile.InventorySourceId,
                                end_date = manifest.EndDate,
                                station_inventory_manifest_audiences =
                                    manifest.ManifestAudiences.Select(
                                        audience => new station_inventory_manifest_audiences()
                                        {
                                            audience_id = audience.Audience.Id,
                                            impressions = audience.Impressions,
                                            rate = audience.Rate
                                        }).ToList(),
                                station_inventory_manifest_dayparts =
                                    manifest.Dayparts.Select(d => new station_inventory_manifest_dayparts()
                                    {
                                        daypart_id = d.Id
                                    }).ToList()
                            }).ToList()
                    }).ToList();

                    context.station_inventory_group.AddRange(newGroups);

                    context.SaveChanges();
                });
        }

        public void UpdateInventoryGroups(List<StationInventoryGroup> inventoryGroups)
        {
            _InReadUncommitedTransaction(
                context =>
                {

                });
        }

        public List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId)
        {
            return  _InReadUncommitedTransaction(
                context =>
                {
                    var manifests =
                        (from m in
                            context.station_inventory_manifest.Include(l => l.station_inventory_manifest_audiences)
                                .Include(s => s.station)
                            join g in context.station_inventory_group on m.station_inventory_group_id equals g.id
                            where m.file_id == fileId
                            select new {g, m}).ToList();

                    return manifests.Select(pv => _MapToInventoryGroup(pv.g, pv.m)).ToList();
                });
        }

        private StationInventoryGroup _MapToInventoryGroup(station_inventory_group stationInventoryGroup,
            station_inventory_manifest stationInventoryManifest)
        {
            return new StationInventoryGroup()
            {
                Id = stationInventoryManifest.id,
                Name = stationInventoryGroup.name,
                DaypartCode = stationInventoryGroup.daypart_code,
                SlotNumber = stationInventoryGroup.slot_number,
                Manifests =
                    stationInventoryGroup.station_inventory_manifest.Select(manifest => new StationInventoryManifest()
                    {
                        Id = manifest.id,
                        Station =
                            new DisplayBroadcastStation()
                            {
                                Affiliation = manifest.station.affiliation,
                                Code = manifest.station_code,
                                CallLetters = manifest.station.station_call_letters,
                                LegacyCallLetters = manifest.station.legacy_call_letters,
                                MarketCode = manifest.station.market_code
                            },
                        DaypartCode = stationInventoryGroup.daypart_code,
                        SpotLengthId = manifest.spot_length_id,
                        SpotsPerWeek = manifest.spots_per_week,
                        SpotsPerDay = manifest.spots_per_day,
                        Dayparts = manifest.station_inventory_manifest_dayparts.Select(d => new DisplayDaypart()
                        {
                            Id = d.daypart_id,
                        }).ToList(),
                        ManifestAudiences =
                            manifest.station_inventory_manifest_audiences.Select(
                                audience => new StationInventoryManifestAudience()
                                {
                                    Audience = new DisplayAudience() {Id = audience.id},
                                    Impressions = audience.impressions,
                                    Rate = audience.rate
                                }).ToList(),
                        FileId = manifest.file_id,
                        InventorySourceId = manifest.inventory_source_id,
                        EffectiveDate = manifest.effective_date
                    }).ToList()
            };
        }
    }
}
