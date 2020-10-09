using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class StationMappingRepositoryTests
    {
        [Test]
        public void GetBroadcastStationStartingWithCallLettersThreeLetterRoot()
        {
            // This test validates that the compare recognizes 'KDF' as the root.
            // This 'KDF%' hits 5 stations, but none should be found at all
            const string testCallLetters = "KDF";
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();

            var result = repo.GetBroadcastStationStartingWithCallLetters(testCallLetters, false);

            Assert.IsNull(result);
        }

        [Test]
        public void GetBroadcastStationStartingWithCallLettersNotFound()
        {
            // this should not be found
            const string testCallLetters = "HHHH";
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();

            var result = repo.GetBroadcastStationStartingWithCallLetters(testCallLetters, false);

            Assert.IsNull(result);
        }

        [Test]
        public void GetBroadcastStationStartingWithCallLettersNotFoundThrow()
        {
            // this should not be found
            const string testCallLetters = "HHHH";
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();

            Assert.Throws<Exception>(() =>
                repo.GetBroadcastStationStartingWithCallLetters(testCallLetters, true));
        }

        [Test]
        public void GetBroadcastStationStartingWithCallLettersThrowsWhenManyFound()
        {
            // This hits many stations and should throw an error
            const string testCallLetters = "KWES";
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();

            Assert.Throws<Exception>(() =>
                repo.GetBroadcastStationStartingWithCallLetters(testCallLetters, false));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBroadcastStationStartingWithCallLetters()
        {
            // finds one stations
            const string testCallLetters = "KFXO";
            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IStationMappingRepository>();

            var result = repo.GetBroadcastStationStartingWithCallLetters(testCallLetters, false);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "Id");
            jsonResolver.Ignore(typeof(DisplayBroadcastStation), "ModifiedDate");
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
        }
    }
}