using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class ProductServiceUnitTests
    {
        [Test]
        public void GetsProductsByAdvertiserId()
        {
            // Arrange
            var aabEngine = new Mock<IAabEngine>();
            var advertiserProducts = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "ProductOne", AdvertiserId = 1 },
                new ProductDto { Id = 2, Name = "ProductTwo", AdvertiserId = 1 },
                new ProductDto { Id = 3, Name = "ProductThree", AdvertiserId = 1 }
            };
            var expectedReturnCount = advertiserProducts.Count;

            aabEngine.Setup(s => s.GetAdvertiserProducts(It.IsAny<Guid>())).Returns(advertiserProducts);

            var tc = new ProductService(aabEngine.Object);

            var advertiserId = new Guid("1806450a-e0a3-416d-b38d-913fb5cf3879");

            // Act
            var result = tc.GetAdvertiserProducts(advertiserId);

            // Assert
            aabEngine.Verify(x => x.GetAdvertiserProducts(advertiserId), Times.Once);
            Assert.AreEqual(expectedReturnCount, result.Count);
        }

        [Test]
        public void GetAdvertiserProducts()
        {
            // Arrange
            var aabEngine = new Mock<IAabEngine>();
            var advertiserProducts = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "ProductOne", AdvertiserId = 1 },
                new ProductDto { Id = 2, Name = "ProductTwo", AdvertiserId = 1 },
                new ProductDto { Id = 3, Name = "ProductThree", AdvertiserId = 1 }
            };
            var expectedReturnCount = advertiserProducts.Count;

            aabEngine.Setup(s => s.GetAdvertiserProducts(It.IsAny<Guid>())).Returns(advertiserProducts);

            var tc = new ProductService(aabEngine.Object);

            // Act
            var result = tc.GetAdvertiserProducts(Guid.NewGuid());

            // Assert
            aabEngine.Verify(x => x.GetAdvertiserProducts(It.IsAny<Guid>()), Times.Once);
            Assert.AreEqual(expectedReturnCount, result.Count);
        }

        [Test]
        public void ThrowsException_WhenCanNotGetProductsByAdvertiserId()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetProductsByAdvertiserId";

            var aabEngine = new Mock<IAabEngine>();

            aabEngine
                .Setup(s => s.GetAdvertiserProducts(It.IsAny<Guid>()))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = new ProductService(aabEngine.Object);
            var advertiserId = new Guid("1806450a-e0a3-416d-b38d-913fb5cf3879");
            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetAdvertiserProducts(advertiserId));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}
