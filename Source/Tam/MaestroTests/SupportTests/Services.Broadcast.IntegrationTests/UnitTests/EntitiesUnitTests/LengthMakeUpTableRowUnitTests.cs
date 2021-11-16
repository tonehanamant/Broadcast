using NUnit.Framework;
using Services.Broadcast.Entities.Plan;

namespace Services.Broadcast.IntegrationTests.UnitTests.EntitiesUnitTests
{
    class LengthMakeUpTableRowUnitTests
    {
        [Test]
        [TestCase(0, 1, 0.0)]
        [TestCase(1000, 0, 0.0)]
        [TestCase(2760, 460, 6)]
        [TestCase(3000, 500, 6)]
        [TestCase(38450.00, 3076, 12.5)]
        public void LengthMakeUpTableRow_WeeklyCpm(decimal budget, double impressions, decimal expectedCpm)
        {
            var testWeek = new LengthMakeUpTableRow
            {
                Budget = budget,
                Impressions = impressions
            };

            var result = testWeek.Cpm;

            Assert.AreEqual(expectedCpm, result);
        }
    }
}
