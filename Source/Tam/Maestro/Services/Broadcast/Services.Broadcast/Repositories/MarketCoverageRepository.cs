using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IMarketCoverageRepository : IDataRepository
    {
        /// <summary>
        /// Removes all the existing market coverages and inserts new ones
        /// </summary>
        /// <param name="marketCoverages">Market coverages for inserting</param>
        void ResetMarketCoverages(IEnumerable<MarketCoverage> marketCoverage);

        /// <summary>
        /// Returns all the market coverages
        /// </summary>
        List<market_coverages> GetAll();
    }

    public class MarketCoverageRepository : BroadcastRepositoryBase, IMarketCoverageRepository
    {
        public MarketCoverageRepository(
            ISMSClient pSmsClient, 
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<market_coverages> GetAll()
        {
            return _InReadUncommitedTransaction(context => context.market_coverages.ToList());
        }

        public void ResetMarketCoverages(IEnumerable<MarketCoverage> marketCoverages)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var marketCoverageDbModels = marketCoverages.Select(_ToDbModel);

                    context.Database.ExecuteSqlCommand("TRUNCATE TABLE [market_coverages]");
                    context.market_coverages.AddRange(marketCoverageDbModels);
                    context.SaveChanges();
                });
        }
        
        private market_coverages _ToDbModel(MarketCoverage marketCoverage)
        {
            return new market_coverages
            {
                rank = marketCoverage.Rank.Value,
                market_code = (short)marketCoverage.MarketCode,
                tv_homes = marketCoverage.TVHomes,
                percentage_of_us = marketCoverage.PercentageOfUS
            };
        }
    }
}
