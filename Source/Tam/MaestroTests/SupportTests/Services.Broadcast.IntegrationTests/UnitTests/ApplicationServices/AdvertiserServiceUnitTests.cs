using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class AdvertiserServiceUnitTests
    {
        [Test]
        public void GetAdvertisers()
        {
            // Arrange
            var advertisers = new List<AdvertiserDto> {new AdvertiserDto {Id = 1}, new AdvertiserDto {Id = 2}};
            var expectedReturnedItemCount = advertisers.Count;
            var aabEngine = new Mock<IAabEngine>();
            var dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            aabEngine.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);
            var tc = new AdvertiserService(aabEngine.Object,dataRepositoryFactory.Object);

            // Act
            var result = tc.GetAdvertisers();

            // Assert
            aabEngine.Verify(s => s.GetAdvertisers(), Times.Once);
            Assert.AreEqual(expectedReturnedItemCount, result.Count);
        }
    }
}
