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
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Services.Broadcast.Entities.StationInventory;
using Tam.Maestro.Data.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.BarterInventory;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryRepository : IDataRepository
    {
        List<InventorySource> GetInventorySources();
        InventorySource GetInventorySourceByName(string sourceName);
        int InventoryExists(string daypartCode, short stationCode, int spotLengthId, int spotsPerWeek, DateTime effectiveDate);
        void AddNewInventory(InventoryFileBase inventoryFile);
        void UpdateInventoryGroups(List<StationInventoryGroup> inventoryGroups);
        void UpdateInventoryManifests(List<StationInventoryManifest> inventoryManifests);
        void UpdateInventoryManifestsDateIntervals(List<StationInventoryManifest> inventoryManifests);
        void ExpireInventoryGroupsAndManifests(List<StationInventoryGroup> inventoryGroups, DateTime expireDate, DateTime newEffectiveDate);
        List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId);
        List<StationInventoryManifest> GetStationInventoryManifestsByFileId(int fileId);
        List<StationInventoryGroup> GetActiveInventoryByTypeAndDapartCodes(InventorySource inventorySource, List<string> daypartCodes);
        List<StationInventoryGroup> GetActiveInventoryBySourceAndName(InventorySource inventorySource, List<string> groupNames, DateTime newEffectiveDate);
        List<StationInventoryManifest> GetActiveInventoryManifestsBySourceAndContractedDaypart(InventorySource source, int contractedDaypartId, DateTime effectiveDate, DateTime endDate);
        List<StationInventoryGroup> GetInventoryBySourceAndName(InventorySource inventorySource, List<string> groupNames);
        List<StationInventoryManifest> GetStationManifestsBySourceAndStationCode(
                                            InventorySource rateSource, int stationCode);
        List<StationInventoryManifest> GetStationManifestsBySourceStationCodeAndDates(
                                            InventorySource rateSource, int stationCode, DateTime startDate, DateTime endDate);
        StationInventoryManifest GetStationManifest(int manifestId);
        void RemoveManifest(int manifestId);
        List<StationInventoryManifest> GetManifestProgramsByStationCodeAndDates(string rateSource, int code,
            DateTime startDate, DateTime endDate);
        void SaveStationInventoryManifest(StationInventoryManifest stationInventoryManifest);
        void UpdateStationInventoryManifest(StationInventoryManifest stationInventoryManifest);
        bool CheckIfManifestByStationProgramFlightDaypartExists(int stationId, string programName, DateTime startDate, DateTime endDate, int daypartId);
        void ExpireManifest(int manifestId, DateTime endDate);
        bool HasSpotsAllocated(int manifestId);
        List<StationInventoryGroup> GetActiveInventoryGroupsBySourceAndContractedDaypart(InventorySource source, int contractedDaypartId, DateTime effectiveDate, DateTime endDate);
        void UpdateInventoryGroupsDateIntervals(List<StationInventoryGroup> inventoryGroups);
	void AddInventoryGroups(List<StationInventoryGroup> groups, InventoryFileBase inventoryFile);
        void AddInventoryManifests(List<StationInventoryManifest> manifests, InventoryFileBase inventoryFile);
        void UpdateInventoryRatesForManifests(List<StationInventoryManifest> manifests);

        /// <summary>
        /// Get inventory data for the sxc file        
        /// </summary>
        /// <param name="startDate">Start date of the quarter</param>
        /// <param name="endDate">End date of the quarter</param>
        /// <returns>List of StationInventoryGroup objects containing the data</returns>
        List<StationInventoryGroup> GetInventoryScxData(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the header information for an inventory file id
        /// </summary>
        /// <param name="inventoryFileId">Inventory file id to get the data for</param>
        /// <returns>BarterInventoryHeader object containing the header data</returns>
        BarterInventoryHeader GetInventoryFileHeader(int inventoryFileId);
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
                                InventoryType = (InventorySourceTypeEnum)a.inventory_source_type,
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
                                InventoryType = (InventorySourceTypeEnum)a.inventory_source_type,
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
                    var inventoryId = (
                        from i in context.station_inventory_group
                        join m in context.station_inventory_manifest
                            .Include(x => x.station)
                            on i.id equals m.station_inventory_group_id
                        where i.daypart_code == daypartCode &&
                              m.station.station_code == stationCode &&
                              m.spot_length_id == spotLengthId &&
                              m.spots_per_week == spotsPerWeek &&
                              m.effective_date == effectiveDate
                        select i.id).SingleOrDefault();

                    return inventoryId;
                });
        }

        public void AddNewInventory(InventoryFileBase inventoryFile)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var newGroups = inventoryFile.InventoryGroups
                        .Where(g => g.Id == null)
                        .Select(inventoryGroup => _MapToStationInventoryGroup(inventoryGroup, inventoryFile))
                        .ToList();

                    context.station_inventory_group.AddRange(newGroups);

                    var newManifests = inventoryFile.InventoryManifests
                        .Where(m => m.Id == null)
                        .Select(manifest => _MapToStationInventoryManifest(manifest, inventoryFile))
                        .ToList();

                    context.station_inventory_manifest.AddRange(newManifests);

                    context.SaveChanges();
                });
        }

        private station_inventory_group _MapToStationInventoryGroup(StationInventoryGroup inventoryGroup, InventoryFileBase inventoryFile)
        {
            return new station_inventory_group()
            {
                daypart_code = inventoryGroup.DaypartCode,
                inventory_source_id = inventoryFile.InventorySource.Id,
                name = inventoryGroup.Name,
                slot_number = (byte)inventoryGroup.SlotNumber,
                start_date = inventoryGroup.StartDate,
                end_date = inventoryGroup.EndDate,
                station_inventory_manifest = inventoryGroup.Manifests
                                    .Where(m => m.Id == null)
                                    .Select(manifest => _MapToStationInventoryManifest(manifest, inventoryFile)).ToList()
            };
        }

        private station_inventory_manifest _MapToStationInventoryManifest(StationInventoryManifest manifest, InventoryFileBase inventoryFile)
        {
            return new station_inventory_manifest()
            {
                station_id = manifest.Station.Id,
                spot_length_id = manifest.SpotLengthId,
                spots_per_day = manifest.SpotsPerDay,
                spots_per_week = manifest.SpotsPerWeek,
                effective_date = manifest.EffectiveDate,
                file_id = inventoryFile.Id,
                inventory_source_id = inventoryFile.InventorySource.Id,
                end_date = manifest.EndDate,
                comment = manifest.Comment,
                station_inventory_manifest_audiences =
                                        manifest.ManifestAudiences.Select(
                                            audience => new station_inventory_manifest_audiences()
                                            {
                                                audience_id = audience.Audience.Id,
                                                impressions = audience.Impressions,
                                                rating = audience.Rating,
                                                cpm = audience.CPM,
                                                is_reference = audience.IsReference
                                            })
                                            .Union(
                                                manifest.ManifestAudiencesReferences.Select(
                                                    audience => new station_inventory_manifest_audiences()
                                                    {
                                                        audience_id = audience.Audience.Id,
                                                        impressions = audience.Impressions,
                                                        rating = audience.Rating,
                                                        cpm = audience.CPM,
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
                                            spot_cost = mr.SpotCost,
                                            spot_length_id = mr.SpotLengthId
                                        }).ToList(),
                station_inventory_manifest_weeks = manifest.ManifestWeeks.Select(
                                        w => new station_inventory_manifest_weeks
                                        {
                                            media_week_id = w.MediaWeek.Id,
                                            spots = w.Spots
                                        }).ToList()
            };
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
                            groupToUpdate.slot_number = (byte)inventoryGroup.SlotNumber;
                            groupToUpdate.start_date = inventoryGroup.StartDate;
                        });
                    context.SaveChanges();
                });
        }

        public void ExpireInventoryGroupsAndManifests(List<StationInventoryGroup> inventoryGroups, DateTime expireDate, DateTime newEffectiveDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var groupIds = inventoryGroups
                        .Where(g => g.Id.HasValue)
                        .Select(g => g.Id);
                    var groups = context.station_inventory_group
                        .Include(g => g.station_inventory_manifest)
                        .Where(g => groupIds.Contains(g.id));
                    groups.ForEach(g =>
                        {
                            g.end_date = expireDate;
                            g.station_inventory_manifest
                                    .Where(m => m.end_date == null || g.end_date >= newEffectiveDate)
                                    .ForEach(m => m.end_date = expireDate);
                        });
                    context.SaveChanges();
                });
        }

        public void UpdateInventoryManifestsDateIntervals(List<StationInventoryManifest> inventoryManifests)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var manifestIds = inventoryManifests.Select(x => x.Id);
                    var manifests = context.station_inventory_manifest
                                            .Include(x => x.station_inventory_manifest_weeks)
                                            .Where(x => manifestIds.Contains(x.id));

                    foreach (var manifest in manifests)
                    {
                        var inventoryManifest = inventoryManifests.Single(x => x.Id == manifest.id);

                        // no need to change weeks if date range hasn`t changed
                        if (inventoryManifest.EffectiveDate != manifest.effective_date ||
                            inventoryManifest.EndDate != manifest.end_date)
                        {
                            context.station_inventory_manifest_weeks.RemoveRange(manifest.station_inventory_manifest_weeks);
                            manifest.station_inventory_manifest_weeks = inventoryManifest.ManifestWeeks
                                .Select(x => new station_inventory_manifest_weeks
                                {
                                    media_week_id = x.MediaWeek.Id,
                                    spots = x.Spots
                                })
                                .ToList();
                        }

                        manifest.effective_date = inventoryManifest.EffectiveDate;
                        manifest.end_date = inventoryManifest.EndDate;
                    }

                    context.SaveChanges();
                });
        }

        public List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests =
                        (from m in
                            context.station_inventory_manifest
                                .Include(l => l.station_inventory_manifest_audiences)
                                .Include(x => x.station_inventory_manifest_weeks)
                                .Include(x => x.station_inventory_manifest_rates)
                                .Include(x => x.station_inventory_manifest_dayparts)
                                .Include(s => s.station)
                         join g in context.station_inventory_group on m.station_inventory_group_id equals g.id
                         where m.file_id == fileId
                         select g).ToList();

                    return manifests.Select(_MapToInventoryGroup).ToList();
                });
        }

        public List<StationInventoryManifest> GetStationInventoryManifestsByFileId(int fileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests =
                        (from m in
                            context.station_inventory_manifest
                                .Include(l => l.station_inventory_manifest_audiences)
                                .Include(x => x.station_inventory_manifest_weeks)
                                .Include(x => x.station_inventory_manifest_rates)
                                .Include(x => x.station_inventory_manifest_dayparts)
                                .Include(s => s.station)
                                .Include(x => x.inventory_files)
                                .Include(x => x.inventory_files.inventory_file_barter_header)
                         where m.file_id == fileId
                         select m).ToList();

                    return manifests
                        .Select(manifest => _MapToInventoryManifest(manifest, manifest.inventory_files.inventory_file_barter_header.SingleOrDefault()?.daypart_code))
                        .ToList();
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
                    InventoryType = (InventorySourceTypeEnum)stationInventoryGroup.inventory_sources.inventory_source_type,
                    Name = stationInventoryGroup.inventory_sources.name,
                    IsActive = stationInventoryGroup.inventory_sources.is_active
                },
                Manifests = stationInventoryGroup.station_inventory_manifest
                    .Select(manifest => _MapToInventoryManifest(manifest, stationInventoryGroup.daypart_code))
                    .ToList()
            };

            return sig;
        }

        private StationInventoryManifest _MapToInventoryManifest(station_inventory_manifest manifest, string daypartCode = null)
        {
            return new StationInventoryManifest()
            {
                Id = manifest.id,
                Station = new DisplayBroadcastStation()
                {
                    Id = manifest.station.id,
                    Affiliation = manifest.station.affiliation,
                    Code = manifest.station.station_code,
                    CallLetters = manifest.station.station_call_letters,
                    LegacyCallLetters = manifest.station.legacy_call_letters,
                    MarketCode = manifest.station.market_code
                },
                EffectiveDate = manifest.effective_date,
                EndDate = manifest.end_date,
                InventorySourceId = manifest.inventory_source_id,
                DaypartCode = daypartCode,
                SpotLengthId = manifest.spot_length_id,
                SpotsPerWeek = manifest.spots_per_week,
                SpotsPerDay = manifest.spots_per_day,
                Comment = manifest.comment,
                InventoryFileId = manifest.file_id,
                ManifestDayparts = manifest.station_inventory_manifest_dayparts.Select(md => new StationInventoryManifestDaypart()
                {
                    Id = md.id,
                    Daypart = DaypartCache.Instance.GetDisplayDaypart(md.daypart_id),
                    ProgramName = md.program_name
                }).ToList(),
                ManifestAudiences = manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                audience => new StationInventoryManifestAudience()
                                {
                                    Audience = new DisplayAudience()
                                    { Id = audience.audience_id, AudienceString = audience.audience.name },
                                    IsReference = false,
                                    Impressions = audience.impressions,
                                    CPM = audience.cpm
                                }).ToList(),
                ManifestAudiencesReferences = manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true).Select(
                                audience => new StationInventoryManifestAudience()
                                {
                                    Audience = new DisplayAudience()
                                    { Id = audience.audience_id, AudienceString = audience.audience.name },
                                    IsReference = true,
                                    Impressions = audience.impressions,
                                    CPM = audience.cpm
                                }).ToList(),
                ManifestRates = manifest.station_inventory_manifest_rates
                                .Select(r => new StationInventoryManifestRate()
                                {
                                    Id = r.id,
                                    SpotCost = r.spot_cost,
                                    SpotLengthId = r.spot_length_id
                                }).ToList(),
                ManifestWeeks = manifest.station_inventory_manifest_weeks.Select(x => new StationInventoryManifestWeek
                {
                    Id = x.id,
                    Spots = x.spots,
                    MediaWeek = new MediaWeek
                    {
                        Id = x.media_weeks.id,
                        MediaMonthId = x.media_weeks.media_month_id,
                        WeekNumber = x.media_weeks.week_number,
                        StartDate = x.media_weeks.start_date,
                        EndDate = x.media_weeks.end_date
                    }
                }).ToList()
            };
        }

        public List<StationInventoryGroup> GetActiveInventoryBySourceAndName(
                                                InventorySource inventorySource,
                                                List<string> groupNames,
                                                DateTime newEffectiveDate)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var inventory = GetInventoryGroupQuery(inventorySource, groupNames, c);

                    var output = inventory.Where(i => i.end_date == null || i.end_date >= newEffectiveDate).ToList();

                    return output.Select(_MapToInventoryGroup).ToList();
                });
        }

        public List<StationInventoryManifest> GetActiveInventoryManifestsBySourceAndContractedDaypart(
            InventorySource source,
            int contractedDaypartId,
            DateTime effectiveDate,
            DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var query = c.station_inventory_manifest
                        .Where(x => x.inventory_source_id == source.Id &&
                                    x.inventory_files.status == (int)FileStatusEnum.Loaded &&
                                    x.inventory_files.inventory_file_barter_header.FirstOrDefault().contracted_daypart_id == contractedDaypartId);

                    query = query.Where(x => endDate >= x.effective_date && endDate < x.end_date ||
                                             endDate >= x.end_date && effectiveDate <= x.end_date);

                    var output = query.ToList();

                    return output.Select(x => _MapToInventoryManifest(x)).ToList();
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
                            .Include(m => m.station_inventory_manifest_dayparts.Select(d => d.station_inventory_manifest_daypart_genres))
                            .Include(m => m.station)
                            .Include(m => m.station_inventory_manifest_audiences)
                            .Include(m => m.station_inventory_manifest_generation)
                            .Include(m => m.station_inventory_group)
                            .Include(m => m.inventory_sources)
                            .Include(m => m.station_inventory_manifest_rates)
                                     where sp.station.station_code == (short)stationCode
                              && sp.inventory_sources.name.ToLower().Equals(rateSource.Name.ToLower())
                                     select new StationInventoryManifest()
                                     {
                                         Id = sp.id,
                                         Station =
                                         new DisplayBroadcastStation()
                                         {
                                             Id = sp.station.id,
                                             Code = sp.station.station_code.Value,
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
                                                 ProgramName = d.program_name,
                                                 Genres = d.station_inventory_manifest_daypart_genres.Select(g => new LookupDto
                                                 {
                                                     Id = g.genre_id,
                                                     Display = g.genre.name
                                                 }).ToList(),
                                             }).ToList(),
                                         InventorySourceId = sp.inventory_source_id,
                                         EffectiveDate = sp.effective_date,
                                         ManifestAudiencesReferences =
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true).Select(ma => new StationInventoryManifestAudience()
                                             {
                                                 Impressions = ma.impressions,
                                                 Rating = ma.rating,
                                                 CPM = ma.cpm,
                                                 Audience = new DisplayAudience() { Id = ma.audience_id, AudienceString = ma.audience.name },
                                                 IsReference = true
                                             }).ToList(),
                                         ManifestAudiences =
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                                 ma => new StationInventoryManifestAudience()
                                                 {
                                                     Impressions = ma.impressions,
                                                     Rating = ma.rating,
                                                     CPM = ma.cpm,
                                                     Audience = new DisplayAudience() { Id = ma.audience_id, AudienceString = ma.audience.name },
                                                     IsReference = false
                                                 }).ToList(),
                                         ManifestRates = sp.station_inventory_manifest_rates.Select(mr => new StationInventoryManifestRate()
                                         {
                                             Id = mr.id,
                                             SpotCost = mr.spot_cost,
                                             SpotLengthId = mr.spot_length_id,
                                         }).ToList(),
                                         EndDate = sp.end_date,
                                         InventoryFileId = sp.file_id
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
                                     where sp.station.station_code == (short)stationCode
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
                                Id = sp.station.id,
                                Code = sp.station.station_code.Value,
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
                                CPM = ma.cpm,
                                Audience = new DisplayAudience() { Id = ma.audience_id, AudienceString = ma.audience.name },
                                IsReference = true
                            }).ToList(),
                                         ManifestAudiences =
                            sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                ma => new StationInventoryManifestAudience()
                                {
                                    Impressions = ma.impressions,
                                    Rating = ma.rating,
                                    CPM = ma.cpm,
                                    Audience = new DisplayAudience() { Id = ma.audience_id, AudienceString = ma.audience.name },
                                    IsReference = false
                                }).ToList(),
                                         ManifestRates = sp.station_inventory_manifest_rates.Select(mr => new StationInventoryManifestRate()
                                         {
                                             Id = mr.id,
                                             SpotCost = mr.spot_cost,
                                             SpotLengthId = mr.spot_length_id,
                                         }).ToList(),
                                         EndDate = sp.end_date,
                                         InventoryFileId = sp.file_id
                                     }).ToList();

                    return manifests;
                });
        }

        public StationInventoryManifest GetStationManifest(int manifestId)
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
                                     where sp.id == manifestId
                                     select new StationInventoryManifest()
                                     {
                                         Id = sp.id,
                                         Station =
                                             new DisplayBroadcastStation()
                                             {
                                                 Id = sp.station.id,
                                                 Code = sp.station.station_code.Value,
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
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true)
                                                 .Select(ma => new StationInventoryManifestAudience()
                                                 {
                                                     Impressions = ma.impressions,
                                                     Rating = ma.rating,
                                                     CPM = ma.cpm,
                                                     Audience =
                                                         new DisplayAudience()
                                                         {
                                                             Id = ma.audience_id,
                                                             AudienceString = ma.audience.name
                                                         },
                                                     IsReference = true
                                                 }).ToList(),
                                         ManifestAudiences =
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                                 ma => new StationInventoryManifestAudience()
                                                 {
                                                     Impressions = ma.impressions,
                                                     Rating = ma.rating,
                                                     CPM = ma.cpm,
                                                     Audience =
                                                         new DisplayAudience()
                                                         {
                                                             Id = ma.audience_id,
                                                             AudienceString = ma.audience.name
                                                         },
                                                     IsReference = false
                                                 }).ToList(),
                                         ManifestRates =
                                             sp.station_inventory_manifest_rates.Select(mr => new StationInventoryManifestRate()
                                             {
                                                 SpotCost = mr.spot_cost,
                                                 SpotLengthId = mr.spot_length_id,
                                             }).ToList(),
                                         EndDate = sp.end_date,
                                         InventoryFileId = sp.file_id
                                     }).Single();

                    return manifests;
                });
        }

        public void RemoveManifest(int manifestId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var manifest = context.station_inventory_manifest.Single(m => m.id == manifestId);

                context.station_inventory_manifest.Remove(manifest);

                context.SaveChanges();
            });
        }

        public List<StationInventoryManifest> GetManifestProgramsByStationCodeAndDates(string rateSource, int code, DateTime startDate, DateTime endDate)
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
                                     where sp.inventory_sources.name == rateSource &&
                                           sp.station.station_code == code &&
                                           sp.effective_date <= endDate &&
                                           startDate <= sp.end_date
                                     select new StationInventoryManifest()
                                     {
                                         Id = sp.id,
                                         Station =
                                             new DisplayBroadcastStation()
                                             {
                                                 Id = sp.station.id,
                                                 Code = sp.station.station_code.Value,
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
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true)
                                                 .Select(ma => new StationInventoryManifestAudience()
                                                 {
                                                     Impressions = ma.impressions,
                                                     Rating = ma.rating,
                                                     CPM = ma.cpm,
                                                     Audience =
                                                         new DisplayAudience()
                                                         {
                                                             Id = ma.audience_id,
                                                             AudienceString = ma.audience.name
                                                         },
                                                     IsReference = true
                                                 }).ToList(),
                                         ManifestAudiences =
                                             sp.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                                 ma => new StationInventoryManifestAudience()
                                                 {
                                                     Impressions = ma.impressions,
                                                     Rating = ma.rating,
                                                     CPM = ma.cpm,
                                                     Audience =
                                                         new DisplayAudience()
                                                         {
                                                             Id = ma.audience_id,
                                                             AudienceString = ma.audience.name
                                                         },
                                                     IsReference = false
                                                 }).ToList(),
                                         ManifestRates =
                                             sp.station_inventory_manifest_rates.Select(mr => new StationInventoryManifestRate()
                                             {
                                                 SpotCost = mr.spot_cost,
                                                 SpotLengthId = mr.spot_length_id,
                                             }).ToList(),
                                         EndDate = sp.end_date,
                                         InventoryFileId = sp.file_id
                                     }).ToList();

                    return manifests;
                });
        }

        public void SaveStationInventoryManifest(StationInventoryManifest stationInventoryManifest)
        {
            _InReadUncommitedTransaction(context =>
            {
                var manifest = new station_inventory_manifest
                {
                    effective_date = stationInventoryManifest.EffectiveDate,
                    end_date = stationInventoryManifest.EndDate,
                    station_id = stationInventoryManifest.Station.Id,
                    spot_length_id = stationInventoryManifest.SpotLengthId,
                    inventory_source_id = stationInventoryManifest.InventorySourceId
                };

                foreach (var manifestDaypart in stationInventoryManifest.ManifestDayparts)
                {
                    var newManifestDaypart = new station_inventory_manifest_dayparts
                    {
                        daypart_id = manifestDaypart.Daypart.Id,
                        program_name = manifestDaypart.ProgramName
                    };

                    foreach (var genre in manifestDaypart.Genres)
                    {
                        newManifestDaypart.station_inventory_manifest_daypart_genres.Add(new station_inventory_manifest_daypart_genres
                        {
                            genre_id = genre.Id
                        });
                    }

                    manifest.station_inventory_manifest_dayparts.Add(newManifestDaypart);
                }

                foreach (var manifestRates in stationInventoryManifest.ManifestRates)
                {
                    manifest.station_inventory_manifest_rates.Add(new station_inventory_manifest_rates
                    {
                        spot_length_id = manifestRates.SpotLengthId,
                        spot_cost = manifestRates.SpotCost
                    });
                }

                foreach (var manifestAudiences in stationInventoryManifest.ManifestAudiencesReferences)
                {
                    manifest.station_inventory_manifest_audiences.Add(new station_inventory_manifest_audiences
                    {
                        audience_id = manifestAudiences.Audience.Id,
                        rating = manifestAudiences.Rating,
                        impressions = manifestAudiences.Impressions,
                        is_reference = manifestAudiences.IsReference
                    });
                }

                context.station_inventory_manifest.Add(manifest);

                context.SaveChanges();
            });
        }

        public void UpdateStationInventoryManifest(StationInventoryManifest stationInventoryManifest)
        {
            _InReadUncommitedTransaction(context =>
            {
                var manifest = context.station_inventory_manifest.Single(i => i.id == stationInventoryManifest.Id);

                manifest.effective_date = stationInventoryManifest.EffectiveDate;
                manifest.end_date = stationInventoryManifest.EndDate;
                manifest.spot_length_id = stationInventoryManifest.SpotLengthId;
                manifest.inventory_source_id = stationInventoryManifest.InventorySourceId;

                context.station_inventory_manifest_rates.RemoveRange(manifest.station_inventory_manifest_rates);

                foreach (var manifestRates in stationInventoryManifest.ManifestRates)
                {
                    manifest.station_inventory_manifest_rates.Add(new station_inventory_manifest_rates
                    {
                        spot_length_id = manifestRates.SpotLengthId,
                        spot_cost = manifestRates.SpotCost
                    });
                }

                context.station_inventory_manifest_dayparts.RemoveRange(manifest.station_inventory_manifest_dayparts);

                foreach (var manifestDaypart in stationInventoryManifest.ManifestDayparts)
                {
                    var newManifestDaypart = new station_inventory_manifest_dayparts
                    {
                        daypart_id = manifestDaypart.Daypart.Id,
                        program_name = manifestDaypart.ProgramName
                    };

                    foreach (var genre in manifestDaypart.Genres)
                    {
                        newManifestDaypart.station_inventory_manifest_daypart_genres.Add(new station_inventory_manifest_daypart_genres
                        {
                            genre_id = genre.Id
                        });
                    }

                    manifest.station_inventory_manifest_dayparts.Add(newManifestDaypart);
                }

                context.station_inventory_manifest_audiences.RemoveRange(manifest.station_inventory_manifest_audiences);

                foreach (var manifestAudiences in stationInventoryManifest.ManifestAudiencesReferences)
                {
                    manifest.station_inventory_manifest_audiences.Add(new station_inventory_manifest_audiences
                    {
                        audience_id = manifestAudiences.Audience.Id,
                        rating = manifestAudiences.Rating,
                        impressions = manifestAudiences.Impressions,
                        is_reference = manifestAudiences.IsReference
                    });
                }

                context.SaveChanges();
            });
        }

        public bool CheckIfManifestByStationProgramFlightDaypartExists(
            int stationId,
            string programName,
            DateTime startDate,
            DateTime endDate,
            int daypartId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var matchingManifestDayparts = context.station_inventory_manifest_dayparts.Where(
                        md =>
                            md.daypart_id == daypartId
                            && md.program_name.Equals(programName, StringComparison.InvariantCultureIgnoreCase)
                            && md.station_inventory_manifest.station_id == stationId
                            && md.station_inventory_manifest.effective_date == startDate
                            && md.station_inventory_manifest.end_date == endDate).Select(md => md.id).ToList();
                    return matchingManifestDayparts.Any();
                });
        }

        public void ExpireManifest(int manifestId, DateTime endDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var manifest = context.station_inventory_manifest.Single(m => m.id == manifestId);

                    manifest.end_date = endDate;

                    context.SaveChanges();
                });
        }

        public bool HasSpotsAllocated(int manifestId)
        {
            return _InReadUncommitedTransaction(
                context => context.station_inventory_spots.Any(s => s.station_inventory_manifest_id == manifestId));
        }

        public List<StationInventoryGroup> GetActiveInventoryGroupsBySourceAndContractedDaypart(InventorySource source, int contractedDaypartId, DateTime effectiveDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    // separation into 2 requests should work faster than looking at groups -> manifests -> inventory_files -> header.contracted_daypart_id
                    var fileIds = c.inventory_files
                        .Where(x =>
                            x.status == (int)FileStatusEnum.Loaded &&
                            x.inventory_file_barter_header.FirstOrDefault().contracted_daypart_id == contractedDaypartId)
                        .Select(x => x.id)
                        .ToList();

                    var query = c.station_inventory_group
                                .Include(x => x.station_inventory_manifest)
                                .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_audiences))
                                .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks))
                                .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_rates))
                                .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts))
                                .Include(x => x.station_inventory_manifest.Select(m => m.station))
                        .Where(x => x.inventory_source_id == source.Id && x.station_inventory_manifest.Any(m => fileIds.Contains(m.file_id.Value)));

                    query = query.Where(x => endDate >= x.start_date && endDate < x.end_date ||
                                             endDate >= x.end_date && effectiveDate <= x.end_date);

                    return query.ToList().Select(x => _MapToInventoryGroup(x)).ToList();
                });
        }

        /// <summary>
        /// Get inventory data for the sxc file
        /// </summary>
        /// <param name="startDate">Start date of the quarter</param>
        /// <param name="endDate">End date of the quarter</param>
        /// <returns>List of StationInventoryGroup objects containing the data</returns>
        public List<StationInventoryGroup> GetInventoryScxData(DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = (from inventory_group in context.station_inventory_group
                                    .Include(x => x.station_inventory_manifest)
                                    .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_audiences))
                                    .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks))
                                    .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_rates))
                                    .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts))
                                    .Include(x => x.station_inventory_manifest.Select(m => m.station))
                                 where inventory_group.start_date >= startDate && inventory_group.end_date <= endDate
                                 select inventory_group);

                    return query.ToList().Select(_MapToInventoryGroup).ToList();
                });
        }

        /// <summary>
        /// Gets the header information for an inventory file id
        /// </summary>
        /// <param name="inventoryFileId">Inventory file id to get the data for</param>
        /// <returns>BarterInventoryHeader object containing the header data</returns>
        public BarterInventoryHeader GetInventoryFileHeader(int inventoryFileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = (from header in context.inventory_file_barter_header
                                 where header.inventory_file_id == inventoryFileId
                                 select header);
                    return query.Select(x => new BarterInventoryHeader
                    {
                        Audience = new BroadcastAudience { Id = x.audience_id.Value },
                        ContractedDaypartId = x.contracted_daypart_id,
                        Cpm = x.cpm,
                        DaypartCode = x.daypart_code,
                        EffectiveDate  = x.effective_date,
                        EndDate = x.end_date,
                        HutBookId = x.hut_projection_book_id,
                        PlaybackType = (ProposalEnums.ProposalPlaybackType)x.playback_type,
                        ShareBookId = x.share_projection_book_id
                    }).Single();
                });
        }

        public void UpdateInventoryGroupsDateIntervals(List<StationInventoryGroup> inventoryGroups)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var groupIds = inventoryGroups.Select(x => x.Id);
                    var groups = context.station_inventory_group
                        .Include(x => x.station_inventory_manifest)
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks))
                        .Where(x => groupIds.Contains(x.id));

                    foreach (var group in groups)
                    {
                        var inventoryGroup = inventoryGroups.Single(x => x.Id == group.id);
                        group.start_date = inventoryGroup.StartDate;
                        group.end_date = inventoryGroup.EndDate;

                        foreach (var manifest in group.station_inventory_manifest)
                        {
                            var inventoryManifest = inventoryGroup.Manifests.SingleOrDefault(x => x.Id == manifest.id);

                            if (inventoryManifest != null)
                            {
                                // no need to change weeks if date range hasn`t changed
                                if (inventoryManifest.EffectiveDate != manifest.effective_date ||
                                    inventoryManifest.EndDate != manifest.end_date)
                                {
                                    context.station_inventory_manifest_weeks.RemoveRange(manifest.station_inventory_manifest_weeks);
                                    manifest.station_inventory_manifest_weeks = inventoryManifest.ManifestWeeks
                                        .Select(x => new station_inventory_manifest_weeks
                                        {
                                            media_week_id = x.MediaWeek.Id,
                                            spots = x.Spots
                                        })
                                        .ToList();
                                }

                                manifest.effective_date = inventoryManifest.EffectiveDate;
                                manifest.end_date = inventoryManifest.EndDate;
                            }
                        }
                    }

                    context.SaveChanges();
                });
        }

        public void AddInventoryGroups(List<StationInventoryGroup> groups, InventoryFileBase inventoryFile)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var newGroups = groups
                        .Select(inventoryGroup => _MapToStationInventoryGroup(inventoryGroup, inventoryFile))
                        .ToList();
                    
                    context.station_inventory_group.AddRange(newGroups);
                    context.SaveChanges();
                });
        }

        public void AddInventoryManifests(List<StationInventoryManifest> manifests, InventoryFileBase inventoryFile)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var newManifests = manifests
                        .Select(manifest => _MapToStationInventoryManifest(manifest, inventoryFile))
                        .ToList();

                    context.station_inventory_manifest.AddRange(newManifests);
                    context.SaveChanges();
                });
        }

        public void UpdateInventoryRatesForManifests(List<StationInventoryManifest> manifests)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   foreach(var manifest in manifests)
                   {
                       var dbManifest = context.station_inventory_manifest
                            .Include(m => m.station_inventory_manifest_rates)
                            .SingleOrDefault(m => m.id == manifest.Id);
                       if (dbManifest == null) continue;

                       foreach(var rate in manifest.ManifestRates)
                       {
                           var dbRate = dbManifest.station_inventory_manifest_rates
                                .Where(r => r.spot_length_id == rate.SpotLengthId).SingleOrDefault();

                           if(dbRate == null)
                           {
                               dbRate = new station_inventory_manifest_rates
                               {
                                   spot_length_id = rate.SpotLengthId
                               };
                               dbManifest.station_inventory_manifest_rates.Add(dbRate);
                           }
                           dbRate.spot_cost = rate.SpotCost;
                       }

                   }

                   context.SaveChanges();
               });
        }
    }
}
