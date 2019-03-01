using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines.InventoryDaypartParsing;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    class InventoryFileImporterBaseTests : InventoryFileImporterBase
    {
        public InventoryFileImporterBaseTests()
        {
            InventoryDaypartParsingEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryDaypartParsingEngine>();
        }

        public override void ExtractFileData(Stream stream, InventoryFile inventoryFile, DateTime effectiveDate)
        {
        }

        [TestCase("M-T-A 10AM-11AM")]
        [TestCase("5:30A-6P:Monday-Friday")]
        [TestCase("MON-SEX 9P-10:11PM")]
        public void DoesNotReturnDaypartsWhenDaypartIsInvalid(string value)
        {
            var dayparts = ParseDayparts(value, "WWTV");
            Assert.True(!dayparts.Any());
        }

        [TestCase("M-T 10AM-11AM")]
        [TestCase("MON 10AM-11AM")]
        [TestCase("MO-FR 12AM-12PM")]
        public void DaypartIsValid(string value)
        {
            var dayparts = ParseDayparts(value, "WWTV");
            Assert.True(dayparts.Any());
        }
    }
}
