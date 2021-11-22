using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.PlanServices
{
    [TestFixture]
    [NUnit.Framework.Category("short_running")]
    public class LockingEngineUnitTests
    {
        private Mock<IBroadcastLockingManagerApplicationService> _LockingManager;
        private Mock<IBroadcastLockingService> _LockingService;
        internal Lazy<bool> _IsLockingConsolidationEnabled;
        private Mock<IBroadcastLockingManagerApplicationService> _LockingManagerApplicationService;
        private Mock<IPlanRepository> _PlanRepository;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private Mock<IDataRepositoryFactory> dataRepositoryFactory;
        protected LockingEngine _GetService(bool isEnableLockingConsolidation = false)
        {
            _LaunchDarklyClientStub = new LaunchDarklyClientStub();
            _LaunchDarklyClientStub.FeatureToggles.Add(FeatureToggles.ENABLE_LOCKING_CONSOLIDATION, isEnableLockingConsolidation);

            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

            return new LockingEngine(
                    _LockingManager.Object,
                    _LockingService.Object,
                    featureToggleHelper,
                    _LockingManagerApplicationService.Object,
                    dataRepositoryFactory.Object
            );
        }
        [SetUp]
        public void SetUp()
        {
            _LockingManager = new Mock<IBroadcastLockingManagerApplicationService>();
            _LockingService = new Mock<IBroadcastLockingService>();
            _LockingManagerApplicationService = new Mock<IBroadcastLockingManagerApplicationService>();
            _PlanRepository = new Mock<IPlanRepository>();
            dataRepositoryFactory = new Mock<IDataRepositoryFactory>();
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(_GetMockStandardDaypartRepository().Object);
            dataRepositoryFactory.Setup(s => s.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepository.Object);
        }

        private Mock<IStandardDaypartRepository> _GetMockStandardDaypartRepository()
        {
            var standardDaypartRepository = new Mock<IStandardDaypartRepository>();

            standardDaypartRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);

            standardDaypartRepository.Setup(s => s.GetAllStandardDaypartsWithAllData())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithFullData);

            var testDefaultDays = DaypartsTestData.GetDayIdsFromStandardDayparts();
            standardDaypartRepository.Setup(s => s.GetDayIdsFromStandardDayparts(It.IsAny<List<int>>()))
                .Returns<List<int>>((ids) =>
                {
                    var items = new List<int>();
                    foreach (var id in ids)
                    {
                        items.AddRange(testDefaultDays[id]);
                    }
                    return items.Distinct().ToList();
                });

            return standardDaypartRepository;
        }

        [Test]
        public void LockPlan_ToggleOn()
        {
            // Arrange
            var service = _GetService(true);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            int planId = 291;
            bool expectedResult = true;
            var key = KeyHelper.GetStationLockingKey(planId);

            _PlanRepository
             .Setup(s => s.GetPlanNameById(It.IsAny<int>()))
           .Returns("broadcast_station : 291");
            _LockingService
             .Setup(s => s.LockObject(It.IsAny<string>()))
           .Returns(new BroadcastLockResponse
           {
               Key = "broadcast_plan : 298",
               LockTimeoutInSeconds =900,
               LockedUserId = null,
               Success = true,
               Error = null
           });
            var result = service.LockPlan(planId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }

        [Test]
        public void UnlockPlan_ToggleOn()
        {
            // Arrange
            var service = _GetService(true);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            int planId = 291;
            bool expectedResult = true;
            var key = KeyHelper.GetStationLockingKey(planId);

            _PlanRepository
             .Setup(s => s.GetPlanNameById(It.IsAny<int>()))
           .Returns("broadcast_station : 291");
            _LockingService
             .Setup(s => s.ReleaseObject(It.IsAny<string>()))
           .Returns(new BroadcastReleaseLockResponse
           {
               Key = "broadcast_plan : 298",
               Success = true,
               Error = null
           });
            var result = service.UnlockPlan(planId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }

        [Test]
        public void LockCampaign_ToggleOn()
        {
            // Arrange
            var service = _GetService(true);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            int campaignId = 271;
            bool expectedResult = true;
            var key = KeyHelper.GetCampaignLockingKey(campaignId);
            _LockingService
             .Setup(s => s.LockObject(It.IsAny<string>()))
           .Returns(new BroadcastLockResponse
           {
               Key = "broadcast_plan : 298",
               LockTimeoutInSeconds = 900,
               LockedUserId = null,
               Success = true,
               Error = null
           });
            var result = service.LockCampaigns(campaignId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }

        [Test]
        public void UnlockCampaign_ToggleOn()
        {
            // Arrange
            var service = _GetService(true);
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            int campaignId = 2711;
            bool expectedResult = true;
            var key = KeyHelper.GetCampaignLockingKey(campaignId);
            _LockingService
             .Setup(s => s.ReleaseObject(It.IsAny<string>()))
           .Returns(new BroadcastReleaseLockResponse
           {
               Key = "broadcast_campaign : 171",
               Success = true,
               Error = null
           });
            var result = service.UnlockCampaigns(campaignId);
            // Assert
            Assert.AreEqual(expectedResult, result.Success);
        }


    }
}
