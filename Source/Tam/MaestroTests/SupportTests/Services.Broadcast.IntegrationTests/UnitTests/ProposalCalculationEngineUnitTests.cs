using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class ProposalCalculationEngineUnitTests
    {
        private readonly IProposalService _proposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();

        [Test]
        public void SetProposalTargetsTest()
        {
            var proposalDto = _proposalService.GetProposalById(250);
            
            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(5999409.21m, proposalDto.TargetBudget);
            Assert.AreEqual(27089, proposalDto.TargetImpressions);
            Assert.AreEqual(4, proposalDto.TargetUnits);
        }

        [Test]
        public void UpdateQuarterTotalsTest()
        {
            var proposalDto = _proposalService.GetProposalById(250);
            var detailDto = proposalDto.Details.First();

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(5241609.21m, detailDto.TotalCost);
            Assert.AreEqual(18089, detailDto.TotalImpressions);
            Assert.AreEqual(3, detailDto.TotalUnits);
        }

        [Test]
        public void UpdateProposalWeekValuesTest()
        {
            var proposalDto = _proposalService.GetProposalById(250);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var weekDto = quarterDto.Weeks.First();
            
            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(1, weekDto.Units);
            Assert.AreEqual(1843.5d, weekDto.Impressions);
            Assert.AreEqual(23099.06m, weekDto.Cost);
        }

        [Test]
        public void UpdateQuarterImpressionGoalTest()
        {
            var proposalDto = _proposalService.GetProposalById(250);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(5500, quarterDto.ImpressionGoal);
        }

        [Test]
        public void UpdateProposalWithAduTest()
        {
            var proposalDto = _proposalService.GetProposalById(250);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var weekDto = quarterDto.Weeks.First();
            
            detailDto.Adu = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(0, quarterDto.Cpm);
            Assert.AreEqual(0, detailDto.TotalCost);
            Assert.AreEqual(0, weekDto.Cost);
        }

        [Test]
        public void DistributeImpressionsTest()
        {
            var proposalDto = _proposalService.GetProposalById(251);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var firstWeekDto = quarterDto.Weeks.First();
            var secondWeekDto = quarterDto.Weeks[1];
            var lastWeekDto = quarterDto.Weeks.Last();
            
            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(3.333d, firstWeekDto.Impressions);
            Assert.AreEqual(3.333d, secondWeekDto.Impressions);
            Assert.AreEqual(3.334d, lastWeekDto.Impressions);
        }

        [Test]
        public void UpdateProposalCostCalculationTest()
        {
            var proposalDto = _proposalService.GetProposalById(251);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var firstWeekDto = quarterDto.Weeks.First();
            var secondWeekDto = quarterDto.Weeks[1];
            var lastWeekDto = quarterDto.Weeks.Last();

            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(41.76m, firstWeekDto.Cost);
            Assert.AreEqual(41.76m, secondWeekDto.Cost);
            Assert.AreEqual(41.78m, lastWeekDto.Cost);
        }


        [Test]
        public void DistributeImpressionsToLastNonHiatusWeekTest()
        {
            var proposalDto = _proposalService.GetProposalById(251);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var firstWeekDto = quarterDto.Weeks.First();
            var secondWeekDto = quarterDto.Weeks[1];
            var lastWeekDto = quarterDto.Weeks.Last();

            quarterDto.ImpressionGoal = 3333.333f;
            lastWeekDto.IsHiatus = true;
            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(1666.666d, firstWeekDto.Impressions);
            Assert.AreEqual(1666.667d, secondWeekDto.Impressions);
            Assert.AreEqual(0, lastWeekDto.Impressions);
        }
    }
}
