using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using audience = EntityFrameworkMapping.Broadcast.audience;
using audience_audiences = EntityFrameworkMapping.Broadcast.audience_audiences;

namespace Services.Broadcast.Repositories
{
    public interface IBroadcastAudienceRepository : IDataRepository
    {
        List<audience> GetAudiencesByRange(int rangeStart, int rangeEnd);
        List<audience_audiences> GetRatingsAudiencesByMaestroAudience(List<int> maestroAudiences);
        Dictionary<int, List<int>> GetMaestroAudiencesGroupedByRatingAudiences(List<int> maestroAudiences);
        Dictionary<int, List<int>> GetRatingAudiencesGroupedByMaestroAudience(List<int> maestroAudiences);
        List<LookupDto> GetAudienceDtosById(List<int> proposalAudienceIds);
    }

    public class BroadcastAudienceRepository : BroadcastRepositoryBase, IBroadcastAudienceRepository
    {

        public BroadcastAudienceRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pContextFactory, pTransactionHelper)
        {

        }

        public List<audience> GetAudiencesByRange(int rangeStart, int rangeEnd)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
                return _InReadUncommitedTransaction(
                context =>
                {
                    var customAudienceQuery = (
                    from aa in context.audience_audiences
                    where aa.rating_category_group_id == 2
                    select aa.custom_audience_id);

                    var query = (
                        from a in context.audiences
                        where customAudienceQuery.Contains(a.id)
                              && a.range_start == rangeStart
                              && a.range_end == rangeEnd
                        select a);

                    return query.ToList();
                });
        }

        public List<audience_audiences> GetRatingsAudiencesByMaestroAudience(List<int> maestroAudiences)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        return (from aa in context.audience_audiences
                                where maestroAudiences.Contains(aa.custom_audience_id)
                                      && aa.rating_category_group_id == BroadcastConstants.RatingsGroupId
                                select aa).ToList();
                    });
            }
        }

        //Rating Audience to Maestro Audiences
        public Dictionary<int, List<int>> GetMaestroAudiencesGroupedByRatingAudiences(List<int> maestroAudiences)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        return (from aa in context.audience_audiences
                                where maestroAudiences.Contains(aa.custom_audience_id)
                                      && aa.rating_category_group_id == BroadcastConstants.RatingsGroupId
                                group new { aa, aa.custom_audience_id }
                                by aa.rating_audience_id
                                into gb
                                select gb).ToList().ToDictionary(gb => gb.Key, gb => gb.Select(g => g.aa.custom_audience_id).ToList());
                    });
            }
        }

        public Dictionary<int, List<int>> GetRatingAudiencesGroupedByMaestroAudience(List<int> maestroAudiences)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        return (from aa in context.audience_audiences
                                where maestroAudiences.Contains(aa.custom_audience_id)
                                      && aa.rating_category_group_id == BroadcastConstants.RatingsGroupId
                                select aa)
                                .GroupBy(aa => aa.custom_audience_id)
                                .ToDictionary(g => g.Key, g => g.Select(aa => aa.rating_audience_id).ToList());
                    });
            }
        }

        public List<LookupDto> GetAudienceDtosById(List<int> proposalAudienceIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
                return _InReadUncommitedTransaction(
                context =>
                {
                    var customAudienceQuery = (
                    from aa in context.audience_audiences
                    where aa.rating_category_group_id == 2
                    select aa.custom_audience_id);

                    var query = (
                        from a in context.audiences
                        where customAudienceQuery.Contains(a.id)
                        && proposalAudienceIds.Contains(a.id)
                        select new LookupDto()
                        {
                            Id = a.id,
                            Display = a.code
                        });

                    return query.ToList();
                });
        }
    }
}