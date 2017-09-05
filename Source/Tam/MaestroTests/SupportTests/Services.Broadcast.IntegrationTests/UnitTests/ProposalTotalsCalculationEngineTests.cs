using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    public class ProposalTotalsCalculationEngineTests
    {
        [Test]
        public void CalculateProposalMarginsTest()
        {
            var proposalDto = new ProposalDto
            {
                TotalImpressions = 5000d,
                TotalCost = 10000m,
                TotalCPM = 10000 / 5000,
                TargetImpressions = 10000d,
                TargetBudget = 10000m,
                TargetCPM = 10000 / 10000
            };

            var proposalCalculationEngine = new ProposalTotalsCalculationEngine(new ProposalMathEngine());

            proposalCalculationEngine.CalculateProposalTotalsMargins(proposalDto);

            Assert.IsTrue(proposalDto.TotalCPMMarginAchieved);
            Assert.AreEqual(240.0d, proposalDto.TotalCPMPercent);
            Assert.IsTrue(proposalDto.TotalCostMarginAchieved);
            Assert.AreEqual(120.0d, proposalDto.TotalCostPercent);
            Assert.IsFalse(proposalDto.TotalImpressionsMarginAchieved);
            Assert.AreEqual(50.0d, proposalDto.TotalImpressionsPercent);
        }

        [Test]
        public void CalculateProposalMarginsAllMarginsAchievedTest()
        {
            var proposalDto = new ProposalDto
            {
                TotalImpressions = 5000d,
                TotalCost = 10000m,
                TotalCPM = 10000 / 5000,
                TargetImpressions = 5000d,
                TargetBudget = 10000m,
                TargetCPM = 10000 / 5000
            };

            var proposalCalculationEngine = new ProposalTotalsCalculationEngine(new ProposalMathEngine());

            proposalCalculationEngine.CalculateProposalTotalsMargins(proposalDto);

            Assert.IsTrue(proposalDto.TotalCPMMarginAchieved);
            Assert.AreEqual(120.0d, proposalDto.TotalCPMPercent);
            Assert.IsTrue(proposalDto.TotalCostMarginAchieved);
            Assert.AreEqual(120.0d, proposalDto.TotalCostPercent);
            Assert.IsFalse(proposalDto.TotalImpressionsMarginAchieved);
            Assert.AreEqual(100.0d, proposalDto.TotalImpressionsPercent);
        }

        [Test]
        public void CalculateProposalMarginsTargetsNullNoExceptionTest()
        {
            var proposalDto = new ProposalDto
            {
                TotalImpressions = 5000d,
                TotalCost = 10000m,
                TotalCPM = 10000 / 5000
            };

            var proposalCalculationEngine = new ProposalTotalsCalculationEngine(new ProposalMathEngine());

            proposalCalculationEngine.CalculateProposalTotalsMargins(proposalDto);

            Assert.IsFalse(proposalDto.TotalCPMMarginAchieved);
            Assert.AreEqual(0, proposalDto.TotalCPMPercent);
            Assert.IsFalse(proposalDto.TotalCostMarginAchieved);
            Assert.AreEqual(0, proposalDto.TotalCostPercent);
            Assert.IsFalse(proposalDto.TotalImpressionsMarginAchieved);
            Assert.AreEqual(0, proposalDto.TotalImpressionsPercent);
        }

        [Test]
        public void CalculateProposalMarginsTargetsAllValuesZeroOrNullNoExceptionTest()
        {
            var proposalDto = new ProposalDto();

            var proposalCalculationEngine = new ProposalTotalsCalculationEngine(new ProposalMathEngine());

            proposalCalculationEngine.CalculateProposalTotalsMargins(proposalDto);

            Assert.IsFalse(proposalDto.TotalCPMMarginAchieved);
            Assert.AreEqual(0, proposalDto.TotalCPMPercent);
            Assert.IsFalse(proposalDto.TotalCostMarginAchieved);
            Assert.AreEqual(0, proposalDto.TotalCostPercent);
            Assert.IsFalse(proposalDto.TotalImpressionsMarginAchieved);
            Assert.AreEqual(0, proposalDto.TotalImpressionsPercent);
        }
    }
}
