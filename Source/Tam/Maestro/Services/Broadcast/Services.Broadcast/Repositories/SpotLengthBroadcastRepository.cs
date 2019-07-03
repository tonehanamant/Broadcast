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
        /// Returns a dictionary of lengths where key is the length and value is the Id
        /// </summary>
        Dictionary<int, int> GetSpotLengthAndIds();
        
        Dictionary<int, double> GetSpotLengthMultipliers();
        Dictionary<int, double> GetSpotLengthIdsAndCostMultipliers();
    }

    public class SpotLengthBroadcastRepository : BroadcastRepositoryBase, ISpotLengthRepository
    {
        public SpotLengthBroadcastRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

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

        /// <summary>
        /// Returns a dictionary of lengths where key is the length and value is the Id
        /// </summary>
        public Dictionary<int, int> GetSpotLengthAndIds()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => context.spot_lengths.ToDictionary(y => y.length, x => x.id));
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

        public Dictionary<int, double> GetSpotLengthMultipliers()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from s in context.spot_lengths
                                select s).ToDictionary(a => a.length, a => a.delivery_multiplier));
            }
        }

        public Dictionary<int, double> GetSpotLengthIdsAndCostMultipliers()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.spot_length_cost_multipliers.ToDictionary(x => x.spot_length_id, y => y.cost_multiplier);
                });
        }
    }
}
