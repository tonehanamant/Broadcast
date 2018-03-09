using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IPostRepository : IDataRepository
    {
        List<PostDto> GetAllPostFiles();       
    }

    public class PostRepository : BroadcastRepositoryBase, IPostRepository
    {
        public PostRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<PostDto> GetAllPostFiles()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var proposals = context.proposal_versions.Where(p =>
                        (ProposalEnums.ProposalStatusType)p.status == ProposalEnums.ProposalStatusType.Contracted).ToList();
                    var posts = new List<PostDto>();

                    foreach (var proposal in proposals)
                    {
                        var spots = (from proposalVersionDetail in proposal.proposal_version_details
                                     from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                     from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                     from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                     select affidavitFileScrub).ToList();

                        posts.Add(new PostDto
                        {
                            ContractId = proposal.proposal_id,
                            ContractName = proposal.proposal.name,
                            UploadDate = (from proposalVersionDetail in proposal.proposal_version_details
                                          from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                          from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                          from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                          orderby affidavitFileScrub.affidavit_file_details.affidavit_files.created_date descending
                                          select (DateTime?)affidavitFileScrub.affidavit_file_details.affidavit_files.created_date).FirstOrDefault(),
                            SpotsInSpec = spots.Count(s => (ScrubbingStatus)s.status == ScrubbingStatus.InSpec),
                            SpotsOutOfSpec = spots.Count(s => (ScrubbingStatus)s.status == ScrubbingStatus.OutOfSpec),
                            PrimaryAudienceImpressions = (from proposalVersionDetail in proposal.proposal_version_details
                                                          from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                                          from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                                          from affidavitFileScrub in proposalVersionWeeks.affidavit_client_scrubs
                                                          from affidavitFileDetailAudience in affidavitFileScrub.affidavit_file_details.affidavit_file_detail_audiences
                                                          where (ScrubbingStatus)affidavitFileScrub.status == ScrubbingStatus.InSpec &&
                                                                affidavitFileDetailAudience.audience_id == proposal.guaranteed_audience_id
                                                          select affidavitFileDetailAudience.impressions).Sum()
                        });
                    }

                    return posts.OrderByDescending(x => x.UploadDate).ToList();
                });
        }
    }
}
