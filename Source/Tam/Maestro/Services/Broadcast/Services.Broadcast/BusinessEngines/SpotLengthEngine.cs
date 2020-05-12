using Common.Services.ApplicationServices;
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
        /// <summary>
        /// Gets the spot length identifier by value.
        /// </summary>
        /// <param name="spotLengthValue">The spot length value.</param>
        /// <returns>Spot length id</returns>
        int GetSpotLengthIdByValue(int spotLengthValue);

        /// <summary>
        /// Gets the spot length value by identifier.
        /// </summary>
        /// <param name="spotLengthId">The spot length identifier.</param>
        /// <returns>Spot length value</returns>
        int GetSpotLengthValueById(int spotLengthId);

        /// <summary>
        /// Check if Spot length exists.
        /// </summary>
        /// <param name="spotLengthValue">The spot length value.</param>
        /// <returns>True or false</returns>
        bool SpotLengthExists(int spotLengthValue);

        /// <summary>
        /// Checks if the spot length id exists.
        /// </summary>
        /// <param name="spotLengthId">The spot length identifier.</param>
        /// <returns>True or false</returns>
        bool SpotLengthIdExists(int spotLengthId);

        /// <summary>
        /// Gets all the spot lengths.
        /// </summary>
        /// <returns>Returns a dictionary of lengths where key is the length and value is the Id</returns>
        Dictionary<int, int> GetSpotLengths();
    }

    public class SpotLengthEngine : ISpotLengthEngine
    {
        private readonly Dictionary<int, int> _SpotLengthsDict;

        /// <inheritdoc/>
        public SpotLengthEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _SpotLengthsDict = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
        }

        /// <inheritdoc/>
        public Dictionary<int, int> GetSpotLengths()
        {
            return _SpotLengthsDict;
        }

        /// <inheritdoc/>
        public int GetSpotLengthIdByValue(int spotLengthValue)
        {
            if (_SpotLengthsDict.TryGetValue(spotLengthValue, out var spotLengthId))
            {
                return spotLengthId;
            }

            throw new Exception($"Invalid spot length value: '{spotLengthValue}' found");
        }

        /// <inheritdoc/>
        public int GetSpotLengthValueById(int spotLengthId)
        {
            var spotLength = _SpotLengthsDict.Single(x => x.Value == spotLengthId, $"Invalid spot length id: '{spotLengthId}' found");
            return spotLength.Key;
        }

        /// <inheritdoc/>
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
