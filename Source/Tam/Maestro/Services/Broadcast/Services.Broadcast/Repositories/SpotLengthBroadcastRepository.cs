using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using System;

namespace Services.Broadcast.Repositories
{
    public interface ISpotLengthRepository : IDataRepository
    {
        /// <summary>
        /// Gets the spot length by spot id
        /// </summary>
        /// <param name="spotLengthId">Spot id to filter by</param>
        /// <returns>Spot length value</returns>
        int GetSpotLengthById(int spotLengthId);

        List<LookupDto> GetSpotLengths();

        /// <summary>
        /// Returns the spot lengths  based on a list of spot length values
        /// </summary>
        /// <param name="lengthList">Spot length values</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetSpotLengthsByLengths(List<int> lengthList);

        /// <summary>
        /// Returns a dictionary of spot lengths where key is the length and value is the Id.
        /// </summary>
        Dictionary<int, int> GetSpotLengthIdsByDuration();

        /// <summary>
        /// Returns a dictionary of spot lengths where key is id and value is length.
        /// </summary>
        /// <returns></returns>
        Dictionary<int, int> GetSpotLengthDurationsById();

        Dictionary<int, double> GetDeliveryMultipliersBySpotLength();
        Dictionary<int, double> GetDeliveryMultipliersBySpotLengthId();
        Dictionary<int, decimal> GetSpotLengthIdsAndCostMultipliers(bool addInventoryCostPremium);
    }

    public class SpotLengthBroadcastRepository : BroadcastRepositoryBase, ISpotLengthRepository
    {
        public SpotLengthBroadcastRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        private static Dictionary<int, int> _SpotLengthDictionary = new Dictionary<int, int>();

        /// <summary>
        /// Gets the spot length by spot id
        /// </summary>
        /// <param name="spotLengthId">Spot id to filter by</param>
        /// <returns>Spot length value</returns>
        public int GetSpotLengthById(int spotLengthId)
        {
            if (_SpotLengthDictionary.TryGetValue(spotLengthId, out int spotLength))
                return spotLength;

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(context =>
                {
                    if (_SpotLengthDictionary.Count == 0)
                        _SpotLengthDictionary = context.spot_lengths.ToDictionary(s => s.id, s => s.length);

                    var single = (from x in context.spot_lengths
                                  where x.id == spotLengthId
                                  select x.length).Single(string.Format("Unable to find spot length with id: {0}", spotLengthId));

                    _SpotLengthDictionary[spotLengthId] = single;
                    return single;
                });
            }
        }

        public List<LookupDto> GetSpotLengths()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from x in context.spot_lengths
                                orderby x.order_by
                                select new LookupDto
                                {
                                    Id = x.id,
                                    Display = x.length.ToString()
                                }).ToList());
            }
        }

        /// <inheritdoc />
        public Dictionary<int, int> GetSpotLengthIdsByDuration()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => context.spot_lengths.ToDictionary(y => y.length, x => x.id));
            }
        }

        /// <inheritdoc />
        public Dictionary<int, int> GetSpotLengthDurationsById()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => context.spot_lengths.ToDictionary(y => y.id, x => x.length));
            }
        }

        /// <summary>
        /// Returns the spot lengths  based on a list of spot length values
        /// </summary>
        /// <param name="lengthList">Spot length values</param>
        /// <returns>List of LookupDto objects</returns>
        public List<LookupDto> GetSpotLengthsByLengths(List<int> lengthList)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from x in context.spot_lengths
                                where lengthList.Contains(x.length)
                                select new LookupDto
                                {
                                    Id = x.id,
                                    Display = x.length.ToString()
                                }).ToList());
            }
        }

        public Dictionary<int, double> GetDeliveryMultipliersBySpotLength()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from s in context.spot_lengths
                                select s).ToDictionary(a => a.length, a => a.delivery_multiplier));
            }
        }

        public Dictionary<int, double> GetDeliveryMultipliersBySpotLengthId()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from s in context.spot_lengths
                                select s).ToDictionary(a => a.id, a => a.delivery_multiplier));
            }
        }

        public Dictionary<int, decimal> GetSpotLengthIdsAndCostMultipliers(bool addInventoryCostPremium)
        {
            var result = _InReadUncommitedTransaction(
                context =>
                {
                    var spotLengthCostMultipliers = context.spot_length_cost_multipliers
                    .ToDictionary(x => x.spot_length_id, y => addInventoryCostPremium ? 
                    Convert.ToDecimal(y.cost_multiplier) + y.inventory_cost_premium 
                    : Convert.ToDecimal(y.cost_multiplier));

                    return spotLengthCostMultipliers;
                });

            return result;
        }
    }
}
