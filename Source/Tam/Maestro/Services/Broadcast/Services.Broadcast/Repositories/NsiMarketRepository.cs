using Common.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.ExternalRating;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface INsiMarketRepository : IDataRepository
    {
        Dictionary<int, int> GetMarketRankingsByMediaMonth(int mediaMonthId);
        List<int> GetTopMarketByMediaMonth(int mediaMonthId, int countOfMarketsToTake);
    }

    public class NsiMarketRepository : ExternalRatingRepositoryBase, INsiMarketRepository
    {
        public NsiMarketRepository(
            ISMSClient pSmsClient,
            IContextFactory<QueryHintExternalRatingContext> pContextFactory,
            ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pContextFactory, pTransactionHelper)
        {
        }

        public Dictionary<int, int> GetMarketRankingsByMediaMonth(int mediaMonthId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var marketRankings =
                            context.market_headers.Where(mh => mh.media_month_id == mediaMonthId)
                                .DistinctBy(m => m.market_code)
                                .ToDictionary(m => Convert.ToInt32(m.market_code), m => Convert.ToInt32(m.market_rank));

                        return marketRankings;
                    });
            }
        }

        public List<int> GetTopMarketByMediaMonth(int mediaMonthId, int countOfMarketsToTake)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => context.market_headers.Where(mh => mh.media_month_id == mediaMonthId)
                        .DistinctBy(m => m.market_code)
                        .OrderBy(m => m.market_rank)
                        .Take(countOfMarketsToTake)
                        .Select(m => Convert.ToInt32(m.market_code))
                        .ToList());
            }
        }
    }
}
