using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Repositories;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.CampaignAggregation
{
    [Category("short_running")]
    public class CampaignAggregationJobTriggerUnitTests
    {
        [Test]
        public void TriggerJob()
        {
            var backgroundJobClient = new MockedHangfireBackgroundJobClient();
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignSummaryRepository = new Mock<ICampaignSummaryRepository>();
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepository.Object);
            var tc = new CampaignAggregationJobTrigger(dataRepositoryFactoryMock.Object, backgroundJobClient);

            tc.TriggerJob(666, "TestUser");

            campaignSummaryRepository.Verify(s => s.SetSummaryProcessingStatusToInProgress(666, "TestUser", It.IsAny<DateTime>()), Times.Once);
            Assert.AreEqual(1, backgroundJobClient.Jobs.Count);
        }
    }
}