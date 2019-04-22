using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IBvsPostDetailsRepository : IDataRepository
    {
        void SavePostDetails(List<BvsPostDetailAudience> postResults);
        void DeletePostDetails(int estimateId);
        void DeletePostDetails(int estimateId,List<int> audienceIds);
    }
    public class BvsPostDetailsRepository : BroadcastRepositoryBase, IBvsPostDetailsRepository
    {
        public BvsPostDetailsRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public void SavePostDetails(List<BvsPostDetailAudience> postResults)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    BulkInsert(context, Convert(postResults));
                });
        }

        public void DeletePostDetails(int estimateId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var postDetails = (from pd in context.bvs_post_details
                        join fd in context.bvs_file_details on pd.bvs_file_detail_id equals fd.id
                        where fd.estimate_id == estimateId
                        select pd);
                    context.bvs_post_details.RemoveRange(postDetails);
                    context.SaveChanges();
                });
        }

        public void DeletePostDetails(int estimateId,List<int> audienceIds )
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var postDetails = (from pd in context.bvs_post_details
                                       join fd in context.bvs_file_details on pd.bvs_file_detail_id equals fd.id
                                       where fd.estimate_id == estimateId 
                                             && audienceIds.Contains(pd.audience_id)
                                       select pd);
                    context.bvs_post_details.RemoveRange(postDetails);
                    context.SaveChanges();
                });
        }

        private List<bvs_post_details> Convert(List<BvsPostDetailAudience> postDetails)
        {
            return postDetails.Select(
                x => new bvs_post_details()
                {
                    bvs_file_detail_id = x.BvsDetailId
                    ,audience_rank = x.AudienceRank
                    ,audience_id = x.AudienceId
                    ,delivery = x.Delivery
                }).ToList();
        }
    }
}
