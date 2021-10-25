using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IMarketDmaMapRepository : IDataRepository
    {
        List<market_dma_map> GetMarketMapFromMarketCodes(IEnumerable<int> marketCodes);
    }

    public class MarketDmaMapRepository : BroadcastRepositoryBase, IMarketDmaMapRepository
    {

        public MarketDmaMapRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public List<market_dma_map> GetMarketMapFromMarketCodes(IEnumerable<int> marketCodes)
        {
            return _InReadUncommitedTransaction(
                context => (
                        from m in context.market_dma_map
                        where marketCodes.Contains(m.market_code)
                        select m).ToList());
        }
    }    
}
