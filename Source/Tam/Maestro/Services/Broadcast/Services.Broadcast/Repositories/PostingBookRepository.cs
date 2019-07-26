using Common.Services.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IPostingBookRepository : IDataRepository
    {
        List<int> GetPostableMediaMonths(int marketThreshold);
    }

    public class PostingBookRepository : BroadcastForecastRepositoryBase, IPostingBookRepository
    {
        public PostingBookRepository(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<int> GetPostableMediaMonths(int marketThreshold)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                context => (from x in context.post_months
                            where x.num_markets > marketThreshold
                            select x.media_month_id).ToList());
            }
        }
    }
}
