using ApprovalTests;
using ApprovalTests.Reporters;
using Castle.Components.DictionaryAdapter;
using Common.Services;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities.Enums;
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
        [TestCase(SpotAllocationModelMode.Quality)]
        [TestCase(SpotAllocationModelMode.Efficiency)]
        [TestCase(SpotAllocationModelMode.Floor)]
        public void ConvertData(SpotAllocationModelMode spotAllocationModelMode)
        {
            const string FILENAME_TIMESTAMP_FORMAT = "yyyyMMdd_HHmmss";
            const string planName = "MyTestPlan";
            var generated = _CurrentDateTime;
            var expectedFileName = "PlanBuying_MyTestPlan";
            var toConvert = new PlanScxData
            {
                PlanName = planName,
                Generated = generated,
                Demos = new List<DemoData>(),
                Orders = new List<OrderData>()
            };

            var tc = _GetTestClass();
            expectedFileName = expectedFileName + "_" + toConvert.Generated.ToString(FILENAME_TIMESTAMP_FORMAT) + "_" + spotAllocationModelMode.ToString().Substring(0, 1) + ".scx"; ;
            var result = tc.ConvertData(toConvert, spotAllocationModelMode);

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

       
    }
}