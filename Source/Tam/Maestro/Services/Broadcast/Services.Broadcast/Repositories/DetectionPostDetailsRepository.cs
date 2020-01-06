using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IDetectionPostDetailsRepository : IDataRepository
    {
        void SavePostDetails(List<DetectionPostDetailAudience> postResults);
        void DeletePostDetails(int estimateId);
        void DeletePostDetails(int estimateId,List<int> audienceIds);
    }
    public class DetectionPostDetailsRepository : BroadcastRepositoryBase, IDetectionPostDetailsRepository
    {
        public DetectionPostDetailsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
        {
        }

        public void SavePostDetails(List<DetectionPostDetailAudience> postResults)
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
                    var postDetails = (from pd in context.detection_post_details
                                       join fd in context.detection_file_details on pd.detection_file_detail_id equals fd.id
                        where fd.estimate_id == estimateId
                        select pd);
                    context.detection_post_details.RemoveRange(postDetails);
                    context.SaveChanges();
                });
        }

        public void DeletePostDetails(int estimateId,List<int> audienceIds )
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var postDetails = (from pd in context.detection_post_details
                                       join fd in context.detection_file_details on pd.detection_file_detail_id equals fd.id
                                       where fd.estimate_id == estimateId 
                                             && audienceIds.Contains(pd.audience_id)
                                       select pd);
                    context.detection_post_details.RemoveRange(postDetails);
                    context.SaveChanges();
                });
        }

        private List<detection_post_details> Convert(List<DetectionPostDetailAudience> postDetails)
        {
            return postDetails.Select(
                x => new detection_post_details()
                {
                    detection_file_detail_id = x.DetectionDetailId
                    ,audience_rank = x.AudienceRank
                    ,audience_id = x.AudienceId
                    ,delivery = x.Delivery
                }).ToList();
        }
    }
}
