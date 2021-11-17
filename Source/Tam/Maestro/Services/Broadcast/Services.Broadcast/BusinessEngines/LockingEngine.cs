﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Systems.LockTokens;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Repositories;
using Common.Services.Repositories;

namespace Services.Broadcast.BusinessEngines
{
    public interface ILockingEngine : IApplicationService
    {
        void LockStations(Dictionary<int, string> stationsDict, List<int> lockedStationIds, List<IDisposable> stationLocks);

        void UnlockStations(List<int> lockedStationIds, List<IDisposable> stationLocks);

        BroadcastLockResponse LockStation(int stationId);

        BroadcastReleaseLockResponse UnlockStation(int stationId);

        PlanLockResponse LockPlan(int planId);

        BroadcastReleaseLockResponse UnlockPlan(int planId);
    }

    public class LockingEngine : ILockingEngine
    {
        private readonly IBroadcastLockingManagerApplicationService _LockingManager;
        private readonly IBroadcastLockingService _LockingService;
        internal static Lazy<bool> _IsLockingConsolidationEnabled;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IBroadcastLockingManagerApplicationService _LockingManagerApplicationService;
        private readonly IPlanRepository _PlanRepository;
        public LockingEngine(
            IBroadcastLockingManagerApplicationService lockingManager,
            IBroadcastLockingService lockingService, IFeatureToggleHelper featureToggleHelper, IBroadcastLockingManagerApplicationService lockingManagerApplicationService, IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _LockingManager = lockingManager;
            _LockingService = lockingService;
            _FeatureToggleHelper = featureToggleHelper;
            _IsLockingConsolidationEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_LOCKING_CONSOLIDATION));
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
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

        public PlanLockResponse LockPlan(int planId)
        {
            PlanLockResponse planLockResponse = null;
            var key = KeyHelper.GetStationLockingKey(planId);
            var planName = _PlanRepository.GetPlanNameById(planId);
            if (_IsLockingConsolidationEnabled.Value)
            {
              var  broadcastLockResponse = _LockingService.LockObject(key);
                if (broadcastLockResponse != null)
                {
                    planLockResponse = new PlanLockResponse
                    {
                        Key = broadcastLockResponse.Key,
                        Success = broadcastLockResponse.Success,
                        LockTimeoutInSeconds = broadcastLockResponse.LockTimeoutInSeconds,
                        LockedUserId = broadcastLockResponse.LockedUserId,
                        Error = broadcastLockResponse.Error,
                        PlanName = planName
                    };
                }

            }
            else
            {
                var lockingResponse = _LockingManagerApplicationService.LockObject(key);
                if (lockingResponse != null)
                {
                    planLockResponse = new PlanLockResponse
                    {
                        Key = lockingResponse.Key,
                        Success = lockingResponse.Success,
                        LockTimeoutInSeconds = lockingResponse.LockTimeoutInSeconds,
                        LockedUserId = lockingResponse.LockedUserId,
                        LockedUserName = lockingResponse.LockedUserName,
                        Error = lockingResponse.Error,
                        PlanName = planName
                    };
                }
            }
            return planLockResponse;
        }

        public BroadcastReleaseLockResponse UnlockPlan(int planId)
        {
            BroadcastReleaseLockResponse broadcastReleaseLockResponse = null;
            var key = KeyHelper.GetStationLockingKey(planId);
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
