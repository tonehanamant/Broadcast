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
        void LockStations(Dictionary<int, string> fileStationsDict, List<int> lockedStationCodes, List<IDisposable> stationLocks);

        void UnlockStations(List<int> lockedStationCodes, List<IDisposable> stationLocks);

        LockResponse LockStation(int stationCode);

        ReleaseLockResponse UnlockStation(int stationCode);
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
            Dictionary<int, string> fileStationsDict, 
            List<int> lockedStationCodes,
            List<IDisposable> stationLocks)
        {
            foreach (var fileStation in fileStationsDict)
            {
                var lockResult = LockStation(fileStation.Key);

                if (lockResult.Success)
                {
                    lockedStationCodes.Add(fileStation.Key);
                    stationLocks.Add(new BomsLockManager(_SmsClient, new StationToken(fileStation.Key)));
                }
                else
                {
                    throw new ApplicationException($"Unable to update station. Station locked for editing {fileStation.Value}.");
                }
            }
        }

        public void UnlockStations(List<int> lockedStationCodes, List<IDisposable> stationLocks)
        {
            foreach (var stationCode in lockedStationCodes)
            {
                UnlockStation(stationCode);
            }

            foreach (var stationLock in stationLocks)
            {
                stationLock.Dispose();
            }
        }

        public LockResponse LockStation(int stationCode)
        {
            return _LockingManager.LockObject($"broadcast_station : {stationCode}");
        }

        public ReleaseLockResponse UnlockStation(int stationCode)
        {
            return _LockingManager.ReleaseObject($"broadcast_station : {stationCode}");
        }
    }
}
