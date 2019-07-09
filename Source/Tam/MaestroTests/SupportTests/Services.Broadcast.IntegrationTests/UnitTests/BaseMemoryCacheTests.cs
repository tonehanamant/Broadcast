using NUnit.Framework;
using Services.Broadcast.Cache;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class BaseMemoryCacheTests
    {
        private BaseMemoryCache<TestModel> _Sut;
        private CacheItemPolicy _Policy;
        private string _ItemKey;

        [SetUp]
        public void SetUp()
        {
            _Sut = new BaseMemoryCache<TestModel>("TestModelCache");
            _Policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(300) };
            _ItemKey = "ItemKey";
        }

        [Test]
        public void CanBeUsedWithValueTypes()
        {
            const int expectedValue = 5;

            var cache = new BaseMemoryCache<int>("TestModelCache_ValueTypes");

            var cacheBefore = cache.Contains(_ItemKey);

            var itemFromCache = cache.GetOrCreate(_ItemKey, () => expectedValue, _Policy);

            var cacheAfter = cache.Contains(_ItemKey);

            Assert.False(cacheBefore);
            Assert.True(cacheAfter);
            Assert.AreEqual(expectedValue, itemFromCache);
        }

        [Test]
        public void SetsCacheItem_WhenNoItemWithGivenKeyInCache()
        {
            var cacheBefore = _Sut.Contains(_ItemKey);

            var itemFromCache = _Sut.GetOrCreate(_ItemKey, () => new TestModel { Id = 5, Name = "TestModelName" }, _Policy);

            var cacheAfter = _Sut.Contains(_ItemKey);

            Assert.False(cacheBefore);
            Assert.True(cacheAfter);
            Assert.AreEqual(new TestModel { Id = 5, Name = "TestModelName" }, itemFromCache);
        }

        [Test]
        public void RemovesCacheItem()
        {
            _Sut.GetOrCreate(_ItemKey, () => new TestModel { Id = 5, Name = "TestModelName" }, _Policy);

            var cacheBeforeRemoving = _Sut.Contains(_ItemKey);

            _Sut.Remove(_ItemKey);

            var cacheAfterRemoving = _Sut.Contains(_ItemKey);

            Assert.True(cacheBeforeRemoving);
            Assert.False(cacheAfterRemoving);
        }

        [Test]
        public void SetsCacheItemOnlyOnce_ForParallelGetOperations()
        {
            var cacheBefore = _Sut.Contains(_ItemKey);

            var task1 = Task.Run(() =>
            {
                return _Sut.GetOrCreate(_ItemKey, () =>
                {
                    // let`s simulate a situation when getting the item from DB takes 1 second
                    Thread.Sleep(1000);

                    return new TestModel { Id = 5, Name = "TestModelName" };
                }, 
                _Policy);
            });

            var task2 = Task.Run(() =>
            {
                // task1 should start getting the item first
                Thread.Sleep(200);

                return _Sut.GetOrCreate(_ItemKey, () =>
                {
                    // let`s simulate a situation when getting the item from DB takes 1 second
                    Thread.Sleep(1000);

                    return new TestModel { Id = 6, Name = "AnotherTestModelName" };
                },
                _Policy);
            });

            Task.WaitAll(task1, task2);

            Assert.False(cacheBefore);

            // Result must be the same because the second task didn`t run his creation function
            // Instead it waited for task1 completion and took the object from the cache
            Assert.AreEqual(task1.Result, task2.Result);
        }

        private class TestModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                return obj is TestModel objModel && Id == objModel.Id && Name == objModel.Name;
            }

            public override int GetHashCode()
            {
                var hashCode = -1919740922;
                hashCode = hashCode * -1521134295 + Id.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                return hashCode;
            }
        }
    }
}
