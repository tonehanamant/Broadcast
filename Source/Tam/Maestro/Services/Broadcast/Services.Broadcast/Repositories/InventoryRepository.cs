using Common.Services;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using System.Diagnostics;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryRepository : IDataRepository
    {
        List<InventorySource> GetInventorySources();
        InventorySource GetInventorySourceByName(string sourceName);
        void AddNewInventoryGroups(InventoryFileBase inventoryFile);
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
        /// Gets the inventory SCX data for barter.
        /// </summary>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        /// <param name="daypartCodeId">The daypart code identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="unitNames">The unit names.</param>
        /// <returns>List of StationInventoryGroup objects</returns>
        List<StationInventoryGroup> GetInventoryScxDataForBarter(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames);

        List<StationInventoryManifest> GetInventoryScxDataForOAndO(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the header information for an inventory file ids
        /// </summary>
        /// <param name="inventoryFileIds">Inventory file ids to get the data for</param>
        /// <returns>ProprietaryInventoryHeader object containing the header data</returns>
        Dictionary<int, ProprietaryInventoryHeader> GetInventoryFileHeader(IEnumerable<int> inventoryFileIds);

        List<StationInventoryManifestWeek> GetStationInventoryManifestWeeks(int inventorySourceId, int daypartCodeId);

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

        List<DateRange> GetInventoryUploadHistoryDatesForInventorySource(int inventorySourceId);

        List<InventoryUploadHistoryDto> GetInventoryUploadHistoryForInventorySource(int inventorySourceId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Adds validation problems for an inventory file to DB
        /// </summary>
        /// <param name="file">File containing validation problems</param>
        void AddValidationProblems(InventoryFileBase file);

        /// <summary>
        /// Adds the new manifests to db without entity validation and detecting changes.
        /// </summary>
        /// <param name="manifests">The manifests.</param>
        /// <param name="inventoryFileId">The inventory file identifier.</param>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        void AddNewManifests(IEnumerable<StationInventoryManifest> manifests, int inventoryFileId, int inventorySourceId);
    }

    public class InventoryRepository : BroadcastRepositoryBase, IInventoryRepository
    {
        public InventoryRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public void AddNewInventoryGroups(InventoryFileBase inventoryFile)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   var newGroups = inventoryFile.InventoryGroups
                       .Where(g => g.Id == null)
                       .Select(inventoryGroup => _MapToStationInventoryGroup(inventoryGroup, inventoryFile)).ToList();

                   //process groups
                   _InsertGroups(context, newGroups);

                   foreach (var group in newGroups)
                   {
                       var mappedManifests = group.station_inventory_manifest.ToList();

                       //process manifests
                       _InsertManifests(context, mappedManifests, group.id);

                       //process manifest audiences
                       _InsertManifestAudiences(context, mappedManifests);

                       //process manifest dayparts
                       _InsertManifestDayparts(context, mappedManifests);

                       //process manifest rates
                       _InsertManifestRates(context, mappedManifests);

                       //process manifest weeks
                       _InsertManifestWeeks(context, mappedManifests);
                   }
               });
        }

        private void _InsertGroups(QueryHintBroadcastContext context, List<station_inventory_group> newGroups)
        {
            var sw = Stopwatch.StartNew();
            var pkGroups = (context.station_inventory_group.Select(x=>(int?)x.id).Max() ?? 0) + 1;
            newGroups.ForEach(m =>
            {
                m.id = pkGroups++;
            });
            BulkInsert(context, newGroups);
            sw.Stop();
            Debug.WriteLine($"Inserted {newGroups.Count} groups in {sw.Elapsed}");
        }

        ///<inheritdoc/>
        public void AddNewManifests(IEnumerable<StationInventoryManifest> manifests, int inventoryFileId, int inventorySourceId)
        {
            _InReadUncommitedTransaction(
              context =>
              {
                  var mappedManifests = manifests
                        .Select(manifest => _MapToStationInventoryManifest(manifest, inventoryFileId, inventorySourceId))
                        .ToList();

                  //process manifests
                  _InsertManifests(context, mappedManifests);

                  //process manifest audiences
                  _InsertManifestAudiences(context, mappedManifests);

                  //process manifest dayparts
                  _InsertManifestDayparts(context, mappedManifests);

                  //process manifest rates
                  _InsertManifestRates(context, mappedManifests);

                  //process manifest weeks
                  _InsertManifestWeeks(context, mappedManifests);
              });
        }

        private void _InsertManifestWeeks(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();
            var pkManifestWeeks = context.station_inventory_manifest_weeks.Max(x => x.id) + 1;
            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_weeks.ToList().ForEach(mw =>
                {
                    mw.station_inventory_manifest_id = m.id;
                    mw.id = pkManifestWeeks++;

                });
            });
            var manifestWeeks = mappedManifests.SelectMany(x => x.station_inventory_manifest_weeks).ToList();
            BulkInsert(context, manifestWeeks);
            sw.Stop();
            Debug.WriteLine($"Inserted {manifestWeeks.Count} manifest weeks in {sw.Elapsed}");
        }

        private void _InsertManifestRates(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();
            var pkManifestRates = context.station_inventory_manifest_rates.Max(x => x.id) + 1;
            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_rates.ToList().ForEach(mr =>
                {
                    mr.station_inventory_manifest_id = m.id;
                    mr.id = pkManifestRates++;
                });
            });
            var manifestRates = mappedManifests.SelectMany(x => x.station_inventory_manifest_rates).ToList();
            BulkInsert(context, manifestRates);
            sw.Stop();
            Debug.WriteLine($"Inserted {manifestRates.Count} manifest rates in {sw.Elapsed}");
        }

        private void _InsertManifestDayparts(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();
            var pkManifestDayparts = context.station_inventory_manifest_dayparts.Max(x => x.id) + 1;
            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_dayparts.ToList().ForEach(md =>
                {
                    md.station_inventory_manifest_id = m.id;
                    md.id = pkManifestDayparts++;
                });
            });
            var manifestDayparts = mappedManifests.SelectMany(x => x.station_inventory_manifest_dayparts).ToList();
            BulkInsert(context, manifestDayparts);
            sw.Stop();
            Debug.WriteLine($"Inserted {manifestDayparts.Count} manifest dayparts in {sw.Elapsed}");
        }

        private void _InsertManifestAudiences(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();
            var pkManifestAudiences = context.station_inventory_manifest_audiences.Max(x => x.id) + 1;
            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_audiences.ToList().ForEach(ma =>
                {
                    ma.station_inventory_manifest_id = m.id;
                    ma.id = pkManifestAudiences++;
                });
            });
            var manifestAudiences = mappedManifests.SelectMany(x => x.station_inventory_manifest_audiences).ToList();
            BulkInsert(context, manifestAudiences);
            sw.Stop();
            Debug.WriteLine($"Inserted {manifestAudiences.Count} manifest audiences in {sw.Elapsed}");
        }

        private void _InsertManifests(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests, int? groupId = null)
        {
            var sw = Stopwatch.StartNew();
            var pkManifests = context.station_inventory_manifest.Max(x => x.id) + 1;
            mappedManifests.ForEach(m =>
            {
                m.id = pkManifests++;
                m.station_inventory_group_id = groupId;
            });
            BulkInsert(context, mappedManifests);
            sw.Stop();
            Debug.WriteLine($"Inserted {mappedManifests.Count} manifests in {sw.Elapsed}");
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
                                    .Select(manifest => _MapToStationInventoryManifest(manifest, inventoryFile.Id, inventoryFile.InventorySource.Id)).ToList()
            };
        }

        private station_inventory_manifest _MapToStationInventoryManifest(StationInventoryManifest manifest, int inventoryFileId, int inventorySourceId)
        {
            return new station_inventory_manifest()
            {
                station_id = manifest.Station?.Id,
                spot_length_id = manifest.SpotLengthId,
                spots_per_day = manifest.SpotsPerDay,
                spots_per_week = manifest.SpotsPerWeek,
                file_id = inventoryFileId,
                inventory_source_id = inventorySourceId,
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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
                        .Select(manifest => _MapToInventoryManifest(manifest, manifest.inventory_files.inventory_file_proprietary_header.SingleOrDefault()?.daypart_codes?.code))
                        .ToList();
                });
        }

        ///<inheritdoc/>
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
                                    Vpvh = audience.vpvh,
                                    SharePlaybackType = (ProposalPlaybackType?)audience.share_playback_type,
                                    HutPlaybackType = (ProposalPlaybackType?)audience.hut_playback_type
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
                                    Vpvh = audience.vpvh,
                                    SharePlaybackType = (ProposalPlaybackType?)audience.share_playback_type,
                                    HutPlaybackType = (ProposalPlaybackType?)audience.hut_playback_type
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public void RemoveManifest(int manifestId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var manifest = context.station_inventory_manifest.Single(m => m.id == manifestId);

                context.station_inventory_manifest.Remove(manifest);

                context.SaveChanges();
            });
        }

        ///<inheritdoc/>
        public bool HasSpotsAllocated(int manifestId)
        {
            return _InReadUncommitedTransaction(
                context => context.station_inventory_spots.Any(s => s.station_inventory_manifest_id == manifestId));
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public List<StationInventoryGroup> GetInventoryScxDataForBarter(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate, List<string> unitNames)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var groups = context.station_inventory_group
                        .SelectMany(x => x.station_inventory_manifest)
                        .SelectMany(x => x.station_inventory_manifest_weeks)
                        .Where(x => x.start_date <= endDate && x.end_date >= startDate) //filter by start/end date
                        .Where(x => unitNames.Contains(x.station_inventory_manifest.station_inventory_group.name))   //filter by units name
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_source_id == inventorySourceId)   //filter by source
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_file_proprietary_header.FirstOrDefault().daypart_code_id == daypartCodeId) //filter by daypart code
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_file_ratings_jobs.FirstOrDefault().status == (int)BackgroundJobProcessingStatus.Succeeded) // take only weeks with ratings calculated
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

                    foreach (var group in groups)
                    {
                        foreach (var manifest in group.Manifests)
                        {
                            manifest.ManifestWeeks = manifest.ManifestWeeks.Where(w => w.StartDate <= endDate && w.EndDate >= startDate).ToList();
                        }

                        group.Manifests = group.Manifests.Where(m => m.ManifestWeeks.Any()).ToList();
                    }

                    return groups;
                });
        }

        ///<inheritdoc/>
        public List<StationInventoryManifest> GetInventoryScxDataForOAndO(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests = context.station_inventory_manifest
                        .Where(x => x.inventory_source_id == inventorySourceId)
                        .Where(x => x.inventory_files.inventory_file_proprietary_header.FirstOrDefault().daypart_code_id == daypartCodeId)
                        .Where(x => x.inventory_files.inventory_file_ratings_jobs.FirstOrDefault().status == (int)BackgroundJobProcessingStatus.Succeeded) // take only manifests with ratings calculated
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
                        manifest.ManifestWeeks = manifest.ManifestWeeks.Where(w => w.StartDate <= endDate && w.EndDate >= startDate).ToList();
                    }

                    manifests = manifests.Where(m => m.ManifestWeeks.Any()).ToList();

                    return manifests;
                });
        }

        public Dictionary<int, ProprietaryInventoryHeader> GetInventoryFileHeader(IEnumerable<int> inventoryFileIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = (from header in context.inventory_file_proprietary_header
                                 where inventoryFileIds.Contains(header.inventory_file_id)
                                 select header).ToList();

                    return query.ToDictionary(x => x.inventory_file_id, x => new ProprietaryInventoryHeader
                    {
                        Audience = new BroadcastAudience { Id = x.audience_id.Value },
                        ContractedDaypartId = x.contracted_daypart_id,
                        Cpm = x.cpm,
                        DaypartCode = x.daypart_codes.code,
                        EffectiveDate = x.effective_date,
                        EndDate = x.end_date,
                        HutBookId = x.hut_projection_book_id,
                        PlaybackType = (ProposalPlaybackType)x.playback_type,
                        ShareBookId = x.share_projection_book_id
                    });
                });
        }

        ///<inheritdoc/>
        public void UpdateInventoryManifests(List<StationInventoryManifest> manifests)
        {
            _InReadUncommitedTransaction(
               context =>
               {
                   List<station_inventory_manifest_audiences> newManifestAudiences = new List<station_inventory_manifest_audiences>();
                   List<station_inventory_manifest_rates> newRates = new List<station_inventory_manifest_rates>();

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
                               newRates.Add(new station_inventory_manifest_rates
                               {
                                   spot_length_id = rate.SpotLengthId,
                                   spot_cost = rate.SpotCost,
                                   station_inventory_manifest_id = dbManifest.id
                               });                               
                           }
                           else
                           {
                               dbRate.spot_cost = rate.SpotCost;
                           }
                       }

                       //add manifest audiences
                       newManifestAudiences.AddRange(manifest.ManifestAudiences.Select(x => new station_inventory_manifest_audiences
                       {
                           audience_id = x.Audience.Id,
                           impressions = x.Impressions,
                           is_reference = x.IsReference,
                           rating = x.Rating,
                           cpm = x.CPM,
                           station_inventory_manifest_id = dbManifest.id,
                           share_playback_type = (int?)x.SharePlaybackType,
                           hut_playback_type = (int?)x.HutPlaybackType
                       }).ToList());
                   }
                   context.SaveChanges();

                   _InsertManifestAudiences(context, newManifestAudiences);
                   _InsertManifestRates(context, newRates);
               });
        }

        private void _InsertManifestRates(QueryHintBroadcastContext context, List<station_inventory_manifest_rates> newRates)
        {
            var sw = Stopwatch.StartNew();
            var pkManifestRates = context.station_inventory_manifest_rates.Max(x => x.id) + 1;
            newRates.ForEach(mr =>
                {
                    mr.id = pkManifestRates++;
                });
            BulkInsert(context, newRates);
            sw.Stop();
            Debug.WriteLine($"Inserted {newRates.Count} manifest rates in {sw.Elapsed} using the second insert method.");
        }

        private void _InsertManifestAudiences(QueryHintBroadcastContext context, List<station_inventory_manifest_audiences> newManifestAudiences)
        {
            var sw = Stopwatch.StartNew();
            var pkManifestAudiences = context.station_inventory_manifest_audiences.Max(x => x.id) + 1;

            newManifestAudiences.ForEach(ma =>
                {
                    ma.id = pkManifestAudiences++;
                });
            BulkInsert(context, newManifestAudiences);
            sw.Stop();
            Debug.WriteLine($"Inserted {newManifestAudiences.Count} manifest audiences in {sw.Elapsed} using the second insert method.");
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public DateRange GetInventorySourceDateRange(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var weeks = context.station_inventory_manifest
                        .Where(x => x.inventory_source_id == inventorySourceId && (FileStatusEnum)x.inventory_files.status == FileStatusEnum.Loaded)
                        .SelectMany(x => x.station_inventory_manifest_weeks);

                   return _GetMinMaxDateRange(weeks);
               });
        }

        ///<inheritdoc/>
        public DateRange GetAllInventoriesDateRange()
        {
            return _InReadUncommitedTransaction(context =>
                _GetMinMaxDateRange(context.station_inventory_manifest_weeks)
         );
        }

        private DateRange _GetMinMaxDateRange(IQueryable<station_inventory_manifest_weeks> weeks)
        {
            if (!weeks.Any())
                return new DateRange(null, null);

            var minDate = weeks.Min(x => x.start_date);
            var maxDate = weeks.Max(x => x.end_date);

            return new DateRange(minDate, maxDate);
        }

        ///<inheritdoc/>
        public List<StationInventoryManifestWeek> GetStationInventoryManifestWeeks(int inventorySourceId, int daypartCodeId)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var weeks = (from week in context.station_inventory_manifest_weeks
                                join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                                join inventoryFile in context.inventory_files on manifest.file_id equals inventoryFile.id
                                join inventoryFileHeader in context.inventory_file_proprietary_header on inventoryFile.id equals inventoryFileHeader.inventory_file_id
                                join ratingProcessingJob in context.inventory_file_ratings_jobs on inventoryFile.id equals ratingProcessingJob.inventory_file_id
                                where manifest.inventory_source_id == inventorySourceId &&
                                      inventoryFileHeader.daypart_code_id == daypartCodeId &&
                                      week.spots > 0 &&
                                      ratingProcessingJob.status == (int)BackgroundJobProcessingStatus.Succeeded
                                group week by week.id into weekGroup
                                select weekGroup.FirstOrDefault())
                                .ToList()
                                .Select(_MapToInventoryManifestWeek)
                                .ToList();

                   return weeks;
               });
        }

        ///<inheritdoc/>
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
                                      join ratingProcessingJob in context.inventory_file_ratings_jobs on inventoryFile.id equals ratingProcessingJob.inventory_file_id
                                      where manifest.inventory_source_id == inventorySourceId &&
                                            inventoryFileHeader.daypart_code_id == daypartCodeId &&
                                            week.start_date <= endDate && week.end_date >= startDate &&
                                            week.spots > 0 &&
                                            ratingProcessingJob.status == (int)BackgroundJobProcessingStatus.Succeeded
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

        ///<inheritdoc/>
        public List<StationInventoryManifest> GetStationInventoryManifestsByIds(IEnumerable<int> manifestIds)
        {
            const int maxChunkSize = 1000;
            var endIndex = manifestIds.Count() / maxChunkSize;
            var chunks = new List<IEnumerable<int>>();

            for (var i = 0; i <= endIndex; i++)
            {
                var manifestsNumberToSkip = maxChunkSize * i;
                var manifestsNumberToTake = Math.Min(maxChunkSize, manifestIds.Count() - manifestsNumberToSkip);
                var manifestsToTake = manifestIds.Skip(manifestsNumberToSkip).Take(manifestsNumberToTake);
                chunks.Add(manifestsToTake);
            }

            var result = chunks
                .AsParallel()
                .SelectMany(chunk =>
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
                                .Where(x => chunk.Contains(x.id))
                                .ToList()
                                .Select(x => _MapToInventoryManifest(x))
                                .ToList();
                        });
                })
                .ToList();

            return result;
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public List<InventoryUploadHistoryDto> GetInventoryUploadHistoryForInventorySource(int inventorySourceId, DateTime? startDate, DateTime? endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var files = context.inventory_files
                             .Include(x => x.inventory_file_proprietary_header)
                             .Include(x => x.inventory_file_proprietary_header.Select(h => h.daypart_codes))
                             .Include(x => x.inventory_file_proprietary_header.Select(h => h.share_media_months))
                             .Include(x => x.inventory_file_proprietary_header.Select(h => h.hut_media_months))
                             .Include(x => x.station_inventory_manifest.Select(h => h.station_inventory_manifest_weeks))
                             .Include(x => x.station_inventory_manifest.Select(h => h.station_inventory_manifest_dayparts.Select(d => d.daypart_codes)))
                             .Include(x => x.inventory_sources)
                             .Include(x => x.inventory_file_ratings_jobs)
                             .Where(x => x.inventory_source_id == inventorySourceId);

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        files = files.Where(f => f.effective_date <= endDate &&
                                                 f.end_date >= startDate);
                    }

                    var result = new List<InventoryUploadHistoryDto>();

                    foreach (var file in files)
                    {
                        var fileHistory = new InventoryUploadHistoryDto()
                        {
                            FileId = file.id,
                            UploadDateTime = file.created_date,
                            Username = file.created_by,
                            Filename = file.name,
                            Rows = file.rows_processed ?? 0,
                            EffectiveDate = file.effective_date,
                            EndDate = file.end_date,
                            FileLoadStatus = (FileStatusEnum)file.status
                        };

                        if (file.inventory_file_proprietary_header.Any()) //Proprietary Inventory file
                        {
                            var header = file.inventory_file_proprietary_header.Single();

                            if (file.inventory_sources.inventory_source_type == (int)InventorySourceTypeEnum.Diginet)
                            {
                                fileHistory.DaypartCodes = _GetDistinctDaypartCodesFromManifests(file.station_inventory_manifest);
                            }
                            else
                            {
                                fileHistory.DaypartCodes = new List<String>() { header.daypart_codes.code };
                            }

                            if (header.hut_media_months != null)
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
                        }

                        if (file.inventory_file_ratings_jobs.Any())
                        {
                            var fileJob = file.inventory_file_ratings_jobs.OrderBy(j => j.id).Last();
                            fileHistory.FileProcessingStatus = (BackgroundJobProcessingStatus)fileJob.status;
                        }

                        result.Add(fileHistory);
                    }

                    return result.OrderByDescending(r => r.UploadDateTime).ToList();
                });
        }

        public List<DateRange> GetInventoryUploadHistoryDatesForInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var files = context.inventory_files
                             .Include(x => x.inventory_file_proprietary_header)
                             .Where(x => x.inventory_source_id == inventorySourceId);

                    var dates = files.Select(f => new { f.effective_date, f.end_date }).ToList();

                    return dates.Select(d => new DateRange(d.effective_date, d.end_date)).ToList();
                });
        }

        private List<string> _GetDistinctDaypartCodesFromManifests(ICollection<station_inventory_manifest> manifests)
        {
            var result = new List<string>();

            if (manifests != null && manifests.Count > 0)
            {
                result = manifests.SelectMany(m => m.station_inventory_manifest_dayparts.Select(d => d.daypart_codes.code)).Distinct().ToList();
            }

            return result;

        }
    }
}
