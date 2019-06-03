using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.Cache
{
    public interface IMarketCoverageCache
    {
        Dictionary<int, double> GetMarketCoverages(IEnumerable<int> marketIds);
    }

    public class MarketCoverageCache : IMarketCoverageCache
    {
        private readonly Dictionary<int, double> _MarketCoverages;

        public MarketCoverageCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            using (var scope = new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                _MarketCoverages = broadcastDataRepositoryFactory
                        .GetDataRepository<IMarketCoverageRepository>()
                        .GetLatestMarketCoverages()
                        .MarketCoveragesByMarketCode;
            }
        }

        public Dictionary<int, double> GetMarketCoverages(IEnumerable<int> marketIds)
        {
            return _MarketCoverages.Where(x => marketIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
