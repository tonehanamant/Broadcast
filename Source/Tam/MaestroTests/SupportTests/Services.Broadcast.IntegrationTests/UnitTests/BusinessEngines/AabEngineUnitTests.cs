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
        private Mock<ITrafficApiCache> _TrafficApiCache;

        private IAabEngine _GetTestClass(bool isAabEnabled = true)
        {
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            featureToggleHelper.Setup(s => s.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION))
                .Returns(isAabEnabled);

            _AabCache = new Mock<IAabCache>();
            _TrafficApiCache = new Mock<ITrafficApiCache>();

            var tc = new AabEngine(
                _AabCache.Object,
                _TrafficApiCache.Object,
                featureToggleHelper.Object
                );

            return tc;
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetAgencies(bool isAabEnabled)
        {
            // Arrange
            var tc = _GetTestClass(isAabEnabled);
            var agencies = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AgencyDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            _AabCache.Setup(s => s.GetAgencies())
                .Returns(agencies);
            _TrafficApiCache.Setup(s => s.GetAgencies())
                .Returns(agencies);

            // Act 
            var result = tc.GetAgencies();

            // Assert
            if (isAabEnabled)
            {
                _AabCache.Verify(s => s.GetAgencies(), Times.Once);
                _TrafficApiCache.Verify(s => s.GetAgencies(), Times.Never);
            }
            else
            {
                _AabCache.Verify(s => s.GetAgencies(), Times.Never);
                _TrafficApiCache.Verify(s => s.GetAgencies(), Times.Once);
            }
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetAgency(bool isAabEnabled)
        {
            // Arrange
            var tc = _GetTestClass(isAabEnabled);
            var agencies = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AgencyDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            var testAgency = agencies.First();
            _AabCache.Setup(s => s.GetAgencies())
                .Returns(agencies);
            _TrafficApiCache.Setup(s => s.GetAgency(It.IsAny<int>()))
                .Returns(testAgency);

            // Act 
            AgencyDto result;
            if (isAabEnabled)
            {
                result = tc.GetAgency(testAgency.MasterId.Value);
            }
            else
            {
                result = tc.GetAgency(testAgency.Id.Value);
            }

            // Assert
            if (isAabEnabled)
            {
                _AabCache.Verify(s => s.GetAgencies(), Times.Once);
                _TrafficApiCache.Verify(s => s.GetAgency(It.IsAny<int>()), Times.Never);
            }
            else
            {
                _AabCache.Verify(s => s.GetAgencies(), Times.Never);
                _TrafficApiCache.Verify(s => s.GetAgency(It.IsAny<int>()), Times.Once);
            }
            Assert.AreEqual(testAgency.Id.Value, result.Id.Value);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetAdvertisers(bool isAabEnabled)
        {
            // Arrange
            var tc = _GetTestClass(isAabEnabled);
            var advertisers = new List<AdvertiserDto>
            {
                new AdvertiserDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AdvertiserDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            _AabCache.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);
            _TrafficApiCache.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);

            // Act 
            var result = tc.GetAdvertisers();

            // Assert
            if (isAabEnabled)
            {
                _AabCache.Verify(s => s.GetAdvertisers(), Times.Once);
                _TrafficApiCache.Verify(s => s.GetAdvertisers(), Times.Never);
            }
            else
            {
                _AabCache.Verify(s => s.GetAdvertisers(), Times.Never);
                _TrafficApiCache.Verify(s => s.GetAdvertisers(), Times.Once);
            }
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetAdvertiser(bool isAabEnabled)
        {
            // Arrange
            var tc = _GetTestClass(isAabEnabled);
            var advertisers = new List<AdvertiserDto>
            {
                new AdvertiserDto{ Id = 1, MasterId = new Guid("64DA4EE6-832A-46A2-9797-85B337FA1200")},
                new AdvertiserDto{ Id = 2, MasterId = new Guid("0087140F-7DEA-42D3-925B-0CB38F1C8AE9")},
            };
            var testAdvertiser = advertisers.First();
            _AabCache.Setup(s => s.GetAdvertisers())
                .Returns(advertisers);
            _TrafficApiCache.Setup(s => s.GetAdvertiser(It.IsAny<int>()))
                .Returns(testAdvertiser);

            // Act 
            AdvertiserDto result;
            if (isAabEnabled)
            {
                result = tc.GetAdvertiser(testAdvertiser.MasterId.Value);
            }
            else
            {
                result = tc.GetAdvertiser(testAdvertiser.Id.Value);
            }

            // Assert
            if (isAabEnabled)
            {
                _AabCache.Verify(s => s.GetAdvertisers(), Times.Once);
                _TrafficApiCache.Verify(s => s.GetAdvertiser(It.IsAny<int>()), Times.Never);
            }
            else
            {
                _AabCache.Verify(s => s.GetAdvertisers(), Times.Never);
                _TrafficApiCache.Verify(s => s.GetAdvertiser(It.IsAny<int>()), Times.Once);
            }
            Assert.AreEqual(testAdvertiser.Id.Value, result.Id.Value);
        }

        [Test]
        public void GetAdvertiserProducts_AdvertiserId()
        {
            // Arrange
            var tc = _GetTestClass();
            var products = new List<ProductDto>
            {
                new ProductDto {Id = 1},
                new ProductDto {Id = 2}
            };
            _TrafficApiCache.Setup(s => s.GetProductsByAdvertiserId(It.IsAny<int>()))
                .Returns(products);

            // Act
            var result = tc.GetAdvertiserProducts(1);

            // Assert
            _TrafficApiCache.Verify(s => s.GetProductsByAdvertiserId(It.IsAny<int>()), Times.Once);
            Assert.AreEqual(2, result.Count);
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

            // Ac
            var result = tc.GetAdvertiserProduct(Guid.NewGuid(), testProductMasterId);

            // Assert
            _AabCache.Verify(s => s.GetAdvertiserProducts(It.IsAny<Guid>()), Times.Once);
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public void GetProduct()
        {
            // Arrange
            var tc = _GetTestClass();
            _TrafficApiCache.Setup(s => s.GetProduct(It.IsAny<int>()))
                .Returns(new ProductDto { Id = 1 });

            // Ac
            var result = tc.GetProduct(1);

            // Assert
            _TrafficApiCache.Verify(s => s.GetProduct(It.IsAny<int>()), Times.Once);
            Assert.AreEqual(1, result.Id);
        }
    }
}