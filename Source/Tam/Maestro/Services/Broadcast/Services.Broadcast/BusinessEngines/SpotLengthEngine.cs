﻿using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface ISpotLengthEngine : IApplicationService
    {
        int GetSpotLengthIdByValue(int spotLengthValue);

        int GetSpotLengthValueById(int spotLengthId);

        bool SpotLengthExists(int spotLengthValue);

        /// <summary>
        /// Checks if the spot length id exists.
        /// </summary>
        /// <param name="spotLengthId">The spot length identifier.</param>
        /// <returns>True or false</returns>
        bool SpotLengthIdExists(int spotLengthId);
    }

    public class SpotLengthEngine : ISpotLengthEngine
    {
        private readonly Dictionary<int, int> _SpotLengthsDict;

        public SpotLengthEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _SpotLengthsDict = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
        }

        public int GetSpotLengthIdByValue(int spotLengthValue)
        {
            if (_SpotLengthsDict.TryGetValue(spotLengthValue, out var spotLengthId))
            {
                return spotLengthId;
            }

            throw new Exception($"Invalid spot length value: '{spotLengthValue}' found");
        }

        public int GetSpotLengthValueById(int spotLengthId)
        {
            var spotLength = _SpotLengthsDict.Single(x => x.Value == spotLengthId, $"Invalid spot length id: '{spotLengthId}' found");
            return spotLength.Key;
        }

        public bool SpotLengthExists(int spotLengthValue)
        {
            return _SpotLengthsDict.ContainsKey(spotLengthValue);
        }

        /// <inheritdoc/>
        public bool SpotLengthIdExists(int spotLengthId)
        {
            return _SpotLengthsDict.ContainsValue(spotLengthId);
        }
    }
}
