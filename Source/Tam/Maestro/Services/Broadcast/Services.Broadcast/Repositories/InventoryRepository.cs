using Common.Services;
using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.ProprietaryInventory;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

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

        InventorySummaryTotals GetInventorySummaryDateRangeTotalsForSource(InventorySource inventorySource, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Removes station_inventory_manifest_weeks records based on their id
        /// </summary>
        /// <param name="weeks">List of StationInventoryManifestWeek to remove</param>
        void RemoveManifestWeeks(List<StationInventoryManifestWeek> weeks);

        void RemoveManifestWeeksByMarketAndDaypart(InventorySourceEnum inventorySource, List<int> mediaWeekIds, int marketCode, int daypartId, int stationId);
        
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
        /// Deletes the inventory manifest audiences for file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        void DeleteInventoryManifestAudiencesForFile(int fileId);

        /// <summary>
        /// Update the rates and adds the audiences for all the manifests in the list
        /// </summary>        
        /// <param name="manifests">List of manifests to process</param>
        void UpdateInventoryManifests(IEnumerable<StationInventoryManifest> manifests);

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
        /// <param name="programName">Program name to filter newPrograms</param>
        /// <param name="daypartId">Daypart id to filter dayparts</param>
        /// <returns>List of StationInventoryManifestWeek objects</returns>
        List<StationInventoryManifestWeek> GetStationInventoryManifestWeeksForOpenMarket(int stationId, string programName, int daypartId);

        List<StationInventoryGroup> GetInventoryGroups(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate);

        List<int> GetStationInventoryManifestWeeksForInventorySource(int inventorySourceId);

        List<DateRange> GetInventoryUploadHistoryDatesForInventorySource(int inventorySourceId);

        List<InventoryUploadHistory> GetInventoryUploadHistoryForInventorySource(int inventorySourceId, DateTime? startDate, DateTime? endDate);

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

        void CreateInventoryPrograms(List<StationInventoryManifestDaypartProgram> newPrograms, DateTime createdAt);

        List<StationInventoryManifestDaypartProgram> GetDaypartProgramsForInventoryDayparts(List<int> stationInventoryManifestDaypartIds);

        List<StationInventoryManifestDaypartProgram> GetDaypartProgramsForInventoryDayparts(List<int> stationInventoryManifestDaypartIds, ProgramSourceEnum source);

        List<StationInventoryManifestDaypart> GetManifestDayparts(List<int> stationInventoryManifestDaypartIds);

        void DeleteInventoryPrograms(List<int> manifestIds, DateTime startDate,
            DateTime endDate);

        void DeleteInventoryPrograms(List<int> manifestDaypartIds);

        void RemovePrimaryProgramFromManifestDayparts(List<int> manifestDaypartIds);

        /// <summary>
        /// Gets the inventory by file identifier for programs processing.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <remarks>
        /// The data is limited by what the process uses.
        /// </remarks>
        List<StationInventoryManifest> GetInventoryByFileIdForProgramsProcessing(int fileId);

        /// <summary>
        /// Gets the inventory by source for programs processing.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="mediaWeekIds">The media week ids.</param>
        /// <remarks>
        /// The data is limited by what the process uses.
        /// </remarks>
        List<StationInventoryManifest> GetInventoryBySourceForProgramsProcessing(int sourceId, List<int> mediaWeekIds);

        /// <summary>
        /// Gets the inventory by source within the date range that is unprocessed.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="mediaWeekIds">The media week ids.</param>
        List<StationInventoryManifest> GetInventoryBySourceWithUnprocessedPrograms(int sourceId, List<int> mediaWeekIds);

        void UpdatePrimaryProgramsForManifestDayparts(List<int> manifestDaypartIds);
        void UpdatePrimaryProgramsForManifestDayparts(IEnumerable<StationInventoryManifestDaypart> manifestDayparts);

        /// <summary>
        /// For tests
        /// </summary>
        /// <param name="groupIds"></param>
        void RemoveManifestGroups(List<int> groupIds);
        
        /// <summary>
        /// Get distinct list of Program Names
        /// </summary>
        /// <returns></returns>
        List<string> GetUnmappedPrograms();

        List<int> GetManuallyMappedPrograms(List<int> inventoryDaypartIds);

        List<StationInventoryManifestDaypart> GetOrphanedManifestDayparts(Action<string> logger);
        /// <summary>
        /// Get the Inventory Scx Open Market Data.
        /// </summary>
        /// <param name="inventorySourceId">The source identifier.</param>
        /// <param name="daypartIds">daypart code id.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="marketRanks">market code.</param>
        /// <param name="exportGenreIds">genre ids.</param>
        /// <param name="affiliates">affiliates.</param>
        /// <returns> Station Inventory Group Data</returns>
        List<StationInventoryGroup> GetInventoryScxOpenMarketData(int inventorySourceId, List<int> daypartIds, DateTime startDate, DateTime endDate, List<int> marketRanks, List<int> exportGenreIds, List<string> affiliates);
    }

    public class InventoryRepository : BroadcastRepositoryBase, IInventoryRepository
    {
        private static object _InventoryBulkInsertLock = new object();
        private static object _ProgramsBulkInsertLock = new object();
      
        public InventoryRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public void RemoveManifestGroups(List<int> groupIds)
        {
            _InReadUncommitedTransaction(context =>
            {
                var manifestIds = context.station_inventory_manifest
                    .Where(x => groupIds.Contains(x.station_inventory_group_id.Value))
                    .Select(x => x.id);

                context.station_inventory_manifest_audiences.RemoveRange(
                    context.station_inventory_manifest_audiences.Where(x => manifestIds.Contains(x.station_inventory_manifest_id)));

                context.station_inventory_manifest_dayparts.RemoveRange(
                    context.station_inventory_manifest_dayparts.Where(x => manifestIds.Contains(x.station_inventory_manifest_id)));

                context.station_inventory_manifest_rates.RemoveRange(
                    context.station_inventory_manifest_rates.Where(x => manifestIds.Contains(x.station_inventory_manifest_id)));

                context.station_inventory_manifest_weeks.RemoveRange(
                    context.station_inventory_manifest_weeks.Where(x => manifestIds.Contains(x.station_inventory_manifest_id)));

                context.station_inventory_manifest.RemoveRange(
                    context.station_inventory_manifest.Where(x => manifestIds.Contains(x.id)));

                context.station_inventory_group.RemoveRange(
                    context.station_inventory_group.Where(x => groupIds.Contains(x.id)));

                context.SaveChanges();
            });
        }

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
            lock (_InventoryBulkInsertLock)
            {
                _InReadUncommitedTransaction(
                   context =>
                   {
                       var newGroups = inventoryFile.InventoryGroups
                           .Where(g => g.Id == null)
                           .Select(inventoryGroup => _MapToStationInventoryGroup(inventoryGroup, inventoryFile)).ToList();

                       _InsertGroups(context, newGroups);

                       foreach (var group in newGroups)
                       {
                           var mappedManifests = group.station_inventory_manifest.ToList();

                           _InsertManifests(context, mappedManifests, group.id);

                           _InsertManifestAudiences(context, mappedManifests);

                           _InsertManifestDayparts(context, mappedManifests);

                           _InsertManifestDaypartPrograms(context, mappedManifests);

                           _UpdateManfiestDaypartsPrimaryProgram(context, mappedManifests);

                           _InsertManifestRates(context, mappedManifests);

                           _InsertManifestWeeks(context, mappedManifests);
                       }
                   });
            }
        }

        private void _UpdateManfiestDaypartsPrimaryProgram(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var manifestDaypartIds = mappedManifests.SelectMany(x => x.station_inventory_manifest_dayparts.Select(y => y.id)).ToList();
            var manifestDayparts = context.station_inventory_manifest_dayparts.Where(x => manifestDaypartIds.Contains(x.id)).ToList();
            var manifestDaypartPrograms = mappedManifests.SelectMany(x => x.station_inventory_manifest_dayparts.SelectMany(y => y.station_inventory_manifest_daypart_programs)).ToList();
            manifestDayparts.ForEach(p =>
            {
                var daypartProgram = manifestDaypartPrograms.First(x => x.station_inventory_manifest_daypart_id == p.id);
                p.primary_program_id = daypartProgram.id;
            });

            context.SaveChanges();
        }

        private void _InsertManifestDaypartPrograms(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();

            var manifestDaypartProgramId = context.station_inventory_manifest_daypart_programs.Select(x => x.id).Max() + 1;

            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_dayparts.ToList().ForEach(md =>
                {
                    md.station_inventory_manifest_daypart_programs.ToList().ForEach(dp =>
                    {
                        dp.id = manifestDaypartProgramId++;
                        dp.station_inventory_manifest_daypart_id = md.id;
                    });
                });
            });

            var manifestDaypartPrograms = mappedManifests.SelectMany(x => x.station_inventory_manifest_dayparts.SelectMany(y => y.station_inventory_manifest_daypart_programs)).ToList();

            BulkInsert(context, manifestDaypartPrograms);

            sw.Stop();

            Debug.WriteLine($"Inserted {manifestDaypartPrograms.Count} manifest dayparts in {sw.Elapsed}");
        }

        private void _InsertGroups(QueryHintBroadcastContext context, List<station_inventory_group> newGroups)
        {
            var sw = Stopwatch.StartNew();

            var pkGroups = (context.station_inventory_group.Select(x => (int?)x.id).Max() ?? 0) + 1;
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
            lock (_InventoryBulkInsertLock)
            {
                _InReadUncommitedTransaction(
                  context =>
                  {
                      var mappedManifests = manifests
                            .Select(manifest => _MapToStationInventoryManifest(manifest, inventoryFileId, inventorySourceId))
                            .ToList();

                      _InsertManifests(context, mappedManifests);

                      _InsertManifestAudiences(context, mappedManifests);

                      _InsertManifestDayparts(context, mappedManifests);

                      _InsertManifestRates(context, mappedManifests);

                      _InsertManifestWeeks(context, mappedManifests);
                  });
            }
        }

        private void _InsertManifestWeeks(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();

            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_weeks.ToList().ForEach(mw =>
                {
                    mw.station_inventory_manifest_id = m.id;
                });
            });

            var manifestWeeks = mappedManifests.SelectMany(x => x.station_inventory_manifest_weeks).ToList();
            var propertiesToIgnore = new List<string> { "sys_start_date", "sys_end_date", "id" };

            BulkInsert(context, manifestWeeks, propertiesToIgnore);

            sw.Stop();

            Debug.WriteLine($"Inserted {manifestWeeks.Count} manifest weeks in {sw.Elapsed}");
        }

        private void _InsertManifestRates(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();

            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_rates.ToList().ForEach(mr =>
                {
                    mr.station_inventory_manifest_id = m.id;
                });
            });

            var manifestRates = mappedManifests.SelectMany(x => x.station_inventory_manifest_rates).ToList();

            BulkInsert(context, manifestRates, propertiesToIgnore: new List<string> { "id" });

            sw.Stop();

            Debug.WriteLine($"Inserted {manifestRates.Count} manifest rates in {sw.Elapsed}");
        }

        private void _InsertManifestDayparts(QueryHintBroadcastContext context, List<station_inventory_manifest> mappedManifests)
        {
            var sw = Stopwatch.StartNew();

            var manifestDaypartsId = context.station_inventory_manifest_dayparts.Max(x => x.id) + 1;

            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_dayparts.ToList().ForEach(md =>
                {
                    md.id = manifestDaypartsId++;
                    md.station_inventory_manifest_id = m.id;
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

            mappedManifests.ForEach(m =>
            {
                m.station_inventory_manifest_audiences.ToList().ForEach(ma =>
                {
                    ma.station_inventory_manifest_id = m.id;
                });
            });

            var manifestAudiences = mappedManifests.SelectMany(x => x.station_inventory_manifest_audiences).ToList();

            BulkInsert(context, manifestAudiences, propertiesToIgnore: new List<string> { "id" });

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
                                            standard_daypart_id = md.StandardDaypart?.Id,
                                            station_inventory_manifest_daypart_programs = md.Programs.Select(x => new station_inventory_manifest_daypart_programs
                                            {
                                                name = x.ProgramName,
                                                end_date = x.EndDate,
                                                start_date = x.StartDate,
                                                maestro_genre_id = x.MaestroGenreId,
                                                source_genre_id = x.SourceGenreId,
                                                program_source_id = x.ProgramSourceId,
                                                start_time = x.StartTime,
                                                end_time = x.EndTime,
                                                show_type = x.ShowType,
                                                created_date = x.CreatedDate
                                            }).ToList()
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
                                .Include(x => x.station_inventory_manifest_genres)
                                .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts.daypart))
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
                                .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts.daypart))
                                .Include(s => s.station)
                                .Include(x => x.inventory_files)
                                .Include(x => x.inventory_files.inventory_file_proprietary_header)
                         where m.file_id == fileId
                         select m).ToList();

                    return manifests
                        .Select(manifest => _MapToInventoryManifest(manifest, 
                                manifest.inventory_files.inventory_file_proprietary_header.SingleOrDefault()?.standard_dayparts?.code))
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
                        .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts))
                        .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts.daypart))
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
            var headerDaypartCode = stationInventoryGroup.station_inventory_manifest
                    .FirstOrDefault().inventory_files.inventory_file_proprietary_header.FirstOrDefault().standard_dayparts.code;
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
                    .ToList(),
            };

            return sig;
        }

        private List<StationInventoryManifestDaypart> _MapToInventoryDayparts(station_inventory_manifest manifest)
        {
            var dayparts = manifest.station_inventory_manifest_dayparts.Select(md => new StationInventoryManifestDaypart()
            {
                Id = md.id,
                Daypart = DaypartCache.Instance.GetDisplayDaypart(md.daypart_id),
                ProgramName = md.program_name,
                StandardDaypart = md.standard_dayparts == null
                    ? null
                    : new StandardDaypartDto
                    {
                        Id = md.standard_dayparts.id,
                        Code = md.standard_dayparts.code,
                        FullName = md.standard_dayparts.name
                    },
                PrimaryProgramId = md.primary_program_id,
                Programs = md.station_inventory_manifest_daypart_programs.Select(_MapToInventoryDaypartProgram).ToList(),
            }).ToList();

            return dayparts;
        }

        private StationInventoryManifestDaypartProgram _MapToInventoryDaypartProgram(station_inventory_manifest_daypart_programs program)
        {
            var result = new StationInventoryManifestDaypartProgram
            {
                Id = program.id,
                StationInventoryManifestDaypartId = program.station_inventory_manifest_daypart_id,
                ProgramName = program.name,
                ShowType = program.show_type,
                SourceGenreId = program.source_genre_id,
                ProgramSourceId = program.program_source_id,
                MaestroGenreId = program.maestro_genre_id,
                StartDate = program.start_date,
                EndDate = program.end_date,
                StartTime = program.start_time,
                EndTime = program.end_time,
                CreatedDate = program.created_date
            };

            return result;
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
                ManifestDayparts = _MapToInventoryDayparts(manifest),
                ManifestAudiences = manifest.station_inventory_manifest_audiences.Where(ma => !ma.is_reference).Select(
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

        private StationInventoryManifest _MapToInventoryManifestForProgramsProcessing(station_inventory_manifest manifest, List<int> mediaWeekIds)
        {
            var result = new StationInventoryManifest()
            {
                Id = manifest.id,
                InventorySourceId = manifest.inventory_source_id,
                DaypartCode = null,
                SpotLengthId = manifest.spot_length_id,
                SpotsPerWeek = manifest.spots_per_week,
                SpotsPerDay = manifest.spots_per_day,
                Comment = manifest.comment,
                InventoryFileId = manifest.file_id,
                ManifestDayparts = _MapToInventoryDayparts(manifest),
                ManifestWeeks = manifest.station_inventory_manifest_weeks
                                .Where(w => mediaWeekIds.Contains(w.media_week_id))
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

        public void RemoveManifestWeeksByMarketAndDaypart(InventorySourceEnum inventorySource, List<int> mediaWeekIds, int marketCode, int daypartId, int stationId)
        {
            _InReadUncommitedTransaction(
                c =>
                {
                    var weekIdList = String.Join(",", mediaWeekIds);
                    var sql = $@"delete w from station_inventory_manifest_weeks w
                        inner join	station_inventory_manifest m on w.station_inventory_manifest_id = m.id
                        inner join station_inventory_manifest_dayparts d on d.station_inventory_manifest_id = m.id
                        inner join stations s on s.id = m.station_id
                        where s.market_code = @marketCode
                        and m.inventory_source_id = @inventorySource
                        and d.daypart_id = @daypart
                        and s.id = @station
                        and w.media_week_id in ({weekIdList})";
                    var marketParam = new SqlParameter("@marketCode", marketCode);
                    var inventorySourceParam = new SqlParameter("@inventorySource", (int)inventorySource);
                    var daypartParam = new SqlParameter("@daypart", daypartId);
                    var stationParam = new SqlParameter("@station", stationId);
                    c.Database.ExecuteSqlCommand(sql, marketParam, inventorySourceParam, daypartParam, stationParam);
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
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_file_proprietary_header.FirstOrDefault().standard_daypart_id == daypartCodeId) //filter by daypart code
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
                        .Where(x => x.inventory_files.inventory_file_proprietary_header.FirstOrDefault().standard_daypart_id == daypartCodeId)
                        .Where(x => x.inventory_files.inventory_file_ratings_jobs.FirstOrDefault().status == (int)BackgroundJobProcessingStatus.Succeeded) // take only manifests with ratings calculated
                        .Include(x => x.station_inventory_manifest_audiences)
                        .Include(x => x.station_inventory_manifest_weeks)
                        .Include(x => x.station_inventory_manifest_rates)
                        .Include(x => x.station_inventory_manifest_dayparts)
                        .Include(x => x.inventory_sources)
                        .Select(x => x)
                        .ToList()
                        .Select(x => _MapToInventoryManifest(x, x.inventory_files.inventory_file_proprietary_header.SingleOrDefault()?.standard_dayparts.code))
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
                        DaypartCode = x.standard_dayparts.code,
                        EffectiveDate = x.effective_date,
                        EndDate = x.end_date,
                        HutBookId = x.hut_projection_book_id,
                        PlaybackType = (ProposalPlaybackType)x.playback_type,
                        ShareBookId = x.share_projection_book_id
                    });
                });
        }

        public void DeleteInventoryManifestAudiencesForFile(int fileId)
        {
            const string sql = "DELETE FROM station_inventory_manifest_audiences "
                    + "WHERE station_inventory_manifest_id in "
                    + "(select id from station_inventory_manifest where file_id = @FileId) "
                    + "and is_reference = 0";

            _InReadUncommitedTransaction(context =>
            {
                context.Database.ExecuteSqlCommand(sql, new SqlParameter("@FileId", fileId));
            }
            );
        }

        ///<inheritdoc/>
        public void UpdateInventoryManifests(IEnumerable<StationInventoryManifest> manifests)
        {
            lock (_InventoryBulkInsertLock)
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
        }

        private void _InsertManifestRates(QueryHintBroadcastContext context, List<station_inventory_manifest_rates> newRates)
        {
            var sw = Stopwatch.StartNew();

            BulkInsert(context, newRates, propertiesToIgnore: new List<string> { "id" });

            sw.Stop();
            Debug.WriteLine($"Inserted {newRates.Count} manifest rates in {sw.Elapsed} using the second insert method.");
        }

        private void _InsertManifestAudiences(QueryHintBroadcastContext context, List<station_inventory_manifest_audiences> newManifestAudiences)
        {
            var sw = Stopwatch.StartNew();

            BulkInsert(context, newManifestAudiences, propertiesToIgnore: new List<string> { "id" });

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
                                      inventoryFileHeader.standard_daypart_id == daypartCodeId &&
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
        public List<StationInventoryGroup> GetInventoryGroups(int inventorySourceId, int standardDaypartId, DateTime startDate, DateTime endDate)
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
                                            inventoryFileHeader.standard_daypart_id == standardDaypartId &&
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
            var chunks = manifestIds.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);

            var result = chunks
                .AsParallel()
                .SelectMany(chunk =>
                {
                    return _InReadUncommitedTransaction(
                        context =>
                        {
                            return context.station_inventory_manifest
                                .Include(x => x.station_inventory_manifest_audiences)
                                .Include(x => x.station_inventory_manifest_audiences.Select(a => a.audience))
                                .Include(x => x.station_inventory_manifest_weeks)
                                .Include(x => x.station_inventory_manifest_weeks.Select(w => w.media_weeks))
                                .Include(x => x.station_inventory_manifest_rates)
                                .Include(x => x.station_inventory_manifest_dayparts)
                                .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts))
                                .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts.daypart))
                                .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.station_inventory_manifest_daypart_programs))
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
        public List<int> GetStationInventoryManifestWeeksForInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.station_inventory_manifest_weeks
                             .Where(x => x.station_inventory_manifest.inventory_source_id == inventorySourceId)
                             .Select(x => x.media_week_id)
                             .Distinct()
                             .ToList();
                });
        }

        ///<inheritdoc/>
        public List<InventoryUploadHistory> GetInventoryUploadHistoryForInventorySource(int inventorySourceId, DateTime? startDate, DateTime? endDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var files = context.inventory_files
                             .Include(x => x.inventory_file_proprietary_header)
                             .Include(x => x.inventory_file_proprietary_header.Select(h => h.standard_dayparts.daypart))
                                 .Include(x => x.inventory_file_proprietary_header.Select(h => h.media_months))
                                 .Include(x => x.inventory_file_proprietary_header.Select(h => h.media_months1));
                    //skip daypart codes for open market
                    if (inventorySourceId != (int)InventorySourceEnum.OpenMarket)
                    {
                        files = files.Include(x => x.station_inventory_manifest.Select(h => h.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts.daypart)));
                    }

                    files = files
                             .Include(x => x.inventory_sources)
                             .Include(x => x.inventory_file_ratings_jobs)
                             .Where(x => x.inventory_source_id == inventorySourceId);

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        files = files.Where(f => f.effective_date <= endDate &&
                                                 f.end_date >= startDate);
                    }

                    var result = new List<InventoryUploadHistory>();

                    foreach (var file in files)
                    {
                        var fileHistory = new InventoryUploadHistory
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
                                fileHistory.DaypartCodes = new List<String>() { header.standard_dayparts.code };
                            }

                            if (header.media_months1 != null)
                            {
                                fileHistory.HutBook = new MediaMonthDto
                                {
                                    Id = header.media_months1.id,
                                    Year = header.media_months1.year,
                                    Month = header.media_months1.month
                                };
                            }

                            if (header.media_months != null)
                            {
                                fileHistory.ShareBook = new MediaMonthDto
                                {
                                    Id = header.media_months.id,
                                    Year = header.media_months.year,
                                    Month = header.media_months.month
                                };
                            }
                        }

                        if (file.inventory_file_ratings_jobs.Any())
                        {
                            var ratingProcessingJob = file.inventory_file_ratings_jobs.OrderBy(j => j.id).Last();
                            fileHistory.RatingProcessingJobStatus = (BackgroundJobProcessingStatus)ratingProcessingJob.status;
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
                result = manifests.SelectMany(m => m.station_inventory_manifest_dayparts.Select(d => d.standard_dayparts.code)).Distinct().ToList();
            }

            return result;

        }

        public List<StationInventoryManifestDaypartProgram> GetDaypartProgramsForInventoryDayparts(List<int> stationInventoryManifestDaypartIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = context.station_inventory_manifest_daypart_programs.Where(p =>
                        stationInventoryManifestDaypartIds.Contains(p.station_inventory_manifest_daypart_id));

                    var result = query.Select(_MapToInventoryManifestDaypartProgram).ToList();

                    return result;
                });
        }

        public List<StationInventoryManifestDaypartProgram> GetDaypartProgramsForInventoryDayparts(List<int> stationInventoryManifestDaypartIds, ProgramSourceEnum source)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var query = context.station_inventory_manifest_daypart_programs.Where(p =>
                        stationInventoryManifestDaypartIds.Contains(p.station_inventory_manifest_daypart_id) && p.program_source_id == (int) source);

                    var result = query.Select(_MapToInventoryManifestDaypartProgram).ToList();

                    return result;
                });
        }

        public List<StationInventoryManifestDaypart> GetManifestDayparts(List<int> stationInventoryManifestDaypartIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var entities = context.station_inventory_manifest_dayparts
                        .Where(d => stationInventoryManifestDaypartIds.Contains(d.id))
                        .ToList();
                    var result = entities.Select(_MapToManifestDaypart).ToList();

                    return result;
                });
        }

        private StationInventoryManifestDaypartProgram _MapToInventoryManifestDaypartProgram(
            station_inventory_manifest_daypart_programs item)
        {
            var dto = new StationInventoryManifestDaypartProgram
            {
                Id = item.id,
                StationInventoryManifestDaypartId = item.station_inventory_manifest_daypart_id,
                ProgramName = item.name,
                ShowType = item.show_type,
                SourceGenreId = item.source_genre_id,
                ProgramSourceId = item.program_source_id,
                MaestroGenreId = item.maestro_genre_id,
                StartDate = item.start_date,
                EndDate = item.end_date,
                StartTime = item.start_time,
                EndTime = item.end_time,
                CreatedDate = item.created_date
            };

            return dto;
        }

        public void DeleteInventoryPrograms(List<int> manifestIds, DateTime startDate, DateTime endDate)
        {
            if (manifestIds.IsEmpty())
                return;

            var manifestIdsCsv = string.Join(",", manifestIds);
            // gather first
            var sql = "SELECT p.id AS ProgramId, station_inventory_manifest_daypart_id AS DaypartId INTO #ProgramsIdsToDelete"
                      + " FROM station_inventory_manifest_daypart_programs p"
                      + " INNER JOIN station_inventory_manifest_dayparts d ON p.station_inventory_manifest_daypart_id = d.id"
                      + $" WHERE d.station_inventory_manifest_id in ({manifestIdsCsv})"
                      + $" AND p.start_date <= '{endDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)} 23:59:59'"
                      + $" AND p.end_date >= '{startDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)} 00:00:00';"
                      + "\r\n"
                      // clear the primary program id
                      + "UPDATE d SET primary_program_id = NULL FROM station_inventory_manifest_dayparts d JOIN #ProgramsIdsToDelete pd ON d.id = pd.DaypartId AND d.primary_program_id = pd.ProgramId;"
                      + "\r\n"
                      // now can delete the programs
                      + "DELETE FROM station_inventory_manifest_daypart_programs WHERE id IN (SELECT ProgramId FROM #ProgramsIdsToDelete);";

            _InReadUncommitedTransaction(
                context =>
                {
                    context.Database.ExecuteSqlCommand(sql);
                });
        }

        public void RemovePrimaryProgramFromManifestDayparts(List<int> manifestDaypartIds)
        {
            if (manifestDaypartIds.IsEmpty())
                return;

            var manifestDaypartIdsCsv = string.Join(",", manifestDaypartIds);
            // clear the primary program id
            var sql = "UPDATE d SET primary_program_id = NULL "
                      + "FROM station_inventory_manifest_dayparts d "
                      + $"WHERE d.id in ({manifestDaypartIdsCsv}); ";

            _InReadUncommitedTransaction(
                context =>
                {
                    context.Database.ExecuteSqlCommand(sql);
                });
        }

        public void DeleteInventoryPrograms(List<int> manifestDaypartIds)
        {
            if (manifestDaypartIds.IsEmpty())
                return;

            var manifestDaypartIdsCsv = string.Join(",", manifestDaypartIds);
            // now can delete the programs
            var sql = 
                      "DELETE FROM station_inventory_manifest_daypart_programs "
                      + "WHERE program_source_id = 1 "
                      + $"AND station_inventory_manifest_daypart_id IN({manifestDaypartIdsCsv}); ";

            _InReadUncommitedTransaction(
                context =>
                {
                    context.Database.ExecuteSqlCommand(sql);
                });
        }

        public void CreateInventoryPrograms(List<StationInventoryManifestDaypartProgram> newPrograms, DateTime createdAt)
        {
            lock (_ProgramsBulkInsertLock)
            {
                _InReadUncommitedTransaction(
                    context =>
                    {
                        var entities = newPrograms.Select(p => new station_inventory_manifest_daypart_programs
                        {
                            station_inventory_manifest_daypart_id = p.StationInventoryManifestDaypartId,
                            name = p.ProgramName,
                            show_type = p.ShowType,
                            source_genre_id = p.SourceGenreId,
                            program_source_id = p.ProgramSourceId,
                            maestro_genre_id = p.MaestroGenreId,
                            start_date = p.StartDate,
                            end_date = p.EndDate,
                            start_time = p.StartTime,
                            end_time = p.EndTime,
                            created_date = createdAt
                        }).ToList();

                        BulkInsert(context, entities, propertiesToIgnore: new List<string> { "id" });
                    });
            }
        }

        public void UpdatePrimaryProgramsForManifestDayparts(List<int> manifestDaypartIds)
        {
            if (!manifestDaypartIds.Any())
            {
                return;
            }

            var manifestDaypartIdsCsv = string.Join(",", manifestDaypartIds);
            // clear the primary program id
            var sql = "UPDATE d SET primary_program_id = p.id "
                      + "FROM station_inventory_manifest_dayparts d "
                      + "INNER JOIN station_inventory_manifest_daypart_programs p "
                      + "ON d.id = p.station_inventory_manifest_daypart_id "
                      + "WHERE p.program_source_id = 1" //mapped
                      + "AND (d.primary_program_id IS NULL OR d.primary_program_id <> p.id) "
                      + $"AND d.id in ({manifestDaypartIdsCsv}); ";

            _InReadUncommitedTransaction(
                context =>
                {
                    context.Database.ExecuteSqlCommand(sql);
                });

        }

        public void UpdatePrimaryProgramsForManifestDayparts(IEnumerable<StationInventoryManifestDaypart> manifestDayparts)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var ids = manifestDayparts.Select(x => x.Id);
                    var itemsToUpdate = context.station_inventory_manifest_dayparts.Where(x => ids.Contains(x.id));

                    foreach (var itemToUpdate in itemsToUpdate)
                    {
                        var manifestDaypart = manifestDayparts.Single(x => x.Id == itemToUpdate.id);
                        itemToUpdate.primary_program_id = manifestDaypart.PrimaryProgramId;
                    }

                    context.SaveChanges();
                });
        }

        public InventorySummaryTotals GetInventorySummaryDateRangeTotalsForSource(InventorySource inventorySource, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                   var manifests = context.station_inventory_manifest
                       .Where(m => m.station_inventory_manifest_weeks.Any(w => w.start_date <= endDate && w.end_date >= startDate))
                       .Where(m => m.inventory_source_id == inventorySource.Id);

                   var impressions = context.station_inventory_manifest_weeks
                        .Where(w => w.station_inventory_manifest.inventory_source_id == 1)
                        .Where(w => w.start_date <= endDate && w.end_date >= startDate)
                        .Select(w => w.station_inventory_manifest.station_inventory_manifest_audiences
                            .Where(ma => ma.is_reference == true && ma.audience_id == 31 && ma.impressions.HasValue).Any() ?
                              w.station_inventory_manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == true && ma.audience_id == 31 && ma.impressions.HasValue).FirstOrDefault().impressions :
                              w.station_inventory_manifest.station_inventory_manifest_audiences.Where(ma => ma.is_reference == false && ma.audience_id == 31 && ma.impressions.HasValue).FirstOrDefault().impressions
                            ).Sum();

                   var result = new InventorySummaryTotals
                   {
                       TotalMarkets = manifests.GroupBy(m => m.station.market_code).Count(),
                       TotalStations = manifests.GroupBy(m => m.station_id).Count(),
                       TotalPrograms = manifests.SelectMany(m => m.station_inventory_manifest_dayparts)
                                        .GroupBy(d => d.program_name).Count(),
                       TotalHouseholdImpressions = impressions ?? 0
                   };

                   return result;
               });
        }

        ///<inheritdoc/>
        public List<StationInventoryManifest> GetInventoryByFileIdForProgramsProcessing(int fileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests =
                        (from m in
                                context.station_inventory_manifest
                                    .Include(x => x.station_inventory_manifest_weeks)
                                    .Include(x => x.station_inventory_manifest_dayparts)
                                    .Include(s => s.station)
                            where m.file_id == fileId &&
                                  m.station != null &&
                                  m.station.affiliation != null &&
                                  m.station_inventory_manifest_weeks.Any() &&
                                  m.station_inventory_manifest_dayparts.Any()
                         select m).ToList();

                    return manifests
                        .Select(manifest => _MapToInventoryManifest(manifest, manifest.inventory_files.inventory_file_proprietary_header.SingleOrDefault()?.standard_dayparts?.code))
                        .ToList();
                });
        }

        ///<inheritdoc/>
        public List<StationInventoryManifest> GetInventoryBySourceForProgramsProcessing(int sourceId, List<int> mediaWeekIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests =
                        (from m in
                                context.station_inventory_manifest
                                    .Include(x => x.station_inventory_manifest_weeks)
                                    .Include(x => x.station_inventory_manifest_dayparts)
                                    .Include(s => s.station)
                         where m.inventory_source_id == sourceId &&
                               m.station != null &&
                               m.station.affiliation != null &&
                               m.station_inventory_manifest_weeks.Any(w => mediaWeekIds.Contains(w.media_week_id)) &&
                               m.station_inventory_manifest_dayparts.Any()
                         select m).ToList();

                    var manifestDtos = manifests.Select(x => _MapToInventoryManifestForProgramsProcessing(x, mediaWeekIds)).ToList();

                    return manifestDtos;
                });
        }

        ///<inheritdoc/>
        public List<StationInventoryManifest> GetInventoryBySourceWithUnprocessedPrograms(int sourceId, List<int> mediaWeekIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manifests =
                        (from m in
                                context.station_inventory_manifest
                                    .Include(x => x.station_inventory_manifest_weeks)
                                    .Include(x => x.station_inventory_manifest_dayparts)
                                    .Include(s => s.station)
                         where m.inventory_source_id == sourceId &&
                               m.station != null &&
                               m.station.affiliation != null &&
                               m.station_inventory_manifest_weeks.Any(w => mediaWeekIds.Contains(w.media_week_id)) &&
                               m.station_inventory_manifest_dayparts.Any() &&
                               m.station_inventory_manifest_dayparts.Any(d => d.station_inventory_manifest_daypart_programs.Any()) == false
                         select m).ToList();

                    var manifestDtos = manifests.Select(x => _MapToInventoryManifestForProgramsProcessing(x, mediaWeekIds)).ToList();
                    return manifestDtos;
                });
        }

        private StationInventoryManifestDaypart _MapToManifestDaypart(station_inventory_manifest_dayparts entity)
        {
            var dto = new StationInventoryManifestDaypart()
            {
                Id = entity.id,
                Daypart = new DisplayDaypart
                {
                    Id = entity.daypart_id,
                    StartTime = entity.daypart.timespan.start_time,
                    EndTime = entity.daypart.timespan.end_time
                },
                ProgramName = entity.program_name,
                PrimaryProgramId = entity.primary_program_id,
                Programs = entity.station_inventory_manifest_daypart_programs.Select(_MapToInventoryManifestDaypartProgram).ToList()
            };
            return dto;
        }

        public List<string> GetUnmappedPrograms()
        {
	        return _InReadUncommitedTransaction(
		        context =>
		        {
                    var unmappedPrograms = context.station_inventory_manifest_dayparts
				        .Include(x=>x.station_inventory_manifest.station_inventory_manifest_weeks)
				        .Where(x => x.station_inventory_manifest.station_inventory_manifest_weeks.Any() &&
					        !x.station_inventory_manifest_daypart_programs.Any(s =>
						        s.program_source_id.Equals((int)ProgramSourceEnum.Maestro)))
				        .OrderBy(x => x.program_name)
                        .Select(x =>
					       x.program_name
					        )
				        .Distinct()
				        .ToList();

			        return unmappedPrograms;
		        });
        }

        public List<int> GetManuallyMappedPrograms(List<int> inventoryDaypartIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var manuallyMapped = context.station_inventory_manifest_dayparts
                        .Where(x =>
                            x.station_inventory_manifest_daypart_programs.Any(s =>
                                s.program_source_id.Equals((int)ProgramSourceEnum.Maestro) &&
                                inventoryDaypartIds.Contains(x.id)))
                        .Select(x => x.id)
                        .ToList();

                    return manuallyMapped;
                });
        }

        public List<StationInventoryManifestDaypart> GetOrphanedManifestDayparts(Action<string> logger)
        {
	        return  _InReadUncommitedTransaction(
		        context =>
		        {
                    var sqlQuery = "SELECT x.id FROM (" +
                    // This inventory doesn't have an enriched program
                    "SELECT DISTINCT d.id FROM station_inventory_manifest_dayparts d " +
                    "JOIN station_inventory_manifest_weeks w ON d.station_inventory_manifest_id = w.station_inventory_manifest_id " +
                    "WHERE d.primary_program_id IS NULL" +
                    " UNION " +
                    // This inventory has an enriched program that no longer has a program_name_mapping 
                    "SELECT DISTINCT d.id FROM station_inventory_manifest_dayparts d " +
                    "JOIN station_inventory_manifest_weeks w ON d.station_inventory_manifest_id = w.station_inventory_manifest_id " +
                    "JOIN station_inventory_manifest_daypart_programs p ON d.primary_program_id = p.id " +
                    "LEFT OUTER JOIN program_name_mappings m ON m.inventory_program_name = d.[program_name] " +
                    "WHERE m.id IS NULL" +
                    ") x ORDER BY id;";

                    var manifestDaypartIds = context.Database.SqlQuery<int>(sqlQuery).ToList();
                    var chunks = manifestDaypartIds.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);
                    var result = new List<StationInventoryManifestDaypart>();

                    logger($"Fetching manifest dayparts data. Total manifest dayparts: {manifestDaypartIds.Count}. Total chunks: {chunks.Count()}");

                    for (var i = 0; i < chunks.Count; i++)
                    {
                        var chunk = chunks[i];

                        logger($"Fetching manifest dayparts data for chunk #{i + 1} / {chunks.Count}, items: {chunk.Count}");

                        var entities = context.station_inventory_manifest_dayparts
                            .Include(s => s.daypart)
                            .Include(s => s.daypart.timespan)
                            .Include(s => s.station_inventory_manifest_daypart_programs)
                            .Where(s => chunk.Contains(s.id))
                            .Select(_MapToManifestDaypart)
                            .ToList();

                        result.AddRange(entities);
                    }

                    return result;
                });
        }

        ///<inheritdoc/>
        public List<StationInventoryGroup> GetInventoryScxOpenMarketData(int inventorySourceId, List<int> daypartIds, DateTime startDate, DateTime endDate, List<int> marketRanks,List<int> exportGenreIds, List<string> affiliates)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var groups = context.station_inventory_group
                        .SelectMany(x => x.station_inventory_manifest)
                        .SelectMany(x => x.station_inventory_manifest_weeks)
                        .Where(x => x.start_date <= endDate && x.end_date >= startDate) //filter by start/end date
                        .SelectMany(x => x.station_inventory_manifest.station_inventory_manifest_dayparts)
                        .Where(x => exportGenreIds.Contains(x.station_inventory_manifest_daypart_genres.FirstOrDefault().genre_id))//filter by Genre
                        .Where(x => affiliates.Contains(x.station_inventory_manifest.station.affiliation) || (affiliates.Contains("IND") && x.station_inventory_manifest.station.is_true_ind))//Filter by affiliation or filter by IND and ind flag true
                        .Where(x => marketRanks.Contains(x.station_inventory_manifest.station.market.market_coverages.FirstOrDefault().rank))//Filter by market rank
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_source_id == inventorySourceId)   //filter by source
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_file_proprietary_header.FirstOrDefault().standard_daypart_id.HasValue && daypartIds.Contains((Int32)x.station_inventory_manifest.inventory_files.inventory_file_proprietary_header.FirstOrDefault().standard_daypart_id)) //filter by daypart code
                        .Where(x => x.station_inventory_manifest.inventory_files.inventory_file_ratings_jobs.FirstOrDefault().status == (int)BackgroundJobProcessingStatus.Succeeded) // take only weeks with ratings calculated
                        .GroupBy(x => x.station_inventory_manifest.station_inventory_group_id)
                        .Select(x => x.FirstOrDefault().station_inventory_manifest.station_inventory_group)
                        .Include(x => x.station_inventory_manifest)
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_audiences))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_weeks.Select(w => w.media_weeks)))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_rates))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station_inventory_manifest_dayparts.Select(y=>y.station_inventory_manifest_daypart_genres)))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station))
                        .Include(x => x.station_inventory_manifest.Select(m => m.station.market.market_coverages))
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

    }
}
