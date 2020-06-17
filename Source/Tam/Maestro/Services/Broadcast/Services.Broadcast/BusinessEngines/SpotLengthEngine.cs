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

        /// <summary>
        /// Gets the spot length multipliers.
        /// </summary>
        /// <returns>Dictionary of spot length id and spot multiplier</returns>
        Dictionary<int, double> GetSpotLengthMultipliers();
    }

    public class SpotLengthEngine : ISpotLengthEngine
    {
        private readonly Lazy<Dictionary<int, int>> _SpotLengthsDict;
        private readonly Lazy<Dictionary<int, double>> _SpotLengthMultipliers;

        /// <inheritdoc/>
        public SpotLengthEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _SpotLengthsDict = new Lazy<Dictionary<int, int>>(() => 
                        broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds());
            _SpotLengthMultipliers = new Lazy<Dictionary<int, double>>(() => 
                        broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthMultipliers());
        }

        /// <inheritdoc/>
        public Dictionary<int, int> GetSpotLengths()
        {
            return _SpotLengthsDict.Value;
        }

        /// <inheritdoc/>
        public Dictionary<int, double> GetSpotLengthMultipliers()
        {
            return _SpotLengthMultipliers.Value.ToDictionary(x=> GetSpotLengthIdByValue(x.Key), x=>x.Value);
        }

        /// <inheritdoc/>
        public int GetSpotLengthIdByValue(int spotLengthValue)
        {
            if (_SpotLengthsDict.Value.TryGetValue(spotLengthValue, out var spotLengthId))
            {
                return spotLengthId;
            }

            throw new Exception($"Invalid spot length value: '{spotLengthValue}' found");
        }

        /// <inheritdoc/>
        public int GetSpotLengthValueById(int spotLengthId)
        {
            var spotLength = _SpotLengthsDict.Value.Single(x => x.Value == spotLengthId, $"Invalid spot length id: '{spotLengthId}' found");
            return spotLength.Key;
        }

        /// <inheritdoc/>
        public bool SpotLengthExists(int spotLengthValue)
        {
            return _SpotLengthsDict.Value.ContainsKey(spotLengthValue);
        }

        /// <inheritdoc/>
        public bool SpotLengthIdExists(int spotLengthId)
        {
            return _SpotLengthsDict.Value.ContainsValue(spotLengthId);
        }
    }
}
