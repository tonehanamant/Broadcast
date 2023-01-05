using NUnit.Framework;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class DateTimeHelperUnitTests
    {
        [Test]
        [TestCase(SpotExceptionsConstants.DateFormat, "08/09/2022")]
        [TestCase(SpotExceptionsConstants.TimeFormat, "04:08:05")]
        [TestCase(SpotExceptionsConstants.DateTimeFormat, "08/09/2022 04:08:05")]
        public void GetForDisplay(string dateType, string formattedDate)
        {
            // Setup
            var dateTime = new DateTime(2022, 8, 9, 4, 8, 5);

            // Act
            var result = DateTimeHelper.GetForDisplay(dateTime, dateType);

            // Assert
            Assert.AreEqual(result, formattedDate);
        }
    }
}
