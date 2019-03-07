using Common.Services;
using Common.Services.ApplicationServices;
using Common.Systems.LockTokens;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.BusinessEngines
{
    public interface ILockingEngine : IApplicationService
    {
        void LockStations(Dictionary<int, string> stationsDict, List<int> lockedStationIds, List<IDisposable> stationLocks);

        void UnlockStations(List<int> lockedStationIds, List<IDisposable> stationLocks);

        LockResponse LockStation(int stationId);

        ReleaseLockResponse UnlockStation(int stationId);
    }

    public class LockingEngine : ILockingEngine
    {
        private readonly ILockingManagerApplicationService _LockingManager;
        private readonly ISMSClient _SmsClient;

        public LockingEngine(
            ILockingManagerApplicationService lockingManager,
            ISMSClient smsClient)
        {
            _LockingManager = lockingManager;
            _SmsClient = smsClient;
        }

        public void LockStations(
            Dictionary<int, string> stationsDict, 
            List<int> lockedStationIds,
            List<IDisposable> stationLocks)
        {
            foreach (var station in stationsDict)
            {
                var lockResult = LockStation(station.Key);

                if (lockResult.Success)
                {
                    lockedStationIds.Add(station.Key);
                    stationLocks.Add(new BomsLockManager(_SmsClient, new StationToken(station.Key)));
                }
                else
                {
                    throw new ApplicationException($"Unable to update station. Station locked for editing {station.Value}.");
                }
            }
        }

        public void UnlockStations(List<int> lockedStationIds, List<IDisposable> stationLocks)
        {
            foreach (var stationId in lockedStationIds)
            {
                UnlockStation(stationId);
            }

            foreach (var stationLock in stationLocks)
            {
                stationLock.Dispose();
            }
        }

        public LockResponse LockStation(int stationId)
        {
            return _LockingManager.LockObject($"broadcast_station : {stationId}");
        }

        public ReleaseLockResponse UnlockStation(int stationId)
        {
            return _LockingManager.ReleaseObject($"broadcast_station : {stationId}");
        }
    }
}
