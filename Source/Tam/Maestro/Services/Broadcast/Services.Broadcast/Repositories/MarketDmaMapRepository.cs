using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IMarketDmaMapRepository : IDataRepository
    {
        List<market_dma_map> GetMarketMapFromMarketCodes(List<int> marketCodes);
    }

    public class MarketDmaMapRepository : BroadcastRepositoryBase, IMarketDmaMapRepository
    {

        public MarketDmaMapRepository(
                    ISMSClient pSmsClient, 
                    IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
                    ITransactionHelper pTransactionHelper) 
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<market_dma_map> GetMarketMapFromMarketCodes(List<int> marketCodes)
        {
            return _InReadUncommitedTransaction(
                context => (
                        from m in context.market_dma_map
                        where marketCodes.Contains(m.market_code)
                        select m).ToList());
        }
    }    
}
