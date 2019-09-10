using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class ProductServiceUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetsProductsByAdvertiserId()
        {
            // Arrange
            var trafficApiClientMock = new Mock<ITrafficApiClient>();
            var getProductsByAdvertiserIdReturn = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "ProductOne", AdvertiserId = 1 },
                new ProductDto { Id = 2, Name = "ProductTwo", AdvertiserId = 1 },
                new ProductDto { Id = 3, Name = "ProductThree", AdvertiserId = 1 }
            };

            trafficApiClientMock.Setup(s => s.GetProductsByAdvertiserId(It.IsAny<int>())).Returns(getProductsByAdvertiserIdReturn);

            var tc = new ProductService(trafficApiClientMock.Object);
            var serExpectedResult = IntegrationTestHelper.ConvertToJson(getProductsByAdvertiserIdReturn);

            // Act
            var result = tc.GetProductsByAdvertiserId(advertiserId: 1);

            // Assert
            trafficApiClientMock.Verify(x => x.GetProductsByAdvertiserId(1), Times.Once);
            // TODO: Bring this back.  Fails on CD test run build.
            //Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            // TODO: When bring that back remove this 
            Assert.AreEqual(serExpectedResult, IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ThrowsException_WhenCanNotGetProductsByAdvertiserId()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetProductsByAdvertiserId";

            var trafficApiClientMock = new Mock<ITrafficApiClient>();

            trafficApiClientMock
                .Setup(s => s.GetProductsByAdvertiserId(It.IsAny<int>()))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = new ProductService(trafficApiClientMock.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetProductsByAdvertiserId(advertiserId: 1));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }
    }
}
