using System.Data.Entity;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
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
        void AddNewInventoryGroups(InventoryFile inventoryFile);
        void UpdateInventoryGroups(List<StationInventoryGroup> inventoryGroups);
        void UpdateInventoryManifests(List<StationInventoryManifest> inventoryManifests);
        void ExpireInventoryGroupsAndManifests(List<StationInventoryGroup> inventoryGroups,DateTime expireDate);
        List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId);
        List<StationInventoryGroup> GetActiveInventoryByTypeAndDapartCodes(InventorySource inventorySource, List<string> daypartCodes);
        List<StationInventoryGroup> GetActiveInventoryBySourceAndName(InventorySource inventorySource, List<string> groupNames);
        List<StationInventoryGroup> GetInventoryBySourceAndName(InventorySource inventorySource, List<string> groupNames);
        List<StationInventoryManifest> GetStationManifestsBySourceAndStationCode(
                                            InventorySource rateSource, int stationCode);
        List<StationInventoryManifest> GetStationManifestsBySourceStationCodeAndDates(
                                            InventorySource rateSource, int stationCode, DateTime startDate, DateTime endDate);
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
                        InventoryType = (InventoryType)a.inventory_source_type,
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
                        InventoryType = (InventoryType)a.inventory_source_type,
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

        public void AddNewInventoryGroups(InventoryFile inventoryFile)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var inventoryGroups = inventoryFile.InventoryGroups;

                    var newGroups =
                        inventoryGroups.Where(g => g.Id == null).Select(inventoryGroup => new station_inventory_group()
                        {
                            daypart_code = inventoryGroup.DaypartCode,
                        inventory_source_id = inventoryFile.InventorySource.Id,
                            name = inventoryGroup.Name,
                            slot_number = (byte) inventoryGroup.SlotNumber,
                            start_date = inventoryGroup.StartDate,
                            end_date = inventoryGroup.EndDate,
                            station_inventory_manifest =
                                inventoryGroup.Manifests
                                    .Where(m => m.Id == null)
                            .Select(manifest =>
                            {                               
                                return new station_inventory_manifest()
                                {
                                    station_code = (short) manifest.Station.Code,
                                    spot_length_id = manifest.SpotLengthId,
                                    spots_per_day = manifest.SpotsPerDay,
                                    spots_per_week = manifest.SpotsPerWeek.GetValueOrDefault(),
                                    //TODO: Update database and make field nullable
                                    effective_date = manifest.EffectiveDate,
                                    file_id = inventoryFile.Id,
                                    inventory_source_id = inventoryFile.InventorySource.Id,
                                    end_date = manifest.EndDate,
                                    station_inventory_manifest_audiences =
                                        manifest.ManifestAudiences.Select(
                                            audience => new station_inventory_manifest_audiences()
                                            {
                                                audience_id = audience.Audience.Id,
                                                impressions = audience.Impressions,
                                                rating = audience.Rating,
                                                rate = audience.Rate,
                                                is_reference = audience.IsReference
                                            })
                                            .Union(
                                                manifest.ManifestAudiencesReferences.Select(
                                                    audience => new station_inventory_manifest_audiences()
                                                    {
                                                        audience_id = audience.Audience.Id,
                                                        impressions = audience.Impressions,
                                                        rating = audience.Rating,
                                                        rate = audience.Rate,
                                                        is_reference = audience.IsReference
                                                    })).ToList(),
                                    station_inventory_manifest_dayparts =
                                        manifest.ManifestDayparts.Select(
                                            md => new station_inventory_manifest_dayparts()
                                            {
                                                daypart_id = md.Daypart.Id
                                            }).ToList(),
                                    station_inventory_manifest_rates = manifest.ManifestRates.Select(
                                        r => new station_inventory_manifest_rates()
                                        {
                                            spot_length_id = r.SpotLengthId,
                                            rate = r.Rate
                                        }).ToList()
                                };
                                    }).ToList()
                        }).ToList();

                    context.station_inventory_group.AddRange(newGroups);

                    //OpenMarket manifests
                    var newManifests =
                        inventoryFile.InventoryManifests.Where(m => m.Id == null)
                            .Select(
                                manifest => new station_inventory_manifest()
                                {
                                    station_code = (short)manifest.Station.Code,
                                    spot_length_id = manifest.SpotLengthId,
                                    spots_per_day = manifest.SpotsPerDay,
                                    spots_per_week = null,
                                    effective_date = manifest.EffectiveDate,
                                    file_id = inventoryFile.Id,
                                    inventory_source_id = inventoryFile.InventorySource.Id,
                                    end_date = manifest.EndDate,
                                    station_inventory_manifest_audiences =
                                        manifest.ManifestAudiences.Select(
                                            audience => new station_inventory_manifest_audiences()
                                            {
                                                audience_id = audience.Audience.Id,
                                                impressions = audience.Impressions,
                                                rating = audience.Rating,
                                                rate = audience.Rate,
                                                is_reference = audience.IsReference
                                            })
                                            .Union(
                                                manifest.ManifestAudiencesReferences.Select(
                                                    audience => new station_inventory_manifest_audiences()
                                                    {
                                                        audience_id = audience.Audience.Id,
                                                        impressions = audience.Impressions,
                                                        rating = audience.Rating,
                                                        rate = audience.Rate,
                                                        is_reference = audience.IsReference
                                                    })).ToList(),
                                    station_inventory_manifest_dayparts =
                                        manifest.ManifestDayparts.Select(md => new station_inventory_manifest_dayparts()
                                        {
                                            daypart_id = md.Daypart.Id,
                                            program_name = md.ProgramName
                                        }).ToList(),
                                    station_inventory_manifest_rates = 
                                        manifest.ManifestRates.Select(mr => new station_inventory_manifest_rates()
                                        {
                                            rate = mr.Rate,
                                            spot_length_id = mr.SpotLengthId
                                        }).ToList()
                                }).ToList();

                    context.station_inventory_manifest.AddRange(newManifests);

                    context.SaveChanges();
                });
        }

        public void UpdateInventoryManifests(List<StationInventoryManifest> inventoryManifests)
        {
            _InReadUncommitedTransaction(
                context =>
                {   // TODO: add when needed
                });
        }

        /// <summary>
        ///  Does not update manifests.
        /// </summary>
        /// <param name="inventoryGroups"></param>
        public void UpdateInventoryGroups(List<StationInventoryGroup> inventoryGroups)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    inventoryGroups
                        .Where(ig => ig.Id.HasValue)
                        .ForEach(inventoryGroup =>
                        {
                            var groupToUpdate = context.station_inventory_group.Single(g => g.id == inventoryGroup.Id);

                            groupToUpdate.daypart_code = inventoryGroup.DaypartCode;
                            groupToUpdate.end_date = inventoryGroup.EndDate;
                            groupToUpdate.inventory_source_id = inventoryGroup.InventorySource.Id;
                            groupToUpdate.name = inventoryGroup.Name;
                            groupToUpdate.slot_number = (byte) inventoryGroup.SlotNumber;
                            groupToUpdate.start_date = inventoryGroup.StartDate;
                        });
                    context.SaveChanges();
                });
        }

        public void ExpireInventoryGroupsAndManifests(List<StationInventoryGroup> inventoryGroups,DateTime expireDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var groupIds = inventoryGroups
                        .Where(g => g.Id.HasValue)
                        .Select(g => g.Id);
                    var groups = context.station_inventory_group
                        .Include(g => g.station_inventory_manifest)
                        .Where(g => g.end_date == null
                                    && groupIds.Contains(g.id));
                    groups.ForEach(g =>
                        {
                            g.end_date = expireDate;
                            g.station_inventory_manifest
                                    .Where(m => m.end_date == null)
                                    .ForEach(m => m.end_date = expireDate);
                        });
                    context.SaveChanges();
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
                            select g).ToList();

                    return manifests.Select(_MapToInventoryGroup).ToList();
                });
        }

        private StationInventoryGroup _MapToInventoryGroup(station_inventory_group stationInventoryGroup)
        {
            var sig = new StationInventoryGroup()
            {
                Id = stationInventoryGroup.id,
                Name = stationInventoryGroup.name,
                DaypartCode = stationInventoryGroup.daypart_code,
                SlotNumber = stationInventoryGroup.slot_number,
                StartDate = stationInventoryGroup.start_date,
                EndDate = stationInventoryGroup.end_date,
                InventorySource = new InventorySource()
                {
                    Id = stationInventoryGroup.inventory_sources.id,
                    InventoryType = (InventoryType)stationInventoryGroup.inventory_sources.inventory_source_type,
                    Name = stationInventoryGroup.inventory_sources.name,
                    IsActive = stationInventoryGroup.inventory_sources.is_active
                },
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
                        ManifestDayparts = manifest.station_inventory_manifest_dayparts.Select(md => new StationInventoryManifestDaypart()
                        {
                            Id = md.id,
                            Daypart = DaypartCache.Instance.GetDisplayDaypart(md.daypart_id),
                        }).ToList(),
                        ManifestAudiences =
                            manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                audience => new StationInventoryManifestAudience()
                                {
                                    Audience = new DisplayAudience()
                                    { Id = audience.audience_id,AudienceString = audience.audience.name},
                                    IsReference = false,
                                    Impressions = audience.impressions,
                                    Rate = audience.rate
                                }).ToList(),
                        ManifestAudiencesReferences = 
                            manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true).Select(
                                audience => new StationInventoryManifestAudience()
                                {
                                    Audience = new DisplayAudience()
                                    { Id = audience.audience_id, AudienceString = audience.audience.name },
                                    IsReference = true,
                                    Impressions = audience.impressions,
                                    Rate = audience.rate
                                }).ToList(),
                        FileId = manifest.file_id,
                        InventorySourceId = manifest.inventory_source_id,
                        EffectiveDate = manifest.effective_date,
                        EndDate = manifest.end_date,
                        ManifestRates = manifest.station_inventory_manifest_rates
                                .Select(r => new StationInventoryManifestRate()
                                {
                                    Id = r.id,
                                    Rate = r.rate,
                                    SpotLengthId = r.spot_length_id
                                }).ToList()
                    }).ToList()
            };
            return sig;
        }

        public List<StationInventoryGroup> GetActiveInventoryBySourceAndName(
                                                InventorySource inventorySource,
                                                List<string> groupNames)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var inventory = GetInventoryGroupQuery(inventorySource, groupNames, c);

                    var output = inventory.Where(i => i.end_date == null).ToList();

                    return output.Select(_MapToInventoryGroup).ToList();
                });
        }
        public List<StationInventoryGroup> GetInventoryBySourceAndName(
                                                InventorySource inventorySource,
                                                List<string> groupNames)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var inventory = GetInventoryGroupQuery(inventorySource, groupNames, c).ToList();
                    return inventory.Select(_MapToInventoryGroup).ToList();
                });
        }

        private static IQueryable<station_inventory_group> GetInventoryGroupQuery(InventorySource inventorySource, List<string> groupNames, QueryHintBroadcastContext c)
        {
            var inventory = (from g in
                c.station_inventory_group
                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts))
                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_group))
                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_audiences))
                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_rates))
                where g.inventory_source_id == inventorySource.Id
                      && groupNames.Contains(g.name)
                select g);
            return inventory;
        }

        public List<StationInventoryGroup> GetActiveInventoryByTypeAndDapartCodes(
                                            InventorySource inventorySource, 
                                            List<string> daypartCodes)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var inventory = (from g in
                                c.station_inventory_group
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts))
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_group))
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_audiences))
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_rates))
                                     where g.inventory_source_id == inventorySource.Id
                                    && daypartCodes.Contains(g.daypart_code) 
                                    && g.end_date == null
                                    
                             select g).ToList();

                    return inventory.Select(i => _MapToInventoryGroup(i)).ToList();
                });
        }

        public List<StationInventoryManifest> GetStationManifestsBySourceAndStationCode(InventorySource rateSource, int stationCode)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests = (from sp in
                        context.station_inventory_manifest
                            .Include(m => m.station_inventory_manifest_dayparts)
                            .Include(m => m.station)
                            .Include(m => m.station_inventory_manifest_audiences)
                            .Include(m => m.station_inventory_manifest_generation)
                            .Include(m => m.station_inventory_group)
                            .Include(m => m.inventory_sources)
                            .Include(m => m.station_inventory_manifest_rates)
                        where sp.station_code == (short) stationCode
                              && sp.inventory_sources.name.ToLower().Equals(rateSource.Name.ToLower())
                                     select new StationInventoryManifest()
                                     {
                                         Id = sp.id,
                                         Station =
                                         new DisplayBroadcastStation()
                                         {
                                             Code = sp.station.station_code,
                                             Affiliation = sp.station.affiliation,
                                             CallLetters = sp.station.station_call_letters,
                                             LegacyCallLetters = sp.station.legacy_call_letters,
                                             MarketCode = sp.station.market_code,
                                             OriginMarket = sp.station.market.geography_name
                                         },
                                         DaypartCode = sp.station_inventory_group.daypart_code,
                                         SpotLengthId = sp.spot_length_id,
                                         SpotsPerWeek = sp.spots_per_week,
                                         SpotsPerDay = sp.spots_per_day,
                                         ManifestDayparts =
                                             sp.station_inventory_manifest_dayparts.Select(d => new StationInventoryManifestDaypart()
                                             {
                                                 Id = d.id,
                                                 Daypart = new DisplayDaypart { Id = d.daypart_id },
                                                 ProgramName = d.program_name
                                             }).ToList(),
                                         InventorySourceId = sp.inventory_source_id,
                                         EffectiveDate = sp.effective_date,
                                         ManifestAudiencesReferences =
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true).Select(ma => new StationInventoryManifestAudience()
                                             {
                                                 Impressions = ma.impressions,
                                                 Rating = ma.rating,
                                                 Rate = ma.rate,
                                                 Audience = new DisplayAudience() { Id = ma.audience_id, AudienceString = ma.audience.name },
                                                 IsReference = true
                                             }).ToList(),
                                         ManifestAudiences =
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                                 ma => new StationInventoryManifestAudience()
                                                 {
                                                     Impressions = ma.impressions,
                                                     Rating = ma.rating,
                                                     Rate = ma.rate,
                                                     Audience = new DisplayAudience() { Id = ma.audience_id, AudienceString = ma.audience.name },
                                                     IsReference = false
                                                 }).ToList(),
                                         ManifestRates = sp.station_inventory_manifest_rates.Select(mr => new StationInventoryManifestRate()
                                         {
                                             Rate = mr.rate,
                                             SpotLengthId = mr.spot_length_id,
                                         }).ToList(),
                                         EndDate = sp.end_date,
                                         FileId = sp.file_id
                                     }).ToList();

                    return manifests;
                });
        }

        public List<StationInventoryManifest> GetStationManifestsBySourceStationCodeAndDates(InventorySource rateSource, int stationCode, DateTime startDateValue, DateTime endDateValue)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests = (from sp in
                                         context.station_inventory_manifest
                                             .Include(m => m.station_inventory_manifest_dayparts)
                                             .Include(m => m.station)
                                             .Include(m => m.station_inventory_manifest_audiences)
                                             .Include(m => m.station_inventory_manifest_generation)
                                             .Include(m => m.station_inventory_group)
                                             .Include(m => m.inventory_sources)
                                             .Include(m => m.station_inventory_manifest_rates)
                                     where sp.station_code == (short)stationCode
                                           && sp.inventory_sources.name.ToLower().Equals(rateSource.Name.ToLower())
                                           && ((sp.effective_date >= startDateValue && sp.effective_date <= endDateValue)
                                                || (sp.end_date >= startDateValue && sp.end_date <= endDateValue)
                                                || (sp.effective_date < startDateValue && sp.end_date > endDateValue))
                                     select new StationInventoryManifest()
                        {
                            Id = sp.id,
                            Station =
                            new DisplayBroadcastStation()
                            {
                                Code = sp.station.station_code,
                                Affiliation = sp.station.affiliation,
                                CallLetters = sp.station.station_call_letters,
                                LegacyCallLetters = sp.station.legacy_call_letters,
                                MarketCode = sp.station.market_code,
                                OriginMarket = sp.station.market.geography_name
                            },
                        DaypartCode = sp.station_inventory_group.daypart_code,
                        SpotLengthId = sp.spot_length_id,
                        SpotsPerWeek = sp.spots_per_week,
                        SpotsPerDay = sp.spots_per_day,
                        ManifestDayparts =
                            sp.station_inventory_manifest_dayparts.Select(d => new StationInventoryManifestDaypart()
                            {
                                Id = d.id,
                                Daypart = new DisplayDaypart { Id = d.daypart_id},
                                ProgramName = d.program_name
                            }).ToList(),
                        InventorySourceId = sp.inventory_source_id,
                        EffectiveDate = sp.effective_date,
                        ManifestAudiencesReferences =
                            sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true).Select(ma => new StationInventoryManifestAudience()
                            {
                                Impressions = ma.impressions,
                                Rating = ma.rating,
                                Rate = ma.rate,
                                Audience = new DisplayAudience() { Id = ma.audience_id, AudienceString = ma.audience.name },
                                IsReference = true
                            }).ToList(),
                        ManifestAudiences =
                            sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                ma => new StationInventoryManifestAudience()
                                {
                                    Impressions = ma.impressions,
                                    Rating = ma.rating,
                                    Rate = ma.rate,
                                    Audience = new DisplayAudience() { Id = ma.audience_id ,AudienceString = ma.audience.name },
                                    IsReference = false
                                }).ToList(),
                        ManifestRates = sp.station_inventory_manifest_rates.Select(mr => new StationInventoryManifestRate()
                        {
                            Rate = mr.rate,
                            SpotLengthId = mr.spot_length_id,
                        }).ToList(),
                        EndDate = sp.end_date,
                        FileId = sp.file_id
                    }).ToList();

                    return manifests;
                });
        }
    }
}
