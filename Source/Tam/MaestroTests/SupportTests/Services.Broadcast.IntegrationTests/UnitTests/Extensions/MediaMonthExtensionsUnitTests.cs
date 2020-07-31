using NUnit.Framework;
using Services.Broadcast.Extensions;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class MediaMonthExtensionsUnitTests
    {
        [Test]
        public void GetShortMonthNameAndYear()
        {
            var result = MediaMonthExtensions.GetShortMonthNameAndYear(_GetMediaMonth());

            Assert.AreEqual("Jan 2020", result);
        }

        [Test]
        [TestCase(1, "0120")]
        [TestCase(11, "1120")]
        public void GetCompactMonthNameAndYear(int month, string expected)
        {
            var mediaMonth = _GetMediaMonth();
            mediaMonth.Month = month;

            var result = MediaMonthExtensions.GetCompactMonthNameAndYear(mediaMonth);

            Assert.AreEqual(expected, result);
        }

        private MediaMonth _GetMediaMonth() =>
            new MediaMonth { Month = 1, Year = 2020 };
    }
}
