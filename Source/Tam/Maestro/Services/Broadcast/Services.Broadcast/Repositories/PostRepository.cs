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

        /// <summary>
        /// Counts all the unlinked iscis
        /// </summary>
        /// <returns>Number of unlinked iscis</returns>
        int CountUnlinkedIscis();

        /// <summary>
        /// Gets the unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        List<UnlinkedIscisDto> GetUnlinkedIscis();
    }

    public class PostRepository : BroadcastRepositoryBase, IPostRepository
    {
        public PostRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
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
                                                          from affidavitFileScrubAudience in affidavitFileScrub.affidavit_client_scrub_audiences
                                                          where (ScrubbingStatus)affidavitFileScrub.status == ScrubbingStatus.InSpec &&
                                                                affidavitFileScrubAudience.audience_id == proposal.guaranteed_audience_id
                                                          select affidavitFileScrubAudience.impressions).Sum()
                        });
                    }

                    return posts.OrderByDescending(x => x.UploadDate).ToList();
                });
        }

        /// <summary>
        /// Counts all the unlinked iscis
        /// </summary>
        /// <returns>Number of unlinked iscis</returns>
        public int CountUnlinkedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from fileDetails in context.affidavit_file_details
                            join clientScrubs in context.affidavit_client_scrubs on fileDetails.id equals clientScrubs.affidavit_file_detail_id
                            into dataGroupped
                            from x in dataGroupped.DefaultIfEmpty()
                            where x == null
                            select fileDetails).Count();
                });
        }
        
        /// <summary>
        /// Gets the unlinked iscis
        /// </summary>
        /// <returns>List of UnlinkedIscisDto objects</returns>
        public List<UnlinkedIscisDto> GetUnlinkedIscis()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var spotLengths = (from sl in context.spot_lengths select sl).ToList();
                    var iscis = _GetUnlinkedIscisQuery(context).ToList();
                    return iscis.Select(x => new UnlinkedIscisDto
                    {
                        Affiliate = x.affiliate,
                        Genre = x.genre,
                        FileDetailId = x.id,
                        ISCI = x.isci,
                        Market = x.market,
                        ProgramName = x.program_name,
                        SpotLength = spotLengths.Single(y => y.id == x.spot_length_id).length,
                        Station = x.station,
                        TimeAired = x.air_time,
                        DateAired = x.original_air_date
                    }).ToList();
                });
        }

        private IQueryable<affidavit_file_details> _GetUnlinkedIscisQuery(QueryHintBroadcastContext context)
        {
            return from fileDetails in context.affidavit_file_details
                   join clientScrubs in context.affidavit_client_scrubs on fileDetails.id equals clientScrubs.affidavit_file_detail_id
                   into dataGroupped
                   from x in dataGroupped.DefaultIfEmpty()
                   where x == null
                   select fileDetails;
        }
    }
}
