using Common.Services.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;

namespace Services.Broadcast.Repositories
{
    public interface INsiUniverseRepository : IDataRepository
    {
        /// <summary>
        /// Returns a dictionary with market codes and subscribers for specified audiences
        /// </summary>
        /// <param name="sweepMediaMonth"></param>
        /// <param name="audienceIds"></param>
        /// <returns></returns>
        Dictionary<short, double> GetUniverseDataByAudience(int sweepMediaMonth, List<int> audienceIds);
    }

    public class NsiUniverseRepository : BroadcastForecastRepositoryBase, INsiUniverseRepository
    {
        public NsiUniverseRepository(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        /// <summary>
        /// Returns a dictionary with market codes and subscribers for specified audiences
        /// </summary>
        /// <param name="sweepMediaMonth"></param>
        /// <param name="audienceIds"></param>
        /// <returns></returns>
        public Dictionary<short, double> GetUniverseDataByAudience(int sweepMediaMonth, List<int> audienceIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, System.Transactions.IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var marketUniverses =
                            (from pu in context.universes
                             where
                                 pu.media_month_id == sweepMediaMonth &&
                                 audienceIds.Contains(pu.audience_id)
                             select new
                             {
                                 pu.market_code,
                                 pu.universe1
                             });

                        var universeData = marketUniverses.GroupBy(g => g.market_code);
                        return universeData.ToDictionary(k => k.Key, v => v.Average(el => el.universe1));
                    });
            }
        }
    }
}
