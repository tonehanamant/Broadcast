using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Data.Entities.DataTransferObjects;
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
            var engine = new InventoryExportEngine();
            var items = new List<InventoryExportDto>();
            // item 1 - forecasted
            items.Add(_GetInventoryExportDto(1, 1, 1, 1,  10000, 20, "One", "One", ProgramSourceEnum.Forecasted, 33));
            items.Add(_GetInventoryExportDto(1, 2, 1, 1, 10000, 20, "One", "One", ProgramSourceEnum.Forecasted, 33));
            items.Add(_GetInventoryExportDto(1, 3, 1, 1, 10000, 20, "One", "One", ProgramSourceEnum.Forecasted, 33));
            // item 2 -  Mapped
            items.Add(_GetInventoryExportDto(2, 1, 2, 2, 10000, 20, "Two", "Two", ProgramSourceEnum.Mapped, 33));
            items.Add(_GetInventoryExportDto(2, 2, 2, 2, 10000, 20, "Two", "Two", ProgramSourceEnum.Mapped, 33));
            items.Add(_GetInventoryExportDto(2, 3, 2, 2, 10000, 20, "Two", "Two", ProgramSourceEnum.Mapped, 33));
            // item 3 - Doesn't have all three weeks
            items.Add(_GetInventoryExportDto(3, 2, 3, 3, 10000, 20, "Three", "Three", ProgramSourceEnum.Mapped, 33));
            items.Add(_GetInventoryExportDto(3, 3, 3, 3, 10000, 20, "Three", "Three", ProgramSourceEnum.Mapped, 33));

            var result = engine.Calculate(items);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculate_WithoutPrograms()
        {
            var engine = new InventoryExportEngine();
            var items = new List<InventoryExportDto>();
            // item 1 
            items.Add(_GetInventoryExportDto(1, 1, 1, 1, 10000, 20, null, "One", null, null));
            items.Add(_GetInventoryExportDto(1, 2, 1, 1, 10000, 20, null, "One", null, null));
            items.Add(_GetInventoryExportDto(1, 3, 1, 1, 10000, 20, null, "One", null, null));
            // item 2 -  Doesn't have all three weeks
            items.Add(_GetInventoryExportDto(2, 1, 2, 2, 10000, 20, null, "Two", null, null));
            items.Add(_GetInventoryExportDto(2, 2, 2, 2, 10000, 20, null, "Two", null, null));

            var result = engine.Calculate(items);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [TestCase(InventoryExportGenreTypeEnum.News, 2020, 2, "Open Market inventory NEWS 2020 Q2.xlsx")]
        [TestCase(InventoryExportGenreTypeEnum.NonNews, 2019, 1, "Open Market inventory NON-NEWS 2019 Q1.xlsx")]
        [TestCase(InventoryExportGenreTypeEnum.NotEnriched, 2019, 1, "Open Market inventory NOT ENRICHED 2019 Q1.xlsx")]
        public void GetInventoryExportFileName(InventoryExportGenreTypeEnum genreEnum, int year, int quarter, string expectedResult)
        {
            var quarterDetail = new QuarterDetailDto {Year = year, Quarter = quarter};
            var engine = new InventoryExportEngine();

            var result = engine.GetInventoryExportFileName(genreEnum, quarterDetail);

            Assert.AreEqual(result, expectedResult);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryTableWeeklyColumnHeaders()
        {
            var engine = new InventoryExportEngine();
            var dates = new List<DateTime>
            {
                new DateTime(2020, 04, 06),
                new DateTime(2020, 04, 13),
                new DateTime(2020, 04, 20),
                new DateTime(2020, 04, 27),
                new DateTime(2020, 05, 04),
            };

            var result = engine.GetInventoryTableWeeklyColumnHeaders(dates);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        public InventoryExportLineDetail _GetInventoryExportLineDetail(int inventoryId, int? stationId, int daypartId, 
            string inventoryProgramNameSeed, string programNameSeed, ProgramSourceEnum? programSource, int? maestroGenreId, 
            List<InventoryExportAudienceDto> providedAudienceImpressions, List<InventoryExportLineWeekDetail> weeks)
        {
            var detail = new InventoryExportLineDetail
            {
                InventoryId = inventoryId,
                StationId = stationId,
                DaypartId = daypartId,
                InventoryProgramName = string.IsNullOrWhiteSpace(inventoryProgramNameSeed) ?  null : $"InventoryProgramName-{inventoryProgramNameSeed}",
                ProgramName = string.IsNullOrWhiteSpace(programNameSeed) ? null : $"ProgramName-{programNameSeed}",
                ProgramSource = programSource,
                MaestroGenreId = maestroGenreId,
                AvgSpotCost = 10,
                AvgHhImpressions = 5000,
                AvgCpm = 2,
                ProvidedAudienceImpressions = providedAudienceImpressions,
                Weeks = weeks
            };

            return detail;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryTableData()
        {
            var engine = new InventoryExportEngine();
            var testWeeks = new List<InventoryExportLineWeekDetail>
            {
                new InventoryExportLineWeekDetail {MediaWeekId = 1, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 2, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 3, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 4, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 5, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
            };
            var testWeeksSansLast = testWeeks.Where(w => w.MediaWeekId != 5).ToList();
            var testWeeksSansFirst = testWeeks.Where(w => w.MediaWeekId != 1).ToList();
            var testWeeksSansMiddle = testWeeks.Where(w => w.MediaWeekId != 3).ToList();
            var testWeekIds = testWeeks.Select(s => s.MediaWeekId).ToList();

            var oneAudience = new List<InventoryExportAudienceDto>
            {
                new InventoryExportAudienceDto  { AudienceId = 31, Impressions = 6000 }
            };
            var manyAudiences = new List<InventoryExportAudienceDto>
            {
                new InventoryExportAudienceDto  { AudienceId = 31, Impressions = 3000 },
                new InventoryExportAudienceDto  { AudienceId = 40, Impressions = 5000 },
                new InventoryExportAudienceDto  { AudienceId = 66, Impressions = 8000 }
            };

            var testItems = new List<InventoryExportLineDetail>();
            // full and mapped
            testItems.Add(_GetInventoryExportLineDetail(1, 1, 1, "one", "one", ProgramSourceEnum.Mapped, 33, manyAudiences, testWeeks));
            // full and enriched
            testItems.Add(_GetInventoryExportLineDetail(2, 2, 1, "two", "two", ProgramSourceEnum.Forecasted, 33, manyAudiences, testWeeks));
            // full and not mapped
            testItems.Add(_GetInventoryExportLineDetail(3, 3, 1, "three", null, null, null, manyAudiences, testWeeks));
            // less audiences
            testItems.Add(_GetInventoryExportLineDetail(4, 4, 1, "four", "four", ProgramSourceEnum.Mapped, 33, oneAudience, testWeeks));
            // less weeks
            testItems.Add(_GetInventoryExportLineDetail(5, 5, 1, "five", "five", ProgramSourceEnum.Mapped, 33, manyAudiences, testWeeksSansLast));
            testItems.Add(_GetInventoryExportLineDetail(6, 6, 1, "six", "six", ProgramSourceEnum.Mapped, 33, manyAudiences, testWeeksSansFirst));
            testItems.Add(_GetInventoryExportLineDetail(7, 7, 1, "seven", "seven", ProgramSourceEnum.Mapped, 33, manyAudiences, testWeeksSansMiddle));
            // Market has comma
            testItems.Add(_GetInventoryExportLineDetail(8, 8, 1, "eight", "eight", ProgramSourceEnum.Mapped, 33, manyAudiences, testWeeks));
            // Market has quote
            testItems.Add(_GetInventoryExportLineDetail(9, 9, 1, "nine", "nine", ProgramSourceEnum.Mapped, 33, manyAudiences, testWeeks));
            // daypart has comma
            testItems.Add(_GetInventoryExportLineDetail(10, 10, 2, "ten", "ten", ProgramSourceEnum.Mapped, 33, manyAudiences, testWeeks));

            var stations = Enumerable.Range(1, 11).Select(i => new DisplayBroadcastStation {Id = i, LegacyCallLetters = $"Station{i}", MarketCode = i, Affiliation = $"AFF{i}"}).ToList();

            var markets = new List<MarketCoverage>
            {
                new MarketCoverage { MarketCode = 1, Market = "MyMarket1", Rank = 21},
                new MarketCoverage { MarketCode = 2, Market = "MyMarket2", Rank = 22},
                new MarketCoverage { MarketCode = 3, Market = "MyMarket3", Rank = 23},
                new MarketCoverage { MarketCode = 4, Market = "MyMarket4", Rank = 24},
                new MarketCoverage { MarketCode = 5, Market = "MyMarket5", Rank = 25},
                new MarketCoverage { MarketCode = 6, Market = "MyMarket6", Rank = 26},
                new MarketCoverage { MarketCode = 7, Market = "MyMarket7", Rank = 27},
                new MarketCoverage { MarketCode = 8, Market = "My,Market8", Rank = 28},
                new MarketCoverage { MarketCode = 9, Market = "My\"Market9", Rank = 29},
                new MarketCoverage { MarketCode = 10, Market = "MyMarket10", Rank = 30},
                new MarketCoverage { MarketCode = 11, Market = "MyMarket11", Rank = 31},
            };

            var audiences = new List<LookupDto>
            {
                new LookupDto(31, "Audience 31"),
                new LookupDto(40, "Audience 40"),
                new LookupDto(66, "Audience 66")
            };

            var genres = new List<LookupDto> {new LookupDto(33, "News")};

            var dayparts = new Dictionary<int, DisplayDaypart>();
            dayparts[1] = new DisplayDaypart(1, 3600, 7200, true, true, true, true, true, true, true);
            dayparts[2] = new DisplayDaypart(2, 18000, 21600, true, true, true, false, false, true, true);

            var result = engine.GetInventoryTableData(testItems, stations, markets, testWeekIds, dayparts, audiences, genres);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryTableData_UnknownStation()
        {
            var engine = new InventoryExportEngine();
            var testWeeks = new List<InventoryExportLineWeekDetail>
            {
                new InventoryExportLineWeekDetail {MediaWeekId = 1, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 2, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 3, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 4, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 5, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
            };
            var testWeekIds = testWeeks.Select(s => s.MediaWeekId).ToList();
            var testAudiences = new List<InventoryExportAudienceDto>
            {
                new InventoryExportAudienceDto  { AudienceId = 31, Impressions = 3000 },
                new InventoryExportAudienceDto  { AudienceId = 40, Impressions = 5000 },
                new InventoryExportAudienceDto  { AudienceId = 66, Impressions = 8000 }
            };

            var testItems = new List<InventoryExportLineDetail>();
            testItems.Add(_GetInventoryExportLineDetail(1, 1, 1, "one", "one", ProgramSourceEnum.Mapped, 33, testAudiences, testWeeks));
            // Unknown Station
            testItems.Add(_GetInventoryExportLineDetail(2, 4, 1, "two", "two", ProgramSourceEnum.Mapped, 33, testAudiences, testWeeks));
            // Unknown Market
            testItems.Add(_GetInventoryExportLineDetail(3, 5, 1, "three", "three", ProgramSourceEnum.Mapped, 33, testAudiences, testWeeks));

            var stations = new List<DisplayBroadcastStation>
            {
                new DisplayBroadcastStation { Id = 1, LegacyCallLetters = "Station1", MarketCode = 1, Affiliation = "Aff1" },
                new DisplayBroadcastStation { Id = 2, LegacyCallLetters = "Station2", MarketCode = 2, Affiliation = "Aff2" },
                new DisplayBroadcastStation { Id = 3, LegacyCallLetters = "Station3", MarketCode = 3, Affiliation = "Aff3" },
                new DisplayBroadcastStation { Id = 5, LegacyCallLetters = "StationFive", MarketCode = 5, Affiliation = "Aff5" }
            };

            var markets = new List<MarketCoverage>
            {
                new MarketCoverage { MarketCode = 1, Market = "MyMarket1", Rank = 21},
                new MarketCoverage { MarketCode = 2, Market = "MyMarket2", Rank = 22},
                new MarketCoverage { MarketCode = 3, Market = "MyMarket3", Rank = 23},
            };

            var audiences = new List<LookupDto>
            {
                new LookupDto(31, "Audience 31"),
                new LookupDto(40, "Audience 40"),
                new LookupDto(66, "Audience 66")
            };

            var genres = new List<LookupDto> { new LookupDto(33, "News") };

            var dayparts = new Dictionary<int, DisplayDaypart>();
            dayparts[1] = new DisplayDaypart(1, 3600, 7200, true, true, true, true, true, true, true);
            dayparts[2] = new DisplayDaypart(2, 18000, 21600, true, true, true, false, false, true, true);

            var result = engine.GetInventoryTableData(testItems, stations, markets, testWeekIds, dayparts, audiences, genres);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryTableData_UnknownDaypart()
        {
            var engine = new InventoryExportEngine();
            var testWeeks = new List<InventoryExportLineWeekDetail>
            {
                new InventoryExportLineWeekDetail {MediaWeekId = 1, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 2, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 3, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 4, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
                new InventoryExportLineWeekDetail {MediaWeekId = 5, SpotCost = 10, HhImpressions = 5000, Cpm = 2},
            };
            var testWeekIds = testWeeks.Select(s => s.MediaWeekId).ToList();
            var testAudiences = new List<InventoryExportAudienceDto>
            {
                new InventoryExportAudienceDto  { AudienceId = 31, Impressions = 3000 },
                new InventoryExportAudienceDto  { AudienceId = 40, Impressions = 5000 },
                new InventoryExportAudienceDto  { AudienceId = 66, Impressions = 8000 }
            };

            var testItems = new List<InventoryExportLineDetail>();
            testItems.Add(_GetInventoryExportLineDetail(1, 1, 1, "one", "one", ProgramSourceEnum.Mapped, 33, testAudiences, testWeeks));
            // unknown daypart
            testItems.Add(_GetInventoryExportLineDetail(2, 1, 666, "two", "two", ProgramSourceEnum.Mapped, 33, testAudiences, testWeeks));

            var stations = new List<DisplayBroadcastStation>
            {
                new DisplayBroadcastStation { Id = 1, LegacyCallLetters = "Station1", MarketCode = 1, Affiliation = "Aff1" },
                new DisplayBroadcastStation { Id = 2, LegacyCallLetters = "Station2", MarketCode = 2, Affiliation = "Aff2" },
                new DisplayBroadcastStation { Id = 3, LegacyCallLetters = "Station3", MarketCode = 3, Affiliation = "Aff3" },
            };

            var markets = new List<MarketCoverage>
            {
                new MarketCoverage { MarketCode = 1, Market = "MyMarket1", Rank = 21},
                new MarketCoverage { MarketCode = 2, Market = "MyMarket2", Rank = 22},
                new MarketCoverage { MarketCode = 3, Market = "MyMarket3", Rank = 23},
            };

            var audiences = new List<LookupDto>
            {
                new LookupDto(31, "Audience 31"),
                new LookupDto(40, "Audience 40"),
                new LookupDto(66, "Audience 66")
            };

            var genres = new List<LookupDto> { new LookupDto(33, "News") };

            var dayparts = new Dictionary<int, DisplayDaypart>();
            dayparts[1] = new DisplayDaypart(1, 3600, 7200, true, true, true, true, true, true, true);
            dayparts[2] = new DisplayDaypart(2, 18000, 21600, true, true, true, false, false, true, true);

            var result = engine.GetInventoryTableData(testItems, stations, markets, testWeekIds, dayparts, audiences, genres);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetExportGeneratedTimestamp()
        {
            var engine = new InventoryExportEngine();

            var result = engine.GetExportGeneratedTimestamp(new DateTime(2020, 3, 5, 13, 20, 30));

            Assert.AreEqual("Generated : 03/05/2020 13:20:30", result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryTableAudienceColumnHeaders()
        {
            var engine = new InventoryExportEngine();

            var audiences = new List<LookupDto>() { 
                new LookupDto(1, "A18+"),
                new LookupDto(2, "W18+")
            };

            var result = engine.GetInventoryTableAudienceColumnHeaders(audiences);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private InventoryExportDto _GetInventoryExportDto(int inventoryId, int mediaWeekId, int stationId, int daypartId,
            int hhImpressions, int spotCost, string programNameSeed, string inventoryProgramNameSeed,
            ProgramSourceEnum? programSource, int? maestroGenreId)
        {
            const int hhAudienceId = 31;
            var programName = string.IsNullOrWhiteSpace(programNameSeed) ? null : $"ProgramName{programNameSeed}";
            var inventoryProgramName = string.IsNullOrWhiteSpace(inventoryProgramNameSeed) ? null : $"InventoryProgramName{inventoryProgramNameSeed}";

            var item = new InventoryExportDto()
            {
                InventoryId = inventoryId,
                StationId = stationId,
                MediaWeekId = mediaWeekId,
                DaypartId = daypartId,
                SpotCost = spotCost,
                ProgramName = programName,
                InventoryProgramName = inventoryProgramName,
                ProgramSource = programSource,
                MaestroGenreId = maestroGenreId,
                HhImpressionsProjected = hhImpressions,
                ProvidedAudiences = new List<InventoryExportAudienceDto>
                {
                    new InventoryExportAudienceDto {AudienceId = hhAudienceId, Impressions = hhImpressions}
                }
            };

            return item;
        }
    }
}