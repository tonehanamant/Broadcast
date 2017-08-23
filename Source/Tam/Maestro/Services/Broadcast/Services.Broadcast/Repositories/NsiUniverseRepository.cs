﻿using Common.Services.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.ExternalRating;
using Tam.Maestro.Services.Clients;

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

    public class NsiUniverseRepository : ExternalRatingRepositoryBase, INsiUniverseRepository
    {
        public NsiUniverseRepository(ISMSClient pSmsClient, IContextFactory<QueryHintExternalRatingContext> pContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pContextFactory, pTransactionHelper)
        {
        }

        /// <summary>
        /// Returns a dictionary with market codes and subscribers for specified audiences
        /// </summary>
        /// <param name="sweepMediaMonth"></param>
        /// <param name="audienceIds"></param>
        /// <returns></returns>
        public Dictionary<short, double> GetUniverseDataByAudience(int sweepMediaMonth, List<int> audienceIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var marketUniverses =
                            (from pu in context.post_universes
                                join mh in context.market_headers on pu.market_header_id equals mh.id
                                where
                                    pu.media_month_id == sweepMediaMonth &&
                                    pu.sample_type == 1 &&
                                    audienceIds.Contains(pu.audience_id)
                                select new
                                {
                                    mh.market_code,
                                    pu.universe
                                });

                        var universeData = marketUniverses.GroupBy(g => g.market_code);
                        return universeData.ToDictionary(k => k.Key, v => v.Sum(el => el.universe));
                    });
            }
        }
    }
}
