using System.Collections.Generic;
using NUnit.Framework;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;

namespace Services.Broadcast.IntegrationTests.UnitTests.EntitiesUnitTests
{
    [TestFixture]
    public class PlanDtoUnitTests
    {
        [Test]
        public void GetPricingBudgetCpmLever_NoParameters()
        {
            var plan = new PlanDto();

            var resultBudgetCpmLever = plan.PricingBudgetCpmLever;

            Assert.AreEqual(BudgetCpmLeverEnum.Cpm, resultBudgetCpmLever);
        }

        [Test]
        [TestCase(BudgetCpmLeverEnum.Cpm)]
        [TestCase(BudgetCpmLeverEnum.Budget)]
        public void GetPricingBudgetCpmLever(BudgetCpmLeverEnum testParamVal)
        {
            var plan = new PlanDto
            {
                PricingParameters = new PlanPricingParametersDto
                {
                    BudgetCpmLever = testParamVal
                }
            };

            var resultBudgetCpmLever = plan.PricingBudgetCpmLever;

            Assert.AreEqual(testParamVal, resultBudgetCpmLever);
        }

        [Test]
        public void GetPricingBudgetCpmLever_PlanV2_NoParameters()
        {
            var plan = new PlanDto_v2();

            var resultBudgetCpmLever = plan.PricingBudgetCpmLever;

            Assert.AreEqual(BudgetCpmLeverEnum.Cpm, resultBudgetCpmLever);
        }

        [Test]
        [TestCase(BudgetCpmLeverEnum.Cpm)]
        [TestCase(BudgetCpmLeverEnum.Budget)]
        public void GetPricingBudgetCpmLever_PlanV2(BudgetCpmLeverEnum testParamVal)
        {
            var plan = new PlanDto_v2
            {
                PricingParameters = new List<PlanPricingParametersDto>
                {
                    new PlanPricingParametersDto {BudgetCpmLever = testParamVal, PostingType = PostingTypeEnum.NSI},
                    new PlanPricingParametersDto {BudgetCpmLever = testParamVal, PostingType = PostingTypeEnum.NTI}
                }
            };

            var resultBudgetCpmLever = plan.PricingBudgetCpmLever;

            Assert.AreEqual(testParamVal, resultBudgetCpmLever);
        }

        [Test]
        public void GetBuyingBudgetCpmLever_NoParameters()
        {
            var plan = new PlanDto();

            var resultBudgetCpmLever = plan.BuyingBudgetCpmLever;

            Assert.AreEqual(BudgetCpmLeverEnum.Cpm, resultBudgetCpmLever);
        }

        [Test]
        [TestCase(BudgetCpmLeverEnum.Cpm)]
        [TestCase(BudgetCpmLeverEnum.Budget)]
        public void GetBuyingBudgetCpmLever(BudgetCpmLeverEnum testParamVal)
        {
            var plan = new PlanDto
            {
                BuyingParameters = new PlanBuyingParametersDto
                {
                    BudgetCpmLever = testParamVal
                }
            };

            var resultBudgetCpmLever = plan.BuyingBudgetCpmLever;

            Assert.AreEqual(testParamVal, resultBudgetCpmLever);
        }
    }
}