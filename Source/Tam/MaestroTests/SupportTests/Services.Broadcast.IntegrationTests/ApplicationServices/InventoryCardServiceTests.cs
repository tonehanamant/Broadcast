using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class InventoryCardServiceTests
    {
        private readonly IInventoryCardService _InventoryCardService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryCardService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInitialData()
        {
            var inventoryCards = _InventoryCardService.GetInitialData(new DateTime(2019, 04, 01));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryCardsInventorySourceTest()
        {
            var inventoryCards = _InventoryCardService.GetInventoryCards(new InventoryCardFilterDto
            {
                InventorySourceId = 7
            }, new DateTime(2019, 2, 1));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryCardsInventorySourceNoDataTest()
        {
            var inventoryCards = _InventoryCardService.GetInventoryCards(new InventoryCardFilterDto
            {
                InventorySourceId = 8
            }, new DateTime(2019, 2, 1));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetInventoryCardsQuarterTest()
        {
            var inventoryCards = _InventoryCardService.GetInventoryCards(new InventoryCardFilterDto
            {
                Quarter = new InventoryCardQuarter
                {
                    Quarter = 1,
                    Year = 2019
                }
            }, new DateTime(2019, 2, 1));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventoryCards));
        }
    }
}
