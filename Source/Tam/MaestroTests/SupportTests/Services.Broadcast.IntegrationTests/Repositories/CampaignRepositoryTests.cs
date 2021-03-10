using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using System.ComponentModel;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [System.ComponentModel.Category("short_running")]
    class CampaignRepositoryTests
    {
        [Test]
        public void GetCampaignsDateRanges()
        {
            /*** Arrange ***/
            List<DateRange> result = null;
            List<DateRange> resultAll = null;
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            /*** Act ***/
            using (new TransactionScopeWrapper())
            {
                result = repo.GetCampaignsDateRanges(PlanStatusEnum.Working);
            }
            using (new TransactionScopeWrapper())
            {
                resultAll = repo.GetCampaignsDateRanges(null);
            }

            /*** Assert ***/
            Assert.IsNotNull(result);
            Assert.IsNotNull(resultAll);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignsDateRangesDiffReporter()
        {
            // Arrange
            List<DateRange> result = null;
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            /*** Act ***/
            using (new TransactionScopeWrapper())
            {
                result = repo.GetCampaignsDateRanges(null);
            }

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

    }
}
