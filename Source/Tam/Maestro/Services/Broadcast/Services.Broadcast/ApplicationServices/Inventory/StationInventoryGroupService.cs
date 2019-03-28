using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using Common.Services.Repositories;
using Tam.Maestro.Common;
using Common.Services;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.Entities.StationInventory;

namespace Services.Broadcast.ApplicationServices
{

    public interface IStationInventoryGroupService : IApplicationService
    {
        void AddNewStationInventoryGroups(InventoryFileBase inventoryFile, DateTime newEffectiveDate);
        void AddNewStationInventory(InventoryFileBase inventoryFile, DateTime newEffectiveDate, DateTime newEndDate, int contractedDaypartId);
        List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId);
    }

    public class StationInventoryGroupService : IStationInventoryGroupService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _audiencesCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public StationInventoryGroupService(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, 
            IBroadcastAudiencesCache audiencesCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _inventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _daypartCache = daypartCache;
            _audiencesCache = audiencesCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public string GenerateGroupName(string daypartCode, int slotNumber)
        {
           return null;    
        }

        public void AddNewStationInventoryGroups(InventoryFileBase inventoryFile, DateTime newEffectiveDate)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            _ExpireExistingInventoryGroups(inventoryFile.InventoryGroups, inventoryFile.InventorySource, newEffectiveDate);
            _inventoryRepository.AddNewInventory(inventoryFile);
        }

        private void _ExpireExistingInventoryGroups(IEnumerable<StationInventoryGroup> groups, InventorySource source, DateTime newEffectiveDate)
        {
            var expireDate = newEffectiveDate.AddDays(-1);
            var groupNames = groups.Select(g => g.Name).Distinct().ToList();
            var existingInventory = _inventoryRepository.GetActiveInventoryBySourceAndName(source, groupNames, newEffectiveDate);

            if (!existingInventory.Any())
                return;

            _inventoryRepository.ExpireInventoryGroupsAndManifests(existingInventory, expireDate, newEffectiveDate);
        }

        public List<StationInventoryGroup> GetStationInventoryGroupsByFileId(int fileId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var stationInventoryGroups = _inventoryRepository.GetStationInventoryGroupsByFileId(fileId);
                _SetInventoryGroupsDayparts(stationInventoryGroups);
                _SetInventoryGroupsAudiences(stationInventoryGroups);
                return stationInventoryGroups;
            }
        }

        private void _SetInventoryGroupsAudiences(List<StationInventoryGroup> stationInventoryGroups)
        {
            var audiences = stationInventoryGroups.SelectMany(m => m.Manifests.SelectMany(d => d.ManifestAudiences));

            audiences.ForEach(a=> a.Audience = _audiencesCache.GetDisplayAudienceById(a.Audience.Id));
        }

        private void _SetInventoryGroupsDayparts(List<StationInventoryGroup> stationInventoryGroups)
        {
            var dayparts = stationInventoryGroups.SelectMany(ig => ig.Manifests.SelectMany(m => m.ManifestDayparts.Select(md => md.Daypart)));

            dayparts.ForEach(d=> d = _daypartCache.GetDisplayDaypart(d.Id));
        }

        public void AddNewStationInventory(InventoryFileBase inventoryFile, DateTime newEffectiveDate, DateTime newEndDate, int contractedDaypartId)
        {
            if (inventoryFile.InventorySource == null || !inventoryFile.InventorySource.IsActive)
                throw new Exception(string.Format("The selected source type is invalid or inactive."));

            _ExpireExistingInventoryGroups(inventoryFile, newEffectiveDate, newEndDate, contractedDaypartId);
            _ExpireExistingInventoryManifests(inventoryFile, newEffectiveDate, newEndDate, contractedDaypartId);

            _inventoryRepository.AddNewInventory(inventoryFile);
        }

        private void _ExpireExistingInventoryGroups(InventoryFileBase inventoryFile, DateTime newEffectiveDate, DateTime newEndDate, int contractedDaypartId)
        {
            var groupsToCreate = new List<StationInventoryGroup>();
            var existingInventoryGroups = _inventoryRepository.GetActiveInventoryGroupsBySourceAndContractedDaypart(inventoryFile.InventorySource, contractedDaypartId, newEffectiveDate, newEndDate);

            existingInventoryGroups.ForEach(group => _ExpireInventoryGroup(group, newEffectiveDate, newEndDate, groupsToCreate));

            _inventoryRepository.UpdateInventoryGroupsDateIntervals(existingInventoryGroups);
            _inventoryRepository.AddInventoryGroups(groupsToCreate, inventoryFile);
        }

        private void _ExpireExistingInventoryManifests(InventoryFileBase inventoryFile, DateTime newEffectiveDate, DateTime newEndDate, int contractedDaypartId)
        {
            var manifestsToCreate = new List<StationInventoryManifest>();
            var existingInventoryManifests = _inventoryRepository.GetActiveInventoryManifestsBySourceAndContractedDaypart(inventoryFile.InventorySource, contractedDaypartId, newEffectiveDate, newEndDate);

            existingInventoryManifests.ForEach(manifest => _ExpireInventoryManifest(manifest, newEffectiveDate, newEndDate, manifestsToCreate));

            _inventoryRepository.UpdateInventoryManifestsDateIntervals(existingInventoryManifests);
            _inventoryRepository.AddInventoryManifests(manifestsToCreate, inventoryFile);
        }

        private void _ExpireInventoryGroup(StationInventoryGroup group, DateTime newEffectiveDate, DateTime newEndDate, List<StationInventoryGroup> groupsToCreate)
        {
            var dayAfterNewEndDate = newEndDate.AddDays(1);
            var dayBeforeNewEffectiveDate = newEffectiveDate.AddDays(-1);

            // covers case when existing inventory intersects with new inventory and 
            // we can save part of existing inventory that goes after the new inventory date interval or before
            if (newEndDate >= group.StartDate && newEndDate < group.EndDate)
            {
                if (newEffectiveDate > group.StartDate)
                {
                    // create new inventory which is before new effective date
                    var newGroup = _CopyInventoryGroup(group);
                    newGroup.EndDate = dayBeforeNewEffectiveDate;
                    newGroup.Manifests.ForEach(manifest =>
                    {
                        manifest.EndDate = dayBeforeNewEffectiveDate;
                        _FilterWeeksByRangeIntersecting(manifest);
                    });

                    groupsToCreate.Add(newGroup);
                }

                // now existing inventory manifest is part after new end date
                group.StartDate = dayAfterNewEndDate;
                group.Manifests.ForEach(manifest =>
                {
                    manifest.EffectiveDate = dayAfterNewEndDate;
                    _FilterWeeksByRangeIntersecting(manifest);
                });
            }
            else if (newEndDate >= group.EndDate && newEffectiveDate <= group.EndDate)
            {
                group.EndDate = dayBeforeNewEffectiveDate;
                group.Manifests.ForEach(manifest =>
                {
                    manifest.EndDate = dayBeforeNewEffectiveDate;
                    _FilterWeeksByRangeIntersecting(manifest);
                });
            }
        }

        private void _ExpireInventoryManifest(StationInventoryManifest manifest, DateTime newEffectiveDate, DateTime newEndDate, List<StationInventoryManifest> manifestsToCreate)
        {
            var dayAfterNewEndDate = newEndDate.AddDays(1);
            var dayBeforeNewEffectiveDate = newEffectiveDate.AddDays(-1);

            // covers case when existing inventory intersects with new inventory and 
            // we can save part of existing inventory that goes after the new inventory date interval or before
            if (newEndDate >= manifest.EffectiveDate && newEndDate < manifest.EndDate)
            {
                if (newEffectiveDate > manifest.EffectiveDate)
                {
                    // create new inventory which is before new effective date
                    var newManifest = _CopyInventoryManifest(manifest);
                    newManifest.EndDate = dayBeforeNewEffectiveDate;
                    _FilterWeeksByRangeIntersecting(newManifest);
                    manifestsToCreate.Add(newManifest);
                }

                // now existing inventory manifest is part after new end date
                manifest.EffectiveDate = dayAfterNewEndDate;
            }
            else if (newEndDate >= manifest.EndDate && newEffectiveDate <= manifest.EndDate)
            {
                manifest.EndDate = dayBeforeNewEffectiveDate;
            }

            _FilterWeeksByRangeIntersecting(manifest);
        }

        private void _FilterWeeksByRangeIntersecting(StationInventoryManifest manifest)
        {
            if (manifest.EndDate >= manifest.EffectiveDate)
            {
                var mediaWeekIds = _MediaMonthAndWeekAggregateCache
                    .GetMediaWeeksIntersecting(manifest.EffectiveDate, manifest.EndDate.Value)
                    .Select(x => x.Id);

                manifest.ManifestWeeks = manifest.ManifestWeeks.Where(x => mediaWeekIds.Contains(x.MediaWeek.Id)).ToList();
            }
            else
            {
                manifest.ManifestWeeks = new List<StationInventoryManifestWeek>();
            }
        }

        private StationInventoryGroup _CopyInventoryGroup(StationInventoryGroup group)
        {
            return new StationInventoryGroup
            {
                Name = group.Name,
                DaypartCode = group.DaypartCode,
                SlotNumber = group.SlotNumber,
                StartDate = group.StartDate,
                EndDate = group.EndDate,
                InventorySource = group.InventorySource,
                Manifests = group.Manifests.Select(_CopyInventoryManifest).ToList()
            };
        }

        private StationInventoryManifest _CopyInventoryManifest(StationInventoryManifest manifest)
        {
            return new StationInventoryManifest
            {
                DaypartCode = manifest.DaypartCode,
                SpotLengthId = manifest.SpotLengthId,
                SpotsPerWeek = manifest.SpotsPerWeek,
                SpotsPerDay = manifest.SpotsPerDay,
                FileId = manifest.FileId,
                InventorySourceId = manifest.InventorySourceId,
                EffectiveDate = manifest.EffectiveDate,
                EndDate = manifest.EndDate,
                Comment = manifest.Comment,
                Station = manifest.Station,
                ManifestDayparts = manifest.ManifestDayparts.Select(x => new StationInventoryManifestDaypart
                {
                    Daypart = x.Daypart,
                    ProgramName = x.ProgramName,
                    Genres = x.Genres
                }).ToList(),
                ManifestAudiences = manifest.ManifestAudiences.Select(x => new StationInventoryManifestAudience
                {
                    Audience = x.Audience,
                    IsReference = x.IsReference,
                    Impressions = x.Impressions,
                    Rating = x.Rating,
                    CPM = x.CPM
                }).ToList(),
                ManifestAudiencesReferences = manifest.ManifestAudiencesReferences.Select(x => new StationInventoryManifestAudience
                {
                    Audience = x.Audience,
                    IsReference = x.IsReference,
                    Impressions = x.Impressions,
                    Rating = x.Rating,
                    CPM = x.CPM
                }).ToList(),
                ManifestRates = manifest.ManifestRates.Select(x => new StationInventoryManifestRate
                {
                    SpotLengthId = x.SpotLengthId,
                    SpotCost = x.SpotCost
                }).ToList(),
                ManifestWeeks = manifest.ManifestWeeks.Select(x => new StationInventoryManifestWeek
                {
                    MediaWeek = x.MediaWeek,
                    Spots = x.Spots
                }).ToList()
            };
        }
    }
}
