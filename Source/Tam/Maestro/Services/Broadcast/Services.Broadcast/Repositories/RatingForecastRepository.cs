using Common.Services.Repositories;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Clients;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Services.Broadcast.Repositories
{
    public interface IRatingForecastRepository : IDataRepository
    {
        List<RatingsResult> ForecastRatings(short hutMediaMonth, short shareMediaMonth, IEnumerable<int> audience, PlaybackType playbackType, IEnumerable<Program> programs, bool useDayByDayImpressions);
        List<RatingsForecastStatus> GetForecastDetails(List<MediaMonth> sweepsMonths);
        void CrunchMonth(short mediaMonthId, DateTime startDate, DateTime endDate);
        List<StationImpressionsWithAudience> GetImpressionsPointInTime(int postingBookId, List<int> uniqueRatingsAudiences, List<StationDetailPointInTime> stationDetails, ProposalEnums.ProposalPlaybackType playbackType, bool useDayByDayImpressions);
        List<StationImpressionsWithAudience> GetImpressionsDaypart(int postingBookId, List<int> uniqueRatingsAudiences, IEnumerable<StationDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType, bool useDayByDayImpressions);
        List<StationImpressions> GetImpressionsDaypart(short hutMediaMonth, short shareMediaMonth, IEnumerable<int> uniqueRatingsAudiences, IEnumerable<StationDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType, bool useDayByDayImpressions);
        List<MarketPlaybackTypes> GetPlaybackForMarketBy(int mediaMonthId, ProposalEnums.ProposalPlaybackType? playbackType);
        Dictionary<short, List<universe>> GetMarketUniverseDataByAudience(int mediaMonth, List<int> audienceIds, List<short> marketIds, List<string> playbackTypes);
    }

    public class RatingForecastRepository : BroadcastForecastRepositoryBase, IRatingForecastRepository
    {
        public RatingForecastRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper) { }

        public List<RatingsResult> ForecastRatings(short hutMediaMonth, short shareMediaMonth, IEnumerable<int> audience, PlaybackType playbackType, IEnumerable<Program> programs, bool useDayByDayImpressions)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var hut = new SqlParameter("hut_media_month_id", SqlDbType.SmallInt) { Value = hutMediaMonth };

                    var share = new SqlParameter("share_media_month_id", SqlDbType.SmallInt) { Value = shareMediaMonth };

                    var audienceId = new SqlParameter("demo", SqlDbType.NVarChar) { Value = string.Join(",", audience) };

                    var ratingsInput = new DataTable();
                    ratingsInput.Columns.Add("station_code");
                    ratingsInput.Columns.Add("mon");
                    ratingsInput.Columns.Add("tue");
                    ratingsInput.Columns.Add("wed");
                    ratingsInput.Columns.Add("thu");
                    ratingsInput.Columns.Add("fri");
                    ratingsInput.Columns.Add("sat");
                    ratingsInput.Columns.Add("sun");
                    ratingsInput.Columns.Add("start_time");
                    ratingsInput.Columns.Add("end_time");

                    programs.Distinct().ForEach(p => ratingsInput.Rows.Add(p.StationCode,
                        p.DisplayDaypart.Monday,
                        p.DisplayDaypart.Tuesday,
                        p.DisplayDaypart.Wednesday,
                        p.DisplayDaypart.Thursday,
                        p.DisplayDaypart.Friday,
                        p.DisplayDaypart.Saturday,
                        p.DisplayDaypart.Sunday,
                        p.DisplayDaypart.StartTime,
                        p.DisplayDaypart.EndTime));
                    var ratingsRequest = new SqlParameter("ratings_request", SqlDbType.Structured)
                    {
                        Value = ratingsInput,
                        TypeName = "RatingsInput"
                    };

                    var minPlaybackType = new SqlParameter("min_playback_type", SqlDbType.VarChar, 1)
                    {
                        Value = (char)playbackType
                    };

                    var storedProcedureName = useDayByDayImpressions ? "usp_ForecastNsiRatingsForMultiplePrograms" : "usp_ForecastNsiRatingsForMultiplePrograms_Averages";

                    return c.Database.SqlQuery<RatingsResult>(string.Format(@"EXEC [nsi].[{0}] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type", storedProcedureName), hut, share, audienceId, ratingsRequest, minPlaybackType).ToList();
                });
            }
        }

        public void CrunchMonth(short mediaMonthId, DateTime startDate, DateTime endDate)
        {
            _InReadUncommitedTransaction(c => c.usp_ForecastNsiRatingsMonth(mediaMonthId, startDate, endDate));
        }

        public List<StationImpressionsWithAudience> GetImpressionsPointInTime(int postingBookId, List<int> uniqueRatingsAudiences, List<StationDetailPointInTime> stationDetails, ProposalEnums.ProposalPlaybackType playbackType, bool useDayByDayImpressions)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var book = new SqlParameter("posting_media_month_id", SqlDbType.SmallInt) { Value = postingBookId };

                    var audienceId = new SqlParameter("demo", SqlDbType.NVarChar) { Value = string.Join(",", uniqueRatingsAudiences) };

                    var ratingsInput = new DataTable();
                    ratingsInput.Columns.Add("id");
                    ratingsInput.Columns.Add("station_code");
                    ratingsInput.Columns.Add("mon");
                    ratingsInput.Columns.Add("tue");
                    ratingsInput.Columns.Add("wed");
                    ratingsInput.Columns.Add("thu");
                    ratingsInput.Columns.Add("fri");
                    ratingsInput.Columns.Add("sat");
                    ratingsInput.Columns.Add("sun");
                    ratingsInput.Columns.Add("start_time");
                    ratingsInput.Columns.Add("end_time");

                    stationDetails.Distinct().ForEach(p => ratingsInput.Rows.Add(p.Id, p.Code,
                        p.DayOfWeek == DayOfWeek.Monday,
                        p.DayOfWeek == DayOfWeek.Tuesday,
                        p.DayOfWeek == DayOfWeek.Wednesday,
                        p.DayOfWeek == DayOfWeek.Thursday,
                        p.DayOfWeek == DayOfWeek.Friday,
                        p.DayOfWeek == DayOfWeek.Saturday,
                        p.DayOfWeek == DayOfWeek.Sunday,
                        p.TimeAired,
                        p.TimeAired));

                    var ratingsRequest = new SqlParameter("ratings_request", SqlDbType.Structured) { Value = ratingsInput, TypeName = "RatingsInputWithId" };

                    var minPlaybackType = new SqlParameter("min_playback_type", SqlDbType.VarChar, 1) { Value = (char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType) };

                    var storedProcedureName = useDayByDayImpressions ? "usp_GetImpressionsForMultiplePrograms_Daypart" : "usp_GetImpressionsForMultiplePrograms_Daypart_Averages";

                    return c.Database.SqlQuery<StationImpressionsWithAudience>(string.Format(@"EXEC [nsi].[{0}] @posting_media_month_id, @demo, @ratings_request, @min_playback_type", storedProcedureName), book, audienceId, ratingsRequest, minPlaybackType).ToList();
                });
            }
        }

        public List<StationImpressionsWithAudience> GetImpressionsDaypart(int postingBookId, List<int> uniqueRatingsAudiences, IEnumerable<StationDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType, bool useDayByDayImpressions)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var book = new SqlParameter("posting_media_month_id", SqlDbType.SmallInt) { Value = postingBookId };

                    var audienceId = new SqlParameter("demo", SqlDbType.NVarChar) { Value = string.Join(",", uniqueRatingsAudiences) };

                    var ratingsInput = new DataTable();
                    ratingsInput.Columns.Add("id");
                    ratingsInput.Columns.Add("station_code");
                    ratingsInput.Columns.Add("mon");
                    ratingsInput.Columns.Add("tue");
                    ratingsInput.Columns.Add("wed");
                    ratingsInput.Columns.Add("thu");
                    ratingsInput.Columns.Add("fri");
                    ratingsInput.Columns.Add("sat");
                    ratingsInput.Columns.Add("sun");
                    ratingsInput.Columns.Add("start_time");
                    ratingsInput.Columns.Add("end_time");

                    stationDetails.Distinct().ForEach(p => ratingsInput.Rows.Add(p.Id, p.Code,
                        p.DisplayDaypart.Monday,
                        p.DisplayDaypart.Tuesday,
                        p.DisplayDaypart.Wednesday,
                        p.DisplayDaypart.Thursday,
                        p.DisplayDaypart.Friday,
                        p.DisplayDaypart.Saturday,
                        p.DisplayDaypart.Sunday,
                        p.DisplayDaypart.StartTime,
                        p.DisplayDaypart.EndTime));

                    //stationDetails
                    //    .Distinct()
                    //    .ForEach(p =>
                    //        Debug.WriteLine(
                    //            string.Format(
                    //                "INSERT INTO @ratings_request SELECT {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    //                p.Id,
                    //                p.Code,
                    //                p.DisplayDaypart.Monday ? "1" : "0",
                    //                p.DisplayDaypart.Tuesday ? "1" : "0",
                    //                p.DisplayDaypart.Wednesday ? "1" : "0",
                    //                p.DisplayDaypart.Thursday ? "1" : "0",
                    //                p.DisplayDaypart.Friday ? "1" : "0",
                    //                p.DisplayDaypart.Saturday ? "1" : "0",
                    //                p.DisplayDaypart.Sunday ? "1" : "0",
                    //                p.DisplayDaypart.StartTime,
                    //                p.DisplayDaypart.EndTime)));

                    var ratingsRequest = new SqlParameter("ratings_request", SqlDbType.Structured) { Value = ratingsInput, TypeName = "RatingsInputWithId" };

                    var minPlaybackType = new SqlParameter("min_playback_type", SqlDbType.VarChar, 1) { Value = (char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType) };

                    var storedProcedureName = useDayByDayImpressions ? "usp_GetImpressionsForMultiplePrograms_Daypart" : "usp_GetImpressionsForMultiplePrograms_Daypart_Averages";

                    return c.Database.SqlQuery<StationImpressionsWithAudience>(string.Format(@"EXEC [nsi].[{0}] @posting_media_month_id, @demo, @ratings_request, @min_playback_type", storedProcedureName), book, audienceId, ratingsRequest, minPlaybackType).ToList();
                });
            }
        }

        public List<RatingsForecastStatus> GetForecastDetails(List<MediaMonth> sweepsMonths)
        {
            return _InReadUncommitedTransaction(c =>
            {
                var details = sweepsMonths.ToDictionary(m => m.Id, m => new RatingsForecastStatus { MediaMonth = m });

                var sweepsMonthIds = sweepsMonths.Select(m => m.Id).ToList();
                var universes = from u in c.universes
                                where sweepsMonthIds.Contains(u.media_month_id)
                                group u by u.media_month_id into gb
                                select new
                                {
                                    MediaMonthId = gb.Key,
                                    Markets = gb.Select(g => g.market_code).Distinct().Count()
                                };
                universes.ForEach(u => details[u.MediaMonthId].UniverseMarkets = u.Markets);

                var viewers = from u in c.viewers
                              where sweepsMonthIds.Contains(u.media_month_id)
                              group u by u.media_month_id into gb
                              select new
                              {
                                  MediaMonthId = gb.Key,
                                  Markets = gb.Select(g => g.market_code).Distinct().Count()
                              };
                viewers.ForEach(u => details[u.MediaMonthId].ViewerMarkets = u.Markets);

                var usages = from u in c.usages
                             where sweepsMonthIds.Contains(u.media_month_id)
                             group u by u.media_month_id into gb
                             select new
                             {
                                 MediaMonthId = gb.Key,
                                 Markets = gb.Select(g => g.market_code).Distinct().Count()
                             };

                usages.ForEach(u => details[u.MediaMonthId].UsageMarkets = u.Markets);

                return details.Select(d => d.Value).ToList();
            });
        }

        public Dictionary<short, List<universe>> GetMarketUniverseDataByAudience(int mediaMonth, List<int> audienceIds, List<short> marketIds, List<string> playbackTypes)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var univ =
                        from u in c.universes
                        where u.media_month_id == mediaMonth
                              && audienceIds.Contains(u.audience_id)
                              && marketIds.Contains(u.market_code)
                              && playbackTypes.Contains(u.playback_type)
                        select u;

                    var ag = univ.GroupBy(g => g.market_code)
                        .ToDictionary(k1 => k1.Key, v2 => v2.Where(u => u.market_code == v2.Key).ToList());
                    return ag;
                });
            }
        }


        public List<StationImpressions> GetImpressionsDaypart(short hutMediaMonth, short shareMediaMonth, IEnumerable<int> uniqueRatingsAudiences, IEnumerable<StationDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType, bool useDayByDayImpressions)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var hut = new SqlParameter("hut_media_month_id", SqlDbType.SmallInt) { Value = hutMediaMonth };

                    var share = new SqlParameter("share_media_month_id", SqlDbType.SmallInt) { Value = shareMediaMonth };

                    var audienceId = new SqlParameter("demo", SqlDbType.NVarChar)
                    {
                        Value = string.Join(",", uniqueRatingsAudiences)
                    };

                    var ratingsInput = new DataTable();
                    ratingsInput.Columns.Add("id");
                    ratingsInput.Columns.Add("station_code");
                    ratingsInput.Columns.Add("mon");
                    ratingsInput.Columns.Add("tue");
                    ratingsInput.Columns.Add("wed");
                    ratingsInput.Columns.Add("thu");
                    ratingsInput.Columns.Add("fri");
                    ratingsInput.Columns.Add("sat");
                    ratingsInput.Columns.Add("sun");
                    ratingsInput.Columns.Add("start_time");
                    ratingsInput.Columns.Add("end_time");

                    stationDetails.Distinct().ForEach(p => ratingsInput.Rows.Add(
                        p.Id,
                        p.Code,
                        p.DisplayDaypart.Monday,
                        p.DisplayDaypart.Tuesday,
                        p.DisplayDaypart.Wednesday,
                        p.DisplayDaypart.Thursday,
                        p.DisplayDaypart.Friday,
                        p.DisplayDaypart.Saturday,
                        p.DisplayDaypart.Sunday,
                        p.DisplayDaypart.StartTime,
                        p.DisplayDaypart.EndTime));

                    var ratingsRequest = new SqlParameter("ratings_request", SqlDbType.Structured)
                    {
                        Value = ratingsInput,
                        TypeName = "RatingsInputWithId"
                    };

                    var minPlaybackType = new SqlParameter("min_playback_type", SqlDbType.VarChar, 1)
                    {
                        Value = (char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType)
                    };

                    var storedProcedureName = useDayByDayImpressions ? "usp_GetImpressionsForMultiplePrograms_TwoBooks" : "usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages";

                    return c.Database.SqlQuery<StationImpressions>(string.Format(@"EXEC [nsi].[{0}] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type", storedProcedureName), hut, share, audienceId, ratingsRequest, minPlaybackType).ToList();
                });
            }
        }

        public List<MarketPlaybackTypes> GetPlaybackForMarketBy(int mediaMonthId, ProposalEnums.ProposalPlaybackType? playbackType)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var mediaMonth = new SqlParameter("media_month_id", SqlDbType.SmallInt) { Value = mediaMonthId };

                    var minPlaybackType = new SqlParameter("min_playback_type", SqlDbType.VarChar, 1) { Value = (char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType) };

                    return c.Database.SqlQuery<MarketPlaybackTypes>(@"SELECT * from nsi.udf_GetMinPlaybackTypes(@media_month_id,@min_playback_type)", mediaMonth, minPlaybackType).ToList();
                });
            }
        }
    }
}