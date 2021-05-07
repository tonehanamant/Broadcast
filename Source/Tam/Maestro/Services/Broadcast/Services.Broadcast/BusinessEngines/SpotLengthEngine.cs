using Common.Services.ApplicationServices;
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
        /// Gets the spot length delivery multipliers.
        /// </summary>
        /// <returns>Dictionary of spot length id and spot multiplier</returns>
        Dictionary<int, double> GetDeliveryMultipliers();

        /// <summary>
        /// Gets the spot length cost multipliers.
        /// </summary>
        /// <returns>Dictionary of spot length id and spot multiplier</returns>
        Dictionary<int, decimal> GetCostMultipliers();

        double GetDeliveryMultiplierBySpotLengthId(int spotLengthId);

        decimal GetSpotCostMultiplierBySpotLengthId(int spotLengthId);
    }

    public class SpotLengthEngine : ISpotLengthEngine
    {
        private readonly Dictionary<int, int> _SpotLengthIdByValue;
        private readonly Dictionary<int, int> _SpotLengthValueById;
        private readonly Dictionary<int, double> _DeliveryMultipliersBySpotLengthId;
        private readonly Dictionary<int, decimal> _SpotCostMultipliersBySpotLengthId;

        /// <inheritdoc/>
        public SpotLengthEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            var spotLengthRepository = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _SpotLengthIdByValue = spotLengthRepository.GetSpotLengthIdsByDuration();
            _SpotLengthValueById = _SpotLengthIdByValue.ToDictionary(x => x.Value, x => x.Key);
            _DeliveryMultipliersBySpotLengthId = spotLengthRepository.GetDeliveryMultipliersBySpotLengthId();
            _SpotCostMultipliersBySpotLengthId = spotLengthRepository.GetSpotLengthIdsAndCostMultipliers(true);
        }

        /// <inheritdoc/>
        public Dictionary<int, int> GetSpotLengths()
        {
            return _SpotLengthIdByValue;
        }

        /// <inheritdoc/>
        public Dictionary<int, double> GetDeliveryMultipliers()
        {
            return _DeliveryMultipliersBySpotLengthId;
        }

        /// <inheritdoc/>
        public Dictionary<int, decimal> GetCostMultipliers()
        {
            return _SpotCostMultipliersBySpotLengthId;
        }

        public double GetDeliveryMultiplierBySpotLengthId(int spotLengthId)
        {
            if (_DeliveryMultipliersBySpotLengthId.TryGetValue(spotLengthId, out var deliveryMultiplier))
            {
                return deliveryMultiplier;
            }

            throw new Exception($"Invalid spot length id: '{spotLengthId}' found");
        }

        public decimal GetSpotCostMultiplierBySpotLengthId(int spotLengthId)
        {
            if (_SpotCostMultipliersBySpotLengthId.TryGetValue(spotLengthId, out var spotCostMultiplier))
            {
                return spotCostMultiplier;
            }

            throw new Exception($"Invalid spot length id: '{spotLengthId}' found");
        }

        /// <inheritdoc/>
        public int GetSpotLengthIdByValue(int spotLengthValue)
        {
            if (_SpotLengthIdByValue.TryGetValue(spotLengthValue, out var spotLengthId))
            {
                return spotLengthId;
            }

            throw new Exception($"Invalid spot length value: '{spotLengthValue}' found");
        }

        /// <inheritdoc/>
        public int GetSpotLengthValueById(int spotLengthId)
        {
            if (_SpotLengthValueById.TryGetValue(spotLengthId, out var spotLengthValue))
            {
                return spotLengthValue;
            }

            throw new Exception($"Invalid spot length id: '{spotLengthId}' found");
        }

        /// <inheritdoc/>
        public bool SpotLengthExists(int spotLengthValue)
        {
            return _SpotLengthIdByValue.ContainsKey(spotLengthValue);
        }

        /// <inheritdoc/>
        public bool SpotLengthIdExists(int spotLengthId)
        {
            return _SpotLengthValueById.ContainsKey(spotLengthId);
        }
    }
}
