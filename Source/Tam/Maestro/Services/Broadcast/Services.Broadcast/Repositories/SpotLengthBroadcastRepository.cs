using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface ISpotLengthRepository : IDataRepository
    {
        int GetSpotLengthById(int spotLengthId);
        List<LookupDto> GetSpotLengths();
        List<LookupDto> GetSpotLengthsByLength(List<int> lengthList);
        Dictionary<int, int> GetSpotLengthAndIds();
        Dictionary<int, int> GetSpotLengthsById();
        Dictionary<int, double> GetSpotLengthMultipliers();
    }

    public class SpotLengthBroadcastRepository : BroadcastRepositoryBase, ISpotLengthRepository
    {
        public SpotLengthBroadcastRepository(ISMSClient pSmsClient, IBroadcastContextFactory pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        private static Dictionary<int, int> _SpotLengthDictionary = new Dictionary<int, int>();
        public int GetSpotLengthById(int spotLengthId)
        {
            int spotLength;
            if (_SpotLengthDictionary.TryGetValue(spotLengthId, out spotLength))
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
        /// Returns dictionary where key is the length, the value is the Id
        /// </summary>
        public Dictionary<int, int> GetSpotLengthAndIds()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => context.spot_lengths.ToDictionary(y => y.length, x => x.id));
            }
        }

        public Dictionary<int, int> GetSpotLengthsById()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => context.spot_lengths.ToDictionary(y => y.id, x => x.length));
            }
        }

        public List<LookupDto> GetSpotLengthsByLength(List<int> lengthList)
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
    }
}
