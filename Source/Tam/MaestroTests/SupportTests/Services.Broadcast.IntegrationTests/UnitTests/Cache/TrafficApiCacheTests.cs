using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.IntegrationTests.Stubs;
using System;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.IntegrationTests.UnitTests.Cache
{
    [TestFixture]
    [Category("short_running")]
    public class TrafficApiCacheTests
    {
        [SetUp]
        public void SetUp()
        {
            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAgencies()
        {
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result = cache.GetAgencies();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
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
        [UseReporter(typeof(DiffReporter))]
        public void GetAdvertisers()
        {
            const int expectedClientCallCount = 1;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            var result = cache.GetAdvertisers();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertisersCalledCount);
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

        [Test]
        public void ClearAgenciesCache()
        {
            const int expectedClientCallCount = 2;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            cache.GetAgencies();
            cache.ClearAgenciesCache();
            cache.GetAgencies();

            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAgenciesCalledCount);
        }

        [Test]
        public void ClearAdvertisersCache()
        {
            const int expectedClientCallCount = 2;
            var trafficApiClient = new TrafficApiClientStub();
            var cache = new TrafficApiCache(trafficApiClient);

            cache.GetAdvertisers();
            cache.GetAdvertiser(1);
            cache.GetProductsByAdvertiserId(1);
            cache.GetProduct(1);
            cache.ClearAdvertisersCache();
            cache.GetAdvertisers();
            cache.GetAdvertiser(1);
            cache.GetProductsByAdvertiserId(1);
            cache.GetProduct(1);

            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertisersCalledCount);
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetAdvertiserCallCount);
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductsByAdvertiserIdCallCount);
            Assert.AreEqual(expectedClientCallCount, trafficApiClient.GetProductCallCount);
        }
    }
}