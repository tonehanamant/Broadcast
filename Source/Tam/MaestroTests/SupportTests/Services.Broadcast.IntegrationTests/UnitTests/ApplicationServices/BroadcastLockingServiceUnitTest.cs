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

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class BroadcastLockingServiceUnitTest
    {
        private Mock<ISMSClient> _SmsClientMock;
        private Mock<IBroadcastLockingService> _LockingManagerApplicationServiceMock;

        protected BroadcastLockingService _GetBroadcastLockingService()
        {
            return new BroadcastLockingService(
                _SmsClientMock.Object);
        }

        [SetUp]
        public void SetUp()
        {
            _LockingManagerApplicationServiceMock = new Mock<IBroadcastLockingService>();
            _SmsClientMock = new Mock<ISMSClient>();

        }
        [Test]
        public void GetLockObject_WhenNotLocked()
        {
            string key = "broadcast_261";
            _SmsClientMock
               .Setup(x => x.IsObjectLocked(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(false);
            var tc = _GetBroadcastLockingService();
            var result = tc.GetLockObject(key);
            Assert.IsTrue(result.Success);
        }
        [Test]
        public void GetLockObject_WhenLocked()
        {
            string key = "broadcast_plan : 171";
            _SmsClientMock
               .Setup(x => x.LockObject(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(
                    new LockResponse
                    {
                        Key = "broadcast_plan : 171",
                        Success = false,
                        LockTimeoutInSeconds = 900,
                        LockedUserId = "123",
                        LockedUserName = "TestUser",
                        Error = null
                    });

            _SmsClientMock
               .Setup(x => x.IsObjectLocked(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);
            var tc = _GetBroadcastLockingService();
            var result = tc.GetLockObject(key);
            Assert.IsFalse(result.Success);

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
        public void GetLockObject_Failed()
        {
            string key = "broadcast_261";
            _SmsClientMock
               .Setup(x => x.IsObjectLocked(It.IsAny<string>(), It.IsAny<string>()))
               .Callback(() =>
               {
                   throw new Exception("Throwing a test exception.");
               });
            var tc = _GetBroadcastLockingService();
            var result = Assert.Throws<Exception>(() => tc.GetLockObject(key));
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }

    }
}
