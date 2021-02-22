using Moq;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.Cache
{
    [TestFixture]
    public class AabCacheUnitTests
    {
        [SetUp]
        public void SetUp()
        {
            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
        }

        [Test]
        public void GetAgencies()
        {
            // Arrange
            var agencies = _GetAgencies();
            var expectedResultCount = agencies.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAgencies())
                .Returns(agencies);

            var tc = new AabCache(aabApiClient.Object);

            // Act
            var result = tc.GetAgencies();

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAgencies(), Times.Once);
        }

        [Test]
        public void GetAgenciesCached()
        {
            // Arrange
            var agencies = _GetAgencies();
            var expectedResultCount = agencies.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAgencies())
                .Returns(agencies);

            var tc = new AabCache(aabApiClient.Object);

            // Act
            tc.GetAgencies();
            tc.GetAgencies();
            var result = tc.GetAgencies();

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAgencies(), Times.Once);
        }

        [Test]
        public void ClearAgenciesCache()
        {
            // Arrange
            const int expectedClientCalledCount = 2;
            var agencies = _GetAgencies();
            var expectedResultCount = agencies.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAgencies())
                .Returns(agencies);

            var tc = new AabCache(aabApiClient.Object);

            // Act
            tc.GetAgencies();
            tc.ClearAgenciesCache();
            var result = tc.GetAgencies();

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAgencies(), Times.Exactly(expectedClientCalledCount));
        }

        [Test]
        public void GetAdvertisers()
        {
            // Arrange
            var advertisers = _GetAdvertisers();
            var expectedResultCount = advertisers.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);

            var tc = new AabCache(aabApiClient.Object);

            // Act
            var result = tc.GetAdvertisers();

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAdvertisers(), Times.Once);
        }

        [Test]
        public void GetAdvertisersCached()
        {
            // Arrange
            var advertisers = _GetAdvertisers();
            var expectedResultCount = advertisers.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);

            var tc = new AabCache(aabApiClient.Object);

            // Act
            tc.GetAdvertisers();
            tc.GetAdvertisers();
            var result = tc.GetAdvertisers();

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAdvertisers(), Times.Once);
        }

        [Test]
        public void ClearAdvertisersCache()
        {
            // Arrange
            const int expectedClientCallCount = 2;
            var advertisers = _GetAdvertisers();
            var expectedResultCount = advertisers.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);

            var tc = new AabCache(aabApiClient.Object);

            // Act
            tc.GetAdvertisers();
            tc.ClearAdvertisersCache();
            var result = tc.GetAdvertisers();

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAdvertisers(), Times.Exactly(expectedClientCallCount));
        }

        [Test]
        public void GetAdvertiserProducts()
        {
            // Arrange
            var testAdvertiserMasterId = Guid.NewGuid();
            var advertiserProducts = _GetAdvertiserProducts();
            var expectedResultCount = advertiserProducts.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAdvertiserProducts(testAdvertiserMasterId))
                .Returns(advertiserProducts);

            var tc = new AabCache(aabApiClient.Object);

            // Act
            var result = tc.GetAdvertiserProducts(testAdvertiserMasterId);

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAdvertiserProducts(testAdvertiserMasterId), Times.Once);
        }

        [Test]
        public void GetAdvertiserProductsNotCached()
        {
            // Arrange
            var testAdvertiserMasterId = Guid.NewGuid();
            var advertiserProducts = _GetAdvertiserProducts();
            var expectedResultCount = advertiserProducts.Count;
            var aabApiClient = new Mock<IAgencyAdvertiserBrandApiClient>();
            aabApiClient.Setup(s => s.GetAdvertiserProducts(testAdvertiserMasterId))
                .Returns(advertiserProducts);
            const int expectedCallCount = 3;

            var tc = new AabCache(aabApiClient.Object);

            // Act
            tc.GetAdvertiserProducts(testAdvertiserMasterId);
            tc.GetAdvertiserProducts(testAdvertiserMasterId);
            var result = tc.GetAdvertiserProducts(testAdvertiserMasterId);

            // Assert
            Assert.AreEqual(expectedResultCount, result.Count);
            aabApiClient.Verify(s => s.GetAdvertiserProducts(testAdvertiserMasterId), Times.Exactly(expectedCallCount));
        }

        private List<AgencyDto> _GetAgencies()
        {
            var result = new List<AgencyDto>
            {
                new AgencyDto { Id = 1 },
                new AgencyDto { Id = 2 },
                new AgencyDto { Id = 3 }
            };
            return result;
        }

        private List<AdvertiserDto> _GetAdvertisers()
        {
            var result = new List<AdvertiserDto>
            {
                new AdvertiserDto { Id = 1 },
                new AdvertiserDto { Id = 2 },
                new AdvertiserDto { Id = 3 }
            };
            return result;
        }

        private List<ProductDto> _GetAdvertiserProducts()
        {
            var result = new List<ProductDto>
            {
                new ProductDto { Id = 1 },
                new ProductDto { Id = 2 },
                new ProductDto { Id = 3 }
            };
            return result;
        }
    }
}