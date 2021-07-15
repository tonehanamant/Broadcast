using System;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    public class AabEngineUnitTests
    {
        private Mock<IAabCache> _AabCache;

        private IAabEngine _GetTestClass()
        {
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();

            _AabCache = new Mock<IAabCache>();

            var tc = new AabEngine(
                _AabCache.Object,
                featureToggleHelper.Object
                );

            return tc;
        }

        [Test]
        public void GetAgencies()
        {
            // Arrange
            var tc = _GetTestClass();
            var agencies = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AgencyDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            _AabCache.Setup(s => s.GetAgencies())
                .Returns(agencies);

            // Act 
            var result = tc.GetAgencies();

            // Assert

            _AabCache.Verify(s => s.GetAgencies(), Times.Once);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetAgency_AgencyId()
        {
            // Arrange
            var tc = _GetTestClass();
            var agencies = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AgencyDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            var testAgency = agencies.First();
            _AabCache.Setup(s => s.GetAgencies())
                .Returns(agencies);

            // Act 
            AgencyDto result;
            result = tc.GetAgency(testAgency.MasterId.Value);

            // Assert

            _AabCache.Verify(s => s.GetAgencies(), Times.Once);

            Assert.AreEqual(testAgency.Id.Value, result.Id.Value);
        }

        [Test]

        public void GetAgency_AgencyMasterId()
        {
            // Arrange
            var tc = _GetTestClass();
            var agencies = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AgencyDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            var testAgency = agencies.First();
            _AabCache.Setup(s => s.GetAgencies())
                .Returns(agencies);

            // Act 
            var result = tc.GetAgency(testAgency.MasterId.Value);

            // Assert
            _AabCache.Verify(s => s.GetAgencies(), Times.Once);
            Assert.AreEqual(testAgency.Id.Value, result.Id.Value);
        }

        [Test]
        public void GetAdvertisers()
        {
            // Arrange
            var tc = _GetTestClass();
            var advertisers = new List<AdvertiserDto>
            {
                new AdvertiserDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AdvertiserDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            _AabCache.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);

            // Act 
            var result = tc.GetAdvertisers();

            // Assert
            _AabCache.Verify(s => s.GetAdvertisers(), Times.Once);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetAdvertiser_AdvertiserId()
        {
            // Arrange
            var tc = _GetTestClass();
            var advertisers = new List<AdvertiserDto>
            {
                new AdvertiserDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AdvertiserDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            var testAdvertiser = advertisers.First();
            _AabCache.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);

            // Act 
            var result = tc.GetAdvertiser(testAdvertiser.MasterId.Value);

            // Assert
            _AabCache.Verify(s => s.GetAdvertisers(), Times.Once);
            Assert.AreEqual(testAdvertiser.Id.Value, result.Id.Value);
        }

        [Test]
        public void GetAdvertiser_AdvertiserMasterId()
        {
            // Arrange
            var tc = _GetTestClass();
            var advertisers = new List<AdvertiserDto>
            {
                new AdvertiserDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AdvertiserDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            var testAdvertiser = advertisers.First();
            _AabCache.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);

            // Act 
            var result = tc.GetAdvertiser(testAdvertiser.MasterId.Value);

            // Assert
            _AabCache.Verify(s => s.GetAdvertisers(), Times.Once);
            Assert.AreEqual(testAdvertiser.Id.Value, result.Id.Value);
        }        

        [Test]
        public void GetAdvertiserProducts_AdvertiserMasterId()
        {
            // Arrange
            var tc = _GetTestClass();
            var products = new List<ProductDto>
            {
                new ProductDto {Id = 1},
                new ProductDto {Id = 2}
            };
            _AabCache.Setup(s => s.GetAdvertiserProducts(It.IsAny<Guid>()))
                .Returns(products);

            // Act
            var result = tc.GetAdvertiserProducts(Guid.NewGuid());

            // Assert
            _AabCache.Verify(s => s.GetAdvertiserProducts(It.IsAny<Guid>()), Times.Once);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetAdvertiserProduct()
        {
            // Arrange
            var testProductMasterId = Guid.NewGuid();
            var tc = _GetTestClass();
            _AabCache.Setup(s => s.GetAdvertiserProducts(It.IsAny<Guid>()))
                .Returns(new List<ProductDto>
                {
                    new ProductDto {Id = 1, MasterId = testProductMasterId},
                    new ProductDto {Id = 2, MasterId = Guid.NewGuid()}
                });

            // Act
            var result = tc.GetAdvertiserProduct(Guid.NewGuid(), testProductMasterId);

            // Assert
            _AabCache.Verify(s => s.GetAdvertiserProducts(It.IsAny<Guid>()), Times.Once);
            Assert.AreEqual(1, result.Id);
        }

    }
}