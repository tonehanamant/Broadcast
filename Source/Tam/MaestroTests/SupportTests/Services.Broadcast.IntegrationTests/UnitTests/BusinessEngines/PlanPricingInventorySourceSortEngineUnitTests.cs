using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanPricingInventorySourceSortEngineUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetSortInventorySourcePercentsTest()
        {
            var toSort = new List<PlanPricingInventorySourceDto>
            {
                new PlanPricingInventorySourceDto
                {
                    Id = 3,
                    Name = "KATZ",
                    Percentage = 3
                },
                new PlanPricingInventorySourceDto
                {
                    Id = 20,
                    Name = "Stranger2",
                    Percentage = 20
                },
                new PlanPricingInventorySourceDto
                {
                    Id = 5,
                    Name = "NBC O&O",
                    Percentage = 5
                },
                new PlanPricingInventorySourceDto
                {
                    Id = 20,
                    Name = "Stranger",
                    Percentage = 20
                },
                new PlanPricingInventorySourceDto
                {
                    Id = 1,
                    Name = "ABC O&O",
                    Percentage = 1
                },
            };

            var result = PlanPricingInventorySourceSortEngine.SortInventorySourcePercents(toSort);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SortInventorySourceTypePercentsTest()
        {
            var toSort = new List<PlanPricingInventorySourceTypeDto>
            {
                new PlanPricingInventorySourceTypeDto
                {
                    Id = 5,
                    Name = "Diginet",
                    Percentage = 5
                },
                new PlanPricingInventorySourceTypeDto
                {
                    Id = 20,
                    Name = "Stranger2",
                    Percentage = 20
                },
                new PlanPricingInventorySourceTypeDto
                {
                    Id = 4,
                    Name = "Syndication",
                    Percentage = 4
                },
                new PlanPricingInventorySourceTypeDto
                {
                    Id = 20,
                    Name = "Stranger",
                    Percentage = 20
                }
            };

            var result = PlanPricingInventorySourceSortEngine.SortInventorySourceTypePercents(toSort);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}
