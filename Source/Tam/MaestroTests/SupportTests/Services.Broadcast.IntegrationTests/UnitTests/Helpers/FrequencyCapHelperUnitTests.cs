using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;

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
    }
}
