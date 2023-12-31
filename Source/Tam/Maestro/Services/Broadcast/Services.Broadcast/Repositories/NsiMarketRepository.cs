﻿using Common.Services.Repositories;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;

namespace Services.Broadcast.Repositories
{
    public interface INsiMarketRepository : IDataRepository
    {
        Dictionary<int, int> GetMarketRankingsByMediaMonth(int mediaMonthId);

        List<MarketRankingsByMediaMonth> GetMarketRankingsByMediaMonths(IEnumerable<int> mediaMonthIds);
    }

    public class NsiMarketRepository : BroadcastForecastRepositoryBase, INsiMarketRepository
    {
        public NsiMarketRepository(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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

        public List<MarketRankingsByMediaMonth> GetMarketRankingsByMediaMonths(IEnumerable<int> mediaMonthIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        return context.market_headers
                                .Where(x => mediaMonthIds.Contains(x.media_month_id))
                                .ToList()
                                .GroupBy(x => x.media_month_id)
                                .Select(group => new MarketRankingsByMediaMonth
                                {
                                    MediaMonthId = group.Key,
                                    MarketCodeRankMappings = group
                                        .DistinctBy(x => x.market_code)
                                        .ToDictionary(m => Convert.ToInt32(m.market_code), m => Convert.ToInt32(m.market_rank))
                                })
                                .ToList();
                    });
            }
        }
    }
}
