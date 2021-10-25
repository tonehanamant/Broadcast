using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using market = EntityFrameworkMapping.Broadcast.market;

namespace Services.Broadcast.Repositories
{
    public interface IMarketRepository : IDataRepository
    {
        List<market> GetMarkets();
        List<market> GetMarketsByMarketCodes(List<int> marketCodes);
        List<LookupDto> GetMarketDtos();
        /// <summary>
        /// Returns a list of markets which are filtered by their geography names
        /// </summary>
        /// <param name="geographyNames">Geography names of markets</param>
        List<market> GetMarketsByGeographyNames(IEnumerable<string> geographyNames);
        LookupDto GetMarket(int marketCode);
    }

    public class MarketRepository : BroadcastRepositoryBase, IMarketRepository
    {
        public MarketRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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
                    select new LookupDto() { Display = m.geography_name, Id = m.market_code }).ToList());
        }

        public List<market> GetMarketsByMarketCodes(List<int> marketCodes)
        {
            return _InReadUncommitedTransaction(
                context =>
                    (from m in context.markets
                     where marketCodes.Contains(m.market_code)
                     select m).ToList());
        }

        public LookupDto GetMarket(int marketCode)
        {
            return _ConvertToLookupDto(_InReadUncommitedTransaction(
                context => context.markets.Single(m => m.market_code == marketCode)));
        }

        public List<market> GetMarketsByGeographyNames(IEnumerable<string> geographyNames)
        {
            return _InReadUncommitedTransaction(
                context =>
                    (from m in context.markets
                     where geographyNames.Contains(m.geography_name)
                     select m).ToList());
        }

        private LookupDto _ConvertToLookupDto(market market)
        {
            if (market == null)
                return null;

            return new LookupDto(market.market_code, market.geography_name);
        }

    }
}
