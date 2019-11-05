using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.IntegrationTests.Stubs;
using System;
using Microsoft.Practices.Unity;
using Services.Broadcast.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class TrafficApiCacheTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAgencies()
        {
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);
            const int expectedCallCount = 68; // number of allowed chars

            var result = cache.GetAgencies();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(expectedCallCount, trafficApiClient.GetFilteredAgenciesCalledCount);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAgency()
        {
            const int agencyId = 3;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result = cache.GetAgency(agencyId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetAgency_WithInvalidId()
        {
            const int agencyId = 666;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);
            const int expectedClientCallCount = 68; // number of allowed chars

            Assert.That(() => cache.GetAgency(agencyId), 
                Throws.TypeOf<Exception>().With.Message.EqualTo($"Agency with id '{agencyId}' not found."));

            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetFilteredAgenciesCalledCount);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAdvertisersByAgencyId()
        {
            const int agencyId = 3;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result = cache.GetAdvertisersByAgencyId(agencyId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertisersByAgencyIdCalledCount);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAdvertisersByAgencyId_VerifyCached()
        {
            const int agencyId = 3;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result1 = cache.GetAdvertisersByAgencyId(agencyId);
            var result2 = cache.GetAdvertisersByAgencyId(agencyId);

            Assert.AreEqual(IntegrationTestHelper.ConvertToJson(result1), IntegrationTestHelper.ConvertToJson(result2));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result2));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertisersByAgencyIdCalledCount);
        }

        [Test]
        public void GetAdvertisersByAgencyId_WithInvalidId()
        {
            const int agencyId = 666;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            Assert.That(() => cache.GetAdvertisersByAgencyId(agencyId),
                Throws.TypeOf<Exception>().With.Message.EqualTo($"Cannot fetch advertisers data for agency {agencyId}."));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertisersByAgencyIdCalledCount);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAdvertiser()
        {
            const int advertiserId = 3;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result = cache.GetAdvertiser(advertiserId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertiserCallCount);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAdvertiser_VerifyCached()
        {
            const int advertiserId = 3;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result1 = cache.GetAdvertiser(advertiserId);
            var result2 = cache.GetAdvertiser(advertiserId);

            Assert.AreEqual(IntegrationTestHelper.ConvertToJson(result1), IntegrationTestHelper.ConvertToJson(result2));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result2));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertiserCallCount);
        }

        [Test]
        public void GetAdvertiser_WithInvalidId()
        {
            const int advertiserId = 666;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            Assert.That(() => cache.GetAdvertiser(advertiserId),
                Throws.TypeOf<Exception>().With.Message.EqualTo($"Cannot fetch data of the advertiser {advertiserId}"));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertiserCallCount);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProductsByAdvertiserId()
        {
            const int advertiserId = 3;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result = cache.GetProductsByAdvertiserId(advertiserId);

            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductsByAdvertiserIdCallCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProductsByAdvertiserId_VerifyCached()
        {
            const int advertiserId = 3;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result1 = cache.GetProductsByAdvertiserId(advertiserId);
            var result2 = cache.GetProductsByAdvertiserId(advertiserId);

            Assert.AreEqual(IntegrationTestHelper.ConvertToJson(result1), IntegrationTestHelper.ConvertToJson(result2));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result2));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductsByAdvertiserIdCallCount);
        }

        [Test]
        public void GetProductsByAdvertiserId_WithInvalid()
        {
            const int advertiserId = 666;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            Assert.That(() => cache.GetProductsByAdvertiserId(advertiserId),
                Throws.TypeOf<Exception>().With.Message.EqualTo($"Cannot fetch products data for advertiser {advertiserId}."));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductsByAdvertiserIdCallCount);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProduct()
        {
            const int productId = 2;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result = cache.GetProduct(productId);

            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductCallCount);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProduct_VerifyCached()
        {
            const int productId = 2;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result1 = cache.GetProduct(productId);
            var result2 = cache.GetProduct(productId);

            Assert.AreEqual(IntegrationTestHelper.ConvertToJson(result1), IntegrationTestHelper.ConvertToJson(result2));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result2));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductCallCount);
        }

        [Test]
        public void GetProduct_WithInvalidId()
        {
            const int productId = 666;
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            Assert.That(() => cache.GetProduct(productId),
                Throws.TypeOf<Exception>().With.Message.EqualTo($"Cannot fetch data of the product {productId}"));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductCallCount);
        }
    }
}