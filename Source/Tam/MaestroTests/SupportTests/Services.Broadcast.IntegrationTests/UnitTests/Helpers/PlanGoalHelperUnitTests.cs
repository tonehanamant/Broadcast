using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Helpers;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{ 
    public class PlanGoalHelperUnitTests
    {
        private const double budgetDifferenceMargin = .1;

        [Test]
        [TestCase(SpotAllocationModelMode.Quality, 100, 150)]
        [TestCase(SpotAllocationModelMode.Efficiency, 1, 15000)]
        [TestCase(SpotAllocationModelMode.Floor, 1, 15000)]
        public void GetPlanBudgetResponseByMode(SpotAllocationModelMode allocationMode, decimal expectedCpmGoal, double expectedImpressionGoal)
        {
            const decimal weeklyBudget = 15;
            const double weeklyImpressions = 150;

            var result = PlanGoalHelper.PlanCalculateBudgetByMode(weeklyBudget,
                weeklyImpressions, allocationMode);

            var resultBudget = ProposalMath.CalculateCost(result.CpmGoal, result.ImpressionGoal);

            Assert.AreEqual(expectedCpmGoal, result.CpmGoal);
            Assert.AreEqual(expectedImpressionGoal, result.ImpressionGoal);
            Assert.That(resultBudget, Is.EqualTo(weeklyBudget).Within(budgetDifferenceMargin));
        }
    }
}
