using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
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

        // This is correct if null value records should NOT exists
        [Test]
        public void AddProjectedImpressionsForComponentsToManifests_AudienceHasNoReturn_NoRecordsCreated()
        {
            const int testFileId = 233551;
            const int nonExistingHutBookId = -1;
            const int nonExistingShareBookId = -2;
            var manifests = _InventoryRepository.GetStationInventoryManifestsByFileId(testFileId);
            
            Assert.Throws<InvalidOperationException>(() => _ImpressionsService.AddProjectedImpressionsForComponentsToManifests(manifests, ProposalEnums.ProposalPlaybackType.LivePlus3, nonExistingHutBookId, nonExistingShareBookId));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
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
    }
}