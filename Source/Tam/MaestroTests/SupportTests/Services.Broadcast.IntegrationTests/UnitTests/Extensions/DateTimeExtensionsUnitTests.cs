using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class DateTimeExtensionsUnitTests
    {
        [Test]
        public void ToFileDateFormat()
        {
            var date = new DateTime(2020, 5, 9, 12, 31, 49);

            var result = date.ToFileDateFormat();

            Assert.AreEqual("20200509", result);
        }

        [Test]
        public void ToFileDateTimeFormat()
        {
            var date = new DateTime(2020, 5, 9, 12, 31, 49);

            var result = date.ToFileDateTimeFormat();

            Assert.AreEqual("20200509_123149", result);
        }

        [Test]
        [TestCase(12, BroadcastDayOfWeek.Sunday)]
        [TestCase(13, BroadcastDayOfWeek.Monday)]
        [TestCase(14, BroadcastDayOfWeek.Tuesday)]
        [TestCase(15, BroadcastDayOfWeek.Wednesday)]
        [TestCase(16, BroadcastDayOfWeek.Thursday)]
        [TestCase(17, BroadcastDayOfWeek.Friday)]
        [TestCase(18, BroadcastDayOfWeek.Saturday)]
        public void GetBroadcastDayOfWeek(int day, BroadcastDayOfWeek expected)
        {
            var date = new DateTime(2020, 7, day, 12, 31, 49);

            var result = date.GetBroadcastDayOfWeek();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(12, 6)]
        [TestCase(13, 13)]
        [TestCase(14, 13)]
        [TestCase(15, 13)]
        [TestCase(16, 13)]
        [TestCase(17, 13)]
        [TestCase(18, 13)]
        [TestCase(19, 13)]
        public void GetWeekMonday(int day, int expected)
        {
            var date = new DateTime(2020, 7, day, 12, 31, 49);

            var result = date.GetWeekMonday();

            Assert.AreEqual(expected, result.Day);
        }

        [Test]
        [TestCase(12, 13)]
        [TestCase(13, 13)]
        [TestCase(14, 20)]
        [TestCase(15, 20)]
        [TestCase(16, 20)]
        [TestCase(17, 20)]
        [TestCase(18, 20)]
        [TestCase(19, 20)]
        public void GetNextMonday(int day, int expected)
        {
            var date = new DateTime(2020, 7, day, 12, 31, 49);

            var result = date.GetNextMonday();

            Assert.AreEqual(expected, result.Day);
        }
    }
}
