﻿using NUnit.Framework;
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
        [TestCase(SpotAllocationModelMode.Floor, 1, 15000)]
        public void GetPlanBudgetResponseByMode_BudgetLever(SpotAllocationModelMode allocationMode, decimal expectedCpmGoal, double expectedImpressionGoal)
        {
            var budgetCpmLever = BudgetCpmLeverEnum.Budget;

            const decimal weeklyBudget = 15;
            const double weeklyImpressions = 150;

            var result = PlanGoalHelper.PlanCalculateBudgetByMode(weeklyBudget, weeklyImpressions, allocationMode, budgetCpmLever);

            var resultBudget = ProposalMath.CalculateCost(result.CpmGoal, result.ImpressionGoal);

            Assert.AreEqual(expectedCpmGoal, result.CpmGoal);
            Assert.AreEqual(expectedImpressionGoal, result.ImpressionGoal);
            Assert.That(resultBudget, Is.EqualTo(weeklyBudget).Within(budgetDifferenceMargin));
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Efficiency, 16.000, 375)]
        public void GetPlanBudgetResponseByModeForEfficiency_BudgetLever(SpotAllocationModelMode allocationMode, decimal expectedCpmGoal, double impressionGoal)
        {
            var budgetCpmLever = BudgetCpmLeverEnum.Budget;

            const decimal weeklyBudget = 6;
            const double weeklyImpressions = 150;

            var result = PlanGoalHelper.PlanCalculateBudgetByMode(weeklyBudget, weeklyImpressions, allocationMode, budgetCpmLever);

            Assert.AreEqual(expectedCpmGoal, result.CpmGoal);
            Assert.AreEqual(impressionGoal, result.ImpressionGoal);
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Quality, 100)]
        [TestCase(SpotAllocationModelMode.Floor, 1)]
        public void GetPlanBudgetResponseByMode_CpmLever(SpotAllocationModelMode allocationMode, decimal expectedCpmGoal)
        {
            var budgetCpmLever = BudgetCpmLeverEnum.Cpm;
            const double expectedImpressionGoal = 150;

            const decimal weeklyBudget = 15;
            const double weeklyImpressions = 150;

            var result = PlanGoalHelper.PlanCalculateBudgetByMode(weeklyBudget, weeklyImpressions, allocationMode, budgetCpmLever);

            Assert.AreEqual(expectedCpmGoal, result.CpmGoal);
            Assert.AreEqual(expectedImpressionGoal, result.ImpressionGoal);
        }

        [Test]
        [TestCase(SpotAllocationModelMode.Efficiency, 40)]
        public void GetPlanBudgetResponseByMode_CpmLever_ForEfficiency(SpotAllocationModelMode allocationMode, decimal expectedCpmGoal)
        {
            var budgetCpmLever = BudgetCpmLeverEnum.Cpm;
            const double expectedImpressionGoal = 150;

            const decimal weeklyBudget = 15;
            const double weeklyImpressions = 150;

            var result = PlanGoalHelper.PlanCalculateBudgetByMode(weeklyBudget, weeklyImpressions, allocationMode, budgetCpmLever);

            Assert.AreEqual(expectedCpmGoal, result.CpmGoal);
            Assert.AreEqual(expectedImpressionGoal, result.ImpressionGoal);
        }
    }
}
