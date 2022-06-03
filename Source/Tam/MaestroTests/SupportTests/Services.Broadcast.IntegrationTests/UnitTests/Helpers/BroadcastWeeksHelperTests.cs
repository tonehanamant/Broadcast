using NUnit.Framework;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class BroadcastWeeksHelperTests
    {
        const string date_format = "yyyyMMdd";

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void GetContainingBroadcastWeek(int daysFromMonday)
        {
            // Arrange
            var monday = new DateTime(2022, 3, 14);
            var candidate = monday.AddDays(daysFromMonday);

            // Act
            var result = BroadcastWeeksHelper.GetContainingWeek(candidate);

            // Assert
            Assert.AreEqual("20220314", result.WeekStartDate.Date.ToString(date_format));
            Assert.AreEqual("20220320", result.WeekEndDate.Date.ToString(date_format));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void GetContainingBroadcastWeeks(int daysFromMonday)
        {
            // Arrange
            var monday = new DateTime(2022, 3, 14);
            var startDate = monday.AddDays(daysFromMonday);
            var endDate = new DateTime(2022, 04, 7); // Thursday

            // Act
            var result = BroadcastWeeksHelper.GetContainingWeeks(startDate, endDate);

            // Assert
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("20220314", result[0].WeekStartDate.Date.ToString(date_format));
            Assert.AreEqual("20220320", result[0].WeekEndDate.Date.ToString(date_format));
            Assert.AreEqual("20220321", result[1].WeekStartDate.Date.ToString(date_format));
            Assert.AreEqual("20220327", result[1].WeekEndDate.Date.ToString(date_format));
            Assert.AreEqual("20220328", result[2].WeekStartDate.Date.ToString(date_format));
            Assert.AreEqual("20220403", result[2].WeekEndDate.Date.ToString(date_format));
            Assert.AreEqual("20220404", result[3].WeekStartDate.Date.ToString(date_format));
            Assert.AreEqual("20220410", result[3].WeekEndDate.Date.ToString(date_format));
        }
    }
}