using Common.Services.Repositories;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.Common;
using IsolationLevel = System.Transactions.IsolationLevel;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using System.Data.Common;

namespace Services.Broadcast.Repositories
{
    public interface IRatingForecastRepository : IDataRepository
    {
        List<RatingsForecastStatus> GetForecastDetails(List<MediaMonth> sweepsMonths);
        ImpressionsPointInTimeResult GetImpressionsPointInTime(int postingBookId, List<int> uniqueRatingsAudiences, List<StationDetailPointInTime> stationDetails, ProposalEnums.ProposalPlaybackType playbackType);
        ImpressionsDaypartResultForSingleBook GetImpressionsDaypart(int postingBookId, List<int> uniqueRatingsAudiences, List<ManifestDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType);
        ImpressionsDaypartResultForTwoBooks GetImpressionsDaypart(short hutMediaMonth, short shareMediaMonth, IEnumerable<int> uniqueRatingsAudiences, List<ManifestDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType);
        List<MarketPlaybackTypes> GetPlaybackForMarketBy(int mediaMonthId, ProposalEnums.ProposalPlaybackType? playbackType);
        Dictionary<short, List<universe>> GetMarketUniverseDataByAudience(int mediaMonth, List<int> audienceIds, List<short> marketIds, List<string> playbackTypes);
        IEnumerable<ManifestDetailDaypart> AdjustDayparts(List<ManifestDetailDaypart> stationDetails);
    }

    public class RatingForecastRepository : BroadcastForecastRepositoryBase, IRatingForecastRepository
    {
        public RatingForecastRepository(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public ImpressionsPointInTimeResult GetImpressionsPointInTime(
            int postingBookId, 
            List<int> uniqueRatingsAudiences, 
            List<StationDetailPointInTime> stationDetails, 
            ProposalPlaybackType playbackType)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var book = new SqlParameter("posting_media_month_id", SqlDbType.SmallInt) { Value = postingBookId };

                    var audienceId = new SqlParameter("demo", SqlDbType.NVarChar) { Value = string.Join(",", uniqueRatingsAudiences) };

                    var ratingsInput = new DataTable();
                    ratingsInput.Columns.Add("id");
                    ratingsInput.Columns.Add("legacy_call_letters");
                    ratingsInput.Columns.Add("mon");
                    ratingsInput.Columns.Add("tue");
                    ratingsInput.Columns.Add("wed");
                    ratingsInput.Columns.Add("thu");
                    ratingsInput.Columns.Add("fri");
                    ratingsInput.Columns.Add("sat");
                    ratingsInput.Columns.Add("sun");
                    ratingsInput.Columns.Add("start_time");
                    ratingsInput.Columns.Add("end_time");

                    stationDetails.Distinct().ForEach(p => ratingsInput.Rows.Add(p.Id, p.LegacyCallLetters,
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

                    var storedProcedureName = "[nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]";

                    //WriteTableSQLDebug(storedProcedureName,stationDetails,postingBookId,audienceId.Value as string,((char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType)).ToString());

                    var command = c.Database.Connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = storedProcedureName;
                    command.Parameters.Add(book);
                    command.Parameters.Add(ratingsRequest);
                    command.Parameters.Add(minPlaybackType);
                    command.Parameters.Add(audienceId);
                    command.CommandTimeout = 0; // This makes it infinite so it will just use the transaction's timeout

                    var reader = command.ExecuteReader();
                    var impressions = new List<StationImpressionsWithAudience>();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            impressions.Add(new StationImpressionsWithAudience
                            {
                                Id = reader.GetInt32(0),
                                LegacyCallLetters = reader.GetString(1),
                                AudienceId = reader.GetInt32(2),
                                Impressions = reader.GetDouble(3)
                            });
                        }
                    }

                    reader.NextResult();

                    var marketPlaybackTypes = _ReadMarketPlaybackTypes(reader);

                    reader.Close();

                    return new ImpressionsPointInTimeResult
                    {
                        Impressions = impressions,
                        UsedMarketPlaybackTypes = marketPlaybackTypes
                    };
                });
            }
        }

        private List<MarketPlaybackType> _ReadMarketPlaybackTypes(DbDataReader reader)
        {
            var result = new List<MarketPlaybackType>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    result.Add(new MarketPlaybackType
                    {
                        MarketCode = reader.GetInt16(0),
                        PlaybackType = PlaybackTypeConverter.ForecastPlaybackTypeToProposalPlaybackType(reader.GetString(1))
                    });
                }
            }

            return result;
        }

        public ImpressionsDaypartResultForSingleBook GetImpressionsDaypart(int postingBookId, List<int> uniqueRatingsAudiences, List<ManifestDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType)
        {
            var adjustedDetails = AdjustDayparts(stationDetails);

            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(c =>
                {
                    var book = new SqlParameter("posting_media_month_id", SqlDbType.SmallInt) { Value = postingBookId };

                    var audienceId = new SqlParameter("demo", SqlDbType.NVarChar) { Value = string.Join(",", uniqueRatingsAudiences) };

                    var ratingsInput = new DataTable();
                    ratingsInput.Columns.Add("id");
                    ratingsInput.Columns.Add("legacy_call_letters");
                    ratingsInput.Columns.Add("mon");
                    ratingsInput.Columns.Add("tue");
                    ratingsInput.Columns.Add("wed");
                    ratingsInput.Columns.Add("thu");
                    ratingsInput.Columns.Add("fri");
                    ratingsInput.Columns.Add("sat");
                    ratingsInput.Columns.Add("sun");
                    ratingsInput.Columns.Add("start_time");
                    ratingsInput.Columns.Add("end_time");

                    adjustedDetails.Distinct().ForEach(p => ratingsInput.Rows.Add(
                        p.Id,
                        p.LegacyCallLetters, 
                        p.DisplayDaypart.Monday,
                        p.DisplayDaypart.Tuesday,
                        p.DisplayDaypart.Wednesday,
                        p.DisplayDaypart.Thursday,
                        p.DisplayDaypart.Friday,
                        p.DisplayDaypart.Saturday,
                        p.DisplayDaypart.Sunday,
                        p.DisplayDaypart.StartTime,
                        p.DisplayDaypart.EndTime));

                    var ratingsRequest = new SqlParameter("ratings_request", SqlDbType.Structured) { Value = ratingsInput, TypeName = "RatingsInputWithId" };

                    var minPlaybackType = new SqlParameter("min_playback_type", SqlDbType.VarChar, 1) { Value = (char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType) };

                    var storedProcedureName = "[nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages_Projections]";

                    //WriteTableSQLDebug(storedProcedureName,stationDetails, postingBookId,audienceId.Value as string,((char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType)).ToString());

                    var command = c.Database.Connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = storedProcedureName;
                    command.Parameters.Add(book);
                    command.Parameters.Add(ratingsRequest);
                    command.Parameters.Add(minPlaybackType);
                    command.Parameters.Add(audienceId);

                    var reader = command.ExecuteReader();
                    var impressions = new List<StationImpressionsWithAudience>();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            impressions.Add(new StationImpressionsWithAudience
                            {
                                Id = reader.GetInt32(0),
                                LegacyCallLetters = reader.GetString(1),
                                AudienceId = reader.GetInt32(2),
                                Impressions = reader.GetDouble(3),
                                Rating = reader.GetDouble(4)
                            });
                        }
                    }

                    reader.NextResult();

                    var marketPlaybackTypes = _ReadMarketPlaybackTypes(reader);

                    reader.Close();

                    return new ImpressionsDaypartResultForSingleBook
                    {
                        Impressions = impressions,
                        UsedMarketPlaybackTypes = marketPlaybackTypes
                    };
                });
            }
        }

        public ImpressionsDaypartResultForTwoBooks GetImpressionsDaypart(short hutMediaMonth, short shareMediaMonth, IEnumerable<int> uniqueRatingsAudiences, List<ManifestDetailDaypart> stationDetails, ProposalEnums.ProposalPlaybackType? playbackType)
        {
            var adjustedDetails = AdjustDayparts(stationDetails);

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
                    ratingsInput.Columns.Add("legacy_call_letters");
                    ratingsInput.Columns.Add("mon");
                    ratingsInput.Columns.Add("tue");
                    ratingsInput.Columns.Add("wed");
                    ratingsInput.Columns.Add("thu");
                    ratingsInput.Columns.Add("fri");
                    ratingsInput.Columns.Add("sat");
                    ratingsInput.Columns.Add("sun");
                    ratingsInput.Columns.Add("start_time");
                    ratingsInput.Columns.Add("end_time");

                    adjustedDetails.Distinct().ForEach(p => ratingsInput.Rows.Add(
                        p.Id,
                        p.LegacyCallLetters,
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

                    var storedProcedureName = "[nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]";

                    //WriteTableSQLDebug(storedProcedureName, stationDetails,hutMediaMonth,shareMediaMonth, string.Join(",", uniqueRatingsAudiences), ((char) PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType)).ToString());

                    var command = c.Database.Connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = storedProcedureName;
                    command.Parameters.Add(hut);
                    command.Parameters.Add(share);
                    command.Parameters.Add(ratingsRequest);
                    command.Parameters.Add(minPlaybackType);
                    command.Parameters.Add(audienceId);

                    var reader = command.ExecuteReader();
                    var impressions = new List<StationImpressions>();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            impressions.Add(new StationImpressions
                            {
                                Id = reader.GetInt32(0),
                                LegacyCallLetters = reader.GetString(1),
                                Impressions = reader.GetDouble(2),
                                Rating = reader.GetDouble(3)
                            });
                        }
                    }

                    reader.NextResult();

                    var hutMarketPlaybackTypes = _ReadMarketPlaybackTypes(reader);

                    reader.NextResult();

                    var shareMarketPlaybackTypes = _ReadMarketPlaybackTypes(reader);

                    reader.Close();

                    return new ImpressionsDaypartResultForTwoBooks
                    {
                        Impressions = impressions,
                        UsedHutMarketPlaybackTypes = hutMarketPlaybackTypes,
                        UsedShareMarketPlaybackTypes = shareMarketPlaybackTypes
                    };
                });
            }
        }

        public IEnumerable<ManifestDetailDaypart> AdjustDayparts(List<ManifestDetailDaypart> stationDetails)
        {
            var timeAdjust = BroadcastComposerWebSystemParameter.ImpressionStartEndTimeAdjustment;
            var startOfNextDay = BroadcastComposerWebSystemParameter.ImpressionStartOfDayForAdjustment;
            var neilsonTimeSlotInSeconds = BroadcastConstants.NeilsonTimeSlotInSeconds;

            List<ManifestDetailDaypart> adjustedDetails = new List<ManifestDetailDaypart>();

            stationDetails.ForEach(d =>
            {
                var adjustedDetail = new ManifestDetailDaypart();
                adjustedDetail.DisplayDaypart = d.DisplayDaypart.Clone() as DisplayDaypart; // clone this cause we gonna to adjust
                adjustedDetail.Id = d.Id;
                adjustedDetail.LegacyCallLetters = d.LegacyCallLetters;

                adjustedDetails.Add(adjustedDetail);
                // use the clone to keep track of original days being used.
                var clonedDaypart = adjustedDetail.DisplayDaypart;
                var timediff = clonedDaypart.EndTime - clonedDaypart.StartTime - 1;
                if (timediff > 0 && timediff > neilsonTimeSlotInSeconds) // timeajust should be half the timeslot size (15 minutes)
                {
                    clonedDaypart.StartTime += timeAdjust;
                    clonedDaypart.EndTime -= timeAdjust;
                }

                if (d.DisplayDaypart.ActiveDays < 7 && d.DisplayDaypart.StartTime >=0 && d.DisplayDaypart.EndTime < startOfNextDay)
                {
                    var weekMap = new bool[7];
                    d.DisplayDaypart.Days.ForEach(dow => weekMap[(int)dow] = true);

                    bool adjusting = false;
                    for (int c = (int)DayOfWeek.Sunday; c <= (int)DayOfWeek.Saturday; c++)
                    {
                        if (adjusting && !weekMap[c])
                        {
                            weekMap[c] = true;
                            adjusting = false;
                        }
                        else
                        if (!adjusting && weekMap[c])
                        {
                            weekMap[c] = false;
                            adjusting = true;
                        }

                    }
                    if (adjusting) weekMap[0] = true;

                    clonedDaypart.Monday = weekMap[1];
                    clonedDaypart.Tuesday = weekMap[2];
                    clonedDaypart.Wednesday = weekMap[3];
                    clonedDaypart.Thursday = weekMap[4];
                    clonedDaypart.Friday = weekMap[5];
                    clonedDaypart.Saturday = weekMap[6];
                    clonedDaypart.Sunday= weekMap[0];
                }
            });
            return adjustedDetails;
        }


        private static void WriteTableSQLDebug(string storedProcedureName,List<StationDetailPointInTime> stationDetails,int postingId,string demos,string playback)
        {
            string declare = string.Format(@"	DECLARE
		@posting_media_month_id SMALLINT = {0},
		@demo VARCHAR(MAX) = '{1}',
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1) = '{2}'",postingId,demos,playback);

            Debug.WriteLine(declare);
            stationDetails
                .Distinct()
                .ForEach(p =>
                    Debug.WriteLine(
                        string.Format(
                            "INSERT INTO @ratings_request SELECT {0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                            p.Id,
                            string.Format("{0}", p.LegacyCallLetters),
                            p.DayOfWeek == DayOfWeek.Monday ? "1" : "0",
                            p.DayOfWeek == DayOfWeek.Tuesday ? "1" : "0",
                            p.DayOfWeek == DayOfWeek.Wednesday ? "1" : "0",
                            p.DayOfWeek == DayOfWeek.Thursday ? "1" : "0",
                            p.DayOfWeek == DayOfWeek.Friday ? "1" : "0",
                            p.DayOfWeek == DayOfWeek.Saturday ? "1" : "0",
                            p.DayOfWeek == DayOfWeek.Sunday ? "1" : "0",
                            p.TimeAired,
                            p.TimeAired)));
            Debug.WriteLine(string.Format("EXEC {0} @posting_media_month_id, @demo, @ratings_request, @min_playback_type", storedProcedureName));
        }

        private static void WriteTableSQLDebug(string storedProcedureName,IEnumerable<ManifestDetailDaypart> stationDetails,int hutMediaMonth,int shareMediaMonth,string demos,string playback)
        {
            string declare = string.Format(@"DECLARE
            @hut_media_month_id SMALLINT = {0},
            @share_media_month_id SMALLINT = {1},
            @demo VARCHAR(MAX) = '{2}',
            @ratings_request RatingsInputWithId,
            @min_playback_type VARCHAR(1) = '{3}'",hutMediaMonth,shareMediaMonth,demos,playback);

            Debug.WriteLine(declare);
            stationDetails
                .Distinct()
                .ForEach(p =>
                    Debug.WriteLine(
                        string.Format(
                            "INSERT INTO @ratings_request SELECT {0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                            p.Id,
                            string.Format("{0}", p.LegacyCallLetters),
                            p.DisplayDaypart.Monday ? "1" : "0",
                            p.DisplayDaypart.Tuesday ? "1" : "0",
                            p.DisplayDaypart.Wednesday ? "1" : "0",
                            p.DisplayDaypart.Thursday ? "1" : "0",
                            p.DisplayDaypart.Friday ? "1" : "0",
                            p.DisplayDaypart.Saturday ? "1" : "0",
                            p.DisplayDaypart.Sunday ? "1" : "0",
                            p.DisplayDaypart.StartTime,
                            p.DisplayDaypart.EndTime)));

            Debug.WriteLine(string.Format("EXEC {0} @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type", storedProcedureName));
        }

        private static void WriteTableSQLDebug(string storedProcedureName, IEnumerable<ManifestDetailDaypart> stationDetails, int postingBookId, string demos, string playback)
        {
            string declare = string.Format(@"DECLARE
            @posting_media_month_id SMALLINT = {0},
            @demo VARCHAR(MAX) = '{1}',
            @ratings_request RatingsInputWithId,
            @min_playback_type VARCHAR(1) = '{2}'", postingBookId, demos, playback);

            Debug.WriteLine(declare);
            stationDetails
                .Distinct()
                .ForEach(p =>
                    Debug.WriteLine(
                        string.Format(
                            "INSERT INTO @ratings_request SELECT {0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                            p.Id,
                            string.Format("{0}", p.LegacyCallLetters),
                            p.DisplayDaypart.Monday ? "1" : "0",
                            p.DisplayDaypart.Tuesday ? "1" : "0",
                            p.DisplayDaypart.Wednesday ? "1" : "0",
                            p.DisplayDaypart.Thursday ? "1" : "0",
                            p.DisplayDaypart.Friday ? "1" : "0",
                            p.DisplayDaypart.Saturday ? "1" : "0",
                            p.DisplayDaypart.Sunday ? "1" : "0",
                            p.DisplayDaypart.StartTime,
                            p.DisplayDaypart.EndTime)));

            Debug.WriteLine(string.Format("EXEC {0} @posting_media_month_id, @demo, @ratings_request, @min_playback_type", storedProcedureName));
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
