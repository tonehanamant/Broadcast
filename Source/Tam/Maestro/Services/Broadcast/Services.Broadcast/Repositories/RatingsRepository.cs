﻿using Common.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;

namespace Services.Broadcast.Repositories
{
    public interface IRatingsRepository : IDataRepository
    {
        List<Tuple<MediaMonth, int>> GetNielsonMarkets(List<MediaMonth> mediaMonths);
        string GetDbInfo();
    }

    public class RatingsRepository : BroadcastForecastRepositoryBase, IRatingsRepository
    {
        public RatingsRepository(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public List<Tuple<MediaMonth, int>> GetNielsonMarkets(List<MediaMonth> mediaMonths)
        {
            return _InReadUncommitedTransaction(c =>
            {
                var mediaMonthIds = mediaMonths.Select(m => m.Id).ToList();

                var months = from mh in c.market_headers
                             where mediaMonthIds.Contains(mh.media_month_id) && mh.reporting_service == "1"
                             group mh by mh.media_month_id into gb
                             select new
                             {
                                 MediaMonthId = gb.Key,
                                 Markets = gb.Select(g => g.dma_id).Distinct().Count()
                             };

                var monthsDict = months.ToDictionary(m => m.MediaMonthId, m => m.Markets);

                return mediaMonths.Select(m =>
                {
                    int markets;
                    var hasValue = monthsDict.TryGetValue(m.Id, out markets);
                    return Tuple.Create(m, hasValue ? markets : 0);
                }).ToList();
            });
        }
    }
}
