using ApprovalTests;
using ApprovalTests.Reporters;
using Castle.Components.DictionaryAdapter;
using Common.Services;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;
using Services.Broadcast.IntegrationTests.TestData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests.Converters.Scx
{
    [TestFixture]
    public class PlanBuyingScxDataConverterUnitTests
    {
        private static DateTime _CurrentDateTime = new DateTime(2017, 10, 17, 19, 30, 12);
        private Mock<IDaypartCache> _DaypartCache;
        private Mock<IDateTimeEngine> _DateTimeEngine;

        [SetUp]
        public void Setup()
        {
            _DaypartCache = new Mock<IDaypartCache>();
            _DaypartCache.Setup(s => s.GetDisplayDaypart(It.IsAny<int>()))
                .Returns<int>(DaypartsTestData.GetDisplayDaypart);

            _DateTimeEngine = new Mock<IDateTimeEngine>();
            _DateTimeEngine.Setup(s => s.GetCurrentMoment())
                .Returns(_CurrentDateTime);
        }

        private PlanBuyingScxDataConverter _GetTestClass()
        {
            var item = new PlanBuyingScxDataConverter(_DaypartCache.Object, _DateTimeEngine.Object);
            return item;
        }

        [Test]
        public void Constructor()
        {
            var tc = _GetTestClass();

            Assert.IsNotNull(tc);
        }

        /// <summary>
        /// This exercises the current class's functionality only.
        /// It creates a simple data package limited to the needs of this unit.
        /// </summary>
        [Test]
        public void ConvertData()
        {
            const string planName = "MyTestPlan";
            var generated = _CurrentDateTime;
            const string expectedFileName = "PlanBuying_MyTestPlan_20171017_193012.scx";

            var toConvert = new PlanScxData
            {
                PlanName = planName,
                Generated = generated,
                Demos = new List<DemoData>(),
                Orders = new List<OrderData>()
            };

            var tc = _GetTestClass();

            var result = tc.ConvertData(toConvert);

            Assert.IsNotNull(result);
            // verify the stream
            Assert.IsNotNull(result.ScxStream);
            Assert.IsTrue(result.ScxStream.Length > 0);
            Assert.AreEqual(0, result.ScxStream.Position);
            // verify the other properties
            Assert.AreEqual(planName, result.PlanName);
            Assert.AreEqual(generated, result.GeneratedTimeStamp);
            Assert.AreEqual(result.FileName, expectedFileName);
        }

        /// <summary>
        /// This exercises the parent class methods that this class depends.
        /// It creates a complex data package.
        /// </summary>
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ConvertDataFullPlanScxData()
        {
            // Arrange
            const string planName = "MyTestPlan";
            var generated = _CurrentDateTime;

            var mediaWeek = MediaMonthAndWeekTestData.GetMediaWeek(871);
            var audience = AudienceTestData.GetBroadcastAudienceById(31);
            audience.RangeStart = 0;
            audience.RangeEnd = 99;

            var toConvert = new PlanScxData
            {
                PlanName = planName,
                Generated = generated,
                AllSortedMediaWeeks = new List<MediaWeek> {mediaWeek}.OrderBy(s => s.Id),
                Demos = new List<DemoData> {new DemoData {DemoRank = 1, Demo = audience}},
                StartDate = new DateTime(2020, 08, 31).Date,
                EndDate = new DateTime(2020, 09, 06).Date,
                Orders = new List<OrderData>
                {
                    new OrderData
                    {
                        InventoryMarkets = new List<ScxMarketDto>
                        {
                            new ScxMarketDto
                            {
                                DmaMarketName = "New York",
                                MarketId = 101,
                                Stations = new List<ScxMarketDto.ScxStation>
                                {
                                    new ScxMarketDto.ScxStation
                                    {
                                        LegacyCallLetters = "KSTP",
                                        Programs = new EditableList<ScxMarketDto.ScxStation.ScxProgram>
                                        {
                                            new ScxMarketDto.ScxStation.ScxProgram
                                            {
                                                DaypartId = 59803,
                                                DemoValues = new List<ScxMarketDto.ScxStation.ScxProgram.DemoValue>
                                                {
                                                    new ScxMarketDto.ScxStation.ScxProgram.DemoValue
                                                    {
                                                        DemoRank = 1,
                                                        Impressions = 2400
                                                    }
                                                },
                                                ProgramAssignedDaypartCode = "EMN",
                                                ProgramName = "MyTestProgram",
                                                SpotCost = 25m,
                                                SpotLength = "30",
                                                TotalCost = 75m,
                                                TotalSpots = 3,
                                                Weeks = new List<ScxMarketDto.ScxStation.ScxProgram.ScxWeek>
                                                {
                                                    new ScxMarketDto.ScxStation.ScxProgram.ScxWeek {Spots = 3, MediaWeek = mediaWeek}
                                                }
                                            },
                                            new ScxMarketDto.ScxStation.ScxProgram
                                            {
                                                DaypartId = 59803,
                                                DemoValues = new List<ScxMarketDto.ScxStation.ScxProgram.DemoValue>
                                                {
                                                    new ScxMarketDto.ScxStation.ScxProgram.DemoValue
                                                    {
                                                        DemoRank = 1,
                                                        Impressions = 3500
                                                    }
                                                },
                                                ProgramAssignedDaypartCode = "EMN",
                                                ProgramName = "MyTestProgram",
                                                SpotCost = 37.5m,
                                                SpotLength = "60",
                                                TotalCost = 0m,
                                                TotalSpots = 0,
                                                Weeks = new List<ScxMarketDto.ScxStation.ScxProgram.ScxWeek>
                                                {
                                                    new ScxMarketDto.ScxStation.ScxProgram.ScxWeek {Spots = 0, MediaWeek = mediaWeek}
                                                }
                                            }
                                        },
                                        StationCode = 102,
                                        TotalCost = 75m,
                                        TotalSpots = 3
                                    }
                                },
                                TotalCost = 75m,
                                TotalSpots = 3
                            }
                        },
                        SurveyString = "Jan20 DMA Nielsen Live+3",
                        TotalCost = 75m,
                        TotalSpots = 3
                    }
                }
            };

            var tc = _GetTestClass();

            // Act
            var result = tc.ConvertData(toConvert);

            // Assert
            // due to strangeness like 'xml namespaces come out different orders on different machines' 
            //  we will hack this by deserializing the xml to an object, then serializing the object to json for comparison
            adx resultContentItem;
            using (var reader = new StreamReader(result.ScxStream))
            {
                var serializer = new XmlSerializer(typeof(adx));
                resultContentItem = (adx)serializer.Deserialize(reader);
            }

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultContentItem));
        }
    }
}