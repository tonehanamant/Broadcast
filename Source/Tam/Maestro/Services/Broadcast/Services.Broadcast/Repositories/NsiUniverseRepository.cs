using Common.Services.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;
using System.Data;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities.Enums;

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

        /// <summary>
        /// Gets the universe value by summing up all the universes for all the markets based on an audience id and a media month
        /// </summary>
        /// <param name="mediaMonthId">Media Month Id</param>
        /// <param name="audienceId">Audience Id</param>
        /// <param name="playbackType">Minumum playback type</param>
        /// <returns>Value of the universe for that media month and audience id</returns>
        double GetAudienceUniverseForMediaMonth(int mediaMonthId, int audienceId, ProposalEnums.ProposalPlaybackType playbackType);
    }

    public class NsiUniverseRepository : BroadcastForecastRepositoryBase, INsiUniverseRepository
    {
        public NsiUniverseRepository(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

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

        ///<inheritdoc/>
        public double GetAudienceUniverseForMediaMonth(int mediaMonthId, int audienceId, ProposalEnums.ProposalPlaybackType playbackType)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, System.Transactions.IsolationLevel.ReadUncommitted))
            {
                string sql = @"SELECT SUM(t1.universe)
                                FROM nsi.universes AS t1
                                INNER JOIN (SELECT * FROM nsi.udf_GetMinPlaybackTypes(@media_month_id,@min_playback_type)) AS t2
                                ON t1.market_code = t2.market_code AND t1.playback_type = t2.available_playback_type
                                WHERE t1.audience_id = @audience_id";
                return _InReadUncommitedTransaction(context =>
                {
                    var mediaMonth = new System.Data.SqlClient.SqlParameter("media_month_id", SqlDbType.SmallInt) { Value = mediaMonthId };
                    var minPlaybackType = new System.Data.SqlClient.SqlParameter("min_playback_type", SqlDbType.VarChar, 1)
                    {
                        Value = (char)PlaybackTypeConverter.ProposalPlaybackTypeToForecastPlaybackType(playbackType)
                    };
                    var audience = new System.Data.SqlClient.SqlParameter("audience_id", SqlDbType.SmallInt) { Value = audienceId };

                    return context.Database.SqlQuery<double>(sql, mediaMonth, minPlaybackType, audience).Single();
                });
            }
        }
    }
}
