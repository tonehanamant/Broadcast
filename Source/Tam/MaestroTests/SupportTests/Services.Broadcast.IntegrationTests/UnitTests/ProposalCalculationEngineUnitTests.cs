using System.Collections.Generic;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using System.Linq;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class ProposalCalculationEngineUnitTests
    {
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();

        [Test]
        public void UpdateProposalWeekValues_Distribute()
        {
            var dto = new ProposalQuarterDto();
            dto.Cpm = 1.25m;
            dto.ImpressionGoal = 123456;
            dto.DistributeGoals = true;
            dto.Weeks = new List<ProposalWeekDto>
            {
                new ProposalWeekDto{IsHiatus = false},
                new ProposalWeekDto{IsHiatus = false},
                new ProposalWeekDto{IsHiatus = false}
            };

            var impressions = dto.ImpressionGoal / dto.Weeks.Count;
            var lastWeekImpressions = impressions + dto.ImpressionGoal % dto.Weeks.Count;

            ProposalCalculationEngine._UpdateProposalWeekValues(dto, false);
            Assert.That(dto.Weeks[0].Impressions, Is.EqualTo(impressions));
            Assert.That(dto.Weeks[1].Impressions, Is.EqualTo(impressions));
            Assert.That(dto.Weeks[2].Impressions, Is.EqualTo(lastWeekImpressions));
            Assert.That(dto.Weeks[0].Cost, Is.EqualTo(51.44m));
            Assert.That(dto.Weeks[1].Cost, Is.EqualTo(51.44m));
            Assert.That(dto.Weeks[2].Cost, Is.EqualTo(51.44m));
        }

        [Test]
        public void UpdateProposalWeekValues_Distribute_Hiatus()
        {
            var dto = new ProposalQuarterDto();
            dto.Cpm = 1.25m;
            dto.ImpressionGoal = 123456;
            dto.DistributeGoals = true;
            dto.Weeks = new List<ProposalWeekDto>
            {
                new ProposalWeekDto{IsHiatus = false},
                new ProposalWeekDto{IsHiatus = false},
                new ProposalWeekDto{IsHiatus = true}
            };

            var impressions = dto.ImpressionGoal / 2;
            var lastWeekImpressions = impressions + dto.ImpressionGoal % 2;

            ProposalCalculationEngine._UpdateProposalWeekValues(dto, false);
            Assert.That(dto.Weeks[0].Impressions, Is.EqualTo(impressions));
            Assert.That(dto.Weeks[1].Impressions, Is.EqualTo(lastWeekImpressions));
            Assert.That(dto.Weeks[2].Impressions, Is.EqualTo(0));
            Assert.That(dto.Weeks[0].Cost, Is.EqualTo(77.16m));
            Assert.That(dto.Weeks[1].Cost, Is.EqualTo(77.16m));
            Assert.That(dto.Weeks[2].Cost, Is.EqualTo(0));

            Assert.That(dto.Weeks[2].Units, Is.EqualTo(0));
        }

        [Test]
        public void UpdateProposalWeekValues_Distribute_ADU()
        {
            var dto = new ProposalQuarterDto();
            dto.Cpm = 1.25m;
            dto.ImpressionGoal = 123456;
            dto.DistributeGoals = true;
            dto.Weeks = new List<ProposalWeekDto>
            {
                new ProposalWeekDto{IsHiatus = false},
                new ProposalWeekDto{IsHiatus = false},
                new ProposalWeekDto{IsHiatus = false}
            };

            var impressions = dto.ImpressionGoal / dto.Weeks.Count;
            var lastWeekImpressions = impressions + dto.ImpressionGoal % dto.Weeks.Count;

            ProposalCalculationEngine._UpdateProposalWeekValues(dto, true);

            Assert.That(dto.Weeks[0].Impressions, Is.EqualTo(impressions));
            Assert.That(dto.Weeks[1].Impressions, Is.EqualTo(impressions));
            Assert.That(dto.Weeks[2].Impressions, Is.EqualTo(lastWeekImpressions));
            Assert.That(dto.Weeks[0].Cost, Is.EqualTo(0));
            Assert.That(dto.Weeks[1].Cost, Is.EqualTo(0));
            Assert.That(dto.Weeks[2].Cost, Is.EqualTo(0));
        }

        [Test]
        public void UpdateProposalWeekValues_Hiatus()
        {
            var dto = new ProposalQuarterDto();
            dto.Cpm = 1.25m;
            dto.ImpressionGoal = 123456;
            dto.DistributeGoals = true;
            dto.Weeks = new List<ProposalWeekDto>
            {
                new ProposalWeekDto{IsHiatus = true},
                new ProposalWeekDto{IsHiatus = true},
                new ProposalWeekDto{IsHiatus = true}
            };

            ProposalCalculationEngine._UpdateProposalWeekValues(dto, true);

            Assert.That(dto.Weeks[0].Impressions, Is.EqualTo(0));
            Assert.That(dto.Weeks[1].Impressions, Is.EqualTo(0));
            Assert.That(dto.Weeks[2].Impressions, Is.EqualTo(0));
            Assert.That(dto.Weeks[0].Cost, Is.EqualTo(0));
            Assert.That(dto.Weeks[1].Cost, Is.EqualTo(0));
            Assert.That(dto.Weeks[2].Cost, Is.EqualTo(0));
            Assert.That(dto.Weeks[0].Units, Is.EqualTo(0));
            Assert.That(dto.Weeks[1].Units, Is.EqualTo(0));
            Assert.That(dto.Weeks[2].Units, Is.EqualTo(0));
        }

        [Test]
        public void UpdateuarterValues_NotDistribute()
        {
            var dto = new ProposalQuarterDto();
            dto.DistributeGoals = false;
            dto.Weeks = new List<ProposalWeekDto>
            {
                new ProposalWeekDto{Impressions = 123},
                new ProposalWeekDto{Impressions = 234}
            };
            ProposalCalculationEngine._UpdateQuarterValues(dto, true);

            Assert.That(dto.ImpressionGoal, Is.EqualTo(123 + 234));
        }

        [Test]
        public void SetQuarterTotals()
        {
            var dto = new ProposalDetailDto();
            var calcEngine = new ProposalCalculationEngine();
            dto.Quarters = new List<ProposalQuarterDto>
            {
                new ProposalQuarterDto
                {
                    Weeks = new List<ProposalWeekDto>
                    {
                        new ProposalWeekDto
                        {
                            Cost = 11.11m,
                            Units = 11,
                            Impressions = 11111
                        },
                        new ProposalWeekDto
                        {
                            Cost = 22.22m,
                            Units = 22,
                            Impressions = 22222
                        }
                    }
                },
                new ProposalQuarterDto
                {
                    Weeks = new List<ProposalWeekDto>
                    {
                        new ProposalWeekDto
                        {
                            Cost = 11.11m,
                            Units = 11,
                            Impressions = 11111
                        },
                        new ProposalWeekDto
                        {
                            Cost = 22.22m,
                            Units = 22,
                            Impressions = 22222
                        }
                    }
                },

            };

            calcEngine.SetQuarterTotals(dto);

            Assert.That(dto.TotalCost, Is.EqualTo(66.66));
            Assert.That(dto.TotalUnits, Is.EqualTo(66));
            Assert.That(dto.TotalImpressions, Is.EqualTo(66666));
        }

        [Test]
        public void SetProposalTargetsTest()
        {
            var proposalDto = _ProposalService.GetProposalById(250);

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(5999409.21m, proposalDto.TargetBudget);
            Assert.AreEqual(27089000, proposalDto.TargetImpressions);
            Assert.AreEqual(4, proposalDto.TargetUnits);
        }

        [Test]
        public void UpdateQuarterTotalsTest()
        {
            var proposalDto = _ProposalService.GetProposalById(250);
            var detailDto = proposalDto.Details.First();

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(5241609.21m, detailDto.TotalCost);
            Assert.AreEqual(18089000, detailDto.TotalImpressions);
            Assert.AreEqual(3, detailDto.TotalUnits);
        }

        [Test]
        public void UpdateProposalWeekValuesTest()
        {
            var proposalDto = _ProposalService.GetProposalById(250);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var weekDto = quarterDto.Weeks.First();

            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(1, weekDto.Units);
            Assert.AreEqual(1843500, weekDto.Impressions);
            Assert.AreEqual(23099.055m, weekDto.Cost);
        }

        [Test]
        public void UpdateQuarterImpressionGoalTest()
        {
            var proposalDto = _ProposalService.GetProposalById(250);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(5500000, quarterDto.ImpressionGoal);
        }

        [Test]
        public void UpdateProposalWithAduTest()
        {
            var proposalDto = _ProposalService.GetProposalById(250);
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
            var proposalDto = _ProposalService.GetProposalById(251);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var firstWeekDto = quarterDto.Weeks.First();
            var secondWeekDto = quarterDto.Weeks[1];
            var lastWeekDto = quarterDto.Weeks.Last();

            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(3333.3333333333335d, firstWeekDto.Impressions);
            Assert.AreEqual(3333.3333333333335d, secondWeekDto.Impressions);
            Assert.AreEqual(3334.3333333333335d, lastWeekDto.Impressions);
        }

        [Test]
        public void UpdateProposalCostCalculationTest()
        {
            var proposalDto = _ProposalService.GetProposalById(251);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var firstWeekDto = quarterDto.Weeks.First();
            var secondWeekDto = quarterDto.Weeks[1];
            var lastWeekDto = quarterDto.Weeks.Last();

            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(41.7666666666666249m, firstWeekDto.Cost);
            Assert.AreEqual(41.7666666666666249m, secondWeekDto.Cost);
            Assert.AreEqual(41.7791966666666249m, lastWeekDto.Cost);
        }


        [Test]
        public void DistributeImpressionsToLastNonHiatusWeekTest()
        {
            var proposalDto = _ProposalService.GetProposalById(251);
            var detailDto = proposalDto.Details.First();
            var quarterDto = detailDto.Quarters.First();
            var firstWeekDto = quarterDto.Weeks.First();
            var secondWeekDto = quarterDto.Weeks[1];
            var lastWeekDto = quarterDto.Weeks.Last();

            quarterDto.ImpressionGoal = 3333333;
            lastWeekDto.IsHiatus = true;
            quarterDto.DistributeGoals = true;

            var proposalCalculationEngine = new ProposalCalculationEngine();

            proposalCalculationEngine.UpdateProposal(proposalDto);

            Assert.AreEqual(1666666.5d, firstWeekDto.Impressions);
            Assert.AreEqual(1666667.5d, secondWeekDto.Impressions);
            Assert.AreEqual(0, lastWeekDto.Impressions);
        }
    }
}
