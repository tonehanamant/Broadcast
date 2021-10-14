using NUnit.Framework;
using Services.Broadcast.Entities.Plan;

namespace Services.Broadcast.IntegrationTests.UnitTests.EntitiesUnitTests
{
    [TestFixture]
    public class WeeklyBreakdownWeekUnitTests
    {
        [Test]
        [TestCase(0,1000, 0.0)]
        [TestCase(1000, 0, 0.0)]
        [TestCase(2760, 460, 6)]
        [TestCase(3000, 500, 6)]
        [TestCase(38450.00, 3076, 12.5)]
        public void WeeklyBreakdownWeek_WeeklyCpm(decimal weeklyBudget, double weeklyImpressions, decimal expectedWeeklyCpm)
        {
            var testWeek = new WeeklyBreakdownWeek
            {
                WeeklyBudget = weeklyBudget,
                // WeeklyImpressions is formatted to (000)
                WeeklyImpressions = weeklyImpressions
            };

            var result = testWeek.WeeklyCpm;

            Assert.AreEqual(expectedWeeklyCpm, result);
        }
    }
}