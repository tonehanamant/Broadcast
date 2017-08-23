using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using market = EntityFrameworkMapping.Broadcast.market;

namespace Services.Broadcast.Repositories
{
    public interface IMarketRepository : IDataRepository
    {
        List<market> GetMarkets();
        List<market> GetMarketsByMarketCodes(List<int> marketCodes);
        List<LookupDto> GetMarketDtos();
    }

    public class MarketRepository: BroadcastRepositoryBase, IMarketRepository
    {

        public MarketRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<market> GetMarkets()
        {
            return _InReadUncommitedTransaction(
                context => (
                        from m in context.markets
                        select m).ToList());
        }

        public List<LookupDto> GetMarketDtos()
        {
            return _InReadUncommitedTransaction(
                context => (
                    from m in context.markets
                    select new LookupDto(){Display = m.geography_name, Id = m.market_code}).ToList());
        } 

        public List<market> GetMarketsByMarketCodes(List<int> marketCodes)
        {
            return _InReadUncommitedTransaction(
                context =>
                    (from m in context.markets
                     where marketCodes.Contains(m.market_code)
                     select m).ToList());
        }
    }    
}
