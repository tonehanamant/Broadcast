using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ImpressionsServiceIntegrationTests
    {
        private readonly IImpressionsService _ImpressionsService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IImpressionsService>();

        private readonly IInventoryRepository _InventoryRepository = IntegrationTestApplicationServiceFactory
            .BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void AddProjectedImpressionsForComponentsToManifests_AudienceHasNoReturn()
        {
            const int testFileId = 233551;
            const int nonExistingHutBookId = -1;
            const int nonExistingShareBookId = -2;
            var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(testFileId);

            _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, ProposalEnums.ProposalPlaybackType.LivePlus3, nonExistingHutBookId, nonExistingShareBookId);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(manifests));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void AddProjectedImpressionsForComponentsToManifests_AudienceHasReturn()
        {
            const int testFileId = 233551;
            const int existingHutBookId = 413;
            const int existingShareBookId = 437;
            const int expectedManifestAudienceCountPerManifest = 21;
            var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(testFileId);

            _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, ProposalEnums.ProposalPlaybackType.LivePlus3, existingHutBookId, existingShareBookId);

            var observationManifestAudiences = manifests.First().ManifestAudiences;
            Assert.AreEqual(expectedManifestAudienceCountPerManifest, observationManifestAudiences.Count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(observationManifestAudiences));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void AddProjectedImpressionsUsingTwoBooksForComponentsToManifests_CrossingMidnight()
        {
            const int testFileId = 233551;
            const int existingHutBookId = 413;
            const int existingShareBookId = 437;
            const int expectedManifestAudienceCountPerManifest = 21;
            var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(testFileId);

            var testManifest = manifests.First();
            var manifestDaypart = testManifest.ManifestDayparts.First();
            manifestDaypart.Daypart.StartTime = 86000;
            manifestDaypart.Daypart.EndTime = 300;
            var manifestList = new List<StationInventoryManifest>();
            manifestList.Add(testManifest);
                
            _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifestList, ProposalEnums.ProposalPlaybackType.LivePlus3, existingHutBookId, existingShareBookId);

            var observationManifestAudiences = testManifest.ManifestAudiences;
            Assert.AreEqual(expectedManifestAudienceCountPerManifest, observationManifestAudiences.Count);

            // For some reason Audience ID 348 gets an Impressions value that flops between 2007.3735762621538 and 2007.3735762621539
            // note the difference in the last digit.
            // in practice this is rounded to <5 decimal points for usage so this flopping at 13 decimal points is acceptable.
            // So just round to 10 decimals
            observationManifestAudiences.ForEach(a => a.Impressions = Math.Round(a.Impressions ?? 0, 10));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(observationManifestAudiences));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void AddProjectedImpressionsUsingSingleBookForComponentsToManifests_CrossingMidnight()
        {
            const int testFileId = 233551;
            const int existingShareBookId = 437;
            const int expectedManifestAudienceCountPerManifest = 21;
            var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(testFileId);

            var testManifest = manifests.First();
            var manifestDaypart = testManifest.ManifestDayparts.First();
            manifestDaypart.Daypart.StartTime = 86000;
            manifestDaypart.Daypart.EndTime = 300;
            var manifestList = new List<StationInventoryManifest>();
            manifestList.Add(testManifest);

            _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifestList, ProposalEnums.ProposalPlaybackType.LivePlus3, existingShareBookId);

            var observationManifestAudiences = testManifest.ManifestAudiences;
            Assert.AreEqual(expectedManifestAudienceCountPerManifest, observationManifestAudiences.Count);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(observationManifestAudiences));
        }
    }
}