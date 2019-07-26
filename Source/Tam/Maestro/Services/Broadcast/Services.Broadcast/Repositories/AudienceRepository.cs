using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using System.Data.Entity;

namespace Services.Broadcast.Repositories
{
    public interface IAudienceRepository : IDataRepository
    {
        DisplayAudience GetDisplayAudienceByCode(string audienceCode);
        List<BroadcastAudience> GetAllAudiences(int ratingsGroupId);
        List<LookupDto> GetAudiencesByIds(List<int> audiencesIds);
        Dictionary<string, LookupDto> GetAudienceMaps();
    }

    public class AudienceRepository : BroadcastRepositoryBase, IAudienceRepository
    {
        public AudienceRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }
        

        public DisplayAudience GetDisplayAudienceByCode(string audienceCode)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from a in context.audiences
                                where a.code == audienceCode
                                select new DisplayAudience()
                                {
                                    Id = a.id,
                                    AudienceString = a.name
                                }).SingleOrDefault());
            }
        }

        public List<BroadcastAudience> GetAllAudiences(int ratingsGroupId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from a in context.audiences
                                join aa in context.audience_audiences on a.id equals aa.custom_audience_id
                                where aa.rating_category_group_id == ratingsGroupId
                                select new BroadcastAudience()
                                {
                                    Id = a.id,
                                    CategoryCode = (EBroadcastAudienceCategoryCode)a.category_code,
                                    SubCategoryCode = a.sub_category_code,
                                    RangeStart = a.range_start,
                                    RangeEnd = a.range_end,
                                    Custom = a.custom,
                                    Code = a.code,
                                    Name = a.name,
                                }).Distinct().ToList());
            }
        }

        public List<LookupDto> GetAudiencesByIds(List<int> audiencesIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from c in context.audiences
                                where audiencesIds.Contains(c.id)
                                select new LookupDto()
                                {
                                    Display = c.name,
                                    Id = c.id
                                }).ToList());
            }
        }

        public Dictionary<string, LookupDto> GetAudienceMaps()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from c in context.audience_maps
                                .Include(l => l.audience)
                            select new { audience_map = c, c.audience })
                            .ToDictionary(x => x.audience_map.map_value, x => new LookupDto(x.audience.id, x.audience.code));
                });
        }
    }
}
