using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Inventory;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    /// <summary>
    /// Test the Inventory Export Engine.
    /// </summary>
    [TestFixture]
    public class InventoryExportEngineUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate()
        {
            var engine = new InventoryExportEngineUnitTestClass();
            var items = new List<InventoryExportDto>();
            // item 1 - goes to line 1
            items.Add(new InventoryExportDto() { InventoryId = 1, MediaWeekId = 1, StationId = 1, DaypartId = 1, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramOne"});
            items.Add(new InventoryExportDto() { InventoryId = 1, MediaWeekId = 2, StationId = 1, DaypartId = 1, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramOne" });
            items.Add(new InventoryExportDto() { InventoryId = 1, MediaWeekId = 3, StationId = 1, DaypartId = 1, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramOne" });
            // item 2 - goes to line 2
            items.Add(new InventoryExportDto() { InventoryId = 2, MediaWeekId = 1, StationId = 2, DaypartId = 2, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramTwo" });
            items.Add(new InventoryExportDto() { InventoryId = 2, MediaWeekId = 2, StationId = 2, DaypartId = 2, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramTwo" });
            items.Add(new InventoryExportDto() { InventoryId = 2, MediaWeekId = 3, StationId = 2, DaypartId = 2, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramTwo" });
            // item 3 - goes to line 3
            items.Add(new InventoryExportDto() { InventoryId = 3, MediaWeekId = 1, StationId = 3, DaypartId = 3, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramThree" });
            items.Add(new InventoryExportDto() { InventoryId = 3, MediaWeekId = 2, StationId = 3, DaypartId = 3, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramThree" });
            items.Add(new InventoryExportDto() { InventoryId = 3, MediaWeekId = 3, StationId = 3, DaypartId = 3, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramThree" });
            // item 4 - goes to line 3
            items.Add(new InventoryExportDto() { InventoryId = 4, MediaWeekId = 1, StationId = 3, DaypartId = 3, Impressions = 4000, SpotCost = 60, ProgramName = "ProgramFour" });
            items.Add(new InventoryExportDto() { InventoryId = 4, MediaWeekId = 2, StationId = 3, DaypartId = 3, Impressions = 4000, SpotCost = 60, ProgramName = "ProgramFour" });
            // item 5 - goes to line 4
            items.Add(new InventoryExportDto() { InventoryId = 5, MediaWeekId = 2, StationId = 4, DaypartId = 3, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramFive" });
            items.Add(new InventoryExportDto() { InventoryId = 5, MediaWeekId = 3, StationId = 4, DaypartId = 3, Impressions = 10000, SpotCost = 20, ProgramName = "ProgramFive" });

            var result = engine.Calculate(items);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryWorksheetColumnDescriptors()
        {
            var engine = new InventoryExportEngineUnitTestClass();
            var dates = new List<DateTime>
            {
                new DateTime(2020, 04, 06),
                new DateTime(2020, 04, 13),
                new DateTime(2020, 04, 20),
                new DateTime(2020, 04, 27),
                new DateTime(2020, 05, 04),
            };

            var result = engine.UT_GetInventoryWorksheetColumnDescriptors(dates);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TransformToExportLines()
        {
            var engine = new InventoryExportEngineUnitTestClass();
            var testWeeks = new List<InventoryExportLineWeekDetail>
            {
                new InventoryExportLineWeekDetail {MediaWeekId = 1, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 2, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 3, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 4, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 5, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
            };
            var testWeekIds = testWeeks.Select(s => s.MediaWeekId).ToList();
            var testWeeksSansLast = testWeeks.Where(w => w.MediaWeekId != 5).ToList();
            var testWeeksSansFirst = testWeeks.Where(w => w.MediaWeekId != 1).ToList();
            var testWeeksSansMiddle = testWeeks.Where(w => w.MediaWeekId != 3).ToList();

            var testItems = new List<InventoryExportLineDetail>();
            testItems.Add(new InventoryExportLineDetail { StationId = 1, DaypartId = 1, ProgramNames = new List<string> { "ProgramOne" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            testItems.Add(new InventoryExportLineDetail { StationId = 2, DaypartId = 1, ProgramNames = new List<string> { "ProgramTwo" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            testItems.Add(new InventoryExportLineDetail { StationId = 3, DaypartId = 1, ProgramNames = new List<string> { "ProgramThree" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            // not in last week
            testItems.Add(new InventoryExportLineDetail { StationId = 4, DaypartId = 1, ProgramNames = new List<string> { "ProgramFour" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeksSansLast });
            // not in first week
            testItems.Add(new InventoryExportLineDetail { StationId = 5, DaypartId = 1, ProgramNames = new List<string> { "ProgramFive" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeksSansFirst });
            // not in middle week
            testItems.Add(new InventoryExportLineDetail { StationId = 6, DaypartId = 1, ProgramNames = new List<string> { "ProgramSix" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeksSansMiddle });
            // multiple program names
            testItems.Add(new InventoryExportLineDetail { StationId = 7, DaypartId = 1, ProgramNames = new List<string> { "ProgramSeven", "ProgramEight" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            // Market has comma
            testItems.Add(new InventoryExportLineDetail { StationId = 8, DaypartId = 1, ProgramNames = new List<string> { "ProgramNine" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            // Market has quote
            testItems.Add(new InventoryExportLineDetail { StationId = 9, DaypartId = 1, ProgramNames = new List<string> { "ProgramNine" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            // daypart has comma
            testItems.Add(new InventoryExportLineDetail { StationId = 8, DaypartId = 2, ProgramNames = new List<string> { "ProgramNine" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            
            var stations = new List<DisplayBroadcastStation>
            {
                new DisplayBroadcastStation { Id = 1, LegacyCallLetters = "Station1", MarketCode = 1},
                new DisplayBroadcastStation { Id = 2, LegacyCallLetters = "Station2", MarketCode = 2},
                new DisplayBroadcastStation { Id = 3, LegacyCallLetters = "Station3", MarketCode = 3},
                new DisplayBroadcastStation { Id = 4, LegacyCallLetters = "Station4", MarketCode = 4},
                new DisplayBroadcastStation { Id = 5, LegacyCallLetters = "Station5", MarketCode = 5},
                new DisplayBroadcastStation { Id = 6, LegacyCallLetters = "Station6", MarketCode = 6},
                new DisplayBroadcastStation { Id = 7, LegacyCallLetters = "Station7", MarketCode = 7},
                new DisplayBroadcastStation { Id = 8, LegacyCallLetters = "Station8", MarketCode = 8},
                new DisplayBroadcastStation { Id = 9, LegacyCallLetters = "Station9", MarketCode = 9},
                new DisplayBroadcastStation { Id = 10, LegacyCallLetters = "Station10", MarketCode = 10},
                new DisplayBroadcastStation { Id = 11, LegacyCallLetters = "Station11", MarketCode = 11},
            };

            var markets = new List<MarketCoverage>
            {
                new MarketCoverage { MarketCode = 1, Market = "MyMarket1", Rank = 21},
                new MarketCoverage { MarketCode = 2, Market = "MyMarket2", Rank = 22},
                new MarketCoverage { MarketCode = 3, Market = "MyMarket3", Rank = 23},
                new MarketCoverage { MarketCode = 4, Market = "MyMarket4", Rank = 24},
                new MarketCoverage { MarketCode = 5, Market = "MyMarket5", Rank = 25},
                new MarketCoverage { MarketCode = 6, Market = "MyMarket6", Rank = 26},
                new MarketCoverage { MarketCode = 7, Market = "MyMarket7", Rank = 27},
                new MarketCoverage { MarketCode = 8, Market = "MyMarket8", Rank = 28},
                new MarketCoverage { MarketCode = 9, Market = "MyMarket9", Rank = 29},
                new MarketCoverage { MarketCode = 10, Market = "MyMarket10", Rank = 30},
                new MarketCoverage { MarketCode = 11, Market = "MyMarket11", Rank = 31},
            };

            var dayparts = new Dictionary<int, DisplayDaypart>();
            dayparts[1] = new DisplayDaypart(1, 3600, 7200, true, true, true, true, true, true, true);
            dayparts[2] = new DisplayDaypart(2, 18000, 21600, true, true, true, false, false, true, true);

            var result = engine.UT_TransformToExportLines(testItems, testWeekIds, stations, markets, dayparts);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TransformToExportLines_UnknownStation()
        {
            var engine = new InventoryExportEngineUnitTestClass();
            var testWeeks = new List<InventoryExportLineWeekDetail>
            {
                new InventoryExportLineWeekDetail {MediaWeekId = 1, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 2, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 3, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 4, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 5, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
            };
            var testWeekIds = testWeeks.Select(s => s.MediaWeekId).ToList();

            var testItems = new List<InventoryExportLineDetail>();
            testItems.Add(new InventoryExportLineDetail { StationId = 1, DaypartId = 1, ProgramNames = new List<string> { "ProgramOne" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            // Unknown Station
            testItems.Add(new InventoryExportLineDetail { StationId = 4, DaypartId = 1, ProgramNames = new List<string> { "ProgramTwo" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            testItems.Add(new InventoryExportLineDetail { StationId = 3, DaypartId = 1, ProgramNames = new List<string> { "ProgramThree" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            // Unknown Market
            testItems.Add(new InventoryExportLineDetail { StationId = 5, DaypartId = 1, ProgramNames = new List<string> { "ProgramThree" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });

            var stations = new List<DisplayBroadcastStation>
            {
                new DisplayBroadcastStation { Id = 1, LegacyCallLetters = "Station1", MarketCode = 1 },
                new DisplayBroadcastStation { Id = 2, LegacyCallLetters = "Station2", MarketCode = 2 },
                new DisplayBroadcastStation { Id = 3, LegacyCallLetters = "Station3", MarketCode = 3 },
                new DisplayBroadcastStation { Id = 5, LegacyCallLetters = "StationFive", MarketCode = 5 }
            };

            var markets = new List<MarketCoverage>
            {
                new MarketCoverage { MarketCode = 1, Market = "MyMarket1", Rank = 21},
                new MarketCoverage { MarketCode = 2, Market = "MyMarket2", Rank = 22},
                new MarketCoverage { MarketCode = 3, Market = "MyMarket3", Rank = 23},
            };

            var dayparts = new Dictionary<int, DisplayDaypart>();
            dayparts[1] = new DisplayDaypart(1, 3600, 7200, true, true, true, true, true, true, true);
            dayparts[2] = new DisplayDaypart(2, 18000, 21600, true, true, true, false, false, true, true);

            var result = engine.UT_TransformToExportLines(testItems, testWeekIds, stations, markets, dayparts);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TransformToExportLines_UnknownDaypart()
        {
            var engine = new InventoryExportEngineUnitTestClass();
            var testWeeks = new List<InventoryExportLineWeekDetail>
            {
                new InventoryExportLineWeekDetail {MediaWeekId = 1, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 2, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 3, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 4, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 5, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
            };
            var testWeekIds = testWeeks.Select(s => s.MediaWeekId).ToList();

            var testItems = new List<InventoryExportLineDetail>();
            testItems.Add(new InventoryExportLineDetail { StationId = 1, DaypartId = 1, ProgramNames = new List<string> { "ProgramOne" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            // unknown daypart
            testItems.Add(new InventoryExportLineDetail { StationId = 2, DaypartId = 3, ProgramNames = new List<string> { "ProgramTwo" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });
            testItems.Add(new InventoryExportLineDetail { StationId = 3, DaypartId = 1, ProgramNames = new List<string> { "ProgramThree" }, AvgSpotCost = 10, AvgHhImpressions = 5000, AvgCpm = 2, Weeks = testWeeks });

            var stations = new List<DisplayBroadcastStation>
            {
                new DisplayBroadcastStation { Id = 1, LegacyCallLetters = "Station1", MarketCode = 1 },
                new DisplayBroadcastStation { Id = 2, LegacyCallLetters = "Station2", MarketCode = 2 },
                new DisplayBroadcastStation { Id = 3, LegacyCallLetters = "Station3", MarketCode = 3 }
            };

            var markets = new List<MarketCoverage>
            {
                new MarketCoverage { MarketCode = 1, Market = "MyMarket1", Rank = 21},
                new MarketCoverage { MarketCode = 2, Market = "MyMarket2", Rank = 22},
                new MarketCoverage { MarketCode = 3, Market = "MyMarket3", Rank = 23},
            };

            var dayparts = new Dictionary<int, DisplayDaypart>();
            dayparts[1] = new DisplayDaypart(1, 3600, 7200, true, true, true, true, true, true, true);
            dayparts[2] = new DisplayDaypart(2, 18000, 21600, true, true, true, false, false, true, true);

            var result = engine.UT_TransformToExportLines(testItems, testWeekIds, stations, markets, dayparts);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}