using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Tam.Maestro.Services.Clients;
using ApprovalTests;
using Services.Broadcast.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Entities.Locking;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class BroadcastLockingServiceUnitTest
    {
        private Mock<ISMSClient> _SmsClientMock;
        private Mock<IGeneralLockingApiClient> _GeneralLockingApiClient;
        private Mock<IBroadcastLockingService> _LockingManagerApplicationServiceMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleHelper;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelper;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        protected BroadcastLockingService _GetBroadcastLockingService(bool isLockingMigrationEnabled = false)
        {
            _LaunchDarklyClientStub = new LaunchDarklyClientStub();
            var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);
            return new BroadcastLockingService(
                _SmsClientMock.Object,
                _GeneralLockingApiClient.Object,
                _ConfigurationSettingsHelper.Object,
                featureToggleHelper
                );
        }

        [SetUp]
        public void SetUp()
        {
            _LockingManagerApplicationServiceMock = new Mock<IBroadcastLockingService>();
            _SmsClientMock = new Mock<ISMSClient>();
            _GeneralLockingApiClient = new Mock<IGeneralLockingApiClient>();
            _ConfigurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            _FeatureToggleHelper = new Mock<IFeatureToggleHelper>();
        }
        
        [Test]
        public void GetNotUserBasedLockObjectForKey()
        {
            string key = "broadcast_261";
            var tc = _GetBroadcastLockingService();
            var result = tc.GetNotUserBasedLockObjectForKey(key);
            Assert.IsNotNull(result);
        }
        [Test]
        public void LockObject_ToggleOn()
        {
            // Arrange
            var tc = _GetBroadcastLockingService(true);
            string key = "broadcast_campaign_261";
            var lockingResponse = _GetLockingResultResponse();
            var expectedResult = true;         
            var request = new LockingApiRequest
            {
                 ExpirationTimeSpan = TimeSpan.FromSeconds(15),
                 IsShared = false,
                 ObjectId = "261",
                 ObjectType = "broadcast_campaign",
                 SharedApplications = null
            };            

            _GeneralLockingApiClient.Setup(x => x.GetLockingRequest(It.IsAny<string>())).
                Returns(request);
            _GeneralLockingApiClient.Setup(x => x.LockObject(It.IsAny<LockingApiRequest>()))
                .Returns(lockingResponse);       
            var result = tc.LockObject(key);

            //Assert
            Assert.AreEqual(expectedResult, result.Success);
        }
        [Test]
        public void ReleaseObject_ToggleOn()
        {
            // Arrange
            var tc = _GetBroadcastLockingService(true);
            string key = "broadcast_campaign:261";
            var releaseResponse = _GetReleaseLockResponse();
            var expectedResult = true;
            var broadcastReleaseLockResponse = new BroadcastReleaseLockResponse
            {
                Error = "No Errors",
                Key = "broadcast_campaign:261",
                Success = true
            };
            _GeneralLockingApiClient.Setup(x => x.ReleaseObject(It.IsAny<string>(), It.IsAny<string>())).
               Returns(releaseResponse);
            //Act
            var result =  tc.ReleaseObject(key);
            //Assert             
            Assert.AreEqual(expectedResult, result.Success);
        }
        [Test]
        public void IsObjectLocked_ToggleOn()
        {
            //Arrange
            var tc = _GetBroadcastLockingService(true);
            string key = "broadcast_campaign:261";            
            _GeneralLockingApiClient.Setup(x=> x.IsObjectLocked(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            //Act
            var result = tc.IsObjectLocked(key);
            //Assert
            Assert.IsTrue(result);
        }
        private BroadcastLockResponse _GetLockResponse()
        {
            return new BroadcastLockResponse
            {
                 Error = "No Errors",
                 Key = "broadcast_campaign_261",
                 LockedUserId = "Test",
                 LockedUserName ="Test User",
                 LockTimeoutInSeconds = 900,
                 Success = true,
            };
        }
        private LockingResultResponse _GetLockingResultResponse()
        {
            return new LockingResultResponse
            {
                Error = "No Errors",
                Key = "broadcast_campaign_261",
                LockedUserId = "Test",
                LockedUserName = "Test User",
                LockTimeoutInSeconds = 900,
                Success = true,
            };

        }
        private Entities.Locking.ReleaseLockResponse _GetReleaseLockResponse()
        {
            return new Entities.Locking.ReleaseLockResponse
            {
                Error = "No Errors",
                Key = "broadcast_campaign:261",
                Success = true
            };
        }


    }
}
