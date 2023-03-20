using Services.Broadcast.ApplicationServices;
using System;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    internal class BroadcastLockingManagerApplicationServiceStub : IBroadcastLockingManagerApplicationService
    {
        public object GetNotUserBasedLockObjectForKey(string key)
        {
            var result = new Object();

            return result;
        }

        public LockResponse LockObject(string key)
        {
            var result = new LockResponse
            {
                Key = key,
                Success = true,
                LockedUserName = "StubUser"
            };
            return result;
        }

        public ReleaseLockResponse ReleaseObject(string key)
        {
            var result = new ReleaseLockResponse
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

        public LockResponse GetLockObject(string key)
        {
            var result = new LockResponse
            {
                Key = key,
                Success = true
            };
            return result;
        }
    }
}
