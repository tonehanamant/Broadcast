using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.IntegrationTests.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters.RateImport
{
    [TestFixture]
    public class InventoryImportManifestDuplicateHandlerUnitTests
    {
        private List<DisplayBroadcastStation> _TestStationsSet;
        private List<StationInventoryManifestWeek> _TestMediaWeekSet;
        private List<StationInventoryManifestDaypart> _TestDaypartsSet;
        private List<List<StationInventoryManifestRate>> _TestSpotRateSets;
        private Dictionary<int, int> _TestSpotLengthDurationsById;

        [SetUp]
        public void Setup()
        {
            // Data
            _TestStationsSet = StationsTestData.GetStations(marketCount: 5, stationsPerMarket: 1);
            _TestMediaWeekSet = new List<StationInventoryManifestWeek>
            {
                new StationInventoryManifestWeek
                    {MediaWeek = new MediaWeek {Id = 1, StartDate = new DateTime(2020, 11, 30), EndDate = new DateTime(2020, 12, 6)}},
                new StationInventoryManifestWeek
                    {MediaWeek = new MediaWeek {Id = 2, StartDate = new DateTime(2020, 12, 7), EndDate = new DateTime(2020, 12, 13)}},
                new StationInventoryManifestWeek
                    {MediaWeek = new MediaWeek {Id = 3, StartDate = new DateTime(2020, 12, 14), EndDate = new DateTime(2020, 12, 20)}},
                new StationInventoryManifestWeek
                    {MediaWeek = new MediaWeek {Id = 4, StartDate = new DateTime(2020, 12, 21), EndDate = new DateTime(2020, 12, 27)}},
                new StationInventoryManifestWeek
                    {MediaWeek = new MediaWeek {Id = 5, StartDate = new DateTime(2020, 12, 28), EndDate = new DateTime(2021, 1, 3)}},
            };
            /** 
             * Hack!!! Why do I have to make a copy of this? 
             *      When I run ALL unit tests these fail 
             *          becase someother test in another class is changing the values.
             * I should be using : DaypartsTestData.GetDisplayDaypart(1)             
             ***/
            var testDaypart = new DisplayDaypart
            {
                Id = 1,
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = false,
                Sunday = false,
                StartTime = 3600, // 1am
                EndTime = 7199 // 2am
            };
            _TestDaypartsSet = new List<StationInventoryManifestDaypart>
            {
                new StationInventoryManifestDaypart {Daypart = testDaypart, ProgramName = "ProgramOne"}
            };
            _TestSpotRateSets = new List<List<StationInventoryManifestRate>>
            {
                new List<StationInventoryManifestRate>
                {
                    new StationInventoryManifestRate {SpotLengthId = 1, SpotCost = 10},
                    new StationInventoryManifestRate {SpotLengthId = 2, SpotCost = 20},
                    new StationInventoryManifestRate {SpotLengthId = 3, SpotCost = 30},
                    new StationInventoryManifestRate {SpotLengthId = 4, SpotCost = 40},
                },
                new List<StationInventoryManifestRate>
                {
                    new StationInventoryManifestRate {SpotLengthId = 1, SpotCost = 110},
                    new StationInventoryManifestRate {SpotLengthId = 2, SpotCost = 120},
                    new StationInventoryManifestRate {SpotLengthId = 3, SpotCost = 130},
                    new StationInventoryManifestRate {SpotLengthId = 4, SpotCost = 140}
                }
            };
            _TestSpotLengthDurationsById = SpotLengthTestData.GetSpotLengthDurationsById();
        }

        // item starts on 11/30/2020.  
        [TestCase(2021, 1, 4, 5)]
        [TestCase(2020, 12, 28, 4)]
        [TestCase(2020, 12, 21, 3)]
        [TestCase(2020, 12, 14, 2)]
        [TestCase(2020, 12, 7, 1)]
        [TestCase(2020, 11, 30,0)]
        [TestCase(2020, 11, 23, -1)]
        public void CalculateNumWeeksSinceWeekStartDate(int currentWeekStartYear, int currentWeekStartMonth, int currentWeekStartDay, int expectedResult)
        {
            // Arrange
            var itemStartMediaWeekStartDate = new DateTime(2020, 11, 30);
            var currentMediaWeekStartDate = new DateTime(currentWeekStartYear, currentWeekStartMonth, currentWeekStartDay);

            // Act
            var result = InventoryImportManifestDuplicateHandler._CalculateNumWeeksSinceWeekStartDate(currentMediaWeekStartDate, itemStartMediaWeekStartDate);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void RemoveWeek(int mediaWeekIdToRemove)
        {
            // Arrange
            var weeks = _TestMediaWeekSet;

            // Act
            var result = InventoryImportManifestDuplicateHandler._RemoveWeek(weeks, mediaWeekIdToRemove);

            // Assert
            var resultWeekIds = result.Select(w => w.MediaWeek.Id).ToList();
            foreach (var week in weeks)
            {
                var currentWeekId = week.MediaWeek.Id;
                var shouldRemove = currentWeekId == mediaWeekIdToRemove;
                var wasRemoved = !resultWeekIds.Contains(currentWeekId);
                Assert.AreEqual(shouldRemove, wasRemoved);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubInventoryManifestGroups()
        {
            // Arrange
            var inventoryGroups = _GetInventoryGroups();

            // Act
            var result = InventoryImportManifestDuplicateHandler.ScrubInventoryManifestGroups(inventoryGroups, _TestSpotLengthDurationsById);

            // Assert
            // verify that everything has weeks...
            var itemsWithNoWeeks = result.SelectMany(s => s.Manifests).Where(i => i.ManifestWeeks.Count == 0).ToList();
            Assert.AreEqual(0, itemsWithNoWeeks.Count);
            // verify that all ids were removed.
            var hasItemsWithIds = result.SelectMany(s => s.Manifests).Where(i => i.Id.HasValue).ToList();
            Assert.AreEqual(0, hasItemsWithIds.Count);
            // verify the result details
            var toVerify = new
            {
                Before = inventoryGroups,
                Result = result
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify, _GetCleanupSettingsForFlattenedInventory()));
        }

        private List<StationInventoryGroup> _GetInventoryGroups()
        {
            var inventoryGroups = new List<StationInventoryGroup>();
            var inventorySource = new InventorySource
            {
                Id = 1,
                Name = "SourceOne",
                IsActive = true,
                InventoryType = InventorySourceTypeEnum.Barter
            };

            var slotNumberIndex = 1;
            var hasDup = false;
            foreach (var station in _TestStationsSet)
            {
                var inventory = _GetInventoryForStationGroup(station, hasDup);
                var group = new StationInventoryGroup
                {
                    Name = station.LegacyCallLetters,
                    SlotNumber = slotNumberIndex,
                    InventorySource = inventorySource,
                    Manifests = inventory
                };

                inventoryGroups.Add(group);

                slotNumberIndex++;
                hasDup = !hasDup;
            }

            return inventoryGroups;
        }

        private List<StationInventoryManifest> _GetInventoryForStationGroup(DisplayBroadcastStation station, bool hasDup)
        {
            var inventory = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    SpotLengthId = 1,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = station,
                    ManifestRates = _TestSpotRateSets[0]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = hasDup ? 1 : 2,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = station,
                    ManifestRates = _TestSpotRateSets[0]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = 3,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = station,
                    ManifestRates = _TestSpotRateSets[0]
                }
            };
            return inventory;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ScrubInventoryManifestsCleanWithAllDupForms()
        {
            // Arrange
            var inventory = _GetCleanInventory(1);
            inventory.AddRange(_GetInventoryWithOverlaps(2));
            inventory.AddRange(_GetInventoryWithFullDups(3));
            inventory.AddRange(_GetInventoryWithScheduleDups(4));

            // Act
            var result = InventoryImportManifestDuplicateHandler.ScrubInventoryManifests(inventory, _TestSpotLengthDurationsById);

            // Assert
            // verify that everything has weeks...
            var itemsWithNoWeeks = result.Where(i => i.ManifestWeeks.Count == 0).ToList();
            Assert.AreEqual(0, itemsWithNoWeeks.Count);
            // verify that all ids were removed.
            var hasItemsWithIds = result.Where(i => i.Id.HasValue).ToList();
            Assert.AreEqual(0, hasItemsWithIds.Count);
            // verify the result details
            var toVerify = new
            {
                Before = inventory,
                Result = result
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetFlattenedInventoryItems()
        {
            // Arrange
            var inventory = _GetCleanInventory();

            // Act
            var flattenedItems = InventoryImportManifestDuplicateHandler._GetFlattenedInventoryItems(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(flattenedItems, _GetCleanupSettingsForFlattenedInventory()));
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetFlattenedInventoryItemsWithOverlaps()
        {
            // Arrange
            var inventory = _GetInventoryWithOverlaps();

            // Act
            var flattenedItems = InventoryImportManifestDuplicateHandler._GetFlattenedInventoryItems(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(flattenedItems, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetFlattenedInventoryItemsWithFullDups()
        {
            // Arrange
            var inventory = _GetInventoryWithFullDups();

            // Act
            var flattenedItems = InventoryImportManifestDuplicateHandler._GetFlattenedInventoryItems(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(flattenedItems, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetFlattenedInventoryItemsWithScheduleDups()
        {
            // Arrange
            var inventory = _GetInventoryWithScheduleDups();

            // Act
            var flattenedItems = InventoryImportManifestDuplicateHandler._GetFlattenedInventoryItems(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(flattenedItems, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HandleOverlapsWhenNoOverlaps()
        {
            // Arrange
            var inventory = _GetCleanInventory();

            // Act
            InventoryImportManifestDuplicateHandler._HandleOverlaps(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HandleOverlapsWhenOverlaps()
        {
            // Arrange
            var inventory = _GetInventoryWithOverlaps();

            // Act
            InventoryImportManifestDuplicateHandler._HandleOverlaps(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HandleDuplicatesWhenNoDups()
        {
            // Arrange
            var inventory = _GetCleanInventory();

            // Act
            var results = InventoryImportManifestDuplicateHandler._HandleFullDuplicates(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HandleDuplicatesWhenHasFullDups()
        {
            // Arrange
            var inventory = _GetInventoryWithFullDups();

            // Act
            var results = InventoryImportManifestDuplicateHandler._HandleFullDuplicates(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetCleanupSettingsForFlattenedInventory()));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HandleDuplicatesWhenHasScheduleDups()
        {
            // Arrange
            var inventory = _GetInventoryWithScheduleDups();

            // Act
            var results = InventoryImportManifestDuplicateHandler._HandleFullDuplicates(inventory, _TestSpotLengthDurationsById);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results, _GetCleanupSettingsForFlattenedInventory()));
        }

        private List<StationInventoryManifest> _GetCleanInventory(int spotLengthId = 1)
        {
            var inventory = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[0],
                    ManifestRates = _TestSpotRateSets[0]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[1],
                    ManifestRates = _TestSpotRateSets[0]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[2],
                    ManifestRates = _TestSpotRateSets[0]
                }
            };
            return inventory;
        }

        private List<StationInventoryManifest> _GetInventoryWithOverlaps(int spotLengthId = 1)
        {
            var inventory = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[0],
                    ManifestRates = _TestSpotRateSets[0]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    // this will cause an interleaving
                    ManifestWeeks = _TestMediaWeekSet.GetRange(1,3),
                    Station = _TestStationsSet[0],
                    ManifestRates = _TestSpotRateSets[0]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[2],
                    ManifestRates = _TestSpotRateSets[0]
                }
            };
            return inventory;
        }

        private List<StationInventoryManifest> _GetInventoryWithFullDups(int spotLengthId = 1)
        {
            var inventory = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[0],
                    ManifestRates = _TestSpotRateSets[0]
                },
                // this is a full dup of the first item.
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[0],
                    ManifestRates = _TestSpotRateSets[0]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[2],
                    ManifestRates = _TestSpotRateSets[0]
                }
            };
            return inventory;
        }

        private List<StationInventoryManifest> _GetInventoryWithScheduleDups(int spotLengthId = 1)
        {
            var inventory = new List<StationInventoryManifest>
            {
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[0],
                    ManifestRates = _TestSpotRateSets[0]
                },
                // this is a schedule dup of the first item.
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[0],
                    // This one has the higher Rates
                    ManifestRates = _TestSpotRateSets[1]
                },
                new StationInventoryManifest
                {
                    SpotLengthId = spotLengthId,
                    ManifestDayparts = _TestDaypartsSet,
                    ManifestWeeks = _TestMediaWeekSet,
                    Station = _TestStationsSet[2],
                    ManifestRates = _TestSpotRateSets[0]
                }
            };
            return inventory;
        }

        private void _LoadResolver(IgnorableSerializerContractResolver jsonResolver, Type itemType, string[] propertyNamesToKeep)
        {
            itemType.GetMembers()
                .Where(p => !propertyNamesToKeep.Contains(p.Name))
                .ForEach(p => jsonResolver.Ignore(itemType, p.Name));
        }

        private JsonSerializerSettings _GetCleanupSettingsForFlattenedInventory()
        {
            var jsonResolver = new IgnorableSerializerContractResolver {IgnoreIsSpecifiedMembers = true};

            var stationInventoryManifestPropertyNamesToKeep = new[] { "ManifestDayparts", "ManifestWeeks", "Station", "Id", "SpotLengthId", "ManifestRates" };
            _LoadResolver(jsonResolver, typeof(StationInventoryManifest), stationInventoryManifestPropertyNamesToKeep);

            var manifestDaypartPropertyNamesToKeep = new[] { "Daypart", "ProgramName" };
            _LoadResolver(jsonResolver, typeof(StationInventoryManifestDaypart), manifestDaypartPropertyNamesToKeep);

            var manifestWeekPropertyNamesToKeep = new[] { "MediaWeek" };
            _LoadResolver(jsonResolver, typeof(StationInventoryManifestWeek), manifestWeekPropertyNamesToKeep);

            var stationPropertyNamesToKeep = new[] { "Id" };
            _LoadResolver(jsonResolver, typeof(DisplayBroadcastStation), stationPropertyNamesToKeep);

            var manifestRatePropertyNamesToKeep = new[] { "SpotCost", "SpotLengthId" };
            _LoadResolver(jsonResolver, typeof(StationInventoryManifestRate), manifestRatePropertyNamesToKeep);

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            return jsonSettings;
        }
    }
}