using System.Collections.Generic;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using System.Linq;

namespace Services.Broadcast.Repositories
{
    public interface INsiComponentAudienceRepository : IDataRepository
    {
        List<BroadcastAudience> GetAllNsiComponentAudiences();
    }

    public class NsiComponentAudienceRepository : BroadcastRepositoryBase, INsiComponentAudienceRepository
    {
        public NsiComponentAudienceRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<BroadcastAudience> GetAllNsiComponentAudiences()
        {
            return _InReadUncommitedTransaction(
                    context => (from a in context.nsi_component_audiences
                                select new BroadcastAudience
                                {
                                    Id = a.audience_id,
                                    CategoryCode = (EBroadcastAudienceCategoryCode)a.category_code,
                                    SubCategoryCode = a.sub_category_code,
                                    RangeStart = a.range_start,
                                    RangeEnd = a.range_end,
                                    Custom = a.custom,
                                    Code = a.code,
                                    Name = a.name
                                }).ToList());
        }
    }
}
