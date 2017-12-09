using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Services.Broadcast.Converters.RateImport;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    class InventoryFileImporterBaseTests : InventoryFileImporterBase
    {
        public override InventorySource InventorySource { get; set; }
        
        public override void ExtractFileData(Stream stream, InventoryFile inventoryFile, DateTime effectiveDate, List<InventoryFileProblem> fileProblems)
        {
        }

        [TestCase("M-T-A 10AM-11AM")]
        [TestCase("3A-4A:M-F")]
        [TestCase("9A-10A:Su")]
        [TestCase("5:30A-6P:Monday-Friday")]
        [TestCase("MON-SEX 9P-10:11PM")]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Invalid daypart", MatchType = MessageMatch.Contains)]
        public void ThrowsExceptionWhenDaypartIsInvalid(string value)
        {
            ParseStringToDaypart(value, "WWTV");
        }

        [TestCase("M-T 10AM-11AM")]
        [TestCase("MON 10AM-11AM")]
        [TestCase("MO-FR 12AM-12PM")]
        public void DaypartIsValid(string value)
        {
            Assert.IsNotNull(ParseStringToDaypart(value, "WWTV"));
        }
    }
}
