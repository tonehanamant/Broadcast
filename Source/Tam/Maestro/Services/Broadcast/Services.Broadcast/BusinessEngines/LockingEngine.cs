using Common.Services;
using Common.Services.ApplicationServices;
using Common.Systems.LockTokens;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;
using Services.Broadcast.Entities;

namespace Services.Broadcast.BusinessEngines
{
    public interface ILockingEngine : IApplicationService
    {
        void LockStations(Dictionary<int, string> stationsDict, List<int> lockedStationIds, List<IDisposable> stationLocks);

        void UnlockStations(List<int> lockedStationIds, List<IDisposable> stationLocks);

        BroadcastLockResponse LockStation(int stationId);

        BroadcastReleaseLockResponse UnlockStation(int stationId);
    }

    public class LockingEngine : ILockingEngine
    {
        private readonly IBroadcastLockingManagerApplicationService _LockingManager;
        private readonly IBroadcastLockingService _LockingService;
        internal static Lazy<bool> _IsLockingConsolidationEnabled;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public LockingEngine(
            IBroadcastLockingManagerApplicationService lockingManager,
            IBroadcastLockingService lockingService, IFeatureToggleHelper featureToggleHelper)
        {
            _LockingManager = lockingManager;
            _LockingService = lockingService;
            _FeatureToggleHelper = featureToggleHelper;
            _IsLockingConsolidationEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_LOCKING_CONSOLIDATION));
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
        }

        public BroadcastLockResponse LockStation(int stationId)
        {
            BroadcastLockResponse broadcastLockResponse = null;
            var key = KeyHelper.GetStationLockingKey(stationId);
            if (_IsLockingConsolidationEnabled.Value)
            {
                broadcastLockResponse = _LockingService.LockObject(key);
            }
            else
            {
                var lockResponse = _LockingManager.LockObject(key);

                if (lockResponse != null)
                {
                    broadcastLockResponse = new BroadcastLockResponse
                    {
                        Error = lockResponse.Error,
                        Key = lockResponse.Key,
                        LockedUserId = lockResponse.LockedUserId,
                        LockTimeoutInSeconds = lockResponse.LockTimeoutInSeconds,
                        Success = lockResponse.Success
                    };
                }
            }
            return broadcastLockResponse;
        }

        public BroadcastReleaseLockResponse UnlockStation(int stationId)
        {
            BroadcastReleaseLockResponse broadcastReleaseLockResponse = null;
            var key = KeyHelper.GetStationLockingKey(stationId);
            if (_IsLockingConsolidationEnabled.Value)
            {
                broadcastReleaseLockResponse = _LockingService.ReleaseObject(key);
            }
            else
            {
                var releaseLockResponse = _LockingManager.ReleaseObject(key);
                if (releaseLockResponse != null)
                {
                    broadcastReleaseLockResponse = new BroadcastReleaseLockResponse
                    {
                        Error = releaseLockResponse.Error,
                        Key = releaseLockResponse.Key,
                        Success = releaseLockResponse.Success
                    };
                }
            }
            return broadcastReleaseLockResponse;
        }
    }
}
