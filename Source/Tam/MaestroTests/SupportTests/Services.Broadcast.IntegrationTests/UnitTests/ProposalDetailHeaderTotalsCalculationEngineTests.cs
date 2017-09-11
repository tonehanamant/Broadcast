using System.Collections.Generic;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    public class ProposalDetailHeaderTotalsCalculationEngineTests
    {
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>();

        [Test]
        public void CalculateOpenMarketsTotalsTest()
        {
            var proposalInventory = _ProposalOpenMarketInventoryService.GetInventory(7);
            var calculationEngine = new ProposalDetailHeaderTotalsCalculationEngine();
            calculationEngine.CalculateTotalsForOpenMarketInventory(proposalInventory, new ProposalDetailSingleInventoryTotalsDto { TotalCost = 2000, TotalImpressions = 2000 }, 20);

            Assert.AreEqual(3040m, proposalInventory.DetailTotalBudget);
            Assert.AreEqual(3000d, proposalInventory.DetailTotalImpressions);
            Assert.AreEqual(1013.33m, proposalInventory.DetailTotalCpm);
        }

        [Test]
        public void CalculateProprietaryTotalsTest()
        {
            var request = new ProposalInventoryTotalsRequestDto
            {
                DetailCpm = 10,
                DetailTargetBudget = 100,
                DetailTargetImpressions = 10000,
                Weeks = new List<ProposalInventoryTotalsRequestDto.InventoryWeek>
                    {
                        new ProposalInventoryTotalsRequestDto.InventoryWeek
                        {
                            MediaWeekId = 1,
                            Budget = 60,
                            ImpressionGoal = 6000,
                            Slots = new List<ProposalInventoryTotalsRequestDto.InventoryWeek.AllocatedSlot>
                            {
                                new ProposalInventoryTotalsRequestDto.InventoryWeek.AllocatedSlot
                                {
                                    Cost = 25,
                                    Impressions = 2000
                                },
                                new ProposalInventoryTotalsRequestDto.InventoryWeek.AllocatedSlot
                                {
                                    Cost = 20,
                                    Impressions = 2000
                                },
                                new ProposalInventoryTotalsRequestDto.InventoryWeek.AllocatedSlot
                                {
                                    Cost = 10,
                                    Impressions = 1000
                                }
                            }
                        },
                        new ProposalInventoryTotalsRequestDto.InventoryWeek
                        {
                            MediaWeekId = 2,
                            Budget = 400,
                            ImpressionGoal = 4000,
                            Slots = new List<ProposalInventoryTotalsRequestDto.InventoryWeek.AllocatedSlot>
                            {
                                new ProposalInventoryTotalsRequestDto.InventoryWeek.AllocatedSlot
                                {
                                    Cost = 30,
                                    Impressions = 3000
                                },
                                new ProposalInventoryTotalsRequestDto.InventoryWeek.AllocatedSlot
                                {
                                    Cost = 20,
                                    Impressions = 2000
                                }
                            }
                        }
                    }
            };

            var calculationEngine = new ProposalDetailHeaderTotalsCalculationEngine();

            var totals = new ProposalInventoryTotalsDto { TotalCost = 5000, TotalImpressions = 10000 };

            var otherInventoryTotals = new ProposalDetailSingleInventoryTotalsDto
            {
                TotalCost = 123456,
                TotalImpressions = 70000
            };

            calculationEngine.CalculateTotalsForProprietaryInventory(totals, request, otherInventoryTotals, 20);

            Assert.AreEqual(128456m, totals.TotalCost);
            Assert.AreEqual(80000.0d, totals.TotalImpressions);
            Assert.AreEqual(1605.7m, totals.TotalCpm);
        }
    }
}
