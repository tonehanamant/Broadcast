using Moq;
using NUnit.Framework;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Locking;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using System;
using System.Net.Http;

namespace Services.Broadcast.IntegrationTests.UnitTests.Cache
{
    [TestFixture]
    public class GeneralLockingCacheUnitTests
    {
        [Test]
        public void LockObject()
        {
            // Arrange
            var lockRequest = _GetLockRequest();
            var lockResponse = _GetLockResponse();
            var apiTokenManager = new Mock<IApiTokenManager>();
            var generalLockingApiclient = new Mock<IGeneralLockingApiClient>();
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            var client = new Mock<HttpClient>();
            generalLockingApiclient.Setup(x => x.LockObject(lockRequest))
                .Returns(lockResponse);
            
            var tc = new LockingCacheStub(generalLockingApiclient.Object, featureToggleHelper.Object, configurationSettingsHelper.Object);
            // Act
            var result = tc.LockObject(lockRequest);
            // Assert
            Assert.AreEqual(lockResponse, result);
            generalLockingApiclient.Verify(s => s.LockObject(lockRequest), Times.Once);
        }

        [Test]
        public void LockObject_AlreadyLock()
        {
            // Arrange
            var lockRequest = _GetLockRequest();
            var lockResponse = _GetLockResponse_AlreadyLocked();
            var apiTokenManager = new Mock<IApiTokenManager>();
            var generalLockingApiclient = new Mock<IGeneralLockingApiClient>();
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            var client = new Mock<HttpClient>();
            generalLockingApiclient.Setup(x => x.LockObject(lockRequest))
                .Returns(lockResponse);

            var tc = new LockingCacheStub(generalLockingApiclient.Object, featureToggleHelper.Object, configurationSettingsHelper.Object);
            // Act
            var result = tc.LockObject(lockRequest);
            // Assert
            Assert.AreEqual(lockResponse, result);
            generalLockingApiclient.Verify(s => s.LockObject(lockRequest), Times.Once);
        }

        [Test]
        public void ReleaseObject()
        {
            // Arrange
            var objectType = "plan";
            var objectId = "550";
            var releaseResponse = _GetReleaseLockResponse();
            var apiTokenManager = new Mock<IApiTokenManager>();
            var generalLockingApiclient = new Mock<IGeneralLockingApiClient>();
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            var client = new Mock<HttpClient>();
            generalLockingApiclient.Setup(x => x.ReleaseObject(objectType, objectId))
                .Returns(releaseResponse);
            var tc = new LockingCacheStub(generalLockingApiclient.Object, featureToggleHelper.Object, configurationSettingsHelper.Object);
            // Act
            var result = tc.ReleaseObject(objectType, objectId);
            // Assert
            Assert.AreEqual(releaseResponse, result);
            generalLockingApiclient.Verify(s => s.ReleaseObject(objectType, objectId), Times.Once);
        }
        [Test]
        public void IsObjectLocked()
        {
            // Arrange
            var objectType = "plan";
            var objectId = "550";
            var releaseResponse = true;
            var apiTokenManager = new Mock<IApiTokenManager>();
            var generalLockingApiclient = new Mock<IGeneralLockingApiClient>();
            var featureToggleHelper = new Mock<IFeatureToggleHelper>();
            var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
            var client = new Mock<HttpClient>();
            generalLockingApiclient.Setup(x => x.IsObjectLocked(objectType, objectId))
                .Returns(releaseResponse);
            var tc = new LockingCacheStub(generalLockingApiclient.Object, featureToggleHelper.Object, configurationSettingsHelper.Object);
            // Act
            var result = tc.IsObjectLocked(objectType, objectId);
            // Assert
            Assert.AreEqual(releaseResponse, result);
            generalLockingApiclient.Verify(s => s.IsObjectLocked(objectType, objectId), Times.Once);
        }
        private ReleaseLockResponse _GetReleaseLockResponse()
        {
            return new ReleaseLockResponse
            {
                Key = "Plan-550",
                Error = "No errors",
                Success = true
            };
        }
        private LockingResultResponse _GetLockResponse()
        {
            var result = new LockingResultResponse
            {
                Key = "Plan-550",
                LockedUserId = "",
                Success = true,
                Error = "No Errors",
                LockedUserName = "Test User",
                LockTimeoutInSeconds = 899


            };
            return result;
        }
        private LockingApiRequest _GetLockRequest()
        {
            return new LockingApiRequest
            {
                ObjectType = "Plan",
                ObjectId = "550",
                ExpirationTimeSpan = TimeSpan.FromMinutes(15),
                IsShared = false,
                SharedApplications = null
            };
        }

        private LockingResultResponse _GetLockResponse_AlreadyLocked()
        {
            var result = new LockingResultResponse
            {
                Key = "Plan-550",
                LockedUserId = "Test User",
                Success = false,
                Error = "Lock Request for 'Object Type: Plan ; Object ID:  550' is already locked by user 'Test User'",
                LockedUserName = "Test User",
                LockTimeoutInSeconds = 899
            };
            return result;
        }
    }
}
