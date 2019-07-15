using System.Data.Entity;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services;
using ConfigurationService.Client;
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
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryRepository : IDataRepository
    {
        List<InventorySource> GetInventorySources();
        InventorySource GetInventorySourceByName(string sourceName);
        void AddNewInventory(InventoryFileBase inventoryFile);
        List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId);
        List<StationInventoryManifest> GetStationInventoryManifestsByFileId(int fileId);
        List<StationInventoryGroup> GetActiveInventoryByTypeAndUnitName(InventorySource inventorySource, List<string> daypartCodes);
        List<StationInventoryManifest> GetStationInventoryManifestsByIds(IEnumerable<int> manifestIds);
        List<StationInventoryManifestWeek> GetStationInventoryManifestWeeks(InventorySource inventorySource, int contractedDaypartId, IEnumerable<int> mediaWeekIds);
        List<StationInventoryManifestWeekHistory> GetStationInventoryManifestWeeksHistory(IEnumerable<int> manifestIds);
        List<StationInventoryManifest> GetInventoryManifestsBySource(InventorySource source);

        /// <summary>
        /// Removes station_inventory_manifest_weeks records based on their id
        /// </summary>
        /// <param name="weeks">List of StationInventoryManifestWeek to remove</param>
        void RemoveManifestWeeks(List<StationInventoryManifestWeek> weeks);

        List<StationInventoryGroup> GetInventoryBySourceAndName(InventorySource inventorySource, List<string> groupNames);
        List<StationInventoryManifest> GetStationManifestsBySourceAndStationCode(
                                            InventorySource rateSource, int stationCode);
        StationInventoryManifest GetStationManifest(int manifestId);
        void RemoveManifest(int manifestId);
        bool HasSpotsAllocated(int manifestId);
        InventorySource GetInventorySource(int inventorySourceId);
        DateRange GetInventorySourceDateRange(int inventorySourceId);
        DateRange GetAllInventoriesDateRange();

        /// <summary>
        /// Update the rates and adds the audiences for all the manifests in the list
        /// </summary>        
        /// <param name="manifests">List of manifests to process</param>
        void UpdateInventoryManifests(List<StationInventoryManifest> manifests);

        /// <summary>
        /// Get inventory data for the sxc file        
        /// </summary>
        /// <param name="startDate">Start date of the quarter</param>
        /// <param name="endDate">End date of the quarter</param>
        /// <returns>List of StationInventoryGroup objects containing the data</returns>
        List<StationInventoryGroup> GetInventoryScxDataForBarter(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames);

        List<StationInventoryManifest> GetInventoryScxDataForOAndO(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate);
        
            /// <summary>
        /// Gets the header information for an inventory file id
        /// </summary>
        /// <param name="inventoryFileId">Inventory file id to get the data for</param>
        /// <returns>ProprietaryInventoryHeader object containing the header data</returns>
        ProprietaryInventoryHeader GetInventoryFileHeader(int inventoryFileId);

        /// <summary>
        /// Adds the manifest audiences
        /// </summary>
        /// <param name="manifests">List of manifests containing audiences</param>
        void AddInventoryAudiencesForManifests(List<StationInventoryManifest> manifests);

        DateRange GetInventoryStartAndEndDates(int inventorySourceId, int daypartCodeId);

        /// <summary>
        /// Returns all the weeks for a station, program name and daypart that need to be expired
        /// </summary>
        /// <param name="stationId">Station id to filter stations</param>
        /// <param name="programName">Program name to filter programs</param>
        /// <param name="daypartId">Daypart id to filter dayparts</param>
        /// <returns>List of StationInventoryManifestWeek objects</returns>
        List<StationInventoryManifestWeek> GetStationInventoryManifestWeeksForOpenMarket(int stationId, string programName, int daypartId);

        List<StationInventoryGroup> GetInventoryGroups(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate);

        List<StationInventoryManifestWeek> GetStationInventoryManifestWeeksForInventorySource(int inventorySourceId);
        List<InventoryUploadHistoryDto> GetInventoryUploadHistoryForInventorySource(int inventorySourceId);

        /// <summary>
        /// Adds validation problems for an inventory file to DB
        /// </summary>
        /// <param name="file">File containing validation problems</param>
        void AddValidationProblems(InventoryFileBase file);
    }

    public class InventoryRepository : BroadcastRepositoryBase, IInventoryRepository
    {
        public InventoryRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        /// <summary>
        /// Adds validation problems for an inventory file to DB
        /// </summary>
        /// <param name="file">File containing validation problems</param>
        public void AddValidationProblems(InventoryFileBase file)
        {
            _InReadUncommitedTransaction(context =>
            {
                var inventoryFile = context.inventory_files.Single(x => x.id == file.Id);
                inventoryFile.status = (byte)file.FileStatus;
                inventoryFile.inventory_file_problems = file.ValidationProblems.Select(
                            x => new inventory_file_problems
                            {
                                problem_description = x
                            }).ToList();
                context.SaveChanges();
            });
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
                inventory_source_id = inventoryFile.InventorySource.Id,
                name = inventoryGroup.Name,
                slot_number = (byte)inventoryGroup.SlotNumber,
                station_inventory_manifest = inventoryGroup.Manifests
                                    .Where(m => m.Id == null)
                                    .Select(manifest => _MapToStationInventoryManifest(manifest, inventoryFile)).ToList()
            };
        }

        private station_inventory_manifest _MapToStationInventoryManifest(StationInventoryManifest manifest, InventoryFileBase inventoryFile)
        {
            return new station_inventory_manifest()
            {
                station_id = manifest.Station?.Id,
                spot_length_id = manifest.SpotLengthId,
                spots_per_day = manifest.SpotsPerDay,
                spots_per_week = manifest.SpotsPerWeek,
                file_id = inventoryFile.Id,
                inventory_source_id = inventoryFile.InventorySource.Id,
                comment = manifest.Comment,
                station_inventory_manifest_audiences =
                                        manifest.ManifestAudiences.Select(
                                            audience => new station_inventory_manifest_audiences()
                                            {
                                                audience_id = audience.Audience.Id,
                                                impressions = audience.Impressions,
                                                rating = audience.Rating,
                                                cpm = audience.CPM,
                                                vpvh = audience.Vpvh,
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
                                            program_name = md.ProgramName,
                                            daypart_code_id = md.DaypartCode?.Id
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
                                            spots = w.Spots,
                                            start_date = w.StartDate,
                                            end_date = w.EndDate
                                        }).ToList()
            };
        }

        public List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var groups =
                        (from m in
                            context.station_inventory_manifest
                                .Include(l => l.station_inventory_manifest_audiences)
                                .Include(x => x.station_inventory_manifest_weeks)
                                .Include(x => x.station_inventory_manifest_rates)
                                .Include(x => x.station_inventory_manifest_dayparts)
                                .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.daypart_codes))
                                .Include(s => s.station)
                         join g in context.station_inventory_group on m.station_inventory_group_id equals g.id
                         where m.file_id == fileId
                         select g).ToList();

                    return groups.Select(_MapToInventoryGroup).ToList();
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
                                .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.daypart_codes))
                                .Include(s => s.station)
                                .Include(x => x.inventory_files)
                                .Include(x => x.inventory_files.inventory_file_proprietary_header)
                         where m.file_id == fileId
                         select m).ToList();

                    return manifests
                        .Select(manifest => _MapToInventoryManifest(manifest, manifest.inventory_files.inventory_file_proprietary_header.SingleOrDefault()?.daypart_codes.code))
                        .ToList();
                });
        }

        public List<StationInventoryManifest> GetInventoryManifestsBySource(InventorySource source)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var query = c.station_inventory_manifest
                        .Include(x => x.station)
                        .Include(x => x.station_inventory_manifest_dayparts)
                        .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.daypart_codes))
                        .Include(x => x.station_inventory_manifest_audiences)
                        .Include(x => x.station_inventory_manifest_rates)
                        .Include(x => x.station_inventory_manifest_weeks)
                        .Where(x => x.inventory_source_id == source.Id &&
                                    x.inventory_files.status == (int)FileStatusEnum.Loaded);

                    return query.ToList().Select(x => _MapToInventoryManifest(x)).ToList();
                });
        }

        private StationInventoryGroup _MapToInventoryGroup(station_inventory_group stationInventoryGroup)
        {
            var headerDaypartCode = stationInventoryGroup.station_inventory_manifest.FirstOrDefault().inventory_files.inventory_file_proprietary_header.FirstOrDefault().daypart_codes.code;
            var sig = new StationInventoryGroup()
            {
                Id = stationInventoryGroup.id,
                Name = stationInventoryGroup.name,
                SlotNumber = stationInventoryGroup.slot_number,
                InventorySource = new InventorySource()
                {
                    Id = stationInventoryGroup.inventory_sources.id,
                    InventoryType = (InventorySourceTypeEnum)stationInventoryGroup.inventory_sources.inventory_source_type,
                    Name = stationInventoryGroup.inventory_sources.name,
                    IsActive = stationInventoryGroup.inventory_sources.is_active
                },
                Manifests = stationInventoryGroup.station_inventory_manifest
                    .Select(manifest => _MapToInventoryManifest(manifest, headerDaypartCode))
                    .ToList()
            };

            return sig;
        }

        private StationInventoryManifest _MapToInventoryManifest(station_inventory_manifest manifest, string daypartCode = null)
        {
            var result = new StationInventoryManifest()
            {
                Id = manifest.id,
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
                    ProgramName = md.program_name,
                    DaypartCode = md.daypart_codes == null ? null : new DaypartCode
                    {
                        Id = md.daypart_codes.id,
                        Code = md.daypart_codes.code,
                        FullName = md.daypart_codes.full_name
                    }
                }).ToList(),
                ManifestAudiences = manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false).Select(
                                audience => new StationInventoryManifestAudience()
                                {
                                    Audience = new DisplayAudience()
                                    {
                                        Id = audience.audience_id,
                                        AudienceString = audience.audience.name
                                    },
                                    IsReference = false,
                                    Impressions = audience.impressions,
                                    CPM = audience.cpm,
                                    Rating = audience.rating,
                                    Vpvh = audience.vpvh
                                }).ToList(),
                ManifestAudiencesReferences = manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true).Select(
                                audience => new StationInventoryManifestAudience()
                                {
                                    Audience = new DisplayAudience()
                                    {
                                        Id = audience.audience_id,
                                        AudienceString = audience.audience.name
                                    },
                                    IsReference = true,
                                    Impressions = audience.impressions,
                                    CPM = audience.cpm,
                                    Rating = audience.rating,
                                    Vpvh = audience.vpvh
                                }).ToList(),
                ManifestRates = manifest.station_inventory_manifest_rates
                                .Select(r => new StationInventoryManifestRate()
                                {
                                    Id = r.id,
                                    SpotCost = r.spot_cost,
                                    SpotLengthId = r.spot_length_id
                                }).ToList(),
                ManifestWeeks = manifest.station_inventory_manifest_weeks
                                .Select(_MapToInventoryManifestWeek)
                                .OrderBy(x => x.Id)
                                .ToList()
            };

            if (manifest.station != null)
            {
                result.Station = new DisplayBroadcastStation()
                {
                    Id = manifest.station.id,
                    Affiliation = manifest.station.affiliation,
                    Code = manifest.station.station_code,
                    CallLetters = manifest.station.station_call_letters,
                    LegacyCallLetters = manifest.station.legacy_call_letters,
                    MarketCode = manifest.station.market_code
                };
            }

            return result;
        }

        private StationInventoryManifestWeek _MapToInventoryManifestWeek(station_inventory_manifest_weeks week)
        {
            return new StationInventoryManifestWeek
            {
                Id = week.id,
                Spots = week.spots,
                StartDate = week.start_date,
                EndDate = week.end_date,
                MediaWeek = new MediaWeek
                {
                    Id = week.media_weeks.id,
                    MediaMonthId = week.media_weeks.media_month_id,
                    WeekNumber = week.media_weeks.week_number,
                    StartDate = week.media_weeks.start_date,
                    EndDate = week.media_weeks.end_date
                }
            };
        }

        private StationInventoryManifestWeekHistory _MapToInventoryManifestWeekHistory(station_inventory_manifest_weeks_history week)
        {
            return new StationInventoryManifestWeekHistory
            {
                Id = week.id,
                Spots = week.spots,
                StartDate = week.start_date,
                EndDate = week.end_date,
                MediaWeekId = week.media_week_id,
                SysStartDate = week.sys_start_date,
                SysEndDate = week.sys_end_date
            };
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
                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks))
                             where g.inventory_source_id == inventorySource.Id
                                   && groupNames.Contains(g.name)
                             select g);
            return inventory;
        }

        public List<StationInventoryGroup> GetActiveInventoryByTypeAndUnitName(InventorySource inventorySource, List<string> unitNames)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var inventory = (from g in c.station_inventory_group
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts))
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_group))
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_audiences))
                                    .Include(ig => ig.station_inventory_manifest.Select(m => m.station_inventory_manifest_rates))
                                     where g.inventory_source_id == inventorySource.Id
                                    && unitNames.Contains(g.name, StringComparer.InvariantCultureIgnoreCase)
                                    && g.station_inventory_manifest.Any(x => x.station_inventory_manifest_weeks.Any())
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

        public bool HasSpotsAllocated(int manifestId)
        {
            return _InReadUncommitedTransaction(
                context => context.station_inventory_spots.Any(s => s.station_inventory_manifest_id == manifestId));
        }

        public List<StationInventoryManifestWeek> GetStationInventoryManifestWeeks(InventorySource inventorySource, int contractedDaypartId, IEnumerable<int> mediaWeekIds)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var query = c.station_inventory_manifest_weeks
                        .Where(x => x.station_inventory_manifest.inventory_source_id == inventorySource.Id &&
                                    x.station_inventory_manifest.inventory_files.status == (int)FileStatusEnum.Loaded &&
                                    x.station_inventory_manifest.inventory_files.inventory_file_proprietary_header.FirstOrDefault().contracted_daypart_id == contractedDaypartId &&
                                    mediaWeekIds.Contains(x.media_week_id));
                    return query.ToList().Select(_MapToInventoryManifestWeek).ToList();
                });
        }

        /// <summary>
        /// Returns all the weeks for a station, program name and daypart that need to be expired
        /// </summary>
        /// <param name="stationId">Station id to filter stations</param>
        /// <param name="programName">Program name to filter programs</param>
        /// <param name="daypartId">Daypart id to filter dayparts</param>
        /// <returns>List of StationInventoryManifestWeek objects</returns>
        public List<StationInventoryManifestWeek> GetStationInventoryManifestWeeksForOpenMarket(int stationId, string programName, int daypartId)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var query = c.station_inventory_manifest.SelectMany(x => x.station_inventory_manifest_dayparts)
                        .Where(md => md.daypart_id == daypartId
                                   && md.program_name.Equals(programName, StringComparison.InvariantCultureIgnoreCase)
                                   && md.station_inventory_manifest.station_id == stationId)
                        .SelectMany(x => x.station_inventory_manifest.station_inventory_manifest_weeks);

                    return query.ToList().Distinct().Select(_MapToInventoryManifestWeek).ToList();
                });
        }

        public List<StationInventoryManifestWeekHistory> GetStationInventoryManifestWeeksHistory(IEnumerable<int> manifestIds)
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    return c.station_inventory_manifest_weeks_history
                        .Where(x => manifestIds.Contains(x.station_inventory_manifest_id))
                        .ToList()
                        .Select(_MapToInventoryManifestWeekHistory)
                        .ToList();
                });
        }

        /// <summary>
        /// Removes station_inventory_manifest_weeks records based on their id
        /// </summary>
        /// <param name="weeks">List of StationInventoryManifestWeek to remove</param>
        public void RemoveManifestWeeks(List<StationInventoryManifestWeek> weeks)
        {
            _InReadUncommitedTransaction(
                c =>
                {
                    var weekIds = weeks.Select(x => x.Id);
                    var dbWeeks = c.station_inventory_manifest_weeks.Where(x => weekIds.Contains(x.id));
                    c.station_inventory_manifest_weeks.RemoveRange(dbWeeks);
                    c.SaveChanges();
                });
        }

        /// <summary>
        /// Get inventory data for the sxc file
        /// </summary>
        /// <remarks>The commented code is there because BE is done but FE not and we need to use the old logic</remarks>
        /// <param name="startDate">Start date of the quarter</param>
        /// <param name="endDate">End date of the quarter</param>
        /// <returns>List of StationInventoryGroup objects containing the data</returns>
        public List<StationInventoryGroup> GetInventoryScxDataForBarter(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var groups = context.station_inventory_group
                        .SelectMany(x => x.station_inventory_manifest)
                        .SelectMany(x => x.station_inventory_manifest_weeks)
                        .Where(x => x.start_date <= endDate && x.end_date >= startDate) //filter by start/end date
                        .Where(x=> unitNames.Contains(x.station_inventory_manifest.station_inventory_group.name))   //filter by units name
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_source_id == inventorySourceId)   //filter by source
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_file_proprietary_header.FirstOrDefault().daypart_code_id == daypartCodeId) //filter by daypart code
                        .GroupBy(x => x.station_inventory_manifest.station_inventory_group_id)
                        .Select(x => x.FirstOrDefault().station_inventory_manifest.station_inventory_group)
                        .Include(x => x.station_inventory_manifest)
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_audiences))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks.Select(w => w.media_weeks)))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_rates))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station))
                        .Include(x => x.inventory_sources)
                        .ToList()
                        .Select(_MapToInventoryGroup)
                        .ToList();

                    // filter out manifests which are out of the date range
                    foreach (var group in groups)
                    {
                        group.Manifests = group.Manifests.Where(m => m.ManifestWeeks.Any(w => w.StartDate <= endDate && w.EndDate >= startDate)).ToList();                        
                    }

                    return groups;
                });
        }

        public List<StationInventoryManifest> GetInventoryScxDataForOAndO(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests = context.station_inventory_manifest
                        .Where(x => x.inventory_source_id == inventorySourceId)
                        .Where(x => x.inventory_files.inventory_file_proprietary_header.FirstOrDefault().daypart_code_id == daypartCodeId)
                        .Include(x => x.station_inventory_manifest_audiences)
                        .Include(x => x.station_inventory_manifest_weeks)
                        .Include(x => x.station_inventory_manifest_rates)
                        .Include(x => x.station_inventory_manifest_dayparts)
                        .Include(x => x.inventory_sources)
                        .Select(x => x)
                        .ToList()
                        .Select(x => _MapToInventoryManifest(x, x.inventory_files.inventory_file_proprietary_header.SingleOrDefault()?.daypart_codes.code))
                        .ToList();

                    foreach (var manifest in manifests)
                    {
                        manifests = manifests.Where(m => m.ManifestWeeks.Any(w => w.StartDate <= endDate && w.EndDate >= startDate)).ToList();
                    }

                    return manifests;
                });
        }

        /// <summary>
        /// Gets the header information for an inventory file id
        /// </summary>
        /// <param name="inventoryFileId">Inventory file id to get the data for</param>
        /// <returns>ProprietaryInventoryHeader object containing the header data</returns>
        public ProprietaryInventoryHeader GetInventoryFileHeader(int inventoryFileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = (from header in context.inventory_file_proprietary_header
                                 where header.inventory_file_id == inventoryFileId
                                 select header);
                    return query.Select(x => new ProprietaryInventoryHeader
                    {
                        Audience = new BroadcastAudience { Id = x.audience_id.Value },
                        ContractedDaypartId = x.contracted_daypart_id,
                        Cpm = x.cpm,
                        DaypartCode = x.daypart_codes.code,
                        EffectiveDate = x.effective_date,
                        EndDate = x.end_date,
                        HutBookId = x.hut_projection_book_id,
                        PlaybackType = (ProposalEnums.ProposalPlaybackType)x.playback_type,
                        ShareBookId = x.share_projection_book_id
                    }).Single();
                });
        }


        /// <summary>
        /// Update the rates and add the audiences for all the manifests in the list
        /// </summary>
        /// <param name="manifests">List of manifests to process</param>
        public void UpdateInventoryManifests(List<StationInventoryManifest> manifests)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   foreach (var manifest in manifests)
                   {
                       var dbManifest = context.station_inventory_manifest
                            .Include(m => m.station_inventory_manifest_rates)
                            .SingleOrDefault(m => m.id == manifest.Id);

                       if (dbManifest == null) continue;

                       foreach (var rate in manifest.ManifestRates)
                       {
                           var dbRate = dbManifest.station_inventory_manifest_rates
                                .Where(r => r.spot_length_id == rate.SpotLengthId).SingleOrDefault();

                           if (dbRate == null)
                           {
                               dbRate = new station_inventory_manifest_rates
                               {
                                   spot_length_id = rate.SpotLengthId
                               };
                               dbManifest.station_inventory_manifest_rates.Add(dbRate);
                           }
                           dbRate.spot_cost = rate.SpotCost;
                       }

                       //add audiences to db
                       var manifestAudiences = manifest.ManifestAudiences.Select(x => new station_inventory_manifest_audiences
                       {
                           audience_id = x.Audience.Id,
                           impressions = x.Impressions,
                           is_reference = x.IsReference,
                           rating = x.Rating,
                           cpm = x.CPM,
                           station_inventory_manifest_id = dbManifest.id
                       }).ToList();
                       context.station_inventory_manifest_audiences.AddRange(manifestAudiences);
                   }
                   context.SaveChanges();
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

        public DateRange GetInventorySourceDateRange(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var weeks = context.station_inventory_manifest
                        .Where(x => x.inventory_source_id == inventorySourceId)
                        .SelectMany(x => x.station_inventory_manifest_weeks)
                        .ToList();

                   return _GetMinMaxDateRange(weeks);
               });
        }

        public DateRange GetAllInventoriesDateRange()
        {
            return _InReadUncommitedTransaction(context => _GetMinMaxDateRange(context.station_inventory_manifest_weeks.ToList()));
        }

        private DateRange _GetMinMaxDateRange(List<station_inventory_manifest_weeks> weeks)
        {
            if (weeks.Any())
            {
                var minDate = weeks.Min(x => x.start_date);
                var maxDate = weeks.Max(x => x.end_date);

                return new DateRange(minDate, maxDate);
            }

            return new DateRange(null, null);
        }

        /// <summary>
        /// Adds the manifest audiences
        /// </summary>
        /// <param name="manifests">List of manifests containing audiences</param>
        public void AddInventoryAudiencesForManifests(List<StationInventoryManifest> manifests)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   var manifestAudiences = manifests.SelectMany(y => y.ManifestAudiences.Select(x => new station_inventory_manifest_audiences
                   {
                       audience_id = x.Audience.Id,
                       impressions = x.Impressions,
                       is_reference = x.IsReference,
                       rating = x.Rating,
                       cpm = x.CPM,
                       station_inventory_manifest_id = y.Id.Value
                   }).ToList()).ToList();
                   SetManifestAudiencesIds(context, manifestAudiences);
                   BulkInsert(context, manifestAudiences);
               });
        }

        // Manually set detail ids based on the next available ids in the database
        private void SetManifestAudiencesIds(QueryHintBroadcastContext context, List<station_inventory_manifest_audiences> manifestAudiences)
        {
            int nextSequence = context.station_inventory_manifest_audiences.Max(detail => detail.id) + 1;
            manifestAudiences.ForEach(audience =>
            {
                // manually set the id to the next available
                audience.id = nextSequence;
                nextSequence++;
            });
        }

        public DateRange GetInventoryStartAndEndDates(int inventorySourceId, int daypartCodeId)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var weeks = (from week in context.station_inventory_manifest_weeks
                                join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                                join inventoryFile in context.inventory_files on manifest.file_id equals inventoryFile.id
                                join inventoryFileHeader in context.inventory_file_proprietary_header on inventoryFile.id equals inventoryFileHeader.inventory_file_id
                                where manifest.inventory_source_id == inventorySourceId && inventoryFileHeader.daypart_code_id == daypartCodeId
                                group week by week.id into weekGroup
                                select weekGroup.FirstOrDefault()).ToList();
                   
                   if (!weeks.Any())
                       return new DateRange(null, null);
                   
                   var start = weeks.Min(x => x.start_date);
                   var end = weeks.Max(x => x.end_date);

                   return new DateRange(start, end);
               });
        }

        public List<StationInventoryGroup> GetInventoryGroups(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var queryResult = (from week in context.station_inventory_manifest_weeks
                                      join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                                      join manifestGroup in context.station_inventory_group on manifest.station_inventory_group_id equals manifestGroup.id
                                      join inventoryFile in context.inventory_files on manifest.file_id equals inventoryFile.id
                                      join inventoryFileHeader in context.inventory_file_proprietary_header on inventoryFile.id equals inventoryFileHeader.inventory_file_id
                                      where manifest.inventory_source_id == inventorySourceId &&
                                            inventoryFileHeader.daypart_code_id== daypartCodeId &&
                                            week.start_date <= endDate && week.end_date >= startDate
                                      group manifestGroup by manifestGroup.id into manifestGroupGrouping
                                      select manifestGroupGrouping.FirstOrDefault()).ToList();

                   return queryResult.Select(x => new StationInventoryGroup
                   {
                       Id = x.id,
                       Name = x.name,
                       SlotNumber = x.slot_number
                   }).ToList();
               });
        }

        public List<StationInventoryManifest> GetStationInventoryManifestsByIds(IEnumerable<int> manifestIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.station_inventory_manifest
                        .Include(x => x.station_inventory_manifest_audiences)
                        .Include(x => x.station_inventory_manifest_weeks)
                        .Include(x => x.station_inventory_manifest_rates)
                        .Include(x => x.station_inventory_manifest_dayparts)
                        .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.daypart_codes))
                        .Include(x => x.station)
                        .Where(x => manifestIds.Contains(x.id))
                        .ToList()
                        .Select(x => _MapToInventoryManifest(x))
                        .ToList();
                });
        }

        public List<StationInventoryManifestWeek> GetStationInventoryManifestWeeksForInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.station_inventory_manifest
                             .Where(x => x.inventory_source_id == inventorySourceId)
                             .SelectMany(x => x.station_inventory_manifest_weeks)
                             .Include(x => x.media_weeks)
                             .ToList()
                             .Select(_MapToInventoryManifestWeek)
                             .ToList();
                });
        }

        public List<InventoryUploadHistoryDto> GetInventoryUploadHistoryForInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var files = context.inventory_files
                             .Include("inventory_file_proprietary_header.daypart_codes")
                             .Include("inventory_file_proprietary_header.share_media_months")
                             .Include("inventory_file_proprietary_header.hut_media_months")
                             .Include("station_inventory_manifest.station_inventory_manifest_weeks")
                             .Include(f => f.inventory_file_ratings_jobs)
                             .Where(x => x.inventory_source_id == inventorySourceId)
                             .OrderByDescending(x => x.id);

                    var result = new List<InventoryUploadHistoryDto>();

                    foreach(var file in files)
                    {
                        var fileHistory = new InventoryUploadHistoryDto()
                        {
                            FileId = file.id,
                            UploadDateTime = file.created_date,
                            Username = file.created_by,
                            Filename = file.name,
                            Rows = file.rows_processed ?? 0
                        };

                        if(file.inventory_file_proprietary_header.Any()) //Proprietary Inventory file
                        {
                            var header = file.inventory_file_proprietary_header.SingleOrDefault();
                            fileHistory.DaypartCode = header.daypart_codes.code;
                            fileHistory.EffectiveDate = header.effective_date;
                            fileHistory.EndDate = header.end_date;

                            if(header.hut_media_months != null)
                            {
                                fileHistory.HutBook = new MediaMonthDto
                                {
                                    Id = header.hut_media_months.id,
                                    Year = header.hut_media_months.year,
                                    Month = header.hut_media_months.month
                                };
                            }

                            if (header.share_media_months != null)
                            {
                                fileHistory.ShareBook = new MediaMonthDto
                                {
                                    Id = header.share_media_months.id,
                                    Year = header.share_media_months.year,
                                    Month = header.share_media_months.month
                                };
                            }

                        } else if (file.station_inventory_manifest.Any(m => m.station_inventory_manifest_weeks.Any())) //Open Market Inventory file
                        {
                            fileHistory.EffectiveDate = file.station_inventory_manifest.SelectMany(m => m.station_inventory_manifest_weeks.Select(w => w.start_date)).Min();
                            fileHistory.EndDate = file.station_inventory_manifest.SelectMany(m => m.station_inventory_manifest_weeks.Select(w => w.end_date)).Max();
                        }

                        fileHistory.FileLoadStatus = (FileStatusEnum)file.status;
                        if (file.inventory_file_ratings_jobs.Any())
                        {
                            var fileJob = file.inventory_file_ratings_jobs.OrderBy(j => j.id).Last();
                            fileHistory.FileProcessingStatus = (InventoryFileRatingsProcessingStatus)fileJob.status;
                        }

                        result.Add(fileHistory);

                    }

                    return result; 

                });
        }

    }
}
