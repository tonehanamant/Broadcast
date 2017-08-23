using Common.Services.Repositories;
using Services.Broadcast.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.ExternalRating;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IRatingsRepository : IDataRepository
    {
        Dictionary<int, ILookup<string, RatingsDetail>> GetDeliveryDetails(int mediaMonthId, List<int> audienceIds, List<string> station);
        List<Tuple<MediaMonth, int>> GetNielsonMarkets(List<MediaMonth> mediaMonths);
    }

    public class RatingsRepository : ExternalRatingRepositoryBase, IRatingsRepository
    {
        public RatingsRepository(ISMSClient pSmsClient, IContextFactory<QueryHintExternalRatingContext> pContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pContextFactory, pTransactionHelper)
        {
        }

        public Dictionary<int, ILookup<string, RatingsDetail>> GetDeliveryDetails(int mediaMonthId, List<int> audienceIds, List<string> station)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
                return _InReadUncommitedTransaction(
                context =>
                {
                    var queryable = from pv in context.post_viewers
                                    join mh in context.market_headers on pv.market_header_id equals mh.id
                                    where pv.media_month_id == mediaMonthId
                                          && pv.sample_type == 1
                                          && station.Contains(pv.legacy_call_letters)
                                          && audienceIds.Contains(pv.audience_id)
                                    select new
                                    {
                                        AudienceId = pv.audience_id,
                                        StartTime = pv.start_time,
                                        EndTime = pv.end_time,
                                        Viewers = pv.viewers,
                                        StationCode = pv.legacy_call_letters,
                                        WeekPart = pv.week_part
                                    };

                    return queryable.GroupBy(q => q.WeekPart).ToDictionary(wp => (int)wp.Key, wp => wp.ToLookup(a => a.StationCode,
                           a =>
                               new RatingsDetail
                               {
                                   AudienceId = a.AudienceId,
                                   EndTime = a.EndTime,
                                   StartTime = a.StartTime,
                                   Viewers = a.Viewers
                               }));
                });
        }

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
