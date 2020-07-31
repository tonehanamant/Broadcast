using NUnit.Framework;
using Services.Broadcast.Extensions;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class DisplayDaypartExtensionsUnitTests
    {

        [Test]
        public void GetTotalDurationInSeconds()
        {
            var daypart = new DisplayDaypart(1, 10, 15, true, false, true, false, true, false, false);

            var result = daypart.GetTotalDurationInSeconds();

            Assert.AreEqual(18, result);
        }

        [Test]
        [TestCase(3600, 7199, 1d)]
        [TestCase(7200, 3599, 23d)]
        [TestCase(7200, 7199, 24d)]
        [TestCase(7200, 7200, 0.00027777777777777778d)]
        public void GetOneDayHoursTest(int startTime, int endTime, double expectedHours)
        {
            var daypart = new DisplayDaypart
            {
                Monday = true,
                Tuesday = true,
                StartTime = startTime,
                EndTime = endTime
            };

            Assert.AreEqual(expectedHours, daypart.GetDurationPerDayInHours());
        }
    }
}
