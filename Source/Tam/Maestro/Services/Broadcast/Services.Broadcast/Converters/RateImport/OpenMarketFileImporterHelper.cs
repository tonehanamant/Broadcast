using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Interception.Utilities;

namespace Services.Broadcast.Converters.RateImport
{
    public static class OpenMarketFileImporterHelper
    {
        public static List<StationInventoryManifest> ScrubInventoryManifests(List<StationInventoryManifest> inventory, Dictionary<int, int> spotLengthDurationsById)
        {
            var cleanInventory = _HandleFullDuplicates(inventory, spotLengthDurationsById);
            _HandleOverlaps(cleanInventory, spotLengthDurationsById);
            _RemoveTempInventoryIds(cleanInventory);

            return cleanInventory;
        }

        internal static List<InventoryStationDaypartWeek> _GetFlattenedInventoryItems(
            IEnumerable<StationInventoryManifest> inventory, Dictionary<int,int> spotLengthDurationsById)
        {
            var inventoryItemIndex = 0;
            var flatItemIndex = 0;
            var flattenedItems = new List<InventoryStationDaypartWeek>();
            // flatten to station, daypart, week
            foreach (var item in inventory)
            {
                // The records are expected to come in with an id.
                // Set a temp id on the item so we can locate and remove the item if it's a duplicate
                // We will remove that temp id later.
                item.Id = inventoryItemIndex;
                inventoryItemIndex++;

                var itemStartWeek = item.ManifestWeeks.OrderBy(w => w.StartDate).First().MediaWeek;
                var itemLastWeek = item.ManifestWeeks.OrderBy(w => w.StartDate).Last().MediaWeek;

                var spotLengthId = item.SpotLengthId;
                var spotLengthDuration = spotLengthDurationsById[spotLengthId];
                var spotCost = item.ManifestRates
                    .Single(r => r.SpotLengthId == spotLengthId)
                    .SpotCost;

                foreach (var manifestWeek in item.ManifestWeeks)
                {
                    var numWeeksSinceItemStartWeek = _CalculateNumWeeksSinceWeekStartDate(manifestWeek.MediaWeek.StartDate, itemStartWeek.StartDate);
                    foreach (var manifestDaypart in item.ManifestDayparts)
                    {
                        var flatItem = new InventoryStationDaypartWeek
                        {
                            Id = ++flatItemIndex,
                            StationId = item.Station.Id,
                            StationLegacyCallLetters = item.Station.LegacyCallLetters,
                            DaypartId = manifestDaypart.Daypart.Id,
                            DaypartText = manifestDaypart.Daypart.Preview,
                            ProgramName = manifestDaypart.ProgramName,
                            MediaWeekId = manifestWeek.MediaWeek.Id,
                            ItemFirstWeekMediaWeekId = itemStartWeek.Id,
                            ItemFirstWeekStartDate = itemStartWeek.StartDate,
                            ItemLastWeekMediaWeekId = itemLastWeek.Id,
                            ItemLastWeekEndDate = itemLastWeek.EndDate,
                            NumWeeksSinceItemStartWeek = numWeeksSinceItemStartWeek,
                            SpotLengthId = spotLengthId,
                            SpotLengthDuration = spotLengthDuration,
                            SpotCost = spotCost,
                            InventoryItem = item
                        };
                        flattenedItems.Add(flatItem);
                    }
                }
            }

            return flattenedItems;
        }

        /// <summary>
        /// A duplicate is where many spots match by station, daypart and daterange.
        /// </summary>
        internal static List<StationInventoryManifest> _HandleFullDuplicates(List<StationInventoryManifest> inventory, Dictionary<int, int> spotLengthDurationsById)
        {
            var flattenedItems = _GetFlattenedInventoryItems(inventory, spotLengthDurationsById);
            var duplicates = flattenedItems
                .Where(i => i.MediaWeekId == i.ItemFirstWeekMediaWeekId)
                .GroupBy(i => new
                {
                    i.SpotLengthId,
                    i.StationId,
                    i.DaypartId,
                    i.ItemFirstWeekMediaWeekId,
                    i.ItemLastWeekMediaWeekId
                })
                .Where(g => g.Count() > 1)
                .ToList();

            var toRemove = new List<StationInventoryManifest>();
            foreach (var duplicateGroup in duplicates)
            {
                var firstPass = true;
                var orderedDuplicateGroupItems = duplicateGroup
                    .OrderByDescending(s => s.SpotCost)
                    .ThenBy(s => s.InventoryItem.Id);
                foreach (var duplicate in orderedDuplicateGroupItems)
                {
                    if (firstPass)
                    {
                        firstPass = false;
                        continue;
                    }

                    toRemove.Add(duplicate.InventoryItem);
                }
            }

            var result = inventory.Except(toRemove).ToList();
            return result;
        }

        /// <summary>
        /// An overlap is where many spots are on the same station and time and their date-ranges overlap.
        /// Example:
        ///     - Spot 1 : station CDNT; M-F 2am-3am; from 10/01/2020 to 10/31/2020; rate $20
        ///     - Spot 2 : station CDNT; M-F 2am-3am; from 10/12/2020 to 10/20/2020; rate $10
        /// </summary>
        internal static void _HandleOverlaps(List<StationInventoryManifest> inventory, Dictionary<int, int> spotLengthDurationsById)
        {
            var flattenedItems = _GetFlattenedInventoryItems(inventory, spotLengthDurationsById);
            var itemGroupings = flattenedItems
                .GroupBy(i => new
                {
                    i.SpotLengthId,
                    i.StationId,
                    i.DaypartId,
                    i.MediaWeekId
                })
                .Where(i => i.Count() > 1)
                .ToList();

            foreach (var itemGroup in itemGroupings)
            {
                // keep the item that started most recently.
                var minDelta = 99;
                var minDeltaId = -1;
                foreach (var item in itemGroup)
                {
                    if (item.NumWeeksSinceItemStartWeek < minDelta)
                    {
                        minDelta = item.NumWeeksSinceItemStartWeek;
                        minDeltaId = item.Id;
                    }
                }

                foreach (var item in itemGroup)
                {
                    if (item.Id != minDeltaId)
                    {
                        var withWeekRemoved = _RemoveWeek(item.InventoryItem.ManifestWeeks, item.MediaWeekId);
                        item.InventoryItem.ManifestWeeks = withWeekRemoved;
                    }
                }
            }
        }

        internal static int _CalculateNumWeeksSinceWeekStartDate(DateTime currentMediaWeekStartDate, DateTime itemStartMediaWeekStartDate)
        {
            var deltaDays = currentMediaWeekStartDate.Subtract(itemStartMediaWeekStartDate).Days;

            if (deltaDays == 0)
            {
                return 0;
            }

            var deltaWeeks = deltaDays / 7;
            return deltaWeeks;
        }

        internal static List<StationInventoryManifestWeek> _RemoveWeek(List<StationInventoryManifestWeek> weeks,
            int mediaWeekIdToRemove)
        {
            var result = new List<StationInventoryManifestWeek>();
            weeks.ForEach(w =>
            {
                if (w.MediaWeek.Id != mediaWeekIdToRemove)
                {
                    result.Add(w);
                }
            });
            return result;
        }

        private static void _RemoveTempInventoryIds(List<StationInventoryManifest> inventory)
        {
            inventory.ForEach(i => i.Id = null);
        }

        /// <summary>
        /// Flattened data class used for aggregation within the <see cref="OpenMarketFileImporterHelper"/>.
        /// </summary>
        internal class InventoryStationDaypartWeek
        {
            public int Id { get; set; }

            public int StationId { get; set; }
            public string StationLegacyCallLetters { get; set; }

            public int DaypartId { get; set; }
            public string DaypartText { get; set; }
            public string ProgramName { get; set; }

            public int SpotLengthId { get; set; }
            public int SpotLengthDuration { get; set; }
            public decimal SpotCost { get; set; }

            public int MediaWeekId { get; set; }
            public int ItemFirstWeekMediaWeekId { get; set; }
            public DateTime ItemFirstWeekStartDate { get; set; }

            public int ItemLastWeekMediaWeekId { get; set; }
            public DateTime ItemLastWeekEndDate { get; set; }
            public int NumWeeksSinceItemStartWeek { get; set; }

            public StationInventoryManifest InventoryItem { get; set; }
        }
    }
}