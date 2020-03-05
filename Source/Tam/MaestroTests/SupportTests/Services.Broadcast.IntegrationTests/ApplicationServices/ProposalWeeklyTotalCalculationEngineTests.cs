using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class ProposalWeeklyTotalCalculationEngineTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CalculateProposalDetailInventoryTotals()
        {
            using (new TransactionScopeWrapper())
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

                var result =
                    IntegrationTestApplicationServiceFactory
                        .GetApplicationService<IProposalWeeklyTotalCalculationEngine>()
                        .CalculateProprietaryDetailTotals(request,
                            new ProposalDetailSingleInventoryTotalsDto()
                            {
                                TotalCost = 4321,
                                TotalImpressions = 5432,
                                Margin = 20
                            }, new List<ProposalDetailSingleWeekTotalsDto>());

                Approvals.Verify(IntegrationTestHelper.ConvertToJsonMoreRounding(result));
            }
        }
    }
}
