using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class PlanPricingInventorySourceSortEngineUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetSortInventorySourcePercentsTest()
        {
            var toSort = new List<PlanInventorySourceDto>
            {
                new PlanInventorySourceDto
                {
                    Id = 3,
                    Name = "KATZ",
                    Percentage = 3
                },
                new PlanInventorySourceDto
                {
                    Id = 20,
                    Name = "Stranger2",
                    Percentage = 20
                },
                new PlanInventorySourceDto
                {
                    Id = 5,
                    Name = "NBC O&O",
                    Percentage = 5
                },
                new PlanInventorySourceDto
                {
                    Id = 20,
                    Name = "Stranger",
                    Percentage = 20
                },
                new PlanInventorySourceDto
                {
                    Id = 1,
                    Name = "ABC O&O",
                    Percentage = 1
                },
            };

            var result = PlanInventorySourceSortEngine.SortInventorySourcePercents(toSort);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SortInventorySourceTypePercentsTest()
        {
            var toSort = new List<PlanInventorySourceTypeDto>
            {
                new PlanInventorySourceTypeDto
                {
                    Id = 5,
                    Name = "Diginet",
                    Percentage = 5
                },
                new PlanInventorySourceTypeDto
                {
                    Id = 20,
                    Name = "Stranger2",
                    Percentage = 20
                },
                new PlanInventorySourceTypeDto
                {
                    Id = 4,
                    Name = "Syndication",
                    Percentage = 4
                },
                new PlanInventorySourceTypeDto
                {
                    Id = 20,
                    Name = "Stranger",
                    Percentage = 20
                }
            };

            var result = PlanInventorySourceSortEngine.SortInventorySourceTypePercents(toSort);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
    }
}
