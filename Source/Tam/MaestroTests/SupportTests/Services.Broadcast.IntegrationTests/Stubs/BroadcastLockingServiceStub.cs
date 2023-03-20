using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    internal class BroadcastLockingServiceStub : IBroadcastLockingService
    {
        public object GetNotUserBasedLockObjectForKey(string key)
        {
            throw new NotImplementedException("Stub has not implemented this.");
        }

        public BroadcastLockResponse GetLockObject(string key)
        {
            var result = new BroadcastLockResponse
            {
                Key = key,
                Success = true
            };
            return result;
        }

        public bool IsObjectLocked(string key)
        {
            throw new NotImplementedException("Stub has not implemented this.");
        }

        public BroadcastLockResponse LockObject(string key)
        {
            var result = new BroadcastLockResponse
            {
                Key = key,
                Success = true,
                LockedUserName = "StubUser"
            };
            return result;
        }

        public BroadcastReleaseLockResponse ReleaseObject(string key)
        {
            var result = new BroadcastReleaseLockResponse
            {
                Key = key,
                Success = true
            };
            return result;
        }
    }
}
