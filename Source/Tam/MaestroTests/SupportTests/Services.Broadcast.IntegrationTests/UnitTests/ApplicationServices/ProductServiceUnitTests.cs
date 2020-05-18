using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
    public class ProductServiceUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsProductsByAdvertiserId()
        {
            // Arrange
            var trafficApiCacheMock = new Mock<ITrafficApiCache>();
            var getProductsByAdvertiserIdReturn = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "ProductOne", AdvertiserId = 1 },
                new ProductDto { Id = 2, Name = "ProductTwo", AdvertiserId = 1 },
                new ProductDto { Id = 3, Name = "ProductThree", AdvertiserId = 1 }
            };

            trafficApiCacheMock.Setup(s => s.GetProductsByAdvertiserId(It.IsAny<int>())).Returns(getProductsByAdvertiserIdReturn);

            var tc = new ProductService(trafficApiCacheMock.Object);

            // Act
            var result = tc.GetProductsByAdvertiserId(advertiserId: 1);

            // Assert
            trafficApiCacheMock.Verify(x => x.GetProductsByAdvertiserId(1), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ThrowsException_WhenCanNotGetProductsByAdvertiserId()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetProductsByAdvertiserId";

            var trafficApiCacheMock = new Mock<ITrafficApiCache>();

            trafficApiCacheMock
                .Setup(s => s.GetProductsByAdvertiserId(It.IsAny<int>()))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = new ProductService(trafficApiCacheMock.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetProductsByAdvertiserId(advertiserId: 1));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}
