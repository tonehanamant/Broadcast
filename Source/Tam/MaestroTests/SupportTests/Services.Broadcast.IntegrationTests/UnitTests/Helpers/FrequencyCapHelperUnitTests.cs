using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [Category("short_running")]
    public class FrequencyCapHelperUnitTests
    {
        [Test]
        [TestCase(UnitCapEnum.Per30Min, "hour", 0.5)]
        [TestCase(UnitCapEnum.PerHour, "hour", 1)]
        [TestCase(UnitCapEnum.PerDay, "day", 1)]
        [TestCase(UnitCapEnum.PerWeek, "week", 1)]
        public void GetFrequencyCapTimeAndCapTypeString(UnitCapEnum unitCap, string expectedCapType, double expectedCapTime)
        {
            var (actualCapTime, actualCapType) = FrequencyCapHelper.GetFrequencyCapTimeAndCapTypeString(unitCap);

            Assert.AreEqual(expectedCapTime, actualCapTime);
            Assert.AreEqual(expectedCapType, actualCapType);
        }

        [Test]
        [TestCase(UnitCapEnum.Per30Min, 3, 6)]
        [TestCase(UnitCapEnum.PerHour, 3, 3)]
        public void GetFrequencyCap(UnitCapEnum unitCap, int unitCaps, int expectedFrequencyCap)
        {
            var actualFrequencyCap = FrequencyCapHelper.GetFrequencyCap(unitCap, unitCaps);

            Assert.AreEqual(expectedFrequencyCap, actualFrequencyCap);
        }

        [Test]
        public void FrequencyCapHelper_ThrowsAnError_OnUnsupportedUnitCapEnum()
        {
            var exception = Assert.Throws<Exception>(() => FrequencyCapHelper.GetFrequencyCap(UnitCapEnum.PerMonth, 5));
            Assert.That(exception.Message, Is.EqualTo("Unsupported unit cap type was discovered"));
        }
    }
}
